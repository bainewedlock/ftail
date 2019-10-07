using PowerArgs;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FairyTail
{
    public class MyArgs
    {
        [ArgRequired]
        [ArgExistingFile]
        public string File { get; set; }
    }

    public partial class MainWindow : Window
    {
        MyArgs args;

        public MainWindow()
        {
            try
            {
                args = Args.Parse<MyArgs>(Environment.GetCommandLineArgs().Skip(1).ToArray());
            }
            catch (ArgException ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }

            InitializeComponent();
            Open_File();
        }

        void Open_File()
        {
            TheLabel.Content = args.File;
        }

        void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
