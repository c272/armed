using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;

namespace armed
{
    public partial class Editor : Form
    {
        private List<Scintilla> editors = new List<Scintilla>();

        public Editor()
        {
            InitializeComponent();

            //Set up the tabs, initialize Scintilla instance.
            tabs.TabPages.Clear();
            CreateNewTab();

            //
        }

        /// <summary>
        /// Gets the currently active Scintilla instance.
        /// </summary>
        public Scintilla GetActiveEditor()
        {
            if (tabs.SelectedIndex == -1) { return null; }
            if (editors.Count < tabs.TabCount) { return null; }
            return editors[tabs.SelectedIndex];
        }

        /// <summary>
        /// Creates a new editor tab and sets up the editor.
        /// </summary>
        public void CreateNewTab()
        {
            //Make tab.
            tabs.TabPages.Add(Constants.NewFileName);

            //Start to configure editor.
            var editor = new Scintilla();
            editor.Dock = DockStyle.Fill;

            //Make sure only the number margin is visible.
            editor.Margins[0].Width = 16;
            for (int i=1; i<editor.Margins.Count; i++)
            {
                editor.Margins[i].Width = 0;
            }

            //Attach handlers.
            editor.TextChanged += editorTextChanged;
            editor.CharAdded += editorCharAdded;
            editor.StyleNeeded += editorStyleNeeded;
            editor.AutoCOrder = Order.PerformSort;
            editor.Lexer = Lexer.Container;
            
            //Save to editor list.
            tabs.TabPages[tabs.TabCount - 1].Controls.Add(editor);
            editors.Add(editor);
        }

        /////////////////////
        /// EDITOR EVENTS ///
        /////////////////////
        
        //Triggered when the editor needs to be styled.
        private void editorStyleNeeded(object sender, StyleNeededEventArgs e)
        {
            //Get active editor, call lexer.
            var editor = GetActiveEditor();

        }

        //Triggered when a character is added to the editor.
        private void editorCharAdded(object sender, EventArgs e)
        {
            //Get the active editor.
            var editor = GetActiveEditor();

            //Get current position, and start of word.
            int currentPos = editor.CurrentPosition;
            int wordStartPos = editor.WordStartPosition(currentPos, true);
            int i = wordStartPos - 1;
            if (i < 0) { i = 0; }

            //Is the word at the start of a line?
            while (editor.GetCharAt(i) == ' ' || editor.GetCharAt(i) == '\t') { i--; }
            Console.WriteLine((char)editor.GetCharAt(i) + " == \\n");
            if (editor.GetCharAt(i) != '\n' && i != 0)
            {
                return;
            }

            //Yes! Show the autocomplete.
            int lenEntered = currentPos - wordStartPos;
            if (lenEntered > 0 && !editor.AutoCActive)
            {
                editor.AutoCShow(lenEntered, Constants.Instructions);
            }
        }

        //Triggered when any editors have text changed on them.
        int maxCharWidth = -1;
        private void editorTextChanged(object sender, EventArgs e)
        {
            //Get the active editor.
            var editor = GetActiveEditor();

            //Is the width identical to last time?
            int lineWidth = editor.Lines.Count.ToString().Length;
            if (maxCharWidth == lineWidth) { return; }

            //No, calculate the new margin width.
            editor.Margins[0].Width = editor.TextWidth(Style.LineNumber, new string('9', lineWidth + 1)) + Constants.MarginPadding;
            maxCharWidth = lineWidth;
        }

        //Triggered when the user switches tab.
        private void tabChanged(object sender, EventArgs e)
        {
            //Reset the max char width for line number margin calculation.
            maxCharWidth = -1;
        }

        //Create a new tab.
        private void newBtn_Click(object sender, EventArgs e)
        {
            CreateNewTab();
            tabs.SelectedIndex = tabs.TabCount - 1;
        }
    }
}
