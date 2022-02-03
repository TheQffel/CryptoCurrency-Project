using System;
using System.IO;
using System.Linq;
using System.Net;

namespace OneCoin
{
    class Logins
    {
        public static bool RememberService(string Address)
        {
            try
            {
                WebClient WebClient = new();
                string Data = WebClient.DownloadString(Address);
                File.WriteAllText(Settings.ServicesPath + Wallets.AddressToShort(Account.PublicKey) + "/" + Data[7..95] + "." + Data[..6], Data[96..]);
                return true;
            }
            catch(Exception Ex)
            {
                return false;   
            }
        }
        
        public static void Menu()
        {
            while (true)
            {
                Program.WelcomeScreen();
                Console.WriteLine("Loading services, please wait...");
                
                string[] ServicesAddresses = Directory.GetFiles(Settings.ServicesPath + Wallets.AddressToShort(Account.PublicKey) + "/");
                string[] ServicesNames = new string[ServicesAddresses.Length];
                string[] ServicesUrls = new string[ServicesAddresses.Length];
        
                for (int i = 0; i < ServicesAddresses.Length; i++)
                {
                    ServicesUrls[i] = File.ReadAllText(ServicesAddresses[i]);
                    ServicesAddresses[i] = Path.GetFileName(ServicesAddresses[i]).Split('.')[0];
                    ServicesNames[i] = Wallets.GetName(ServicesAddresses[i]);
                    
                    WebClient WebClient = new();
                    if(WebClient.DownloadString(ServicesUrls[i] + "?account=" + Wallets.AddressToShort(Account.PublicKey)).Length > 10)
                    {
                        ServicesNames[i] += "    -    Pending login!";
                    }
                }
                
                Program.WelcomeScreen();
                int UserChoice = Program.DisplayMenu(new[] { "Back to previous menu" }.Concat(ServicesNames).ToArray());
                
                Program.WelcomeScreen();
                if (UserChoice == 0)
                {
                    break;
                }
                else
                {
                    UserChoice--;
                    
                    WebClient WebClient = new();
                    string Data = WebClient.DownloadString(ServicesUrls[UserChoice] + "?account=" + Wallets.AddressToShort(Account.PublicKey));
                    Console.WriteLine("[" + ServicesAddresses[UserChoice] + "]");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    
                    if(Data.Length > 10)
                    {
                        Console.WriteLine("Action required for " + ServicesNames[UserChoice].Replace("    -    ", ": "));
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine("Time: " + Data.Split(" | ")[1]);
                        Console.WriteLine("Details: " + Data.Split(" | ")[0]);
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write("Confirm? Press (Y)es or (N)o: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        ConsoleKey Button = Console.ReadKey().Key;
                        Console.WriteLine("");
                        
                        if (Button == ConsoleKey.Y)
                        {
                            Console.WriteLine(WebClient.DownloadString(ServicesUrls[UserChoice] + "?account=" + Wallets.AddressToShort(Account.PublicKey) + "&signature=" + Wallets.GenerateSignature(Account.PrivateKey, Data)));
                            
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine("Confirmed login request!");
                        }
                        if (Button == ConsoleKey.N)
                        {
                            WebClient.DownloadString(ServicesUrls[UserChoice] + "?account=" + Wallets.AddressToShort(Account.PublicKey) + "&signature=x");
                            
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("Canceled login request!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("There are no pending actions for " + ServicesNames[UserChoice] + ".");
                    }
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("Press any key to continue...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                }
                Program.WelcomeScreen();
            }
        }
    }
}
