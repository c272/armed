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
        //List of currently open editors.
        private List<Scintilla> editors = new List<Scintilla>();

        public Editor()
        {
            InitializeComponent();

            //Load the instruction lexer file.
            Constants.Instructions = ArmInstructionLoader.Load();

            //Set up instruction auto complete.
            Constants.InstructionAutoCString = string.Join(" ", Constants.Instructions.Keys.ToList());

            //Set up the tabs, initialize Scintilla instance.
            tabs.TabPages.Clear();
            CreateNewTab();
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

            //Set up editor syntax highlighting.
            editor.StyleClearAll();
            editor.Styles[ArmLexer.StyleDefault].ForeColor = Color.Black;
            editor.Styles[ArmLexer.StyleKeyword].ForeColor = Color.Blue;
            editor.Styles[ArmLexer.StyleImmediate].ForeColor = Color.LightSeaGreen;
            editor.Styles[ArmLexer.StyleComment].ForeColor = Color.Gray;
            editor.Styles[ArmLexer.StyleLabel].ForeColor = Color.Gray;
            editor.Styles[ArmLexer.StyleRegister].ForeColor = Color.LightSkyBlue;
            editor.Styles[ArmLexer.StyleSpecial].ForeColor = Color.Magenta;

            //Set up indicators (error highlights, etc.)
            editor.Indicators[ArmLexer.IndicatorError].Style = IndicatorStyle.Squiggle;
            editor.Indicators[ArmLexer.IndicatorError].ForeColor = Color.Red;

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
            ArmLexer.Style(editor, editor.GetEndStyled(), e.Position);
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
            if (editor.GetCharAt(i) != '\n' && i != 0)
            {
                return;
            }

            //Yes! Show the autocomplete.
            int lenEntered = currentPos - wordStartPos;
            if (lenEntered > 0 && !editor.AutoCActive)
            {
                editor.AutoCShow(lenEntered, Constants.InstructionAutoCString);
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

        /////////////////////
        /// WINDOW EVENTS ///
        /////////////////////
        
        /// <summary>
        /// Processes non-standard shortcut keys for the editor.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var editor = GetActiveEditor();
            if (editor == null) { return base.ProcessCmdKey(ref msg, keyData); }

            //CTRL and +
            if (keyData == (Keys.Control | Keys.Oemplus))
            {
                //Zoom active editor (if not null).
                editor.ZoomIn();
                return true;
            }
            //CTRL and -
            else if (keyData == (Keys.Control | Keys.OemMinus))
            {
                //Zoom out.
                editor.ZoomOut();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
