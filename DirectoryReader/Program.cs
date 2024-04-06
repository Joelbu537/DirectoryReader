﻿using System.Diagnostics;
using System.Security.Principal;
using static System.Net.Mime.MediaTypeNames;

class Program
{
    static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    static string currentpath = Directory.GetCurrentDirectory();
    static string workingpath = currentpath;
    static bool alive = true;
    static void Main(string[] args)
    {
        bool alive = true;
        while(alive)
        {
            try //BUG HUNTING
            {
                WriteCurrent();
                string input = Console.ReadLine();
                HandleInput(input);
            }catch (Exception ex)
            {
                ThrowError(ex.Message);
                Console.ReadKey();
            }
        }
    }
    static void WriteCurrent()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = ConsoleColor.White;
        Console.Write("[ADMIN]    ");
        Console.ResetColor();

        if (IsElevated)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" YES");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" NO");
        }
        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = ConsoleColor.White;
        Console.Write("[DIRECOTRY]");
        Console.ResetColor();
        Console.Write(" ");
        Console.WriteLine(currentpath);
        string[] filesInDirectory = Directory.GetFiles(currentpath);
        string[] foldersInDirectory = Directory.GetDirectories(currentpath);
        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = ConsoleColor.White;
        Console.Write("[FILES]    ");
        Console.ResetColor();
        Console.WriteLine($" {filesInDirectory.Length + foldersInDirectory.Length} ({filesInDirectory.Length}/{foldersInDirectory.Length})");
        Console.WriteLine();
        foreach(string folder in foldersInDirectory)
        {
            string folderName = new DirectoryInfo(folder).Name;
            Console.WriteLine($"{folderName}\\..");
        }
        foreach (string file in filesInDirectory)
        {
            string filename = Path.GetFileName(file);
            Console.WriteLine($"{filename}");
        }
        Console.WriteLine();
        string firstPart = Path.GetPathRoot(currentpath);
        string lastPart = Path.GetFileName(currentpath);
        string newFilePath = Path.Combine(firstPart,"...", lastPart);
        Console.Write(newFilePath + "\\");
    }
    static void HandleInput(string input)
    {
        string[] inputs = input.Split(' ');
        switch(inputs[0])
        {
            case "cd":
                if (inputs[1] == "..")
                {
                    currentpath = currentpath.Substring(0, currentpath.LastIndexOf('\\'));
                    if (Directory.Exists(currentpath))
                    {
                        workingpath = currentpath;
                    }
                    else
                    {
                        currentpath = workingpath;
                    }
                }
                else
                {
                    inputs[1] = inputs[1].Replace('*', ' ');
                    //Subdirectory besuchen
                    currentpath = Path.Combine(currentpath, inputs[1]);
                    if (Directory.Exists(currentpath))
                    {
                        workingpath = currentpath;
                    }
                    else
                    {
                        currentpath = workingpath;
                        ThrowError("Directory does not exist!");
                        Console.WriteLine("Press any key to continue");
                        Console.ReadKey();
                    }
                }
                break;
            case "admin":
                Console.WriteLine("This will close the current program!");
                Console.WriteLine("Press 'y' to continue");
                char key = Console.ReadKey().KeyChar;
                if (key == 'y')
                {
                    alive = false;
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Environment.CurrentDirectory,
                        FileName = "DirectoryReader.exe", // Korrigieren Sie den Dateinamen
                        Verb = "runas" // Fordert die Ausführung mit Adminrechten an
                    };
                    try
                    {
                        Process.Start(startInfo);
                        Process currentProcess = Process.GetCurrentProcess(); // Holen Sie sich die aktuelle Prozessinstanz
                        currentProcess.Kill();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message); // Gibt die Fehlermeldung aus, wenn ein Fehler auftritt
                    }
                }
                break;
            case "volume":
                if (inputs[1].Length == 1)
                {
                    
                    string rootPath = inputs[1].ToUpper() + ":\\";
                    if (Directory.Exists(rootPath))
                    {
                        currentpath = rootPath;
                        workingpath = currentpath;
                    }
                    else
                    {
                        currentpath = workingpath;
                        ThrowError("Volume does not exist!");
                        Console.WriteLine("Press any key to continue");
                        Console.ReadKey();
                    }
                }
                else
                {
                    ThrowError("Invalid volume name");
                    Console.ReadKey();
                }
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                ThrowError("Command unknown. Check if there are any spelling mistakes.");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                break;
        }
    }
    static void ThrowError(string error)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {error}");
        Console.ResetColor();
    }
}