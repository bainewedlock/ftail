using System;
using System.Diagnostics;
using System.IO;

namespace FairyTail
{
    class FileHandle
    {
        FileSystemWatcher watcher;
        readonly FileInfo file;

        public FileHandle(FileInfo file)
        {
            this.file = file;
        }


        public void Start()
        {

            if (watcher != null) throw new InvalidOperationException();
            watcher = new FileSystemWatcher(file.DirectoryName, file.Name);
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;

            Debug.WriteLine("STARTED");
        }

        void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            {
                Debug.WriteLine($"new length: {stream.Length}");
            }
        }

        public void Stop()
        {
            if (watcher == null) return;
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;

            Debug.WriteLine("STOPPED");
        }
    }
}
