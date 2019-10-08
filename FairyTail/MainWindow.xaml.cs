using PowerArgs;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FairyTail
{
    public partial class MainWindow : Window
    {
        MyArgs args;
        Encoding encoding;
        FileHandle file_handle;
        AsyncAutoResetEvent file_was_changed = new AsyncAutoResetEvent();
        TimeSpan Min_Delay_Between_Updates = TimeSpan.FromMilliseconds(200);
        LineCollector line_collector;

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

            file_handle = new FileHandle(new FileInfo(args.File), File_Changed);
            file_was_changed.Set();
            line_collector = new LineCollector(50);
            encoding = Encoding.UTF8;

            InitializeComponent();

            TheListBox.ItemsSource = line_collector.Get_Collection();
        }

        async Task Start_It()
        {
            file_handle.Start();

            while (true)
            {
                await file_was_changed.WaitAsync();

                using (var stream = new FileStream(args.File, FileMode.Open, FileAccess.Read))
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

                var index = TheListBox.Items.Count - 1;

                TheListBox.SelectedIndex = TheListBox.Items.Count - 1;
                //TheListBox.ScrollIntoView(TheListBox.SelectedItem);

                await Task.Delay(Min_Delay_Between_Updates);
            }
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            file_handle.Stop();
        }

        void File_Changed(FileInfo file)
        {
            file_was_changed.Set();
        }

        async void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();

            if (e.Key == Key.F12)
            {
                await Start_It();
            }

            if (e.Key == Key.F11)
            {
                file_was_changed.Set();
            }
        }

        async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Start_It();
        }
    }
}
