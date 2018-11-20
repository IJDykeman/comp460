using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dungeon_monogame
{
    static class FileManagement
    {
        public static string getPathFromDialogue()
        {
            using (var fbd = new FolderBrowserDialog())
            {

                DialogResult result = fbd.ShowDialog();
                
                return fbd.SelectedPath;
            }
        }

        public static void ShowDialog(string text, string caption)
        {
            Form prompt = new Form();
            prompt.Width = 500;
            prompt.Height = 500;
            prompt.Text = caption;
            Label textLabel = new Label() { Left = 50, Top = 20, Height = 300, Width=400, Text = text };
            System.Windows.Forms.Button confirmation = new System.Windows.Forms.Button() { Text = "Ok", Left = 350, Width = 100, Top = 400 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.TopMost = true;
            prompt.Focus();
            prompt.BringToFront();
            prompt.ShowDialog();
        }


        public static int getIntFromDialogBox(string text, string caption)
        {
            Form prompt = new Form();
            prompt.Width = 500;
            prompt.Height = 500;
            prompt.Text = caption;

            Label textLabel = new Label() { Left = 50, Top = 20, Height = 200, Width = 400, Text = text };
            NumericUpDown inputBox = new NumericUpDown() { Left = 50, Top = 300, Width = 400 };
            System.Windows.Forms.Button confirmation = new System.Windows.Forms.Button() { Text = "Ok", Left = 350, Width = 100, Top = 400 };

            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            
            prompt.TopMost = true;
            prompt.Focus();
            prompt.BringToFront();
            prompt.ShowDialog();
            return (int)inputBox.Value;
        }
    }
}
