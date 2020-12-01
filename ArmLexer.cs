using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
        public static int StyleHex = 0;
        public static int StyleDefault = 0;
        public static int StyleInstruction = 0;
        public static int StyleNum = 0;

        public void Style(Scintilla editor, int startPos, int endPos)
        {
            //Set up styling state.
            var state = LexerState.UNKNOWN;
            
            //Roll back to the start of the line.
            while (editor.GetCharAt(startPos) != '\n' && startPos > 0) { startPos--; }
            int lineStartPos = startPos;
            string curLine = "";

            //Start styling.
            editor.StartStyling(startPos);
            for (int pos=startPos; pos<endPos; pos++)
            {
                //Add to current line.
                var c = (char)editor.GetCharAt(startPos);
                curLine += c;

                //Skip spaces and tabs.
                if (c == ' ' || c == '\t') { continue; }

                //If the character is an endline, just reset to unknown and start over.
                if (c == '\n')
                {
                    lineStartPos = pos + 1;
                    state = LexerState.UNKNOWN;
                    curLine = "";
                    continue;
                }

                //If at the start of the line, put into instruction name state.
                if (pos == lineStartPos)
                {
                    state = LexerState.INSTRNAME;
                }

                //Switch on state.
                switch (state)
                {
                    //STATE - Start of line instruction name.
                    case LexerState.INSTRNAME:
                        //Is it a valid instruction?
                        if (Constants.Instructions.Contains(curLine))
                        {
                            //Colour the length of the string!
                            editor.SetStyling(curLine.Length, StyleInstruction);
                            break;
                        }

                        //If character is space, break out of
                        break;
                }
            }
        }
    }

    public enum LexerState
    { 
        HEX,
        NUM,
        LABEL,
        INSTRNAME,
        UNKNOWN
    }
}
