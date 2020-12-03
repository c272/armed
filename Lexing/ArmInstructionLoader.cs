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
        public static Dictionary<string, List<Operand>> Load()
        {
            //Does the file exist?
            string filename = "instructions.alx";
            if (!File.Exists(filename))
            {
                throw new Exception("Instruction lexing file does not exist, cannot load syntax highlighting instructions.");
            }

            //Load the file.
            string[] instructions = File.ReadAllLines(filename);
            var parsed = new Dictionary<string, List<Operand>>();
            for (int i=0; i<instructions.Length; i++)
            {
                //If it's a blank or comment line, ignore.
                string line = instructions[i];
                if (line.StartsWith("//") || line.Replace(" ", "") == "") { continue; }

                //Get the name of the instruction.
                string instrName = line.Split(' ').FirstOrDefault();
                if (parsed.ContainsKey(instrName))
                {
                    throw new Exception("Duplicate instruction with name '" + instrName + "' in instruction lexer file.");
                }

                //Parse operands out.
                string[] ops = line.Substring(instrName.Length).Replace(" ", "").Split(',');
                var parsedOps = new List<Operand>();
                for (int j=0; j<ops.Length; j++)
                {
                    //Ignore blank.
                    if (ops[j] == "") { continue; }

                    //Switch on operand.
                    switch (ops[j])
                    {
                        case "reg":
                            parsedOps.Add(Operand.Register);
                            break;
                        case "imm":
                            parsedOps.Add(Operand.Immediate);
                            break;
                        case "reg/imm":
                            parsedOps.Add(Operand.RegOrImmediate);
                            break;
                        case "op2":
                            parsedOps.Add(Operand.Operand2);
                            break;
                        default:
                            throw new Exception("Unrecognized operand '" + ops[j] + "' on line " + (i + 1) + " of instruction lexer file.");
                    }
                }

                //Add to dictionary.
                parsed.Add(instrName, parsedOps);
            }

            //Return the list of instructions.
            return parsed;
        }
    }
}
