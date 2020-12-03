using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armed
{
    /// <summary>
    /// Loads instructions from the editor's "instructions.alx" into memory.
    /// </summary>
    static class ArmInstructionLoader
    {
        /// <summary>
        /// Loads "instructions.alx" from the program's working directory into memory and
        /// returns it.
        /// </summary>
        public static Dictionary<string, ArmInstruction> Load()
        {
            //Does the file exist?
            string filename = "instructions.alx";
            if (!File.Exists(filename))
            {
                throw new Exception("Instruction lexing file does not exist, cannot load syntax highlighting instructions.");
            }

            //Load the file.
            string[] instructions = File.ReadAllLines(filename);
            var parsed = new Dictionary<string, ArmInstruction>();
            for (int i=0; i<instructions.Length; i++)
            {
                //If it's a blank or comment line, ignore.
                string line = instructions[i];
                if (line.StartsWith("//") || line.Replace(" ", "") == "") { continue; }

                //Get the name of the instruction.
                string instrName = line.Replace("\t", " ").Split(' ').FirstOrDefault();
                int instrNameLen = instrName.Length;

                //Check whether it supports cond codes or not (suffix [cond] and [s]).
                bool allowsCond = false, allowsFlagSet = false;
                if (instrName.EndsWith("[c]"))
                {
                    allowsCond = true;
                    instrName = instrName.Substring(0, instrName.Length - "[c]".Length);
                }
                if (instrName.EndsWith("[s]"))
                {
                    allowsFlagSet = true;
                    instrName = instrName.Substring(0, instrName.Length - "[s]".Length);
                }

                //Check the instruction isn't a duplicate.
                if (parsed.ContainsKey(instrName))
                {
                    throw new Exception("Duplicate instruction with name '" + instrName + "' in instruction lexer file.");
                }

                //Parse operands out.
                string[] ops = line.Substring(instrNameLen).Replace(" ", "").Split(',');
                var parsedOps = new List<Operand>();
                var optionalOps = new List<bool>();
                for (int j=0; j<ops.Length; j++)
                {
                    //Ignore blank.
                    if (ops[j] == "") { continue; }
                    Operand thisOp = Operand.None;

                    //Optional property?
                    bool optional = ops[j].EndsWith("?");
                    if (optional)
                    {
                        ops[j] = ops[j].Substring(0, ops[j].Length - 1);
                    }

                    //Loop over possibilities.
                    string[] subOps = ops[j].Split('/');
                    for (int k = 0; k < subOps.Length; k++) 
                    {
                        //Switch on operand.
                        switch (subOps[k])
                        {
                            case "reg":
                                thisOp |= Operand.Register;
                                break;
                            case "imm":
                                thisOp |= Operand.Immediate;
                                break;
                            case "lbl":
                                thisOp |= Operand.Label;
                                break;
                            case "op2":
                                thisOp |= Operand.Operand2;
                                break;
                            case "sp":
                                thisOp |= Operand.StackPointer;
                                break;
                            case "dxb":
                                thisOp |= Operand.DataXBarrier;
                                break;
                            case "end":
                                thisOp |= Operand.EndianSpecifier;
                                break;
                            default:
                                throw new Exception("Unrecognized operand '" + subOps[k] + "' on line " + (i + 1) + " of instruction lexer file.");
                        }
                    }

                    //Add op.
                    parsedOps.Add(thisOp);
                    optionalOps.Add(optional);
                }

                //Add to dictionary.
                parsed.Add(instrName, new ArmInstruction()
                {
                    Operands = parsedOps,
                    OperandOptional = optionalOps,
                    AllowsFlagSet = allowsFlagSet,
                    AllowsCondCode = allowsCond
                });
            }

            //Return the list of instructions.
            return parsed;
        }
    }
}
