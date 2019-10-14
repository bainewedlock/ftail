using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FairyTail
{
    public partial class MainWindow : Window
    {
        const int Lines_to_keep = 150;
        TimeSpan Min_Delay_Between_Updates = TimeSpan.FromMilliseconds(150);

        Encoding encoding;
        FileHandle file_handle;
        AsyncAutoResetEvent file_was_changed = new AsyncAutoResetEvent();
        LineCollector line_collector;
        bool follow_file = true;
        bool started = false;
        string file;

        public ReadOnlyObservableCollection<Line> Lines { get; private set; }

        public ICommand FollowFileCommand { get; private set; }
        public ICommand ToggleEncodingCommand { get; private set; }
        public ICommand EditFileCommand { get; private set; }

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
            encoding = Encoding.Default;
            FollowFileCommand = new DelegateCommand(Toggle_Follow_File);
            ToggleEncodingCommand = new DelegateCommand(Toggle_Encoding);
            EditFileCommand = new DelegateCommand(Edit_File);
            InitializeComponent();

            DataContext = this;
            Lines = line_collector.Get_Collection();

            Update_From_File();

            Title = $"{Path.GetFileNameWithoutExtension(file)} - FTail";
            TheLabel.Text = file;
            Update_UI();
        }

        void Setup()
        {
            var config_file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
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
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                long total = stream.Length;
                line_collector.File_Size_Changed(total,
                    seek_and_read: seek =>
                    {
                        stream.Seek(seek, SeekOrigin.Begin);
                        int size = (int)(total - seek);
                        var bytes = new byte[size];
                        stream.Read(bytes, 0, size);
                        return encoding.GetString(bytes);
                    });
            }

            if (TheListBox.Items.Count > 0)
            {
                TheListBox.SelectedIndex = TheListBox.Items.Count - 1;
                TheListBox.ScrollIntoView(TheListBox.SelectedItem);
            }
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Stop_It();
        }

        void Stop_It()
        {
            file_handle.Stop();
        }

        void File_Changed(FileInfo file)
        {
            file_was_changed.Set();
        }

        void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        public void Edit_File()
        {
            Process.Start(file);
            Close();
        }

        public void Toggle_Follow_File()
        {
            follow_file = !follow_file;

            if(follow_file)
            {
                Update_From_File();
            }

            Update_UI();
        }

        public void Toggle_Encoding()
        {
            if (encoding == Encoding.Default)
                encoding = Encoding.UTF8;
            else
                encoding = Encoding.Default;

            Update_UI();
        }

        void Update_UI()
        {
            if (encoding == Encoding.Default)
                TheEncodingLabel.Text = "ansi";
            else
                TheEncodingLabel.Text = encoding.BodyName;

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
