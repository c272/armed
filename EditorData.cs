using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace armed
{
    /// <summary>
    /// Backing data for all of the Scintilla instances opened in the editor.
    /// </summary>
    public class EditorData
    {
        //Whether the editor has been changed since last save.
        public bool EditedSinceSaved = false;

        //The file this editor instance is writing to.
        public string File = null;
    }
}
