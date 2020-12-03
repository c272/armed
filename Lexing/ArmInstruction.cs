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
    }
}
