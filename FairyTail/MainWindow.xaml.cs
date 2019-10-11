using System;
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
        bool auto_update = true;
        bool started = false;
        string file;

        public MainWindow()
        {
            file = Environment.GetCommandLineArgs().ElementAtOrDefault(1) ?? "";

            if (file == "")
            {
                MessageBox.Show("Need filename!");
                Close();
            }

            file_handle = new FileHandle(new FileInfo(file), File_Changed);
            file_was_changed.Set();
            line_collector = new LineCollector(Lines_to_keep);
            encoding = Encoding.Default;

            InitializeComponent();

            Title = $"{Path.GetFileNameWithoutExtension(file)} - FTail";
            TheLabel.Text = file;
            TheListBox.DataContext = line_collector.Get_Collection();
            Update_UI();
        }

        async Task Start_It()
        {
            if (started) throw new InvalidOperationException();
            started = true;

            file_handle.Start();

            while (true)
            {
                await file_was_changed.WaitAsync();

                if (auto_update)
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

            TheListBox.SelectedIndex = TheListBox.Items.Count - 1;
            TheListBox.ScrollIntoView(TheListBox.SelectedItem);
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

            if (e.Key == Key.F)
            {
                Toggle_Auto_Update();
            }

            if (e.Key == Key.E)
            {
                Toggle_Encoding();
            }

            if(e.Key == Key.F4)
            {
                Process.Start(file);
                Close();
            }
        }

        void Toggle_Auto_Update()
        {
            auto_update = !auto_update;

            Update_UI();
        }

        void Toggle_Encoding()
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

            if (auto_update)
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
