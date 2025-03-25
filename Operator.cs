using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using static SmartSync_Console.FileData;
#pragma warning disable CA2254

namespace SmartSync_Console
{
    class FileOperator
    {
        private static readonly ILogger<FileOperator> _logger;
        public static ILogger<FileOperator> Logger => _logger;
        static FileOperator()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
            });
            _logger = factory.CreateLogger<FileOperator>();
        }

        public static void SetFileData(FileTree fileTree, FileData fileData, string path)
        {
            string absolutePath = GetAbsolutePath(fileTree, path);
            if (File.Exists(absolutePath))
            {
                Logger.LogTrace($"Initializing {absolutePath}");
                fileData.Kind = KIND.FILE;
                fileData.Path = path;

                FileInfo fileInfo = new(absolutePath);
                fileData.FileSize = fileInfo.Length;
                fileData.LastWriteTime = fileInfo.LastWriteTime;
                fileData.LastAccessTime = fileInfo.LastAccessTime;

                fileData.FileAttributes = File.GetAttributes(absolutePath);
            }
            if (Directory.Exists(absolutePath))
            {
                Logger.LogTrace($"Initializing {absolutePath}");
                fileData.Kind = KIND.DIRECTORY;
                fileData.Path = path;

                DirectoryInfo directoryInfo = new(absolutePath);
                fileData.LastWriteTime = directoryInfo.LastWriteTime;
                fileData.LastAccessTime = directoryInfo.LastAccessTime;
            }
        }
        public static void RecursiveInitialize(FileTree fileTree, string path, TreeNode<FileData> treeNode, FileData father)
        {
            string absolutePath = GetAbsolutePath(fileTree, path);
            FileData fileData = new();
            FileOperator.SetFileData(fileTree, fileData, path);
            treeNode.Data = fileData;
            fileTree.GeneralFileMap.AddPair(path, fileData);
            if (father.Kind != FileData.KIND.NOFATHER)
            {
                father.AddSubFile(path, fileData);
            }
            if (Directory.Exists(absolutePath))
            {
                string[] files = Directory.GetFiles(absolutePath);
                foreach (string subFileName in files)
                {
                    TreeNode<FileData> newNode = new();
                    treeNode.GetList().Add(newNode);
                    string relevantPath = GetRelevantPath(fileTree, subFileName);
                    RecursiveInitialize(fileTree, relevantPath, newNode, fileData);
                }
                string[] directories = Directory.GetDirectories(absolutePath);
                foreach (string subDirectoryName in directories)
                {
                    TreeNode<FileData> newNode = new();
                    treeNode.GetList().Add(newNode);
                    string relevantPath = GetRelevantPath(fileTree, subDirectoryName);
                    RecursiveInitialize(fileTree, relevantPath, newNode, fileData);
                }
            }
        }
        public static FileData FindFile(FileTree fileTree, string path)
        {
            if (fileTree.GeneralFileMap.NameDataPairs.TryGetValue(path, out FileData? value))
            {
                return value;
            }
            Logger.LogWarning($"file {path} can not be found");
            return new FileData { Kind = FileData.KIND.NULL };
        }
        public static void FindParent(FileTree fileTree, string path, out string? value)
        {
            string absolutePath = GetAbsolutePath(fileTree, path);
            string? parent = Path.GetDirectoryName(absolutePath);
            if (parent == null)
            {
                Logger.LogWarning("father directory do not exist (file system)");
                value = "";
                return;
            }
            value = FileOperator.GetRelevantPath(fileTree, parent);
            return;
        }
        public static FileData AddFile(FileTree fileTree, string absolutePath)
        {
            string relevantPath = GetRelevantPath(fileTree, absolutePath);
            Logger.LogTrace($"FileTree add file {relevantPath}");
            FindParent(fileTree, relevantPath, out string? parent);
            if (parent == null) { return new FileData { Kind = FileData.KIND.NULL }; }
            if (fileTree.GeneralFileMap.NameDataPairs.
                TryGetValue(parent, out FileData? parentValue))
            {
                FileData newFile = new();
                FileOperator.SetFileData(fileTree, newFile, relevantPath);
                parentValue.AddSubFile(relevantPath, newFile);
                fileTree.GeneralFileMap.AddPair(relevantPath, newFile);
                return newFile;
            }
            else
            {
                Logger.LogWarning("Can not Add");
                return new FileData { Kind = FileData.KIND.NULL };
            }
        }
        public static void DeleteFile(FileTree fileTree, string absolutePath)
        {
            string relevantPath = GetRelevantPath(fileTree, absolutePath);
            Logger.LogTrace($"FileTree delete file {relevantPath}");
            FindParent(fileTree, relevantPath, out string? parent);
            if (parent == null) { return; }
            FileData fileToDel = FindFile(fileTree, relevantPath);
            if (fileToDel.Kind == FileData.KIND.NULL) { return; }
            if (fileTree.GeneralFileMap.NameDataPairs.TryGetValue(parent, out FileData? parentValue))
            {
                parentValue.DeleteSubFile(relevantPath);
                fileTree.GeneralFileMap.DeletePair(relevantPath);
            }
            else
            {
                Logger.LogWarning("Can not Delete");
            }
        }
        public static FileData RenameFile
            (FileTree fileTree, string oldAbsolutePath, string newAbsolutePath)
        {
            string oldRelevantPath = GetRelevantPath(fileTree, oldAbsolutePath);
            string newRelevantPath = GetRelevantPath(fileTree, newAbsolutePath);
            Logger.LogTrace($"FileTree rename file from {oldRelevantPath} to {newRelevantPath}");
            FileData fileToRename = FindFile(fileTree, oldRelevantPath);
            if (fileToRename.Kind == FileData.KIND.NULL)
            {
                return new FileData { Kind = FileData.KIND.NULL };
            }
            fileTree.GeneralFileMap.NameDataPairs.Remove(oldRelevantPath);
            fileTree.GeneralFileMap.NameDataPairs.Add(newRelevantPath, fileToRename);

            FileOperator.FindParent(fileTree, oldRelevantPath, out string? parent);
            if (parent != null)
            {
                FileOperator.FindFile(fileTree, parent).DeleteSubFile(oldRelevantPath);
                FileOperator.FindFile(fileTree, parent).AddSubFile(newRelevantPath, fileToRename);
                return fileToRename;
            }
            else
            {
                Logger.LogWarning("Can not Rename");
                return new FileData { Kind = FileData.KIND.NULL };
            }
        }
        public static FileData UpdateFile(FileTree fileTree, string absolutePath)
        {
            string relevantPath = GetRelevantPath(fileTree, absolutePath);
            Logger.LogTrace($"FileTree update file {relevantPath}");
            FileData fileToUpdate = FindFile(fileTree, relevantPath);
            if (fileToUpdate.Kind == FileData.KIND.NULL) return new FileData { Kind = FileData.KIND.NULL };
            FileOperator.UpdateFileData(fileTree, relevantPath);
            return fileToUpdate;
        }
        public static void UpdateFileData(FileTree fileTree, string path)
        {
            string absolutePath = GetAbsolutePath(fileTree, path);
            if(fileTree.GeneralFileMap.NameDataPairs.TryGetValue(path, out FileData? fileData) == false)
            {
                Logger.LogWarning($"file {path} does not exist, unable to update");
                return;
            }           
            if (File.Exists(absolutePath))
            {
                Logger.LogTrace($"Updating file {absolutePath}");

                FileInfo fileInfo = new(absolutePath);
                fileData.FileSize = fileInfo.Length;
                fileData.LastWriteTime = fileInfo.LastWriteTime;
                fileData.LastAccessTime = fileInfo.LastAccessTime;

                fileData.FileAttributes = File.GetAttributes(absolutePath);
            }
            if (Directory.Exists(absolutePath))
            {
                Logger.LogTrace($"Updating directory {absolutePath}");

                DirectoryInfo directoryInfo = new(absolutePath);
                fileData.LastWriteTime = directoryInfo.LastWriteTime;
                fileData.LastAccessTime = directoryInfo.LastAccessTime;

                if (CheckFolder(fileTree, fileData, path) == false)
                {
                    Logger.LogWarning($"{path} has different sub-item sum from filesystem and from filemap");
                }
            }
        }
        private static bool CheckFolder(FileTree fileTree, FileData filedata, string path)
        {
            int cnt = 0;
            string absolutePath = GetAbsolutePath(fileTree, path);
            string[] files = Directory.GetFiles(absolutePath);
            cnt += files.Length;
            foreach (string i in files)
            {
                string relevantPath = GetRelevantPath(fileTree, i);
                if (filedata.SubFileMap.NameDataPairs.ContainsKey(relevantPath))
                {
                    continue;
                }
            }
            string[] directories = Directory.GetDirectories(absolutePath);
            cnt += directories.Length;
            foreach (string i in directories)
            {
                string relevantPath = GetRelevantPath(fileTree, i);
                if (filedata.SubFileMap.NameDataPairs.ContainsKey(relevantPath))
                {
                    continue;
                }
            }
            if (cnt == filedata.SubFileMap.NameDataPairs.Count)
            {
                return true;
            }
            return false;
        }
        public static void UpdateHash(FileTree fileTree, string path)
        {
            string absolutePath = GetAbsolutePath(fileTree, path);
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(absolutePath);
            byte[] hashBytes = sha256.ComputeHash(stream);
            if (fileTree.GeneralFileMap.NameDataPairs.TryGetValue(path, out FileData? fileData) == false)
            {
                Logger.LogWarning($"file {path} does not exist, unable to calculate hash");
                return;
            }
            fileData.HashValue = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
        public static string GetAbsolutePath(FileTree fileTree, string relevantPath)
        {
            return fileTree.rootDirectory + @"\" + relevantPath;
        }
        public static string GetRelevantPath(FileTree fileTree, string absolutePath)
        {
            return absolutePath.Replace(fileTree.rootDirectory + @"\", "");
        }
    }
}
