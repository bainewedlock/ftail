using System;
using System.Diagnostics;
using System.IO;

namespace FTail
{
    class FileHandle
    {
        FileSystemWatcher watcher;
        readonly FileInfo file;
        readonly Action<FileInfo> callback;

        public FileHandle(FileInfo file, Action<FileInfo> callback)
        {
            this.file = file;
            this.callback = callback;
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
            callback(file);
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
