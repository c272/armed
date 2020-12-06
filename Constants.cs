using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armed
{
    /// <summary>
    /// Contains all non-mutable values within the Armed editor.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The name of a tab when the new file button is pressed.
        /// </summary>
        public static string NewFileName { get; set; } = "Untitled";

        /// <summary>
        /// The amount of right-padding given to the line number margin.
        /// </summary>
        public static int MarginPadding { get; set; } = 2;

        /// <summary>
        /// All base ARM instruction mnemonics available in ARM.
        /// </summary>
        public static Dictionary<string, ArmInstruction> Instructions = null;

        /// <summary>
        /// The autocomplete string for all loaded ARM instructions.
        /// </summary>
        public static string InstructionAutoCString = "";
    }
}
