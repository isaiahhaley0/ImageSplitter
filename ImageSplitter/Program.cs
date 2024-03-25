

using ImageSplitter.models;
using NotificationService.Models;
using NotificationService.Services;
using System.Diagnostics;

string os = Environment.OSVersion.ToString();
Console.WriteLine(os);
//will monitor this folder and all subdirectories
string toMonitor = @"/mnt/share/ToSplit";
CrossPlatformPath baseMonitor = new CrossPlatformPath()
{
    UnixPath = toMonitor,
    WindowsPath = @"Z:\ToSplit"
};
//will move movies here when done splitting
string donePath = @"/mnt/share/done/";
CrossPlatformPath doneFolder = new CrossPlatformPath()
{
    UnixPath = donePath,
    WindowsPath = @"Z:\done\"
};



JMessage message = new JMessage()
{
    HasAttachment = false,
    Message = "Starting Image Splitter",
    Recipient = "Isaiah Haley",
    Subject = "Image Splitter",
    SendingType = "Email"
};

MongoService.InsertMessage(message);



if(os.Contains("Windows"))
{
    Console.WriteLine("Cannot split from windows, but will enumerate directories and files to split");
    DirectoryInfo di = new DirectoryInfo(baseMonitor.WindowsPath);
    var directory = di.GetDirectories();
    foreach(DirectoryInfo dir in directory)
    { 
        Console.WriteLine(dir.FullName); 
        var files = dir.GetFiles();
        foreach(FileInfo file in files)
        {
            if(file.Extension==".MP4")
            {
                Console.WriteLine($"\t{file.FullName}");
                string name = file.Name;
                Console.WriteLine(name);
                string NewPath = doneFolder.WindowsPath + name;
                Console.WriteLine(NewPath);
            }
        }
    }
}
else
{
    Console.WriteLine("unix detected, splitting");
    DirectoryInfo di = new DirectoryInfo(baseMonitor.UnixPath);
    var directory = di.GetDirectories();
    foreach (DirectoryInfo dir in directory)
    {
        Console.WriteLine(dir.FullName);
        var files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Extension == ".MP4")
            {
                Console.WriteLine($"\t{file.FullName}");
                string name = file.Name;
                string withoutExt = name.Split('.')[0];
                string NewPath = doneFolder.WindowsPath + name;
                Console.WriteLine(NewPath);
                string cmd = $@"-i {file.FullName} {dir.FullName}/{withoutExt}_%04d.jpg";
                Console.WriteLine(cmd);
                ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = @"ffmpeg", Arguments = cmd, };
                Process proc = new Process() { StartInfo = startInfo, };
                proc.Start();
                proc.WaitForExit();
                File.Move(file.FullName, NewPath);
                JMessage finishMessage = new JMessage()
                {
                    HasAttachment = false,
                    Message = $"Finshed Splitting {name}",
                    Recipient = "Isaiah Haley",
                    Subject = "Finished Splitting File",
                    SendingType = "Email"
                };
                MongoService.InsertMessage(finishMessage);
            }
        }
    }
}