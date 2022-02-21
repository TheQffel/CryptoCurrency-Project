using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneCoin
{
    class Mining
    {
        public static bool KeepMining = false;
        public static bool PauseMining = false;
        public static string MiningAddress = "";
        public static string PoolAddress = "";
        public static bool MonitorMining = false;

        public static List<Transaction> PendingTransactions = new();
        public static Transaction MinerReward = new();

        static uint ValidHashes;
        static uint TotalHashes;
        static DateTime LastReset;

        public static string ImageData;
        public static string MessageData;
        public static Block[] ToBeMined;
        
        public static int[] HashrateStats = new int[24];
        public static int[] SolutionsStats = new int[24];
        public static byte CurrentHour = 111;

        public static string[] GetHashrate()
        {
            string[] Hashrates = new string[5];
            Hashrates[0] = ValidHashes + " Hashes";
            double PerSecond = TotalHashes / (DateTime.Now - LastReset).TotalSeconds;
            LastReset = DateTime.Now;
            TotalHashes = 0;
            Hashrates[1] = Math.Round(PerSecond, 0) + " H/s";
            PerSecond *= 0.001;
            Hashrates[2] = Math.Round(PerSecond, 1) + " KH/s";
            PerSecond *= 0.001;
            Hashrates[3] = Math.Round(PerSecond, 2) + " MH/s";
            PerSecond *= 0.001;
            Hashrates[4] = Math.Round(PerSecond, 3) + " GH/s";
            return Hashrates;
        }
        
        public static string[] GetMemories()
        {
            string[] Memories = new string[5];
            double Memory = GC.GetTotalMemory(true);
            Memory *= 0.0009765625;
            Memories[0] = Math.Round(Memory, 0) + " KB";
            Memory *= 0.0009765625;
            Memories[1] = Math.Round(Memory, 1) + " MB";
            Memory *= 0.0009765625;
            Memories[2] = Math.Round(Memory, 2) + " GB";
            Memory *= 0.0009765625;
            Memories[3] = Math.Round(Memory, 3) + " TB";
            Memory *= 0.0009765625;
            Memories[4] = Math.Round(Memory, 4) + " PB";
            return Memories;
        }
        
        public static void TimeWatch()
        {
            byte Temp;
            
            while (true)
            {
                Temp = CurrentHour;
                if(Temp != CurrentHour)
                {
                    Statistics.Update();
                    Task.Run(() => Discord.UpdateService());
                }
                CurrentHour = (byte)DateTime.Now.Hour;
                Thread.Sleep(1000);
            }
        }
        public static void PrepareToMining(Block CurrentBlock, byte CustomDifficulty = 0)
        {
            if(CurrentHour > 99)
            {
                Task.Run(() => TimeWatch());
            }
            
            PauseMining = true;
            Thread.Sleep(100);

            if (Program.DebugLogging) { Console.WriteLine("Preparing to mine: " + (CurrentBlock.BlockHeight + 1)); }
            
            byte NextDifficulty = CurrentBlock.Difficulty;
            ulong[] TimestampDifferences = new ulong[10];
            bool CanBeChanged = true;

            for (uint i = 0; i < 10; i++)
            {
                if(Blockchain.GetBlock(CurrentBlock.BlockHeight - i).Difficulty != Blockchain.GetBlock(CurrentBlock.BlockHeight - (i + 1)).Difficulty && i != 9)
                {
                    CanBeChanged = false;
                }
                TimestampDifferences[i] = Blockchain.GetBlock(CurrentBlock.BlockHeight - i).Timestamp - Blockchain.GetBlock(CurrentBlock.BlockHeight - (i + 1)).Timestamp;
            }
            
            if(CanBeChanged)
            {
                if (TimestampDifferences.Max() < CurrentBlock.Difficulty || CurrentBlock.BlockHeight == 0 || CurrentBlock.BlockHeight == 1) { NextDifficulty++; }
                if (TimestampDifferences.Min() > (ulong)CurrentBlock.Difficulty * (ulong)CurrentBlock.Difficulty) { NextDifficulty--; }
            }

            ToBeMined = new Block[256];
            GC.Collect();
            
            lock (ToBeMined)
            {
                uint NewBlockHeight = CurrentBlock.BlockHeight+1;
                for (ushort i = 0; i < 256; i++)
                {
                    ToBeMined[i] = new();
                    ToBeMined[i].BlockHeight = NewBlockHeight;
                    ToBeMined[i].PreviousHash = CurrentBlock.CurrentHash;
                    if (CustomDifficulty > 1) { ToBeMined[i].Difficulty = CustomDifficulty; }
                    else { ToBeMined[i].Difficulty = NextDifficulty; }
                }
                MinerReward.From = "OneCoin";
                MinerReward.To = MiningAddress;
                MinerReward.Amount = Wallets.MinerRewards[(NewBlockHeight-1)/1000000];
                MinerReward.Fee = 0;
                MinerReward.Timestamp = NewBlockHeight;
                MinerReward.Message = "";
                MinerReward.Signature = NewBlockHeight + "";
            }
            UpdateTransactions();
            ImageData = Media.GenerateImage();
            MessageData = Media.GenerateMessage();

            if (Program.DebugLogging) { Console.WriteLine("Block difficulty: " + NextDifficulty); }

            Thread.Sleep(100);
            PauseMining = false;
        }

        public static void UpdateTransactions()
        {
            if(ToBeMined != null)
            {
                lock (ToBeMined)
                {
                    List<Transaction> Transactions = new List<Transaction>();
                    bool[] RemoveThis = new bool[PendingTransactions.Count];
                    Transactions.Add(MinerReward);
                    
                    Dictionary<string, byte> UserTransactions = new();
                    Dictionary<string, BigInteger> UserBalance = new();
                    Dictionary<string, bool> NicknameAvatarChange = new();

                    for (int i = 0; i < PendingTransactions.Count; i++)
                    {
                        RemoveThis[i] = true;
                        
                        if (!UserTransactions.ContainsKey(PendingTransactions[i].From))
                        {
                            UserTransactions[PendingTransactions[i].From] = 0;
                            UserBalance[PendingTransactions[i].From] = Wallets.GetBalance(PendingTransactions[i].From);
                            NicknameAvatarChange[PendingTransactions[i].From] = false;
                        }
                        
                        if(PendingTransactions[i].Timestamp + 750000 > (ulong)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds())
                        {
                            if(!Wallets.CheckTransactionAlreadyIncluded(PendingTransactions[i]))
                            {
                                if(PendingTransactions[i].CheckTransactionCorrect(UserBalance[PendingTransactions[i].From]))
                                {
                                    if (UserTransactions[PendingTransactions[i].From] < Blockchain.GetBlock(Blockchain.CurrentHeight).Difficulty)
                                    {
                                        if(PendingTransactions[i].To.Length == 88 && !NicknameAvatarChange[PendingTransactions[i].From])
                                        {
                                            UserBalance[PendingTransactions[i].From] -= PendingTransactions[i].Amount;
                                            UserTransactions[PendingTransactions[i].From]++;
                                            Transactions.Add(PendingTransactions[i]);
                                        }
                                        if(PendingTransactions[i].To.Length != 88 && UserTransactions[PendingTransactions[i].From] == 0)
                                        {
                                            UserBalance[PendingTransactions[i].From] -= PendingTransactions[i].Amount;
                                            NicknameAvatarChange[PendingTransactions[i].From] = true;
                                            UserTransactions[PendingTransactions[i].From]++;
                                            Transactions.Add(PendingTransactions[i]);
                                        }
                                    }
                                    RemoveThis[i] = false;
                                }
                            }
                        }
                    }
                    
                    for (int i = 0; i < 256; i++)
                    {
                        ToBeMined[i].Transactions = Transactions.ToArray();
                        ToBeMined[i].RecalculateHash();
                    }
                    
                    for (int i = PendingTransactions.Count-1; i >= 0; i--)
                    {
                        if(RemoveThis[i])
                        {
                            PendingTransactions.RemoveAt(i);
                        }
                    }
                }
            }
        }

        public static void StartOrStop(byte Threads)
        {
            KeepMining = !KeepMining;
            
            if(KeepMining)
            {
                PrepareToMining(Blockchain.GetBlock(Blockchain.CurrentHeight));
                
                LastReset = DateTime.Now;
                TotalHashes = 0;
                ValidHashes = 0;

                for (byte i = 0; i < Threads; i++)
                {
                    Thread NewThread = new Thread(Mine);
                    NewThread.Start(i);
                    Thread.Sleep(100);
                }
            }
        }

        static void Mine(object Obj)
        {
            try
            {
                byte Index = (byte) Obj;
                
                Random Random = new();
                string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789 abcdefghijklmnopqrstuvwxyz";
                
                while (KeepMining)
                {
                    if (PauseMining || Blockchain.Synchronising || Blockchain.BlockExists(ToBeMined[Index].BlockHeight))
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        StringBuilder RandomData = new StringBuilder(ImageData + "|" + MessageData + "|");
                    
                        for (int i = RandomData.Length; i < 4096; i++)
                        {
                            RandomData.Append(Chars[Random.Next(0, Chars.Length)]);
                        }
                        
                        ToBeMined[Index].Timestamp = (ulong)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                        ToBeMined[Index].ExtraData = RandomData.ToString();
                        ToBeMined[Index].RecalculateHash(false);

                        if (CheckSolution(ToBeMined[Index].CurrentHash, ToBeMined[Index].Difficulty))
                        {
                            if (Program.DebugLogging) { Console.WriteLine("Solution found, verifying now."); }
                            lock (ToBeMined) { SolutionFound(ToBeMined[Index]); }
                        }

                        TotalHashes++;
                        HashrateStats[CurrentHour]++;
                    }
                }
            }
            catch(Exception Ex)
            {
                Console.WriteLine(Ex);
            }
        }

        static void SolutionFound(Block ToBeChecked)
        {
            lock (ToBeChecked)
            {
                if (ToBeChecked.CheckBlockCorrect())
                {
                    if (Program.DebugLogging) { Console.WriteLine("Solution is correct."); }

                    ValidHashes++;
                    SolutionsStats[CurrentHour]++;
                    string[] BlockData = new string[ToBeChecked.Transactions.Length * 6 + 9];

                    BlockData[0] = "Block";
                    BlockData[1] = "Set";
                    BlockData[2] = ToBeChecked.BlockHeight.ToString();
                    BlockData[3] = ToBeChecked.PreviousHash;
                    BlockData[4] = ToBeChecked.CurrentHash;
                    BlockData[5] = ToBeChecked.Timestamp.ToString();
                    BlockData[6] = ToBeChecked.Difficulty.ToString();
                    BlockData[7] = ToBeChecked.ExtraData;
                    BlockData[8] = ToBeChecked.Transactions.Length.ToString();

                    for (int i = 0; i < ToBeChecked.Transactions.Length; i++)
                    {
                        BlockData[9 + i * 6] = ToBeChecked.Transactions[i].From;
                        BlockData[10 + i * 6] = ToBeChecked.Transactions[i].To;
                        BlockData[11 + i * 6] = ToBeChecked.Transactions[i].Amount.ToString();
                        BlockData[12 + i * 6] = ToBeChecked.Transactions[i].Fee.ToString();
                        BlockData[13 + i * 6] = ToBeChecked.Transactions[i].Timestamp.ToString();
                        BlockData[14 + i * 6] = ToBeChecked.Transactions[i].Signature;
                    }
                    
                    Network.Broadcast(BlockData);
                    Network.Send(null, Network.AddressToClient(PoolAddress), BlockData);

                    if(MonitorMining)
                    {
                        Thread.Sleep(1000);
                        string[] Rates = GetHashrate();
                        string[] Memories = GetMemories();
                        string Balance = Wallets.GetBalance(MiningAddress).ToString();
                        Balance = "0." + new string('0', 24 - Balance.Length) + Balance + " ①";
                        Console.WriteLine("Solution found at height: " + ToBeChecked.BlockHeight);
                        Console.WriteLine("Difficulty: " + ToBeChecked.Difficulty + " Time: " + ToBeChecked.Timestamp);
                        Console.WriteLine("Total founded solutions: " + Rates[0]);
                        Console.WriteLine("Your total speed of all threads is:");
                        Console.WriteLine(Rates[1] + " ≈ " + Rates[2] + " ≈ " + Rates[3]);
                        Console.WriteLine("Your total memory consumption is:");
                        Console.WriteLine(Memories[0] + " ≈ " + Memories[1] + " ≈ " + Memories[2] + " ≈ " + Memories[3]);
                        Console.WriteLine("You are mining to address: " + MiningAddress[..10] + "..." + MiningAddress[^10..]);
                        Console.WriteLine("Current balance of this address: " + Balance);
                        Console.WriteLine("");
                    }
                    
                    Statistics.Update();
                    Task.Run(() => Discord.UpdateService());
                }
                else
                {
                    if (Program.DebugLogging) { Console.WriteLine("Solution is incorrect."); }
                }
            }
        }

        public static bool CheckSolution(string Hash, byte Difficulty)
        {
            int[] Counts = new int[32];
            int Counter = 0;

            for (int i = 0; i < 205; i++)
            {
                switch (Hash[i])
                {
                    case '0': Counts[0]++; break;
                    case '1': Counts[1]++; break;
                    case '2': Counts[2]++; break;
                    case '3': Counts[3]++; break;
                    case '4': Counts[4]++; break;
                    case '5': Counts[5]++; break;
                    case '6': Counts[6]++; break;
                    case '7': Counts[7]++; break;
                    case '8': Counts[8]++; break;
                    case '9': Counts[9]++; break;
                    case 'A': Counts[10]++; break;
                    case 'B': Counts[11]++; break;
                    case 'C': Counts[12]++; break;
                    case 'D': Counts[13]++; break;
                    case 'E': Counts[14]++; break;
                    case 'F': Counts[15]++; break;
                    case 'G': Counts[16]++; break;
                    case 'H': Counts[17]++; break;
                    case 'I': Counts[18]++; break;
                    case 'J': Counts[19]++; break;
                    case 'K': Counts[20]++; break;
                    case 'L': Counts[21]++; break;
                    case 'M': Counts[22]++; break;
                    case 'N': Counts[23]++; break;
                    case 'O': Counts[24]++; break;
                    case 'P': Counts[25]++; break;
                    case 'R': Counts[26]++; break;
                    case 'S': Counts[27]++; break;
                    case 'T': Counts[28]++; break;
                    case 'U': Counts[29]++; break;
                    case 'W': Counts[30]++; break;
                    case 'Z': Counts[31]++; break;
                }
            }
            for (int i = 0; i < 32; i++)
            {
                if (Counts[i] > Counter)
                {
                    Counter = Counts[i];
                }
            }
            return Counter >= Difficulty;
        }
        
        public static void PrintControls()
        {
            Console.WriteLine("You are now in mining mode, use keys to control mining:");
            Console.WriteLine("K - Show keys and controls.");
            Console.WriteLine("A - Account information.");
            Console.WriteLine("S - Start or stop mining.");
            Console.WriteLine("H - Show hashrate.");
            Console.WriteLine("D - Show block difficulty.");
            Console.WriteLine("M - Show memory usage.");
            Console.WriteLine("T - Show threads.");
            Console.WriteLine("B - Blockchain explorer.");
            Console.WriteLine("X - Exit to main menu.");
        }
        
        public static void Menu()
        {
            MonitorMining = true;
            PrintControls();
            
            while (MonitorMining)
            {
                Console.Write("You are currently");
                if(!KeepMining) { Console.Write(" not"); }
                Console.WriteLine(" mining!");
                
                ConsoleKey Key = Console.ReadKey().Key;
                Console.CursorLeft = 0;
                
                if(Key == ConsoleKey.K)
                {
                    PrintControls();
                }
                if(Key == ConsoleKey.A)
                {
                    string Balance = Wallets.GetBalance(MiningAddress).ToString();
                    Balance = "0." + new string('0', 24 - Balance.Length) + Balance + " ①";
                    Console.WriteLine("You are mining to address: " + MiningAddress);
                    Console.WriteLine("Current balance of this address: " + Balance);
                }
                if(Key == ConsoleKey.S)
                {
                    StartOrStop((byte)Environment.ProcessorCount);
                }
                if(Key == ConsoleKey.H)
                {
                    string[] Rates = GetHashrate();
                    Console.WriteLine("Your total speed of all threads is:");
                    Console.WriteLine(Rates[1] + " ≈ " + Rates[2] + " ≈ " + Rates[3]);
                }
                if(Key == ConsoleKey.D)
                {
                    Console.WriteLine("Current block difficulty is: " + ToBeMined[234].Difficulty);
                }
                if(Key == ConsoleKey.M)
                {
                    string[] Memories = GetMemories();
                    Console.WriteLine("Your total memory consumption is:");
                    Console.WriteLine(Memories[0] + " ≈ " + Memories[1] + " ≈ " + Memories[2] + " ≈ " + Memories[3]);
                }
                if(Key == ConsoleKey.T)
                {
                    Console.WriteLine("You are mining on " + Environment.ProcessorCount + " threads.");
                }
                if(Key == ConsoleKey.B)
                {
                    MonitorMining = false;
                    Blockchain.Explore();
                    PrintControls();
                    MonitorMining = true;
                }
                if(Key == ConsoleKey.X)
                {
                    MonitorMining = !MonitorMining;
                }
            }
        }
    }
}
