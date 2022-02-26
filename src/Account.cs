using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using Org.BouncyCastle.Math.EC;

namespace OneCoin
{
    class Account
    {
        public static string PrivateKey;
        public static string PublicKey;

        public static string LocalName;
        public static string[] Mnemonics;

        public static string[] MnemonicsA, MnemonicsB, MnemonicsC, MnemonicsD, MnemonicsE, MnemonicsF, MnemonicsG, MnemonicsH, MnemonicsI, MnemonicsJ, MnemonicsK, MnemonicsL, MnemonicsM, MnemonicsN, MnemonicsO, MnemonicsP, MnemonicsQ, MnemonicsR, MnemonicsS, MnemonicsT, MnemonicsU, MnemonicsV, MnemonicsW, MnemonicsX, MnemonicsY, MnemonicsZ;

        public static bool ReloadMnemonics()
        {
            bool WarningPrinted = false;
            WebClient WebClient = new WebClient();
            WebClient.Headers.Add("User-Agent", "Application");
            
            if(!File.Exists(Settings.WordsPath + "a.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/a.txt", Settings.WordsPath + "a.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "b.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/b.txt", Settings.WordsPath + "b.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "c.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/c.txt", Settings.WordsPath + "c.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "d.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/d.txt", Settings.WordsPath + "d.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "e.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/e.txt", Settings.WordsPath + "e.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "f.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/f.txt", Settings.WordsPath + "f.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "g.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/g.txt", Settings.WordsPath + "g.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "h.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/h.txt", Settings.WordsPath + "h.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "i.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/i.txt", Settings.WordsPath + "i.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "j.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/j.txt", Settings.WordsPath + "j.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "k.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/k.txt", Settings.WordsPath + "k.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "l.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/l.txt", Settings.WordsPath + "l.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "m.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/m.txt", Settings.WordsPath + "m.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "n.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/n.txt", Settings.WordsPath + "n.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "o.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/o.txt", Settings.WordsPath + "o.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "p.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/p.txt", Settings.WordsPath + "p.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "q.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/q.txt", Settings.WordsPath + "q.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "r.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/r.txt", Settings.WordsPath + "r.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "s.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/s.txt", Settings.WordsPath + "s.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "t.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/t.txt", Settings.WordsPath + "t.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "u.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/u.txt", Settings.WordsPath + "u.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "v.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/v.txt", Settings.WordsPath + "v.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "w.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/w.txt", Settings.WordsPath + "w.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "x.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/x.txt", Settings.WordsPath + "x.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "y.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/y.txt", Settings.WordsPath + "y.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(!File.Exists(Settings.WordsPath + "z.txt")) { WebClient.DownloadFile("http://raw.githubusercontent.com/TheQffel/OneCoin/main/words/z.txt", Settings.WordsPath + "z.txt"); if(!WarningPrinted) { WarningPrinted = true; Console.WriteLine("Missing mnemonic words file(s), downloading from source..."); } }
            if(WarningPrinted) { Console.WriteLine("Downloaded all missing mnemonic words files."); }
            
            MnemonicsA = File.ReadAllLines(Settings.WordsPath + "a.txt");
            MnemonicsB = File.ReadAllLines(Settings.WordsPath + "b.txt");
            MnemonicsC = File.ReadAllLines(Settings.WordsPath + "c.txt");
            MnemonicsD = File.ReadAllLines(Settings.WordsPath + "d.txt");
            MnemonicsE = File.ReadAllLines(Settings.WordsPath + "e.txt");
            MnemonicsF = File.ReadAllLines(Settings.WordsPath + "f.txt");
            MnemonicsG = File.ReadAllLines(Settings.WordsPath + "g.txt");
            MnemonicsH = File.ReadAllLines(Settings.WordsPath + "h.txt");
            MnemonicsI = File.ReadAllLines(Settings.WordsPath + "i.txt");
            MnemonicsJ = File.ReadAllLines(Settings.WordsPath + "j.txt");
            MnemonicsK = File.ReadAllLines(Settings.WordsPath + "k.txt");
            MnemonicsL = File.ReadAllLines(Settings.WordsPath + "l.txt");
            MnemonicsM = File.ReadAllLines(Settings.WordsPath + "m.txt");
            MnemonicsN = File.ReadAllLines(Settings.WordsPath + "n.txt");
            MnemonicsO = File.ReadAllLines(Settings.WordsPath + "o.txt");
            MnemonicsP = File.ReadAllLines(Settings.WordsPath + "p.txt");
            MnemonicsQ = File.ReadAllLines(Settings.WordsPath + "q.txt");
            MnemonicsR = File.ReadAllLines(Settings.WordsPath + "r.txt");
            MnemonicsS = File.ReadAllLines(Settings.WordsPath + "s.txt");
            MnemonicsT = File.ReadAllLines(Settings.WordsPath + "t.txt");
            MnemonicsU = File.ReadAllLines(Settings.WordsPath + "u.txt");
            MnemonicsV = File.ReadAllLines(Settings.WordsPath + "v.txt");
            MnemonicsW = File.ReadAllLines(Settings.WordsPath + "w.txt");
            MnemonicsX = File.ReadAllLines(Settings.WordsPath + "x.txt");
            MnemonicsY = File.ReadAllLines(Settings.WordsPath + "y.txt");
            MnemonicsZ = File.ReadAllLines(Settings.WordsPath + "z.txt");
            
            return true;
        }

        public static string[] GenerateMnemonic()
        {
            Random Random = new();
            string[] Mnemonic = new string[26];
            Mnemonic[0] = MnemonicsA[Random.Next(MnemonicsA.Length)];
            Mnemonic[1] = MnemonicsB[Random.Next(MnemonicsB.Length)];
            Mnemonic[2] = MnemonicsC[Random.Next(MnemonicsC.Length)];
            Mnemonic[3] = MnemonicsD[Random.Next(MnemonicsD.Length)];
            Mnemonic[4] = MnemonicsE[Random.Next(MnemonicsE.Length)];
            Mnemonic[5] = MnemonicsF[Random.Next(MnemonicsF.Length)];
            Mnemonic[6] = MnemonicsG[Random.Next(MnemonicsG.Length)];
            Mnemonic[7] = MnemonicsH[Random.Next(MnemonicsH.Length)];
            Mnemonic[8] = MnemonicsI[Random.Next(MnemonicsI.Length)];
            Mnemonic[9] = MnemonicsJ[Random.Next(MnemonicsJ.Length)];
            Mnemonic[10] = MnemonicsK[Random.Next(MnemonicsK.Length)];
            Mnemonic[11] = MnemonicsL[Random.Next(MnemonicsL.Length)];
            Mnemonic[12] = MnemonicsM[Random.Next(MnemonicsM.Length)];
            Mnemonic[13] = MnemonicsN[Random.Next(MnemonicsN.Length)];
            Mnemonic[14] = MnemonicsO[Random.Next(MnemonicsO.Length)];
            Mnemonic[15] = MnemonicsP[Random.Next(MnemonicsP.Length)];
            Mnemonic[16] = MnemonicsQ[Random.Next(MnemonicsQ.Length)];
            Mnemonic[17] = MnemonicsR[Random.Next(MnemonicsR.Length)];
            Mnemonic[18] = MnemonicsS[Random.Next(MnemonicsS.Length)];
            Mnemonic[19] = MnemonicsT[Random.Next(MnemonicsT.Length)];
            Mnemonic[20] = MnemonicsU[Random.Next(MnemonicsU.Length)];
            Mnemonic[21] = MnemonicsV[Random.Next(MnemonicsV.Length)];
            Mnemonic[22] = MnemonicsW[Random.Next(MnemonicsW.Length)];
            Mnemonic[23] = MnemonicsX[Random.Next(MnemonicsX.Length)];
            Mnemonic[24] = MnemonicsY[Random.Next(MnemonicsY.Length)];
            Mnemonic[25] = MnemonicsZ[Random.Next(MnemonicsZ.Length)];
            return Mnemonic;
        }

        public static void GenerateKeyPair(string[] SeedPhrase)
        {
            string Text = "";

            for (int i = 0; i < SeedPhrase.Length; i++)
            {
                string Temp = SeedPhrase[i].ToLower();
                for (int j = 0; j < Temp.Length; j++)
                {
                    if (Temp[j] == 'a') { Text += "00001"; }
                    if (Temp[j] == 'b') { Text += "00010"; }
                    if (Temp[j] == 'c') { Text += "00011"; }
                    if (Temp[j] == 'd') { Text += "00100"; }
                    if (Temp[j] == 'e') { Text += "00101"; }
                    if (Temp[j] == 'f') { Text += "00110"; }
                    if (Temp[j] == 'g') { Text += "00111"; }
                    if (Temp[j] == 'h') { Text += "01000"; }
                    if (Temp[j] == 'i') { Text += "01001"; }
                    if (Temp[j] == 'j') { Text += "01010"; }
                    if (Temp[j] == 'k') { Text += "01011"; }
                    if (Temp[j] == 'l') { Text += "01100"; }
                    if (Temp[j] == 'm') { Text += "01101"; }
                    if (Temp[j] == 'n') { Text += "01110"; }
                    if (Temp[j] == 'o') { Text += "01111"; }
                    if (Temp[j] == 'p') { Text += "10000"; }
                    if (Temp[j] == 'q') { Text += "10001"; }
                    if (Temp[j] == 'r') { Text += "10010"; }
                    if (Temp[j] == 's') { Text += "10011"; }
                    if (Temp[j] == 't') { Text += "10100"; }
                    if (Temp[j] == 'u') { Text += "10101"; }
                    if (Temp[j] == 'v') { Text += "10110"; }
                    if (Temp[j] == 'w') { Text += "10111"; }
                    if (Temp[j] == 'x') { Text += "11000"; }
                    if (Temp[j] == 'y') { Text += "11001"; }
                    if (Temp[j] == 'z') { Text += "11010"; }
                }
                Text += "11110";
            }

            byte[] Seed = new byte[Text.Length / 8];
            for (int i = 0; i < Text.Length / 8; i++)
            {
                Seed[i] = Convert.ToByte(Text.Substring(8 * i, 8), 2);
            }

            X9ECParameters Curve = ECNamedCurveTable.GetByName("secp256k1");
            ECDomainParameters Domain = new(Curve.Curve, Curve.G, Curve.N, Curve.H, Curve.GetSeed());
            ECKeyGenerationParameters Key = new(Domain, new SecureRandom(Seed));
            ECKeyPairGenerator Generator = new("ECDSA");
            Generator.Init(Key);

            AsymmetricCipherKeyPair Keys = Generator.GenerateKeyPair();
            ECPrivateKeyParameters Private = Keys.Private as ECPrivateKeyParameters;
            ECPublicKeyParameters Public = Keys.Public as ECPublicKeyParameters;

            PrivateKey = Hashing.BytesToHex(Private.D.ToByteArrayUnsigned());
            PublicKey = Hashing.BytesToHex(Public.Q.GetEncoded());
        }

        public static string GetPublicKeyFromPrivateKey(string PrivateKey)
        {
            X9ECParameters Curve = SecNamedCurves.GetByName("secp256k1");
            ECDomainParameters Domain = new ECDomainParameters(Curve.Curve, Curve.G, Curve.N, Curve.H);
            Org.BouncyCastle.Math.BigInteger Number = new Org.BouncyCastle.Math.BigInteger(PrivateKey, 16);
            ECPoint Point = Domain.G.Multiply(Number);
            ECPublicKeyParameters PublicKey = new ECPublicKeyParameters(Point, Domain);
            return Hashing.BytesToHex(PublicKey.Q.GetEncoded());
        }

        public static void Load()
        {
            using BinaryReader AccountFile = new(File.Open(Settings.WalletsPath + LocalName + ".dat", FileMode.Open));

            Mnemonics = new string[AccountFile.ReadByte()];
            for (int i = 0; i < Mnemonics.Length; i++)
            {
                Mnemonics[i] = AccountFile.ReadString();
            }
            GenerateKeyPair(Mnemonics);
        }

        public static void Save()
        {
            using BinaryWriter AccountFile = new(File.Open(Settings.WalletsPath + LocalName + ".dat", FileMode.Create));

            AccountFile.Write((byte)Mnemonics.Length);
            for (int i = 0; i < Mnemonics.Length; i++)
            {
                AccountFile.Write(Mnemonics[i]);
            }
        }

        public static void Transaction(string AddressTo, BigInteger Amount, string Message = "")
        {
            Transaction NewTransaction = new();
            NewTransaction.From = Wallets.AddressToShort(PublicKey);
            NewTransaction.To = AddressTo;
            NewTransaction.Amount = Amount;
            NewTransaction.Fee = 0;
            NewTransaction.Timestamp = (ulong)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            NewTransaction.Message = Message;
            NewTransaction.GenerateSignature(PrivateKey);
            
            Network.Broadcast(new[] { "Transaction", NewTransaction.From, NewTransaction.To, NewTransaction.Amount.ToString(), NewTransaction.Fee.ToString(), NewTransaction.Timestamp.ToString(), NewTransaction.Message, NewTransaction.Signature });
        }

        public static void Menu()
        {
            Task.Run(() => Discord.UpdateService());
            
            if (!Directory.Exists(Settings.ServicesPath + Wallets.AddressToShort(PublicKey)))
            {
                Directory.CreateDirectory(Settings.ServicesPath + Wallets.AddressToShort(PublicKey));
            }
            
            Save();
            while (true)
            {
                Program.WelcomeScreen();
                int UserChoice = Program.DisplayMenu(new[] { "Back to main menu", "Show general info", "Show keys pair", "Show mnemonic words", "Generate receive qr code", "Transfer coins", "Change public nickname", "Rename account file name", "Change avatar", "Explore blockchain", "Mine coins", "Add login service", "Manage logins", "Save account info to hardware wallet file", "Change settings", "Remove account from this computer" });

                Program.WelcomeScreen();
                if (UserChoice == 0)
                {
                    Save();
                    break;
                }
                else if (UserChoice == 1)
                {
                    Console.WriteLine("Your account is saved in this file: " + LocalName + ".dat");
                    Console.WriteLine("Recieve address: " + Wallets.AddressToShort(PublicKey));
                    BigInteger Balance = Wallets.GetBalance(Wallets.AddressToShort(PublicKey));
                    Console.WriteLine("Your balance: 0." + new string('0', 24 - Balance.ToString().Length) + Balance + " ①");

                    if (Wallets.GetName(Wallets.AddressToShort(PublicKey)) == null)
                    {
                        Console.WriteLine("");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("You won't set your public nickname. You can do it, so anyone can send coins to you, without need to type your full recieve address.");
                    }
                    else
                    {
                        Console.WriteLine("Your public nickname: " + Wallets.GetName(Wallets.AddressToShort(PublicKey)));
                        Console.WriteLine("");
                    }
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("You can safely send someone your recieve address. It is used to send money to you.");
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
                        Console.WriteLine("");
                        Console.WriteLine("Private key: " + PrivateKey);
                        Console.WriteLine("Public key: " + PublicKey);
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
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Are you sure to display mnemonic words now? Anyone who knows these words can recover your account.");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("Press (Y)es or (N)o: ");
                    Console.ForegroundColor = ConsoleColor.White;

                    if (Console.ReadKey().Key == ConsoleKey.Y)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("");
                        for (int i = 0; i < Mnemonics.Length; i++)
                        {
                            Console.WriteLine("[" + i + "]: " + Mnemonics[i]);
                        }
                        Console.WriteLine("");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("Make sure you have copy of them in safe place.");
                        Console.WriteLine("");
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("Press any key to continue...");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                    }
                }
                else if (UserChoice == 4)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("We suggest maximize this window and set small font, otherwise qr code may not fit in console's window.");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("Press any key to continue...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                    Console.WriteLine("");
                    Console.WriteLine("");
                    Wallets.PrintQrCode(Wallets.AddressToShort(PublicKey));
                    Console.WriteLine("");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("Press any key to continue...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                }
                else if (UserChoice == 5)
                {
                    Console.WriteLine("Enter address (or nickname) you want to send coins (or 0 to exit).");
                    Console.Write("To: ");
                    string AccountAddress = Console.ReadLine();
                    if(AccountAddress.Length > 1)
                    {
                        if(AccountAddress.Length < 25)
                        {
                            AccountAddress = Wallets.GetAddress(AccountAddress);
                        }
                        if(AccountAddress.Length == 88)
                        {
                            Console.WriteLine("Enter amount of coins you want to send (or 0 to exit).");
                            BigInteger Balance = Wallets.GetBalance(Wallets.AddressToShort(PublicKey));
                            Console.WriteLine("You currently have: 0." + new string('0', 24 - Balance.ToString().Length) + Balance + " ①");
                            Console.Write("Amount to transfer: 0.");
                            string AmountToSend = Console.ReadLine();
                            if(AmountToSend.Length < 24)
                            {
                                AmountToSend = AmountToSend + new string('0', 24 - AmountToSend.Length);
                            }
                            _ = BigInteger.TryParse(AmountToSend, out BigInteger Amount);
                            if (Amount > 0)
                            {
                                if (Amount <= Balance)
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.WriteLine("You are sending:    0." + new string('0', 24 - Amount.ToString().Length) + Amount);
                                    Console.WriteLine("From address:     " + Wallets.AddressToShort(PublicKey));
                                    Console.WriteLine("To address:       " + AccountAddress);
                                    Console.Write("Is this correct?  Press (Y)es or (N)o: ");
                                    Console.ForegroundColor = ConsoleColor.White;

                                    if (Console.ReadKey().Key == ConsoleKey.Y)
                                    {
                                        Console.WriteLine("");
                                        Transaction(AccountAddress, Amount);
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.Write("Transaction sent!");
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                    else
                                    {
                                        Console.WriteLine("");
                                        Console.Write("Cancelled...");
                                    }
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.Write("You dont have enough coins!");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write("Address or nickname is not valid!");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    Console.WriteLine("");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("Press any key to continue...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                }
                else if (UserChoice == 6)
                {
                    Console.Write("New nickname (leave empty to keep unchanged): ");
                    string NewNickname = Console.ReadLine();
                    if(Hashing.CheckStringFormat(NewNickname, 5, 4, 24))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("You are changing your nickname to: " + NewNickname);
                        Console.WriteLine("This operation will cost you " + BigInteger.Pow(2, 24 - NewNickname.Length) + " ones.");
                        Console.Write("Proceed? Press (Y)es or (N)o: ");
                        Console.ForegroundColor = ConsoleColor.White;

                        if (Console.ReadKey().Key == ConsoleKey.Y)
                        {
                            Console.WriteLine("");
                            Transaction(NewNickname, BigInteger.Pow(2, 24 - NewNickname.Length));
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write("Transaction sent!");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.WriteLine("");
                            Console.Write("Cancelled...");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Invalid nickname entered! Must be alphanumeric and between 4 and 24 characters.");
                    }
                    Console.WriteLine("");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("Press any key to continue...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                }
                else if (UserChoice == 7)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("This will only change the file name, where your keys are stored. ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("If you want to change your public nickname, go backwards and select the appropriate option.");
                    Console.Write("New account name (leave empty to keep unchanged): ");
                    string NewLocalName = Console.ReadLine();
                    if (File.Exists(Settings.WalletsPath + NewLocalName + ".dat"))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("Account with this name already exists!");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        string OldLocalName = LocalName;
                        LocalName = NewLocalName;
                        Save();
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write("Successfully renamed " + OldLocalName + " to " + LocalName);
                        File.Delete(Settings.WalletsPath + OldLocalName + ".dat");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.WriteLine("");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("Press any key to continue...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                }
                else if (UserChoice == 8)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("Please enter full path (like C:\\Users\\file.jpg or /home/user/file.png).");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Avatar file name (leave empty to keep unchanged): ");
                    string NewAvatar = Media.ImageToText((Bitmap) Image.FromFile(Console.ReadLine()));
                    if(Hashing.CheckStringFormat(NewAvatar, 5, 128, 1048576) && Media.ImageDataCorrect(NewAvatar))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("You are changing your avatar, make sure you selected right one.");
                        Console.WriteLine("This operation will cost you " + NewAvatar.Length + " ones.");
                        Console.Write("Proceed? Press (Y)es or (N)o: ");
                        Console.ForegroundColor = ConsoleColor.White;

                        if (Console.ReadKey().Key == ConsoleKey.Y)
                        {
                            Console.WriteLine("");
                            Transaction(NewAvatar, NewAvatar.Length);
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write("Transaction sent!");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.WriteLine("");
                            Console.Write("Cancelled...");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Invalid avatar selected! Must be square, between 16 and 1024 pixels length, and total size must not exceed 1MB (in OneCoin Rgb format).");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("To reduce size, reduce number of colors in your avatar. Pixels of the same color one after another are compressed, which reduce file size.");
                    }
                    Console.WriteLine("");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("Press any key to continue...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                }
                else if (UserChoice == 9)
                {
                    Blockchain.Explore();
                }
                else if (UserChoice == 10)
                {
                    Mining.MiningAddress = Wallets.AddressToShort(PublicKey);
                    Mining.Menu();
                }
                else if (UserChoice == 11)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("Please enter address (like http://one-coin.org/myservice/ocnapi/).");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Service address (leave empty to exit): ");
                    string Service = Console.ReadLine();
                    Console.WriteLine("");
                    
                    if(Service.Length > 1)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write("Adding service, please wait...");
                        if(Logins.RememberService(Service))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine("Service successfully added!");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("Something went wrong, please try again.");
                        }
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("Press any key to continue...");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                    }
                }
                else if (UserChoice == 12)
                {
                    Logins.Menu();
                }
                else if (UserChoice == 13)
                {
                    Console.Write("Your WiFi name (press enter to skip): ");
                    string WifiName = Console.ReadLine();
                    Console.Write("Your WiFi password (press enter to skip): ");
                    string WifiPass = Console.ReadLine();
                    Hardware.GenerateSettingsFile(PrivateKey, PublicKey, WifiName, WifiPass, LocalName);
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("Settings file generated. You can now upload code to your hardware wallet. Press any key to continue...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ReadKey();
                }
                else if (UserChoice == 14)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("Some setting require restart app, before changes take effect.");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Which setting you want to change?");
                    string[] AllSettings = Settings.List();
                    string[] AllSettingsWithValues = new string[AllSettings.Length];
                    for (int i = 0; i < AllSettings.Length; i++)
                    {
                        AllSettingsWithValues[i] = AllSettings[i] + ": " + Settings.Get(AllSettings[i]);
                    }
                    UserChoice = Program.DisplayMenu(new[] { "Back to previous menu" }.Concat(AllSettingsWithValues).ToArray());
                    if (UserChoice > 0)
                    {
                        Console.Write("New value: ");
                        Settings.Set(AllSettings[UserChoice - 1], Console.ReadLine());
                        Console.WriteLine("");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("Changed successfully!");
                        Settings.Save();
                        Console.WriteLine("");
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("Press any key to continue...");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                    }
                }
                else if (UserChoice == 15)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Are you sure to remove this account from this computer? This operation is irreversible!");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("Press (Y)es or (N)o: ");
                    Console.ForegroundColor = ConsoleColor.White;

                    if (Console.ReadKey().Key == ConsoleKey.Y)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("");
                        File.Delete(Settings.WalletsPath + LocalName + ".dat");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("Removed successfully!");
                        Console.WriteLine("");
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("Press any key to continue...");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey();
                        break;
                    }
                }
                Program.WelcomeScreen();
            }
            
            Task.Run(() => Discord.UpdateService());
        }
    }
}
