using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using System.Security.Principal;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;

class Program
{
    static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    static string currentpath = Directory.GetCurrentDirectory();
    static string workingpath = currentpath;
    static bool alive = true;
    static string copyfile = "";

    static int stable_version = 0;
    static int version = 4;
    static int sub_version = 3;
    static void Main(string[] args)
    {
        bool alive = true;
        while(alive)
        {
            Console.Clear();
            try //BUG HUNTING
            {
                Console.Title = workingpath;
                WriteCurrent();
                string input = Console.ReadLine();
                HandleInput(input);
            }catch (Exception ex)
            {
                ThrowError(ex.Message);
                currentpath = workingpath;
                Console.ReadKey();
            }
            Console.Clear();
        }
    }
    static void WriteCurrent()
    {
        Console.Clear();
        if(currentpath.Length == 2)
        {
            currentpath += "\\";
            if(Directory.Exists(currentpath))
            {
                workingpath = currentpath;
            }
            else
            {
                currentpath = workingpath;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("[!GHOST DIRECTORY!]");
                Console.WriteLine("RESTART PROGRAM");
                Console.ReadKey();
            }
        }
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
        Console.WriteLine();
        if(filesInDirectory.Length == 0 && foldersInDirectory.Length == 0)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("[!DIRECTORY EMPTY OR CORRUPTED!]");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.Write("[FOLDERS]  ");
            Console.ResetColor();
            Console.WriteLine($" {foldersInDirectory.Length}");
            foreach (string folder in foldersInDirectory)
            {
                string folderName = new DirectoryInfo(folder).Name;
                Console.WriteLine($"{folderName}\\..");
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.Write("[FILES]    ");
            Console.ResetColor();
            Console.WriteLine($" {filesInDirectory.Length}");
            foreach (string file in filesInDirectory)
            {
                string filename = Path.GetFileName(file);
                Console.WriteLine($"{filename}");
            }
            Console.WriteLine();
            string firstPart = Path.GetPathRoot(currentpath);
            string lastPart = Path.GetFileName(currentpath);
            string newFilePath = Path.Combine(firstPart, "...", lastPart);
            Console.Write(newFilePath + "\\>");
        }
    }
    static void HandleInput(string input)
    {
        string[] inputs = input.Split(' ');
        string[] filesInDirectory = Directory.GetFiles(currentpath);
        switch (inputs[0])
        {
            case "admin":
                if (!IsElevated)
                {
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
                }
                else
                {
                    ThrowError("Admin permissions already granted!");
                    Console.ReadKey();
                }
                break;
            case "cd":
                if (inputs[1] == "..")
                {
                    if(currentpath.Length > 4)
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
                        ThrowError("Invalid path!");
                        Console.WriteLine("Press any key to continue");
                        Console.ReadKey();
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
            case "copy":
                if (inputs.Length == 2)
                {
                    if(File.Exists(Path.Combine(currentpath, inputs[1])))
                    {
                        copyfile = Path.Combine(currentpath, inputs[1]);
                        Console.WriteLine("Copied!");
                        Console.ReadKey();
                    }
                    else
                    {
                        ThrowError("File does not exist!");
                        Console.ReadKey();
                    }
                }
                else
                {
                    ThrowError("Specify file!");
                    Console.ReadKey();
                }
                break;
            case "decrypt":
                if(inputs.Length == 2)
                {
                    Console.Clear();

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{filesInDirectory.Length} files will be decrypted!");
                    Console.ResetColor();
                    string entertext2 = "Enter password to confirm: ";
                    string password2 = CheckPassword(entertext2);
                    string hash2 = getHashSha256(password2);
                    password2 = null;
                    GC.Collect();
                    if (hash2 == "49f74582bd0a7b97b806e65bd529fbcedaff4b3bdf01b1fd31c63769c8a050d0")
                    {
                        if (inputs[1] == "current")
                        {
                            for (int i = 10; i > -1; i--)
                            {
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Password correct!");
                                Console.ResetColor();
                                Console.WriteLine($"Decrypting {filesInDirectory.Length} files in {i} seconds!");
                                Thread.Sleep(1000);
                            }
                            Console.Clear();
                            DecryptFolder(filesInDirectory);
                            Console.WriteLine($"All files were decrypted.");
                            Console.WriteLine("Press any key to continue");
                            Console.ReadKey();
                        }
                        else if (inputs[1] == "all")
                        {
                            filesInDirectory = Directory.GetFiles(currentpath, "*", SearchOption.AllDirectories);
                            for (int i = 10; i > -1; i--)
                            {
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Password correct!");
                                Console.ResetColor();
                                Console.WriteLine($"Decrypting all files in {i} seconds!");
                                Thread.Sleep(1000);
                            }
                            Console.Clear();
                            DecryptFolder(filesInDirectory);
                            Console.WriteLine($"{filesInDirectory.Length} were decrypted.");
                            Console.WriteLine("Press any key to continue");
                            Console.ReadKey();
                        }
                        else
                        {
                            Console.Clear();
                            ThrowError("Unknown depth!");
                            Console.ReadKey();
                        }
                        
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ThrowError("Password incorrect!");
                        Console.ReadKey();
                    }
                }
                else
                {
                    ThrowError("Specify the depth!");
                    Console.ReadKey();
                }
                break;
            case "delete":
                if(inputs.Length == 2)
                {
                    string target_path = Path.Combine(currentpath, inputs[1]);
                    if (File.Exists(target_path))
                    {
                        Console.Clear();
                        Console.WriteLine($"Are you sure you want to delete {inputs[1]}?");
                        Console.WriteLine("Press \"y\" to continue");
                        char key = Console.ReadKey().KeyChar;
                        if(key == 'y')
                        {
                            File.Delete(target_path);
                            Console.Clear();
                            Console.WriteLine($"Deleted {target_path}!");
                            Console.ReadKey();
                        }
                        else
                        {
                            Console.Clear();
                        }
                        File.Delete(target_path);
                    }
                    else
                    {
                        ThrowError("File does not exist!");
                        Console.ReadKey();
                    }
                }
                else
                {
                    ThrowError("Specify the file to delete!");
                    Console.ReadKey();
                }
                break;
            case "encrypt"://DEBUG
                if (inputs.Length == 2)
                {
                    Console.Clear();

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{filesInDirectory.Length} files will be encrypted!");
                    Console.ResetColor();
                    string entertext2 = "Enter password to confirm: ";
                    string password2 = CheckPassword(entertext2);
                    string hash2 = getHashSha256(password2);
                    password2 = null;
                    GC.Collect();
                    if (hash2 == "49f74582bd0a7b97b806e65bd529fbcedaff4b3bdf01b1fd31c63769c8a050d0")
                    {
                        if (inputs[1] == "current")
                        {
                            for (int i = 10; i > -1; i--)
                            {
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Password correct!");
                                Console.ResetColor();
                                Console.WriteLine($"Encrypting {filesInDirectory.Length} files in {i} seconds!");
                                Thread.Sleep(1000);
                            }
                            Console.Clear();
                            EncryptFolder(filesInDirectory);
                            Console.WriteLine($"{filesInDirectory.Length} were encrypted.");
                            Console.WriteLine("Press any key to continue");
                            Console.ReadKey();
                        }
                        else if (inputs[1] == "all")
                        {
                            filesInDirectory = Directory.GetFiles(currentpath, "*", SearchOption.AllDirectories);
                            for (int i = 10; i > -1; i--)
                            {
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Password correct!");
                                Console.ResetColor();
                                Console.WriteLine($"Encrypting all files in {i} seconds!");
                                Thread.Sleep(1000);
                            }
                            Console.Clear();
                            EncryptFolder(filesInDirectory);
                            Console.WriteLine($"All were encrypted.");
                            Console.WriteLine("Press any key to continue");
                            Console.ReadKey();
                        }
                        else
                        {
                            Console.Clear();
                            ThrowError("Unknown depth!");
                            Console.ReadKey();
                        }

                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ThrowError("Password incorrect!");
                        Console.ReadKey();
                    }
                }
                else
                {
                    ThrowError("Specify the depth!");
                    Console.ReadKey();
                }
                break;
            case "execute":
                Console.Clear();
                if (inputs.Length > 1)
                {
                    try
                    {
                        Process.Start($"{inputs[1]}");
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        Console.Clear();
                        ThrowError("Change working directory to one that includes a working browser!");
                        Console.ReadKey();
                    }
                    catch (Exception ex)
                    {
                        ThrowError(ex.Message);
                        Console.ReadKey();
                    }
                }
                else
                {
                    ThrowError("Specify the executable!");
                    Console.ReadKey();
                }

                break;
            case "help":
                Console.Clear();
                Console.WriteLine("DirectoryReader by Joelbu");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write("[VERSION]  ");
                Console.ResetColor();
                Console.WriteLine($"  {stable_version}.{version}.{sub_version}");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine("[COMMANDS] ");
                Console.WriteLine();
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor= ConsoleColor.White;
                Console.Write("admin");
                Console.ResetColor();
                Console.WriteLine("           - restart with admin permissions");
                Console.WriteLine("cd <directory>  - navigate between subdirectories");
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Write("decrypt <current/all>");
                Console.ResetColor();
                Console.WriteLine(" - decrypt  all files in the current directory/all subdirectories");
                Console.WriteLine("delete <file>   - delete a specific file");
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Write("encrypt <current/all>");
                Console.ResetColor();
                Console.WriteLine(" - encrypt <current/all> all files in the current directory/all subdirectories");
                Console.WriteLine("encrypt <current/all>");
                Console.WriteLine("execute <path>  - execute a file/html path");
                Console.WriteLine("help            - general info and a list with all commands");
                Console.WriteLine("volume <volume> - switch between volumes");
                ////////////////////
                Console.WriteLine();
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Write("Red");
                Console.ResetColor();
                Console.WriteLine(" commands need a password and/or admin permissions to be executed!");
                Console.Write("Press any key to exit");
                Console.ReadKey();
                break;
            case "paste":
                if (File.Exists(copyfile))
                {
                    try
                    {
                        string filename = new FileInfo(copyfile).Name;
                        File.Copy(copyfile, Path.Combine(currentpath, filename));
                        Console.WriteLine("File copied!");
                    }
                    catch(Exception ex)
                    {
                        ThrowError($"{ex.Message}!");
                        Console.ReadKey();
                    }
                }
                else
                {
                    ThrowError("File does not exist anymore!");
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
                Console.ReadKey();
                break;
        }
        Console.Clear();
    }
    static void ThrowError(string error)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {error}");
        Console.ResetColor();
    }
    static string CheckPassword(string EnterText)
    {
        string EnteredVal = "";
        try
        {
            Console.Write(EnterText);
            EnteredVal = "";
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    EnteredVal += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && EnteredVal.Length > 0)
                    {
                        EnteredVal = EnteredVal.Substring(0, (EnteredVal.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        if (string.IsNullOrWhiteSpace(EnteredVal))
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Empty value not allowed.");
                            CheckPassword(EnterText);
                            break;
                        }
                        else
                        {
                            Console.WriteLine("");
                            break;
                        }
                    }
                }
            } while (true);
        }
        catch (Exception ex)
        {
            throw ex;
        }
        return EnteredVal;
    }
    public static string getHashSha256(string text)
    {
        string salt = "jimmy";
        text = salt + text;
        byte[] bytes = Encoding.Unicode.GetBytes(text);
        SHA256Managed hashstring = new SHA256Managed();
        byte[] hash = hashstring.ComputeHash(bytes);
        string hashString = string.Empty;
        foreach (byte x in hash)
        {
            hashString += String.Format("{0:x2}", x);
        }
        return hashString;
    }
    static void EncryptFolder(string[] files)
    {
        for (int i = 0; i < files.Length; i++)
        {
            if (File.Exists(files[i]))
            {
                byte[] content = File.ReadAllBytes(files[i]);
                for (int j = 0; j < content.Length; j++)
                {
                    content[j] += 20;
                }
                File.WriteAllBytes(files[i], content);
                string newpath = Path.ChangeExtension(files[i], null);
                newpath = files[i] + ".encrypted";
                File.Move(files[i], newpath);
            }
        }
    }
    static void DecryptFolder(string[] files)
    {
        for (int i = 0; i < files.Length; i++)
        {
            if (File.Exists(files[i]))
            {
                byte[] content = File.ReadAllBytes(files[i]);
                for (int j = 0; j < content.Length; j++)
                {
                    content[j] -= 20;
                }
                File.WriteAllBytes(files[i], content);
                string newpath = Path.ChangeExtension(files[i], null);
                File.Move(files[i], newpath);
            }
        }
    }

}