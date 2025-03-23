using System;

namespace SmartSync_Console
{
    class FileTree : Tree<FileData>
    {
        public readonly string rootDirectory = "";
        private readonly FileMap _fileMap = new();
        public FileMap fileMap
        {
            get
            {
                return _fileMap;
            }
        }
        public FileTree(string directory)
        {
            this.rootDirectory = directory;
            RecursiveSet(directory, root);
        }
        public void RecursiveSet(string directory, TreeNode<FileData> treeNode)
        {
            FileData fileData = new(directory);
            treeNode.data=fileData;
            _fileMap.AddPair(directory, fileData);
            if (Directory.Exists(directory))
            {
                string[] files = Directory.GetFiles(directory);
                foreach(string i in files){
                    TreeNode<FileData> newNode = new();                    
                    treeNode.GetList().Add(newNode);
                    RecursiveSet(i, newNode);
                }
                string[] direvtories = Directory.GetDirectories(directory);
                foreach(string i in direvtories)
                {
                    TreeNode<FileData> newNode = new();
                    treeNode.GetList().Add(newNode);
                    RecursiveSet(i, newNode);
                }
            }
        } 
        //public FileData FindFile(string directory)
        //{

        //}
    }

    class FileData : IComparable<FileData>
    {
        private long _fileSize;
        private DateTime _lastWriteTime;
        private DateTime _lastAccessTime;
        private FileAttributes _fileAttributes;
        private string _path="";
        private bool _hashed = false;
        private string _hashValue = "";
        private SortedList<string, FileData> _subFile = [];
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

        public string path
        {
            get
            {
                return _path;
            }
        }

        public FileData() { }
        public FileData(string path)
        {
            if (File.Exists(path))
            {
                Console.WriteLine($"registering file {path}");
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
                Console.WriteLine($"registering directory {path}");
                _kind = KIND.DIRECTORY;
                this._path = path;

                DirectoryInfo directoryInfo = new(path);
                _lastWriteTime = directoryInfo.LastWriteTime;
                _lastAccessTime = directoryInfo.LastAccessTime;
            }
        }
        public int CompareTo(FileData? fileData)
        {
            if (fileData == null) return 1;
            return path.CompareTo(fileData.path);
        }
        //public bool Contain(string name)
        //{
        //    if (_kind == KIND.FILE)
        //    {
        //        return false;
        //    }
        //    if (_kind == KIND.DIRECTORY)
        //    {
        //        return 
        //    }
        //}
    }

    class FileOperator
    {
        //public FileData FindFile(FileTree fileTree, string directory)
        //{
        //    Finder finder = new();
        //    return finder.FindNode(fileTree, directory);
        //}
        public void AddFile(FileData fileData)
        {

        }
    }

    class FileMap
    {
        private readonly SortedList<string, FileData> _nameDataPairs;
        public SortedList<string, FileData> nameDataPairs
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
