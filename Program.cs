namespace SmartSync_Console
{
    class MainProgram
    {
        static void Main()
        {
            string TrackDirectory = @"C:\Users\fmz2024\Desktop\TEST";
            FileTree fileTree = new(TrackDirectory);
            Tracker tracker = new(fileTree);
            tracker.TrackDirectory();
            
        }
    }
    
}