namespace SmartSync_Console
{
    class Tracker
    {
        private readonly string directory = "";
        private readonly FileTree trackedFileTree;
        public Tracker(FileTree fileTree)
        {
            directory = fileTree.rootDirectory;
            trackedFileTree = fileTree;
        }
        public void TrackDirectory()
        {
            FileSystemWatcher watcher = new()
            {
                Path = directory,
                Filter = "*.*",
                NotifyFilter = NotifyFilters.LastWrite
                    | NotifyFilters.FileName
                    | NotifyFilters.DirectoryName
            };

            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Changed += OnChanged;
            watcher.Renamed += OnRenamed;

            watcher.EnableRaisingEvents = true;

            Console.ReadKey();
        }
        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"文件创建：{e.FullPath}");
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"文件删除：{e.FullPath}");
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"文件更改：{e.FullPath}");
        }
        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            Console.WriteLine($"文件重命名：从 {e.OldFullPath} 到 {e.FullPath}");
        }
    }
}
