using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armed
{
    /// <summary>
    /// Represents a single ARM instruction loaded from the instruction lexer file.
    /// </summary>
    public class ArmInstruction
    {
        //Whether the instruction allows the use of cond code suffixes.
        public bool AllowsCondCode = false;

        //Whether the instruction allows the flag set suffix "S".
        public bool AllowsFlagSet = false;

        //The operands for this instruction.
        public List<Operand> Operands = new List<Operand>();

        //Whether the corresponding operand is optional or not.
        public List<bool> OperandOptional = new List<bool>();
    }

    /// <summary>
    /// Represents a single operand in an instruction.
    /// </summary>
    public enum Operand
    {
        None = 0b_0000_0000,
        Immediate = 0b_0000_0001,
        Register = 0b_0000_0010,
        Label = 0b_0000_0100,
        Operand2 = 0b_0000_1000,
        StackPointer = 0b_0001_0000,
        DataXBarrier = 0b_0010_0000, //see p379 of ARMv7 reference
        EndianSpecifier = 0b_0010_0000 //see p606 of ARMv7 reference
    }
}
