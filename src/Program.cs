using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneCoin
{
    class Program
    {
        public static bool CheckUpdates = true;
        public static bool MainMenuLoop = true;
        public static bool DebugLogging = false;
        public static string RunningVersion = "";

        static void Main(string[] Args)
        {
            ConsoleColor Back = Console.BackgroundColor;
            ConsoleColor Front = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                new Bitmap(256, 256).Dispose();
            }
            catch (Exception Ex)
            {
                if(Ex.ToString().ToLower().Contains("libgdiplus"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("You do not have \"libgdiplus\" installed on your system, or it is broken!");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("You can run app, but any image dependent operations will probably crash app.");
                    Console.WriteLine("To fix this error, please install \"libgdiplus\" on your system.");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("  Debian / Ubuntu:  sudo apt-get install libgdiplus  ");
                    Console.WriteLine("  RedHat / Fedora:  sudo yum install libgdiplus      ");
                    Console.WriteLine("  Arch / Manjaro:   sudo pacman -S libgdiplus        ");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("Close app and install missing library. Alternatively, press any key to continue...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                }
            }
            
            if(File.Exists(Settings.AppPath + "/version.txt"))
            {
                RunningVersion = File.ReadAllText(Settings.AppPath + "/version.txt");
                
                if(RunningVersion.Length > 9 || RunningVersion.Split('.').Length != 3)
                {
                    RunningVersion = "1.0.0";
                }
            }

            for (int i = 0; i < Args.Length; i++)
            {
                string[] Arguments = Args[i].Split(":");
                if (Arguments[0][0] == '-')
                {
                    switch (Arguments[0].Replace("-", ""))
                    {
                        case "help": case "cmd":
                        {
                            MainMenuLoop = false;
                            Console.WriteLine("  All possible arguments:");
                            Console.WriteLine("-help, -cmd");
                            Console.WriteLine("-v, -version");
                            Console.WriteLine("-debug");
                            Console.WriteLine("-skipupdate");
                            Console.WriteLine("-forceupdate");
                            Console.WriteLine("-syncspeed:[speed]");
                            Console.WriteLine("-mining:[address]:(threads):(pooladdress):(poolport)");
                            Console.WriteLine("-database:[host]:[user]:[pass]:[db]");
                            Console.WriteLine("-storeusers:(avatarsdir):(qrcodesdir)");
                            Console.WriteLine("-storeblocks:(blocksdir)");
                            Console.WriteLine("-generatesignature:[key]:[message]");
                            Console.WriteLine("-verifysignature:[address]:[signature]:[message]");
                            Console.WriteLine("-generatemnemonics");
                            Console.WriteLine("-getkeyfrommnemonics:[mnemonic1]:(mnemonic2):(mnemonic3)...");
                            Console.WriteLine("-getaddressfromkey:[key]");
                            Console.WriteLine("-testnode:[ipaddress]:(port)");
                            Console.WriteLine("-sendpacket:[ipaddress]:[message1]:(message2):(message3)...");
                            Console.WriteLine("-runaspool:[privatekey]:[difficulty]");
                            Console.WriteLine("-calculator:[expression]");
                            Console.WriteLine("  Info:");
                            Console.WriteLine("Words in [] are required parameters.");
                            Console.WriteLine("Words in () are optional parameters.");
                            break;
                        }
                        case "v": case "version":
                        {
                            MainMenuLoop = false;
                            Console.WriteLine(RunningVersion);
                            break;
                        }
                        case "debug":
                        {
                            DebugLogging = true;
                            break;
                        }
                        case "skipupdate":
                        {
                            CheckUpdates = false;
                            break;
                        }
                        case "forceupdate":
                        {
                            RunningVersion = "0.1.0";
                            CheckUpdates = true;
                            break;
                        }
                        case "syncspeed":
                        {
                            if (Arguments.Length > 1)
                            {
                                string Speed = Arguments[1].ToLower();
                                if(Speed is "veryslow" or "slow" or "fast" or "veryfast" or "medium" or "normal" or "default")
                                {
                                    if(Speed == "veryslow")
                                    {
                                        Blockchain.SyncSpeed = 500;
                                    }
                                    if(Speed == "slow")
                                    {
                                        Blockchain.SyncSpeed = 250;
                                    }
                                    if(Speed == "fast")
                                    {
                                        Blockchain.SyncSpeed = 50;
                                    }
                                    if(Speed == "veryfast")
                                    {
                                        Blockchain.SyncSpeed = 25;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Unknown speed, using \"normal\". Other values are: \"veryslow\", \"slow\", \"fast\", \"veryfast\".");
                                }
                            }
                            else
                            {
                                Console.WriteLine("To few arguments, use \"OneCoin -help\", to get all possible commands with arguments...");
                            }
                            break;
                        }
                        case "mining":
                        {
                            MainMenuLoop = false;
                            if (Arguments.Length > 1)
                            {
                                Settings.CheckPaths();
                                if (CheckUpdates)
                                {
                                    CheckForUpdates();
                                }

                                if(Arguments[1].Length != 88)
                                {
                                    string[] Nickname = (Arguments[1].Replace('#', '|') + "|1").Split("|");
                                    Arguments[1] = Wallets.GetAddress(Nickname[0], byte.Parse(Nickname[1]));
                                }
                                Mining.MiningAddress = Arguments[1];
                                
                                if (Arguments.Length > 2)
                                {
                                    int TempCount = int.Parse(Arguments[2]);
                                    if(TempCount > 255) { TempCount = 255; }
                                    if(TempCount > 0)
                                    {
                                        Mining.ThreadsCount = (byte)TempCount;
                                    }
                                }
                                if (Arguments.Length > 3)
                                {
                                    Pool.PoolAddress = Arguments[3];
                                    if (Arguments.Length > 4)
                                    {
                                        Pool.PoolPort = int.Parse(Arguments[4]);
                                    }
                                    Task.Run(() => Pool.ConnectionLoop());
                                }
                                
                                Console.WriteLine("Starting node, please wait...");
                                Task.Run(() => Network.ListenForConnections());
                                Task.Run(() => Network.ListenForPackets());
                    
                                Console.WriteLine("Searching for verified nodes...");
                                Network.SearchVerifiedNodes();
                    
                                while(Network.ConnectedNodes(true) == 0)
                                {
                                    Console.WriteLine("You are not connected to any nodes!");
                                    Console.WriteLine("Make sure you have internet connection!");
                                    Console.WriteLine("Trying again in 10 seconds...");
                                    Thread.Sleep(10000);
                                    Console.WriteLine("Searching for verified nodes...");
                                    Network.SearchVerifiedNodes();
                                }
                    
                                Console.WriteLine("Synchronising blocks...");
                                Blockchain.CurrentHeight = Blockchain.LastBlockExists();
                                Task.Run(() => Mining.MiningWatchdog());
                                Blockchain.FixCorruptedBlocks();
                                Blockchain.SyncBlocks();
                                
                                Task.Run(() => new WebClient().DownloadString("http://one-coin.org/nodes/?version=" + RunningVersion));
                                
                                Settings.Load();
                                Discord.StartService();
                                
                                WelcomeScreen();
                                Mining.StartOrStop(Mining.ThreadsCount);
                                Mining.Menu();
                                Mining.KeepMining = false;
                                Console.WriteLine("Thank you for using One Coin! Hope to see you soon :)");
                                
                                Discord.StopService();
                                Settings.Save();
                                
                                Task.Run(() => Network.FlushConnections());
                                Database.DatabaseHost = "";
                            }
                            else
                            {
                                Console.WriteLine("To few arguments, use \"OneCoin -help\", to get all possible commands with arguments...");
                            }
                            break;
                        }
                        case "database":
                        {
                            if (Arguments.Length > 4)
                            {
                                Database.DatabaseHost = Arguments[1];
                                Database.DatabaseUser = Arguments[2];
                                Database.DatabasePass = Arguments[3];
                                Database.DatabaseDb = Arguments[4];
                                
                                Task.Run(() => Database.ConnectionLoop());
                            }
                            else
                            {
                                Console.WriteLine("To few arguments, use \"OneCoin -help\", to get all possible commands with arguments...");
                            }
                            break;
                        }
                        case "runaspool":
                        {
                            MainMenuLoop = false;
                            if (Arguments.Length > 2)
                            {
                                Settings.CheckPaths();
                                if (CheckUpdates)
                                {
                                    CheckForUpdates();
                                }
                                
                                Pool.PoolKey = Arguments[1];
                                Mining.MiningAddress = Account.GetPublicKeyFromPrivateKey(Pool.PoolKey);
                                Mining.MiningAddress = Wallets.AddressToShort(Mining.MiningAddress);
                                Pool.CustomDifficulty = byte.Parse(Arguments[2]);
                                Pool.PoolWallet = Mining.MiningAddress;
                                
                                Task.Run(() => Pool.DatabaseSync());
                                Console.WriteLine("Starting pool, please wait...");
                                Task.Run(() => Network.ListenForConnections());
                                Task.Run(() => Network.ListenForPackets());
                    
                                Console.WriteLine("Searching for verified nodes...");
                                Network.SearchVerifiedNodes();
                    
                                while(Network.ConnectedNodes(true) == 0)
                                {
                                    Console.WriteLine("You are not connected to any nodes!");
                                    Console.WriteLine("Make sure you have internet connection!");
                                    Console.WriteLine("Trying again in 10 seconds...");
                                    Thread.Sleep(10000);
                                    Console.WriteLine("Searching for verified nodes...");
                                    Network.SearchVerifiedNodes();
                                }
                    
                                Console.WriteLine("Synchronising blocks...");
                                Blockchain.CurrentHeight = Blockchain.LastBlockExists();
                                Task.Run(() => Mining.MiningWatchdog());
                                Blockchain.FixCorruptedBlocks();
                                Blockchain.SyncBlocks();
                                
                                Task.Run(() => new WebClient().DownloadString("http://one-coin.org/nodes/?version=" + RunningVersion));

                                Settings.Load();
                                Discord.StartService();
                                
                                WelcomeScreen();
                                Mining.Menu();
                                Mining.KeepMining = false;
                                Console.WriteLine("Thank you for using One Coin! Hope to see you soon :)");
                                
                                Discord.StopService();
                                Settings.Save();
                                
                                Task.Run(() => Network.FlushConnections());
                                Database.DatabaseHost = "";
                            }
                            else
                            {
                                Console.WriteLine("To few arguments, use \"OneCoin -help\", to get all possible commands with arguments...");
                            }
                            break;
                        }
                        case "storeusers":
                        {
                            if (Arguments.Length > 1)
                            {
                                Wallets.TempAvatarsPath = Arguments[1];
                                
                                if (Arguments.Length > 2)
                                {
                                    Wallets.TempQrCodesPath = Arguments[2];
                                }
                            }
                            Task.Run(() => Wallets.DatabaseSync());
                            break;
                        }
                        case "storeblocks":
                        {
                            if (Arguments.Length > 1)
                            {
                                Blockchain.TempBlocksPath = Arguments[1];
                            }
                            Task.Run(() => Blockchain.DatabaseSync());
                            break;
                        }
                        case "generatesignature":
                        {
                            MainMenuLoop = false;
                            if (Arguments.Length > 2)
                            {
                                string Message = Arguments[2];
                                for (int j = 3; j < Arguments.Length; j++) { Message += ":" + Arguments[j]; }
                                Console.Write(Wallets.GenerateSignature(Arguments[1], Message));
                            }
                            else
                            {
                                Console.WriteLine("To few arguments, use \"OneCoin -help\", to get all possible commands with arguments...");
                            }
                            break;
                        }
                        case "verifysignature":
                        {
                            MainMenuLoop = false;
                            if (Arguments.Length > 3)
                            {
                                string Message = Arguments[3];
                                for (int j = 4; j < Arguments.Length; j++) { Message += ":" + Arguments[j]; }
                                Console.Write(Wallets.VerifySignature(Message, Wallets.AddressToLong(Arguments[1]), Arguments[2]).ToString().ToLower());
                            }
                            else
                            {
                                Console.WriteLine("To few arguments, use \"OneCoin -help\", to get all possible commands with arguments...");
                            }
                            break;
                        }
                        case "generatemnemonics":
                        {
                            MainMenuLoop = false;
                            if(Account.ReloadMnemonics())
                            {
                                while (Wallets.AddressToShort(Account.PublicKey).Contains("|") || Wallets.AddressToShort(Account.PublicKey).Contains(" "))
                                {
                                    Account.Mnemonics = Account.GenerateMnemonic();
                                    Account.GenerateKeyPair(Account.Mnemonics);
                                }
                                string CompleteMnemonics = "";
                                for (int j = 0; j < Account.Mnemonics.Length; j++)
                                {
                                    CompleteMnemonics += " " + Account.Mnemonics[j];
                                }
                                Console.Write(CompleteMnemonics[1..]);
                            }
                            else
                            {
                                Console.Write("NULL");
                            }
                            break;
                        }
                        case "getkeyfrommnemonics":
                        {
                            MainMenuLoop = false;
                            if (Arguments.Length > 1)
                            {
                                Account.Mnemonics = Arguments.Skip(1).ToArray();
                                Account.GenerateKeyPair(Account.Mnemonics);
                                
                                if(Wallets.AddressToShort(Account.PublicKey).Contains(" ") || Wallets.AddressToShort(Account.PublicKey).Contains("|"))
                                {
                                    Console.Write("NULL");
                                }
                                else
                                {
                                    Console.Write(Account.PrivateKey);
                                }
                            }
                            else
                            {
                                Console.WriteLine("To few arguments, use \"OneCoin -help\", to get all possible commands with arguments...");
                            }
                            break;
                        }
                        case "getaddressfromkey":
                        {
                            MainMenuLoop = false;
                            if (Arguments.Length > 1)
                            {
                                Console.Write(Wallets.AddressToShort(Account.GetPublicKeyFromPrivateKey(Arguments[1])));
                            }
                            else
                            {
                                Console.WriteLine("To few arguments, use \"OneCoin -help\", to get all possible commands with arguments...");
                            }
                            break;
                        }
                        case "testnode":
                        {
                            MainMenuLoop = false;
                            if (Arguments.Length > 1)
                            {
                                int Port = 10101;
                                if (Arguments.Length > 2)
                                {
                                    Port = int.Parse(Arguments[2]);
                                }
                                
                                try
                                {
                                    TcpClient Test = new();
                                    Test.Connect(IPAddress.Parse(Arguments[1]), Port);
                                    Test.GetStream().Write(Encoding.UTF8.GetBytes("Block~Get~X").Concat(new byte[] { 4 }).ToArray());
                                    Test.GetStream().ReadByte();
                                    Test.Close();
                                    
                                    Console.Write(true);
                                }
                                catch (Exception Ex)
                                {
                                    Console.Write(false);
                                }
                            }
                            else
                            {
                                Console.WriteLine("To few arguments, use \"OneCoin -help\", to get all possible commands with arguments...");
                            }
                            break;
                        }
                        case "sendpacket":
                        {
                            MainMenuLoop = false;
                            if (Arguments.Length > 2)
                            {
                                DebugLogging = true;
                                UdpClient Node = new();
                                Node.Connect(IPAddress.Parse(Arguments[1]), 10101);
                                Network.Send(null, Node, Arguments.Skip(2).ToArray());
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                Console.WriteLine("To few arguments, use \"OneCoin -help\", to get all possible commands with arguments...");
                            }
                            break;
                        }
                        case "calculator":
                        {
                            MainMenuLoop = false;
                            if (Arguments.Length > 1)
                            {
                                Console.Write(Hashing.CalculateBigNumbers(Arguments[1], "0", '+'));
                            }
                            else
                            {
                                Console.WriteLine("To few arguments, use \"OneCoin -help\", to get all possible commands with arguments...");
                            }
                            break;
                        }
                        default:
                        {
                            Console.WriteLine("Unknown argument: " + Arguments[0]);
                            MainMenuLoop = false;
                            break;
                        }
                    }
                }
            }
            if(MainMenuLoop)
            {
                if(!Console.IsOutputRedirected) // Prevent "double-click" in linux gui instead of launching from terminal.
                {
                    Console.Clear();
                    Settings.CheckPaths();
                    
                    if (CheckUpdates)
                    {
                        CheckForUpdates();
                    }
                    
                    Console.WriteLine("Starting node, please wait...");
                    Task.Run(() => Network.ListenForConnections());
                    Task.Run(() => Network.ListenForPackets());
                    
                    Console.WriteLine("Searching for verified nodes...");
                    Network.SearchVerifiedNodes();
                    
                    while(Network.ConnectedNodes(true) == 0)
                    {
                        Console.WriteLine("You are not connected to any nodes!");
                        Console.WriteLine("Make sure you have internet connection!");
                        Console.WriteLine("Then press any key to try again...");
                        Console.ReadKey();
                        Console.WriteLine("Searching for verified nodes...");
                        Network.SearchVerifiedNodes();
                    }
                    
                    Console.WriteLine("Synchronising blocks...");
                    Blockchain.CurrentHeight = Blockchain.LastBlockExists();
                    Task.Run(() => Mining.MiningWatchdog());
                    Blockchain.FixCorruptedBlocks();
                    Blockchain.SyncBlocks();
                    
                    Task.Run(() => new WebClient().DownloadString("http://one-coin.org/nodes/?version=" + RunningVersion));

                    Settings.Load();
                    Discord.StartService();
                    MainMenu();
                    Discord.StopService();
                    Settings.Save();
                    Task.Run(() => Network.FlushConnections());
                    Database.DatabaseHost = "";
                    
                    if (CheckUpdates)
                    {
                        CheckForUpdates();
                    }
                    Thread.Sleep(5000);
                }
                else
                {
                    Console.WriteLine("If you see this text you have probably redirected output!");
                    File.WriteAllText(Settings.AppPath + "/Run_Me_From_Terminal", "Alternatively, create onecoin.desktop file.");
                    Thread.Sleep(100000);
                    File.Delete(Settings.AppPath + "/Run_Me_From_Terminal");
                }
            }

            Console.BackgroundColor = Back;
            Console.ForegroundColor = Front;
        }

        static void CheckForUpdates()
        {
            string[] AppFiles = Directory.GetFiles(Settings.AppPath);

            for (int i = 0; i < AppFiles.Length; i++)
            {
                if(AppFiles[i][^7..] == ".backup")
                {
                    File.Delete(AppFiles[i]);
                }
            }
            
            string ReleaseName = "";
            string FileName = Settings.AppPath + "/OneCoin";
            
            if (OperatingSystem.IsWindows()) { ReleaseName = "Windows"; FileName += ".exe"; }
            if (OperatingSystem.IsLinux()) { ReleaseName = "Linux"; }
            if (RuntimeInformation.ProcessArchitecture.ToString().ToUpper()[0] != 'X') ReleaseName = "Arm";
            if (OperatingSystem.IsMacOS()) { ReleaseName = "Macos"; }
            
            if(ReleaseName.Length > 1)
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("Detected System: " + ReleaseName);
                ReleaseName = "cli-" + ReleaseName.ToLower() + ".zip";
                
                if(!File.Exists(Settings.AppPath + "/version.txt"))
                {
                    RunningVersion = "0.0.1";
                }
                
                WebClient WebClient = new WebClient();
                WebClient.Headers.Add("User-Agent", "Application");
                string LatestVersion = RunningVersion;
                string[] VersionData = WebClient.DownloadString("http://api.github.com/repos/TheQffel/OneCoin/releases/latest").Split("\"");
                for (int i = 0; i < VersionData.Length; i++)
                {
                    if(VersionData[i] == "tag_name")
                    {
                        LatestVersion = VersionData[i+2];
                        break;
                    }
                }
               
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Your version is: " + RunningVersion);
                Console.WriteLine("Latest version is: " + LatestVersion);

                if(LatestVersion == RunningVersion)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("Your app is up to date!");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("New version available!");
                    
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Downloading update...");
                    WebClient.DownloadFile("http://github.com/TheQffel/OneCoin/releases/latest/download/" + ReleaseName, Settings.AppPath + "/Update.zip");
                    Console.WriteLine("Extracting update...");
                    ZipFile.ExtractToDirectory(Settings.AppPath + "/Update.zip", Settings.UpdatePath, true);
                    for (int i = 0; i < AppFiles.Length; i++)
                    {
                        if(AppFiles[i][^4..] == ".dll" || AppFiles[i][^3..] == ".so" || AppFiles[i][^6..] == ".dylib")
                        {
                            File.Move(AppFiles[i], AppFiles[i] + ".backup");
                        }
                    }
                    File.Move(FileName, FileName + ".backup");
                    FileName = Settings.AppPath + "/Update.zip";
                    File.Move(FileName, FileName + ".backup");
                    AppFiles = Directory.GetFiles(Settings.UpdatePath);
                    for (int i = 0; i < AppFiles.Length; i++)
                    {
                        File.Move(Settings.UpdatePath + Path.GetFileName(AppFiles[i]), Settings.AppPath + Path.GetFileName(AppFiles[i]));
                    }
                    File.WriteAllText(Settings.AppPath + "/version.txt", LatestVersion);
                    Console.WriteLine("Update done, new version will be launched next time you run this app!");
                    
                    if(!OperatingSystem.IsWindows() && File.Exists(Settings.AppPath + "/OneCoin"))
                    {
                        new Process { StartInfo = new ProcessStartInfo("chmod", "+x " + Settings.AppPath + "/OneCoin") }.Start();
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Your system is not supported for auto updates!");
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("Check github every few days for newer version.");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Thread.Sleep(1000);
        }

        static void MainMenu()
        {
            Task.Run(() => Discord.UpdateService());
            
            while (MainMenuLoop)
            {
                WelcomeScreen();
                int UserChoice = DisplayMenu(new[] { "Exit to desktop", "Load account from disk", "Recover account from mnemonic seed", "Generate new account randomly", "Connect to hardware wallet", "Mine to address", "Explore blockchain", "Connect to specific node" });

                WelcomeScreen();
                if (UserChoice == 0)
                {
                    Console.WriteLine("Thank you for using One Coin! Hope to see you soon :)");
                    MainMenuLoop = false;
                }
                else if (UserChoice == 1)
                {
                    string[] Accounts = Directory.GetFiles(Settings.WalletsPath);
                    for (int i = 0; i < Accounts.Length; i++)
                    {
                        Accounts[i] = Accounts[i].Replace("\\", "/").Split("/")[^1].Replace(".dat", "");
                    }

                    if (Accounts.Length > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("You have " + Accounts.Length + " accounts stored on this computer:");
                        Console.ForegroundColor = ConsoleColor.White;
                        UserChoice = DisplayMenu(new[] { "Back to main menu" }.Concat(Accounts).ToArray());
                        if (UserChoice > 0)
                        {
                            Account.LocalName = Accounts[UserChoice - 1];
                            Account.Load();
                            Account.Menu();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("You don't have any accounts stored on this computer!");
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("Press any key to continue...");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                    }
                }
                else if (UserChoice == 2)
                {
                    Console.Write("Please enter your mnemonics words. When you type all, just leave empty input and press enter. ");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("Make sure you entered everything correctly! One mistake makes you get completely another address. ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Letters size doesn't matter.");

                    string Mnemonics = "";
                    for (int i = 1; i < 100; i++)
                    {
                        Console.Write("[" + i + "]: ");
                        string Word = Console.ReadLine().ToLower();
                        if (Word.Length > 0)
                        {
                            Mnemonics += " " + Word;
                        }
                        else
                        {
                            if (Mnemonics.Length < 1)
                            {
                                Mnemonics = "a b c d e";
                            }
                            Account.Mnemonics = Mnemonics[1..].Split(" ");
                            Account.GenerateKeyPair(Account.Mnemonics);
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine("Account recovered using " + (i - 1) + " words.");
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        }
                    }
                    if (!Wallets.CheckAddressCorrect(Wallets.AddressToShort(Account.PublicKey)))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Your address is not valid - it contains illegal characters (space or separator).");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Wallet address (not valid): " + Wallets.AddressToShort(Account.PublicKey));

                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("Press any key to continue...");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.Write("Your private key ");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("(don't give it to anyone - to keep it secure we won't display whole key now)");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(": " + Account.PrivateKey[..8] + "...");
                        Console.WriteLine("Your public key: " + Account.PublicKey);
                        Console.WriteLine("Your wallet address: " + Wallets.AddressToShort(Account.PublicKey));
                        Account.LocalName = "one-coin-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("Press any key to continue...");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                        Account.Menu();
                    }
                }
                else if (UserChoice == 3)
                {
                    if(Account.ReloadMnemonics())
                    {
                        Account.PublicKey = null;
                        while (!Wallets.CheckAddressCorrect(Wallets.AddressToShort(Account.PublicKey)))
                        {
                            Account.Mnemonics = Account.GenerateMnemonic();
                            Account.GenerateKeyPair(Account.Mnemonics);
                        }
                        Account.LocalName = "one-coin-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write("Account successfully generated! ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("This is your mnemonic. It is used to recover your account. ");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write("Without it, recovering your account is impossible! ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Write it down in a safe place. Best way to do it is to store it offline, for example on a piece of paper. ");
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine(""); Console.WriteLine("");
                        Console.WriteLine("Your mnemonic words:");
                        Console.ForegroundColor = ConsoleColor.White;
                        for (int i = 0; i < Account.Mnemonics.Length; i++)
                        {
                            Console.WriteLine("[" + (i + 1) + "]: " + Account.Mnemonics[i]);
                        }
                        Console.WriteLine("");
                        Console.Write("Your private key ");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("(don't give it to anyone - to keep it secure we won't display whole key now)");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(": " + Account.PrivateKey[..8] + "...");
                        Console.WriteLine("Your public key: " + Account.PublicKey);
                        Console.WriteLine("Your wallet address: " + Wallets.AddressToShort(Account.PublicKey));
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        for (int i = 99; i > 0; i--)
                        {
                            Console.Write("You need to wait " + i + " seconds, before you can continue.  ");
                            Thread.Sleep(1000);
                            Console.SetCursorPosition(0, Console.CursorTop);
                            if (Console.KeyAvailable) { Console.ReadKey(); }
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("You can continue now (if you already write down your mnemonics).");
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("Press any key to continue...");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                        Account.Menu();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("Mnemonic dictionary not found!");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write("Make sure you have all files in 'words' directory.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Press any key to continue...");
                        Console.ReadKey();
                    }
                }
                else if (UserChoice == 4)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("If you don't have hardware wallet configured correctly you need to generate setting first and upload code to your arduino or esp.");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("Make sure your hardware wallet is connected to your computer with USB cable and press enter to continue...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Select serial port with your hardware wallet:");
                    string[] SerialPorts = SerialPort.GetPortNames();
                    UserChoice = DisplayMenu(new[] { "Back to main menu" }.Concat(SerialPorts).ToArray());
                    if (UserChoice > 0)
                    {
                        Hardware.PortName = SerialPorts[UserChoice - 1];
                        Hardware.Menu();
                    }
                }
                else if (UserChoice == 5)
                {
                    Console.Write("Mining address or nickname: (type 0 to exit): ");
                    string[] Address = (Console.ReadLine().Replace("#", "|") + "|1").Split('|');
                    if(Address[0].Length != 88)
                    {
                        Address[0] = Wallets.GetAddress(Address[0], byte.Parse(Address[1]));
                    }
                    if(Hashing.CheckStringFormat(Address[0], 4, 88, 88))
                    {
                        Mining.MiningAddress = Address[0];
                        Console.Write("Mining pool address: (type 0 to mine solo): ");
                        string[] PoolAddress = (Console.ReadLine() + ":10101").Split(':');
                        if(PoolAddress[0].Length > 5)
                        {
                            Pool.PoolAddress = PoolAddress[0];
                            Pool.PoolPort = int.Parse(PoolAddress[1]);
                            Task.Run(() => Pool.ConnectionLoop());
                        }
                        Mining.Menu();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("You entered wrong nickname or address!");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Press any key to continue...");
                        Console.ReadKey();
                    }
                }
                else if (UserChoice == 6)
                {
                    Blockchain.Explore();
                }
                else if (UserChoice == 7)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("Total connected nodes: " + Network.ConnectedNodes() + "/256");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Type address (in format IP:PORT): ");
                    string[] Address = (Console.ReadLine() + ":10101").Split(":");
                    Task.Run(() => Network.Discover(Address[0], int.Parse(Address[1])));
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("Your node will connect to " + Address[0] + ":" + Address[1]);
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("(Only if this address is correct full node)");
                    Thread.Sleep(2500);
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Total connected nodes: " + Network.ConnectedNodes() + "/256");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Press any key to continue...");
                    Console.ReadKey();
                }
            }
            
            Task.Run(() => Discord.UpdateService());
        }

        public static int DisplayMenu(string[] Options)
        {
            for (int i = 1; i < Options.Length; i++)
            {
                Console.WriteLine("[" + i + "] " + Options[i]);
            }
            Console.WriteLine("[0] " + Options[0]);
            Console.Write("Your Choice: ");

            while(true)
            {
                if(int.TryParse(Console.ReadLine(), out int Response))
                {
                    if(Response >= 0 && Response < Options.Length)
                    {
                        return Response;
                    }
                }
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.Write("Wrong choice, please try again: ");
            }
        }

        public static void WelcomeScreen()
        {
            if (DebugLogging)
            {
                Console.WriteLine("\n");
                Console.WriteLine("[OneCoin] (Debugging)");
            }
            else
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkRed; Console.Write("╔══");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("════════════════════════════════════════════════════════════════");
                Console.ForegroundColor = ConsoleColor.DarkRed; Console.Write("══╗");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.DarkRed; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.DarkCyan; Console.Write("  .d88888b.                      ");
                Console.ForegroundColor = ConsoleColor.DarkYellow; Console.Write("   .d8888b.           d8b          ");
                Console.ForegroundColor = ConsoleColor.DarkRed; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.DarkCyan; Console.Write(" d88P' 'Y88b                     ");
                Console.ForegroundColor = ConsoleColor.DarkYellow; Console.Write("  d88P  Y88b          Y8P          ");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.DarkCyan; Console.Write(" 888     888                     ");
                Console.ForegroundColor = ConsoleColor.DarkYellow; Console.Write("  888    888                       ");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.DarkCyan; Console.Write(" 888     888 88888b.   .d88b.    ");
                Console.ForegroundColor = ConsoleColor.DarkYellow; Console.Write("  888         .d88b.  888 88888b.  ");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.DarkCyan; Console.Write(" 888     888 888 '88b d8P  Y8b   ");
                Console.ForegroundColor = ConsoleColor.DarkYellow; Console.Write("  888        d88''88b 888 888 '88b ");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.DarkCyan; Console.Write(" 888     888 888  888 88888888   ");
                Console.ForegroundColor = ConsoleColor.DarkYellow; Console.Write("  888    888 888  888 888 888  888 ");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.DarkCyan; Console.Write(" Y88b. .d88P 888  888 Y8b.       ");
                Console.ForegroundColor = ConsoleColor.DarkYellow; Console.Write("  Y88b  d88P Y88..88P 888 888  888 ");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.DarkRed; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.DarkCyan; Console.Write("  'Y88888P'  888  888  'Y8888    ");
                Console.ForegroundColor = ConsoleColor.DarkYellow; Console.Write("   'Y8888P'   'Y88P'  888 888  888 ");
                Console.ForegroundColor = ConsoleColor.DarkRed; Console.Write("║");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.DarkRed; Console.Write("╚══");
                Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("════════════════════════════════════════════════════════════════");
                Console.ForegroundColor = ConsoleColor.DarkRed; Console.Write("══╝");
                Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("");
            }
        }
	}
}

