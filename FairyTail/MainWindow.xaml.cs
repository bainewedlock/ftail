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

namespace FairyTail
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

            if (file == "")
            {
                MessageBox.Show("Need filename!");
                Environment.Exit(2);
            }

            file_handle = new FileHandle(new FileInfo(file), File_Changed);
            line_collector = new LineCollector(Lines_to_keep);
            Update_Filters();
            encoding = Encoding.UTF8; // use UTF8 until something else is detected
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

            file_was_changed.Set();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowPlacement.SetPlacement(new WindowInteropHelper(this).Handle, Properties.Settings.Default.MainWindowPlacement);
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
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite))
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

        void Update_Encoding(byte[] bytes)
        {
            if (encoding_was_detected) return;

            if(EncodingDetection.TryDetect(bytes, out var new_encoding))
            {
                encoding_was_detected = true;
                encoding = new_encoding;

                if (encoding == Encoding.GetEncoding(1252))
                    TheEncodingLabel.Text = "ansi";
                else
                    TheEncodingLabel.Text = encoding.BodyName;

            }
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Stop_It();

            Properties.Settings.Default.MainWindowPlacement = WindowPlacement.GetPlacement(new WindowInteropHelper(this).Handle);
            Properties.Settings.Default.Save();
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
            if(close) Close();
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
