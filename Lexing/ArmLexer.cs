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
        public static int StyleHex = 1;
        public static int StyleKeyword = 2;
        public static int StyleLabel = 3;
        public static int StyleNum = 4;
        public static int StyleComment = 5;

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
            //Start styling at this line.
            string line = lines[index];
            int lineStartPos = GetLineStart(lines, startPos, index);
            editor.StartStyling(lineStartPos);

            //Clear indicators for this line.
            editor.IndicatorClearRange(lineStartPos, line.Length);

            //Don't style an empty line.
            if (line.Replace(" ", "").Replace("\t", "").Replace("\r", "") == "") { return; }

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
            string instr = line.Split(' ').FirstOrDefault();
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
                validCodes = (condUsed && !instrInfo.AllowsCondCode) || (flagSetUsed && !instrInfo.AllowsFlagSet);
            }

            //Still null or invalid codes? Error out.
            if (instrInfo == null || validCodes)
            {
                //Invalid instruction, underline red. (skip starting spaces)
                int errorStartPos = lineStartPos;
                while (editor.GetCharAt(errorStartPos) == ' ' || editor.GetCharAt(errorStartPos) == '\t') { errorStartPos++; }
                editor.SetStyling(line.Length, StyleDefault);
                editor.IndicatorFillRange(errorStartPos, line.Length - (errorStartPos - lineStartPos));
                return;
            }
            
            //Valid instruction, style name and continue.
            editor.SetStyling(instr.Length, StyleKeyword);

            string opString = string.Join(" ", line.Split(' ').Skip(1));
            string[] opArr = opString.Replace("\t", "").Replace(" ", "").Split(',');
            for (int i=0; i<opArr.Length; i++)
            {

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
