using PowerArgs;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FairyTail
{
    public partial class MainWindow : Window
    {
        MyArgs args;
        Task bg;
        Encoding encoding;
        FileHandle file_handle;
        AutoResetEvent file_was_changed = new AutoResetEvent(false);
        TimeSpan Min_Delay_Between_Updates = TimeSpan.FromMilliseconds(1000);
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
            line_collector = new LineCollector(5);
            encoding = Encoding.UTF8;

            InitializeComponent();

            TheListBox.DataContext = line_collector.Get_Collection();
            Start_Thread();
            file_handle.Start();
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
            
            if(e.Key == Key.F12)
            {

            }
        }

        void Start_Thread()
        {
            if (bg != null) throw new InvalidOperationException();
            bg = Task.Run((Action)Task_Loop);
        }

        async void Task_Loop()
        {
            while (true)
            {
                file_was_changed.WaitOne();

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

                await Task.Delay(Min_Delay_Between_Updates);
            }
        }
    }
}
