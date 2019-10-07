using PowerArgs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        Encoding read_encoding;
        LineParser line_parser;
        long last_byte_position;
        int max_bytes_to_read = 100 * 1024;

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
            read_encoding = Encoding.UTF8;

            TheLabel.Content = args.File;

            var path = Path.GetDirectoryName(args.File);
            var filter = Path.GetFileName(args.File);

            //var watcher = new FileSystemWatcher(path, filter);
            //watcher.Changed += Watcher_Changed;
            //watcher.EnableRaisingEvents = true;

            line_parser = new LineParser();
            last_byte_position = 0;
        }

        void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var stream = new FileStream(args.File, FileMode.Open, FileAccess.Read);

        }

        void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }

            if (e.Key == Key.F12)
            {
                Update();
            }
        }

        void Update()
        {
            using (var fs = File.Open(args.File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var missing_bytes = fs.Length - last_byte_position;
                if (missing_bytes < 0) throw new ApplicationException("File shrunk!?");

                var size = (int)Math.Min(missing_bytes, max_bytes_to_read);

                fs.Seek(-size, SeekOrigin.End);
                var bytes = new byte[size];
                fs.Read(bytes, 0, size);

                line_parser.Append_Text(read_encoding.GetString(bytes));
                line_parser.Remove_Except_Last(100);

                TheListBox.Text = String.Join(Environment.NewLine, line_parser.Get_Lines(20));
            }
        }
    }
}
