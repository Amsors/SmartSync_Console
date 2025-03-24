using System.IO;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
#pragma warning disable CA2254

namespace SmartSync_Console
{
    class FileTree : Tree<TreeNode<FileData>>
    {
        public readonly string rootDirectory = "";
        private readonly FileMap _fileMap = new();
        public FileMap GeneralFileMap => _fileMap;

        private static readonly ILogger<FileTree> _logger;
        public static ILogger<FileTree> Logger => _logger;
        static FileTree()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            _logger = factory.CreateLogger<FileTree>();
        }
        public FileTree(string directory)
        {
            this.rootDirectory = directory;
            FileOperator.RecursiveInitialize(this, directory, root);
        }       
    }

    class FileData : IComparable<FileData>
    {
        private string _name = "";
        private long _fileSize;
        private DateTime _lastWriteTime;
        private DateTime _lastAccessTime;
        private FileAttributes _fileAttributes;

        private readonly string _path="";
        public string Path => _path;

        private bool _hashed = false;
        public bool Hashed => _hashed;
        private string _hashValue = "";
        public string HashValue => _hashValue;

        private readonly FileMap _fileMap = new();
        public enum KIND:int
        {
            NA=0,
            FILE=1,
            DIRECTORY=2,
            NULL=-1
        }
        private KIND _kind=KIND.NA;
        public KIND Kind
        {
            get
            {
                return _kind;
            }
            set
            {
                _kind = value;
            }
        }
        private static readonly ILogger<FileData> _logger;
        public static ILogger<FileData> Logger => _logger;
        static FileData()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            _logger = factory.CreateLogger<FileData>();
        }
        public FileData() { }
        public FileData(string path)
        {
            if (File.Exists(path))
            {
                Logger.LogInformation($"Initializing {path}");
                _kind = KIND.FILE;
                this._path = path;

                FileInfo fileInfo = new(path);
                _fileSize = fileInfo.Length;
                _lastWriteTime = fileInfo.LastWriteTime;
                _lastAccessTime = fileInfo.LastAccessTime;

                _fileAttributes = File.GetAttributes(path);
            }
            if (Directory.Exists(path))
            {
                Logger.LogInformation($"Initializing {path}");
                _kind = KIND.DIRECTORY;
                this._path = path;

                DirectoryInfo directoryInfo = new(path);
                _lastWriteTime = directoryInfo.LastWriteTime;
                _lastAccessTime = directoryInfo.LastAccessTime;
            }
        }
        public void UpdateFileData()
        {
            if (File.Exists(Path))
            {
                Logger.LogInformation($"Updating {Path}");

                FileInfo fileInfo = new(Path);
                _fileSize = fileInfo.Length;
                _lastWriteTime = fileInfo.LastWriteTime;
                _lastAccessTime = fileInfo.LastAccessTime;

                _fileAttributes = File.GetAttributes(Path);
            }
            if (Directory.Exists(Path))
            {
                Logger.LogInformation($"Updating {Path}");

                DirectoryInfo directoryInfo = new(Path);
                _lastWriteTime = directoryInfo.LastWriteTime;
                _lastAccessTime = directoryInfo.LastAccessTime;
            }
        }
        public void UpdateHash()
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(Path);
            byte[] hashBytes = sha256.ComputeHash(stream);
            _hashValue = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
        public int CompareTo(FileData? fileData)
        {
            if (fileData == null) return 1;
            return Path.CompareTo(fileData.Path);
        }
        public void AddSubFile(string directory, FileData fileData)
        {
            if (_fileMap.NameDataPairs.ContainsKey(directory))
            {
                Logger.LogWarning($"File {0} {1} , unable to {2}", directory, "already exist", "add");
                return;
            }
            _fileMap.NameDataPairs.Add(directory, fileData);
        }
        public void DeleteSubFile(string directory)
        {
            if (_fileMap.NameDataPairs.ContainsKey(directory)==false)
            {
                Logger.LogWarning($"File {0} {1} , unable to {2}", directory, "does not exist", "delete");
                return;
            }
            _fileMap.NameDataPairs.Remove(directory);
        }
        public void Rename(string name)
        {
            _name = name;
        }
        public void Change()
        {
            UpdateFileData();
        }
    }

    class FileOperator
    {
        private static readonly ILogger<FileOperator> _logger;
        public static ILogger<FileOperator> Logger => _logger;
        static FileOperator()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            _logger = factory.CreateLogger<FileOperator>();
        }

        public static void RecursiveInitialize(FileTree fileTree, string directory, TreeNode<FileData> treeNode)
        {
            FileData fileData = new(directory);
            treeNode.Data = fileData;
            fileTree.GeneralFileMap.AddPair(directory, fileData);
            if (Directory.Exists(directory))
            {
                string[] files = Directory.GetFiles(directory);
                foreach (string i in files)
                {
                    TreeNode<FileData> newNode = new();
                    treeNode.GetList().Add(newNode);
                    RecursiveInitialize(fileTree, i, newNode);
                }
                string[] direvtories = Directory.GetDirectories(directory);
                foreach (string i in direvtories)
                {
                    TreeNode<FileData> newNode = new();
                    treeNode.GetList().Add(newNode);
                    RecursiveInitialize(fileTree, i, newNode);
                }
            }
        }
        public static FileData FindFile(FileTree fileTree, string directory)
        {
            if (fileTree.GeneralFileMap.NameDataPairs.TryGetValue(directory, out FileData? value))
            {
                return value;
            }
            Logger.LogWarning($"file {directory} can not be found");
            return new FileData { Kind = FileData.KIND.NULL };
        }
        public static void FindParent(string directory, out string? value)
        {
            string? parent = Path.GetDirectoryName(directory);
            if (parent == null)
            {
                Logger.LogWarning("father directory do not exist (from file system)");
            }
            value = parent;
            return;
        }
        public static FileData AddFile(FileTree fileTree, string directory)
        {
            Logger.LogInformation($"FileTree add file {directory}");
            FindParent(directory, out string? parent);
            if (parent == null) { return new FileData { Kind = FileData.KIND.NULL }; }
            if (fileTree.GeneralFileMap.NameDataPairs.TryGetValue(parent, out FileData? parentValue))
            {
                FileData newFile = new(directory);
                parentValue.AddSubFile(directory, newFile);
                return newFile;
            }
            else
            {
                Logger.LogWarning("father directory do not exist (from FileData instance)");
                return new FileData { Kind = FileData.KIND.NULL };
            }
        }
        public static void DeleteFile(FileTree fileTree, string directory)
        {
            Logger.LogInformation($"FileTree delete file {directory}");
            FindParent(directory, out string? parent);
            if (parent == null) { return; }
            FileData fileToDel = FindFile(fileTree, directory);
            if (fileToDel.Kind == FileData.KIND.NULL) { return; }
            if(fileTree.GeneralFileMap.NameDataPairs.TryGetValue(parent, out FileData? parentValue))
            {
                parentValue.DeleteSubFile(directory);
            }
        }
        public static FileData MoveFile(FileTree fileTreeSRC, FileTree fileTreeDST,
            string pathSRC, string pathDST)
        {
            Logger.LogInformation($"FileTree moved from {pathSRC} to {pathDST}");
            DeleteFile(fileTreeSRC, pathSRC);
            return AddFile(fileTreeDST, pathDST);
        }
        public static FileData RenameFile(FileTree fileTree, string oldPath, string newPath)
        {
            Logger.LogInformation($"FileTree rename file from {oldPath} to {newPath}");
            FileData fileToRename = FindFile(fileTree, newPath);
            fileToRename.Rename(newPath);
            return fileToRename;
        }
        public static FileData UpdateFile(FileTree fileTree, string path)
        {
            Logger.LogInformation($"FileTree update file {path}");
            FileData fileToUpdate = FindFile(fileTree, path);
            fileToUpdate.UpdateFileData();
            return fileToUpdate;
        }
    }

    class FileMap
    {
        private readonly SortedList<string, FileData> _nameDataPairs;
        public SortedList<string, FileData> NameDataPairs
        {
            get
            {
                return _nameDataPairs;
            }
        }
        public void AddPair(string key, FileData value)
        {
            _nameDataPairs.Add(key, value);
        }
        public FileMap()
        {
            _nameDataPairs = [];
        }
    }
}
