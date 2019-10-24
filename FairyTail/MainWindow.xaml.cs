using FairyTail.Properties;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace FTail
{
    public partial class MainWindow : Window
    {
        const int Lines_to_keep = 150;
        TimeSpan Min_Delay_Between_Updates = TimeSpan.FromMilliseconds(150);

        Encoding encoding;
        bool encoding_was_detected;
        FileHandle file_handle;
        AsyncAutoResetEvent file_was_changed = new AsyncAutoResetEvent();
        LineCollector line_collector;
        bool follow_file = true;
        bool started;
        string file;
        Regex interactive_highlight_pattern;

        public ReadOnlyObservableCollection<Line> Lines { get; }

        public ICommand FollowFileCommand { get; }
        public ICommand EditFileCommand { get; }
        public ICommand EditFileAndCloseCommand { get; }
        public ICommand HighlightCommand { get; }
        public ICommand ExitCommand { get; }


        public MainWindow()
        {
            Setup();
            file = Environment.GetCommandLineArgs().ElementAtOrDefault(1) ?? "";

            Sanity_Check();

            file_handle = new FileHandle(new FileInfo(file), File_Changed);
            line_collector = new LineCollector(Lines_to_keep);
            Update_Filters();
            FollowFileCommand = new DelegateCommand(Toggle_Follow_File);
            EditFileCommand = new DelegateCommand(() => Edit_File(close: false));
            EditFileAndCloseCommand = new DelegateCommand(() => Edit_File(close: true));
            HighlightCommand = new DelegateCommand(Change_Interactive_Highlight);
            ExitCommand = new DelegateCommand(Close);

            InitializeComponent();

            DataContext = this;
            Lines = line_collector.Get_Collection();

            Title = $"{Path.GetFileNameWithoutExtension(file)} - FTail";
            TheLabel.Text = file;
            TheEncodingLabel.Text = "?";
            Update_UI();

            Guess_Encoding_From_BOM();
            file_was_changed.Set();
        }

        bool Guess_Encoding_From_BOM()
        {
            var bom = new byte[4];
            using (var file = GetStream())
                file.Read(bom, 0, 4);

            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Set_Encoding(Encoding.UTF7);
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Set_Encoding(Encoding.UTF8);
            if (bom[0] == 0xff && bom[1] == 0xfe) return Set_Encoding(Encoding.Unicode); //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Set_Encoding(Encoding.BigEndianUnicode); //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Set_Encoding(Encoding.UTF32);

            return Reset_Encoding();
        }

        bool Reset_Encoding()
        {
            encoding = Encoding.ASCII;
            encoding_was_detected = false;
            Update_Encoding_Display();
            return true;
        }

        bool Set_Encoding(Encoding new_encoding)
        {
            encoding = new_encoding;
            encoding_was_detected = true;
            Update_Encoding_Display();
            return true;
        }

        void Sanity_Check()
        {
            if (file == "")
            {
                MessageBox.Show("You need to pass a file name as argument!");
                Environment.Exit(2);
            }

            var attr = File.GetAttributes(file);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                MessageBox.Show("Expected: Filename, Got: Directoryname");
                Environment.Exit(2);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowPlacement.SetPlacement(new WindowInteropHelper(this).Handle, Settings.Default.MainWindowPlacement);
        }

        void Change_Interactive_Highlight()
        {
            var w = new PromptWindow();
            w.TheLabel.Content = "Enter a highlight pattern";

            if (interactive_highlight_pattern != null)
                w.TheTextBox.Text = interactive_highlight_pattern.ToString();
            else
                w.TheTextBox.Clear();

            w.TheTextBox.Focus();
            w.TheTextBox.SelectAll();

            w.Title = "Change interactive highlight";

            w.Owner = this;

            if (w.ShowDialog() == false)
                return;

            var pattern = w.TheTextBox.Text;

            if (pattern == "")
                interactive_highlight_pattern = null;
            else
                interactive_highlight_pattern = new Regex(pattern, RegexOptions.IgnoreCase);

            Update_Filters();
        }

        void Update_Filters()
        {
            var all_filters = Config.Filters.ToList();

            if (interactive_highlight_pattern != null)
                all_filters.Insert(0, new Config.Filter
                {
                    Pattern = interactive_highlight_pattern,
                    Foreground = Config.InteractiveHighlightForeground,
                    Background = Config.InteractiveHighlightBackground
                });

            line_collector.Set_Filters(all_filters.ToArray());
        }

        void Setup()
        {
            var config_file = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "ftail.xml");

            if (File.Exists(config_file))
                Config.Load(config_file);
            else
                Config.Use_Defaults();
        }

        async Task Start_It()
        {
            if (started) throw new InvalidOperationException();
            started = true;

            file_handle.Start();

            while (true)
            {
                await file_was_changed.WaitAsync();

                if (follow_file)
                {
                    Update_From_File();
                }

                await Task.Delay(Min_Delay_Between_Updates);
            }
        }

        void Update_From_File()
        {
            using (var stream = GetStream())
            {
                long total = stream.Length;
                line_collector.File_Size_Changed(total,
                    seek_and_read: seek =>
                    {
                        stream.Seek(seek, SeekOrigin.Begin);
                        int size = (int)(total - seek);
                        var bytes = new byte[size];
                        stream.Read(bytes, 0, size);

                        Update_Encoding(bytes);

                        return encoding.GetString(bytes);
                    });
            }

            if (TheListBox.Items.Count > 0)
            {
                TheListBox.SelectedIndex = TheListBox.Items.Count - 1;
                TheListBox.ScrollIntoView(TheListBox.SelectedItem);
            }
        }

        FileStream GetStream() => new FileStream(file, FileMode.Open, FileAccess.Read,
                            FileShare.ReadWrite);

        void Update_Encoding(byte[] bytes)
        {
            if (encoding_was_detected) return;

            if (EncodingDetection.TryDetect(bytes, out var new_encoding))
            {
                encoding_was_detected = true;
                encoding = new_encoding;

                Update_Encoding_Display();

            }
        }

        void Update_Encoding_Display()
        {
            if (encoding == Encoding.GetEncoding(1252))
                TheEncodingLabel.Text = "ansi";
            else
                TheEncodingLabel.Text = encoding.BodyName;
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Stop_It();

            Settings.Default.MainWindowPlacement = WindowPlacement.GetPlacement(new WindowInteropHelper(this).Handle);
            Settings.Default.Save();
        }

        void Stop_It()
        {
            file_handle.Stop();
        }

        void File_Changed(FileInfo file)
        {
            file_was_changed.Set();
        }

        public void Edit_File(bool close)
        {
            Process.Start(file);
            if (close) Close();
        }

        public void Toggle_Follow_File()
        {
            follow_file = !follow_file;

            if (follow_file)
            {
                Update_From_File();
            }

            Update_UI();
        }

        void Update_UI()
        {
            if (follow_file)
                Background = Brushes.White;
            else
                Background = Brushes.DarkGray;
        }

        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Start_It();
        }
    }
}
