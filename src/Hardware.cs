using System;
using System.IO;
using System.IO.Ports;
using System.Numerics;
using System.Threading;

namespace OneCoin
{
    class Hardware
    {
        public static string PortName = "";
        
        public static void GenerateSettingsFile(string PrivateKey, string PublicKey, string WifiName, string WifiPass, string AccountName = "")
        {
            string FileData = "// Settings file for account: " + AccountName;
            FileData += "\n#define PrivateKey \"" + PrivateKey + "\"";
            FileData += "\n#define PublicKey \"" + PublicKey + "\"";
            FileData += "\n#define WifiName \"" + WifiName + "\"";
            FileData += "\n#define WifiPass \"" + WifiPass + "\"";
            File.WriteAllText(Settings.ExtrasPath + "HardwareWallet/Settings.h", FileData);
        }

        public static void Menu()
        {
            SerialPort SerialPort = new SerialPort(PortName, 9600);
            SerialPort.DtrEnable = false;
            SerialPort.Open();

            while (SerialPort.IsOpen)
            {
                Program.WelcomeScreen();
                int UserChoice = Program.DisplayMenu(new[] { "Back to main menu", "Hardware wallet info", "Display keys", "Sign transaction" });

                Program.WelcomeScreen();
                if (UserChoice == 0)
                {
                    break;
                }
                else if (UserChoice == 1)
                {
                    Console.WriteLine("");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("Open info app on your wallet and press any key to continue. If you have already opened this app you need to relaunch it.");
                    SerialPort.ReadExisting();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                    string[] Versions = SerialPort.ReadExisting().Split("|");
                    Console.WriteLine("");
                    Console.WriteLine("Hardware version: " + Versions[1]);
                    Console.WriteLine("Software version: " + Versions[2]);
                    Console.WriteLine("");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("Press any key to continue...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                }
                else if (UserChoice == 2)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Are you sure to display these keys now? Private keys are only way to prove your identity. Anyone who knows your private key can withdraw coins from your account.");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("Press (Y)es or (N)o: ");
                    Console.ForegroundColor = ConsoleColor.White;

                    if (Console.ReadKey().Key == ConsoleKey.Y)
                    {
                        Console.WriteLine("");
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine("Open keys app on your wallet and press any key to continue. If you have already opened this app you need to relaunch it.");
                        SerialPort.ReadExisting();
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                        string[] Keys = SerialPort.ReadExisting().Split("|");
                        Console.WriteLine("");
                        Console.WriteLine("Private key: " + Keys[1]);
                        Console.WriteLine("Public key: " + Keys[2]);
                        Console.WriteLine("");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Do not share them with anyone!");
                        Console.WriteLine("");
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("Press any key to continue...");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                    }
                }
                else if (UserChoice == 3)
                {
                    Console.WriteLine("");
                    Console.Write("To (address or nickname): ");
                    string Address = Console.ReadLine();
                    Console.Write("Amount (leave empty to cancel): 0.");
                    string Amount = Console.ReadLine();
                    if (Address.Length > 0 && Amount.Length > 0)
                    {
                        Transaction Transaction = new();
                        Console.WriteLine("");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("Make sure your hardware wallet is connected to internet and at least one node, to be able to send transaction.");
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine("Open send app on your wallet and press any key to continue. If you have already opened this app you need to relaunch it.");
                        SerialPort.ReadExisting();
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                        string[] Keys = SerialPort.ReadExisting().Split("|");
                        Transaction.From = Wallets.AddressToShort(Keys[2]);
                        Transaction.To = Address;
                        Transaction.Amount = BigInteger.Parse(Amount + new string('0', 24 - Amount.Length));
                        Transaction.Timestamp = (ulong)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                        Transaction.GenerateSignature(Keys[1]);
                        SerialPort.WriteLine("|" + Address + "|" + Amount + "|" + Transaction.Signature + "|");
                        Console.WriteLine("Transaction successfully generated, you can confirm it now in your hardware wallet. Press both keys to accept, press left or right to cancel.");
                        Console.WriteLine("");
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("Press any key to continue...");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                    }
                }
            }
            
            SerialPort.Close();
        }
    }
}
