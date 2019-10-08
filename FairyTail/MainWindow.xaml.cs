using PowerArgs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

    public class Line
    {
        public string Text { get; set; }
    }

    public partial class MainWindow : Window, IDisposable
    {
        MyArgs args;
        Encoding encoding;
        LineCollector line_parser;
        long last_byte_position;
        int max_bytes_to_read = 100 * 1024;
        FileHandle file_handle;
        AutoResetEvent file_was_changed = new AutoResetEvent(false);
        Task bg;
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

            InitializeComponent();
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
            if (e.Key == Key.F12) Start_Thread();
            if (e.Key == Key.F5) file_handle.Start();
            if (e.Key == Key.F6) file_handle.Stop();
        }

        void Start_Thread()
        {
            if (bg != null) throw new InvalidOperationException();

            bg = Task.Run((Action)Task_Loop);

            //using (var fs = File.Open(args.File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //{
            //    var missing_bytes = fs.Length - last_byte_position;
            //    if (missing_bytes < 0) throw new ApplicationException("File shrunk!?");

            //    var size = (int)Math.Min(missing_bytes, max_bytes_to_read);

            //    fs.Seek(-size, SeekOrigin.End);
            //    var bytes = new byte[size];
            //    fs.Read(bytes, 0, size);

            //    line_parser.Append_Text(read_encoding.GetString(bytes));

            //    //TheListBox.Text = String.Join(Environment.NewLine, line_parser.Get_Lines(20));
            //    //Lines.Add(new Line { Text = $@"{DateTime.Now:HH\:mm\:ss}" });
            //}
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
                    Debug.WriteLine($"new length: {total}");
                }

                await Task.Delay(Min_Delay_Between_Updates);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MainWindow() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
