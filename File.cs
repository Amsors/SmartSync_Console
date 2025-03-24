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
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
            });
            _logger = factory.CreateLogger<FileTree>();
        }
        public FileTree(string directory)
        {
            this.rootDirectory = directory;
            FileData noFather = new() { Kind = FileData.KIND.NOFATHER };
            FileOperator.RecursiveInitialize(this, directory, root, noFather);
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

        private readonly FileMap _subFileMap = new();
        public FileMap SubFileMap => _subFileMap;
        public enum KIND:int
        {
            NA=0,
            FILE=1,
            DIRECTORY=2,
            NULL=-1,
            NOFATHER=-2
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
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
            });
            _logger = factory.CreateLogger<FileData>();
        }
        public FileData() { }
        public FileData(string path)
        {
            if (File.Exists(path))
            {
                Logger.LogTrace($"Initializing {path}");
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
                Logger.LogTrace($"Initializing {path}");
                _kind = KIND.DIRECTORY;
                this._path = path;

                DirectoryInfo directoryInfo = new(path);
                _lastWriteTime = directoryInfo.LastWriteTime;
                _lastAccessTime = directoryInfo.LastAccessTime;
            }
        }
        public void UpdateFileData()
        {
            if (File.Exists(_path))
            {
                Logger.LogTrace($"Updating file {_path}");

                FileInfo fileInfo = new(_path);
                _fileSize = fileInfo.Length;
                _lastWriteTime = fileInfo.LastWriteTime;
                _lastAccessTime = fileInfo.LastAccessTime;

                _fileAttributes = File.GetAttributes(_path);
            }
            if (Directory.Exists(_path))
            {
                Logger.LogTrace($"Updating directory {_path}");

                DirectoryInfo directoryInfo = new(_path);
                _lastWriteTime = directoryInfo.LastWriteTime;
                _lastAccessTime = directoryInfo.LastAccessTime;

                if(CheckFolder(this, _path) == false)
                {
                    Logger.LogWarning($"{_path} has different sub-item sum from filesystem and from filemap");
                }
            }
        }
        private static bool CheckFolder(FileData filedata, string path)
        {
            int cnt = 0;
            string[] files = Directory.GetFiles(path);
            cnt += files.Length;
            foreach(string i in files)
            {
                if (filedata.SubFileMap.NameDataPairs.ContainsKey(i))
                {
                    continue;
                }
            }
            string[] directories = Directory.GetDirectories(path);
            cnt += directories.Length;
            foreach (string i in directories)
            {
                if (filedata.SubFileMap.NameDataPairs.ContainsKey(i))
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
            if (_subFileMap.NameDataPairs.ContainsKey(directory))
            {
                Logger.LogWarning($"File {directory} already exist, unable to add");
                return;
            }
            _subFileMap.NameDataPairs.Add(directory, fileData);
        }
        public void DeleteSubFile(string directory)
        {
            if (_subFileMap.NameDataPairs.ContainsKey(directory)==false)
            {
                Logger.LogWarning($"File {directory} does not exist, unable to delete");
                return;
            }
            _subFileMap.NameDataPairs.Remove(directory);
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
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
            });
            _logger = factory.CreateLogger<FileOperator>();
        }

        public static void RecursiveInitialize(FileTree fileTree, string path, TreeNode<FileData> treeNode, FileData father)
        {
            FileData fileData = new(path);
            treeNode.Data = fileData;
            fileTree.GeneralFileMap.AddPair(path, fileData);
            if (father.Kind != FileData.KIND.NOFATHER)
            {
                father.AddSubFile(path, fileData);
            }
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                foreach (string i in files)
                {
                    TreeNode<FileData> newNode = new();
                    treeNode.GetList().Add(newNode);
                    RecursiveInitialize(fileTree, i, newNode, fileData);
                }
                string[] direvtories = Directory.GetDirectories(path);
                foreach (string i in direvtories)
                {
                    TreeNode<FileData> newNode = new();
                    treeNode.GetList().Add(newNode);
                    RecursiveInitialize(fileTree, i, newNode, fileData);
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
                Logger.LogWarning("father directory do not exist (file system)");
            }
            value = parent;
            return;
        }
        public static FileData AddFile(FileTree fileTree, string directory)
        {
            Logger.LogTrace($"FileTree add file {directory}");
            FindParent(directory, out string? parent);
            if (parent == null) { return new FileData { Kind = FileData.KIND.NULL }; }
            if (fileTree.GeneralFileMap.NameDataPairs.TryGetValue(parent, out FileData? parentValue))
            {
                FileData newFile = new(directory);
                parentValue.AddSubFile(directory, newFile);
                fileTree.GeneralFileMap.AddPair(directory, newFile);
                return newFile;
            }
            else
            {
                Logger.LogWarning("father directory do not exist (FileData instance)");
                return new FileData { Kind = FileData.KIND.NULL };
            }
        }
        public static void DeleteFile(FileTree fileTree, string directory)
        {
            Logger.LogTrace($"FileTree delete file {directory}");
            FindParent(directory, out string? parent);
            if (parent == null) { return; }
            FileData fileToDel = FindFile(fileTree, directory);
            if (fileToDel.Kind == FileData.KIND.NULL) { return; }
            if(fileTree.GeneralFileMap.NameDataPairs.TryGetValue(parent, out FileData? parentValue))
            {
                parentValue.DeleteSubFile(directory);
                fileTree.GeneralFileMap.DeletePair(directory);
            }
        }
        //public static FileData MoveFile(FileTree fileTreeSRC, FileTree fileTreeDST,
        //    string pathSRC, string pathDST)
        //{
        //    Logger.LogTrace($"FileTree moved from {pathSRC} to {pathDST}");
        //    DeleteFile(fileTreeSRC, pathSRC);
        //    return AddFile(fileTreeDST, pathDST);
        //}
        public static FileData RenameFile(FileTree fileTree, string oldPath, string newPath)
        {
            Logger.LogTrace($"FileTree rename file from {oldPath} to {newPath}");
            FileData fileToRename = FindFile(fileTree, oldPath);
            if (fileToRename.Kind == FileData.KIND.NULL) return new FileData { Kind = FileData.KIND.NULL };
            fileToRename.Rename(newPath);
            return fileToRename;
        }
        public static FileData UpdateFile(FileTree fileTree, string path)
        {
            Logger.LogTrace($"FileTree update file {path}");
            FileData fileToUpdate = FindFile(fileTree, path);
            if (fileToUpdate.Kind == FileData.KIND.NULL) return new FileData { Kind = FileData.KIND.NULL };
            fileToUpdate.UpdateFileData();
            return fileToUpdate;
        }
    }

    class FileMap
    {
        private readonly SortedList<string, FileData> _nameDataPairs;
        public SortedList<string, FileData> NameDataPairs => _nameDataPairs;
        private static readonly ILogger<FileMap> _logger;
        public static ILogger<FileMap> Logger => _logger;
        static FileMap()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
            });
            _logger = factory.CreateLogger<FileMap>();
        }
        public void AddPair(string key, FileData value)
        {
            if (_nameDataPairs.ContainsKey(key))
            {
                Logger.LogWarning($"File {key} already exist when trying to add it to FileMap");
                return;
            }
            _nameDataPairs.Add(key, value);
        }
        public void DeletePair(string key)
        {
            if (_nameDataPairs.ContainsKey(key)==false)
            {
                Logger.LogWarning($"File {key} does not exist when trying to delete it in FileMap");
                return;
            }
            _nameDataPairs.Remove(key);
        }
        public FileMap()
        {
            _nameDataPairs = [];
        }
    }
}
