using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ScintillaNET;

namespace armed
{
    /// <summary>
    /// Lexer for ARM assembly text.
    /// </summary>
    public class ArmLexer
    {
        //Style colours.
        public static int StyleDefault = 0;
        public static int StyleImmediate = 1;
        public static int StyleKeyword = 2;
        public static int StyleLabel = 3;
        public static int StyleComment = 4;
        public static int StyleRegister = 5;
        public static int StyleSpecial = 6;

        //Indicator IDs.
        public static int IndicatorError = 0;

        /// <summary>
        /// Styles a given portion of the Scintilla editor according to ARM assembly
        /// syntax rules.
        /// </summary>
        public static void Style(Scintilla editor, int startPos, int endPos)
        {
            //Roll back to the start of the line.
            while (editor.GetCharAt(startPos) != '\n' && startPos > 0) { startPos--; }

            //Get the portion of the editor to style as a string.
            string toStyle = editor.GetTextRange(startPos, endPos - startPos);
            while (!toStyle.EndsWith("\r") && endPos < editor.TextLength)
            {
                endPos++;
                toStyle += editor.GetCharAt(endPos);
            }
            string[] lines = toStyle.Split('\n');
            
            //Loop over lines and process what instructions they are.
            for (int i=0; i<lines.Length; i++)
            {
                ProcessLine(editor, startPos, lines, i);
            }
        }

        /// <summary>
        /// Processes a single line for styling in Armed.
        /// </summary>
        public static void ProcessLine(Scintilla editor, int startPos, string[] lines, int index)
        {
            //Start styling at this line. Disregard starting spaces/tabs.
            string line = string.Join("", lines[index].SkipWhile(x => x == ' ' || x == '\t'));
            int lineStartPos = GetLineStart(lines, startPos, index) + (lines[index].Length - line.Length);
            editor.StartStyling(lineStartPos);

            //Clear indicators for this line.
            editor.IndicatorClearRange(lineStartPos, line.Length);

            //Don't style an empty line.
            if (line.Replace(" ", "").Replace("\t", "") == "") { return; }

            //Split between comment and rest of line (if there is one).
            string[] lineAndComment = line.Split(';');
            if (lineAndComment.Length > 1)
            {
                //There is a comment.
                line = lineAndComment[0];
                string comment = ";" + string.Join(";", lineAndComment.Skip(1));

                //Style comment, set back to line.
                editor.StartStyling(lineStartPos + line.Length);
                editor.SetStyling(comment.Length, StyleComment);
                editor.StartStyling(lineStartPos);
            }

            //Is it a label? (string ending with colon)
            if (Regex.IsMatch(line, "^[A-Za-z_-]+:[ \\t\\r]*$"))
            {
                editor.SetStyling(line.Length, StyleLabel);
                return;
            }

            //Not a label, so must be assembly instruction. Valid instruction?
            string instr = line.Split(' ').FirstOrDefault().Replace("\r", "").Replace("\t", "");

            string curInstr = instr;
            ArmInstruction instrInfo = GetInstruction(instr);
            bool condUsed = false, flagSetUsed = false;

            //If instruction ends with a cond code, retry without the cond code.
            if (instrInfo == null && instr.Length > 2 && Regex.IsMatch(instr.ToLower(), "^.+(eq|ne|cs|hs|cc|lo|mi|pl|vs|vc|hi|ls|ge|lt|gt|le|al)$"))
            {
                curInstr = instr.Substring(0, instr.Length - 2);
                instrInfo = GetInstruction(curInstr);
                condUsed = true;
            }

            //If *still* null and ends in S (for set condition), try again.
            if (instrInfo == null && curInstr.Length > 1 && curInstr.EndsWith("s"))
            {
                instrInfo = GetInstruction(curInstr.Substring(0, curInstr.Length - 1));
                flagSetUsed = true;
            }

            //Check if the loaded instruction actually supports cond codes/flag set (if used).
            bool validCodes = true;
            if (instrInfo != null)
            {
                validCodes = (!condUsed || instrInfo.AllowsCondCode) && (!flagSetUsed || instrInfo.AllowsFlagSet);
            }

            //Still null or invalid codes? Error out.
            if (instrInfo == null || !validCodes)
            {
                //Invalid instruction, underline red. (skip starting spaces)
                int errorStartPos = lineStartPos;
                while ((editor.GetCharAt(errorStartPos) == ' ' || editor.GetCharAt(errorStartPos) == '\t') 
                        && editor.GetCharAt(errorStartPos) != '\r' && editor.GetCharAt(errorStartPos) != '\n')
                { 
                    errorStartPos++; 
                }
                editor.SetStyling(line.Length, StyleDefault);
                editor.IndicatorFillRange(errorStartPos, line.Length - (errorStartPos - lineStartPos));
                return;
            }
            
            //Valid instruction, style name and continue.
            editor.SetStyling(instr.Length, StyleKeyword);

            //Break the rest of the string into a set of operations, loop.
            string opString = string.Join(" ", line.Split(' ').Skip(1));
            string[] opArr = opString.Replace("\t", "").Replace("\r", "").Replace(" ", "").Split(',');
            int[] opLens = opString.Split(',').Select(x => x.Length).ToArray();
            int opPos = lineStartPos + instr.Length + 1;
            int opIndex = -1;
            for (int i=0; i<opArr.Length; i++)
            {
                //Set styling start.
                editor.StartStyling(opPos);
                opPos += opLens[i] + 1;
                opIndex++;

                //Ignore blanks.
                string op = opArr[i];
                if (op == "") { continue; }

                //Is this operation included in the instruction?
                if (opIndex >= instrInfo.Operands.Count)
                {
                    //Style to end of line with error.
                    editor.IndicatorFillRange(opPos - opLens[i] - 1, opLens.Skip(i).Sum() + (opArr.Length - i - 1));
                    break;
                }

                //Operation included, attempt to scan for type.
                Operand operand = instrInfo.Operands[opIndex];
                if ((operand & Operand.Immediate) == Operand.Immediate)
                {
                    //Immediate
                    //Could be hex, bin or int. Detect via. regex!
                    if (Regex.IsMatch(op, "^\\#((0b[0|1]+)|(0x[0-9A-F]+)|((-)?[0-9]+))$"))
                    {
                        //Style to end of op with immediate colour.
                        editor.SetStyling(opLens[i], StyleImmediate);
                        continue;
                    }
                }
                if ((operand & Operand.Register) == Operand.Register)
                {
                    //Register
                    //Detect via. regex whether it's valid.
                    if (Regex.IsMatch(op, "^r(([0-9])|(1[0-6]))$"))
                    {
                        //Style to end of op with register colour.
                        editor.SetStyling(opLens[i], StyleRegister);
                        continue;
                    }
                }
                if ((operand & Operand.Label) == Operand.Label)
                {
                    //Label
                    //Is it a valid label?
                    if (Regex.IsMatch(op, "^[A-Za-z0-9_-]+$"))
                    {
                        //Style as label.
                        editor.SetStyling(opLens[i], StyleLabel);
                        continue;
                    }
                }
                if ((operand & Operand.StackPointer) == Operand.StackPointer)
                {
                    //SP
                    if (op == "SP")
                    {
                        //Style as "special".
                        editor.SetStyling(opLens[i], StyleSpecial);
                        continue;
                    }
                }
                if ((operand & Operand.Operand2) == Operand.Operand2)
                {
                    //This... isn't done yet.
                    throw new NotImplementedException();
                }
                if ((operand & Operand.EndianSpecifier) == Operand.EndianSpecifier)
                {
                    //Endianness (big/little, pg.606 ARMv7 Reference)
                    if (op == "BE" || op == "LE")
                    {
                        //Style as special.
                        editor.SetStyling(opLens[i], StyleSpecial);
                        continue;
                    }
                }
                if ((operand & Operand.DataXBarrier) == Operand.DataXBarrier)
                {
                    //DMB and DSB barrier options (pg.379 ARMv7 Reference)
                    switch (op)
                    {
                        case "SY":
                        case "ST":
                        case "ISH":
                        case "ISHST":
                        case "NSH":
                        case "NSHST":
                        case "OSH":
                        case "OSHST":
                        case "SH":
                        case "SHST":
                        case "UN":
                        case "UNST":
                            //Style as special.
                            editor.SetStyling(opLens[i], StyleSpecial);
                            continue;

                        default: break;
                    }
                }

                //Got to here, and no operand parsed?
                //If operand was optional, then move on the operand but not the op text.
                if (instrInfo.OperandOptional[opIndex])
                {
                    opIndex--;
                    continue;
                }

                //Not optional? Style with error.
                editor.IndicatorFillRange(opPos - opLens[i] - 1, opLens[i]);
            }
        }

        /// <summary>
        /// Gets the list of operands associated with a specific instruction.
        /// </summary>
        private static ArmInstruction GetInstruction(string instr)
        {
            //Return by checking the operation dictionary.
            if (!Constants.Instructions.ContainsKey(instr)) { return null; }
            return Constants.Instructions[instr];
        }

        /// <summary>
        /// Gets the position of the given line index.
        /// </summary>
        private static int GetLineStart(string[] lines, int startPos, int index)
        {
            int pos = 0;
            for (int i=0; i<index; i++)
            {
                //don't subtract here to account for \n.
                pos += lines[i].Length + 1;
            }

            return startPos + pos;
        }
    }
}
