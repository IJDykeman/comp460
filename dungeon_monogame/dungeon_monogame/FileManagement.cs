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
        static string usersLastPath = "C:\\";
        public static string getDirectoryFromDialogue()
        {
            var fbd = new FolderBrowserDialog();
            fbd.SelectedPath = usersLastPath;


            using (fbd)
            {
                DialogResult result = fbd.ShowDialog();
                usersLastPath = fbd.SelectedPath;
                return fbd.SelectedPath;
            }
        }

        public static bool AskWhetherWorldIsFlat()
        {
            Form prompt = new Form();
            prompt.Width = 500;
            prompt.Height = 500;
            prompt.Text = "Select world style";
            string longExplanation = "Select whether you want your world to extend infinitely in all directions, including up and down, or whether you want a world that is one tile high, meaning that it is a flat plane. "+
                                     "One tile high worlds are great for things like landscapes and cityscapes where you may not be interested in what goes under the surface of the ground.  " +
                                     "Infinite in all directoins worlds let you create things like dungeons with infinite levels extending down.";
            Label textLabel = new Label() { Left = 50, Top = 20, Height = 300, Width = 400, Text = longExplanation };
            System.Windows.Forms.Button OneTileHigh = new System.Windows.Forms.Button() { Text = "One tile high world", Left = 50, Width = 200, Top = 200 };
            System.Windows.Forms.Button Infinite = new System.Windows.Forms.Button() { Text = "Inifinite in all directions world", Left = 50, Width = 200, Top = 250 };
            bool result = false;
            OneTileHigh.Click += (sender, e) => { prompt.Close(); result = true; };
            Infinite.Click += (sender, e) => { prompt.Close(); result = false; };
            prompt.Controls.Add(OneTileHigh); 
            prompt.Controls.Add(Infinite);
            prompt.Controls.Add(textLabel);
            prompt.TopMost = true;
            prompt.Focus();
            prompt.BringToFront();
            prompt.ShowDialog();
            return result;
        }

        public static string OpenFileDialog()
        {
            using (var fbd = new OpenFileDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (fbd.CheckPathExists)
                {
                    return fbd.FileName;
                }
                throw new Exception("The file you opened does not exist.");
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

            Label textLabel = new Label() { Left = 50, Top = 20, Height = 200, Width = 400, Text = text};
            NumericUpDown inputBox = new NumericUpDown() { Left = 50, Top = 300, Width = 400, Minimum=1, Maximum=20 };
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
