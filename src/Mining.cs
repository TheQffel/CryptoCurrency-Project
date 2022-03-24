using System;
using System.Collections.Generic;
using System.Net.Sockets;
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
        public static bool MonitorMining = false;
        public static byte ThreadsCount = (byte)Environment.ProcessorCount;

        public static List<Transaction> PendingTransactions = new();
        public static Transaction MinerReward = new();
        public static Thread[] MinerThreads = new Thread[256];
        
        static uint ValidHashes;
        static uint TotalHashes;
        static DateTime LastReset;

        public static string ImageData;
        public static string MessageData;
        public static Block[] ToBeMined;
        
        public static int[] HashrateStats = new int[24];
        public static int[] SolutionsStats = new int[24];
        public static byte CurrentHour = 111;

        public static string[] GetHashrate(bool Reset = false)
        {
            string[] Hashrates = new string[5];
            Hashrates[0] = ValidHashes + " Hashes";
            double PerSecond = TotalHashes / (DateTime.Now - LastReset).TotalSeconds;
            if(Reset)
            {
                LastReset = DateTime.Now;
                TotalHashes = 0;
            }
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
        
        public static void MiningWatchdog()
        {
            uint HashesChange = 0;
            byte Timeout = 0;
            byte NodeSearch = 0;
            
            while (true)
            {
                CurrentHour = (byte)DateTime.Now.Hour;

                if(KeepMining)
                {
                    if(HashesChange != TotalHashes)
                    {
                        HashesChange = TotalHashes;
                        Timeout = 0;
                    }
                    
                    if(Timeout++ > 10)
                    {
                        if(MonitorMining)
                        { 
                            Console.WriteLine("Detected out of sync! Restarting miner...");
                        }
                        
                        KeepMining = false;
                        Thread.Sleep(500);
                        Blockchain.FixCorruptedBlocks();
                        PrepareToMining(Blockchain.GetBlock(Blockchain.CurrentHeight));
                        Thread.Sleep(500);
                        StartOrStop(ThreadsCount);
                        
                        Timeout = 1;
                    }
                    
                    Statistics.Update();
                    Discord.UpdateService();
                }
                
                if(Network.ConnectedNodes(true) < 4)
                {
                    if(NodeSearch++ > 100)
                    {
                        if(MonitorMining)
                        {
                            Console.WriteLine("Lost connection to nodes, reconnecting...");
                        }
                        Network.SearchVerifiedNodes();
                        NodeSearch = 0;
                    }
                }
                
                Thread.Sleep(10000);
            }
        }
        
        public static void PrepareToMining(Block CurrentBlock)
        {
            PauseMining = true;
            Thread.Sleep(100);

            if (Program.DebugLogging) { Console.WriteLine("Preparing to mine: " + (CurrentBlock.BlockHeight + 1)); }
            
            byte NextDifficulty = CurrentBlock.Difficulty;
            ulong TimestampDifferences = 5;;
            bool CanBeChanged = true;

            for (uint i = 0; i < 10; i++)
            {
                if(!Blockchain.BlockExists(CurrentBlock.BlockHeight - (i + 1))) { if(Program.DebugLogging) { Console.WriteLine("Cannot prepare to mine: Missing blocks!"); } return; }
                
                if(Blockchain.GetBlock(CurrentBlock.BlockHeight - i).Difficulty != Blockchain.GetBlock(CurrentBlock.BlockHeight - (i + 1)).Difficulty && i != 9)
                {
                    CanBeChanged = false;
                }
                TimestampDifferences += (Blockchain.GetBlock(CurrentBlock.BlockHeight - i).Timestamp - Blockchain.GetBlock(CurrentBlock.BlockHeight - (i + 1)).Timestamp);
            }
            
            if(CanBeChanged)
            {
                TimestampDifferences /= 10;
                
                if (TimestampDifferences < CurrentBlock.Difficulty) { NextDifficulty++; }
                if (TimestampDifferences > (ulong)CurrentBlock.Difficulty * (ulong)CurrentBlock.Difficulty) { NextDifficulty--; }
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
                    ToBeMined[i].Difficulty = NextDifficulty;
                }
                MinerReward.From = "OneCoin";
                MinerReward.To = MiningAddress;
                MinerReward.Amount = Wallets.MinerRewards[(NewBlockHeight-1)/1000000];
                MinerReward.Fee = 0;
                MinerReward.Timestamp = NewBlockHeight;
                MinerReward.Message = "";
                MinerReward.Signature = NewBlockHeight + "";
                
                if(Pool.PoolAddress.Length > 5)
                {
                    MinerReward.To = Pool.PoolWallet;
                    if(Pool.CustomDifficulty > 200)
                    {
                        Network.Send(Pool.PoolNode, null, new[] { "Pool", "Info", "Get" });
                    }
                }
            }
            UpdateTransactions();
            
            MessageData = Media.GenerateMessage();
            ImageData = Media.GenerateImage();
            
            if(Pool.PoolMessage.Length > 1) { MessageData = Pool.PoolMessage; }
            if(Pool.PoolImage.Length > 1) { ImageData = Pool.PoolImage; }
            
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
                    Dictionary<string, bool> SpecialTransaction = new();
                    List<string> AlreadyAddedTransactions = new();

                    for (int i = 0; i < PendingTransactions.Count; i++)
                    {
                        RemoveThis[i] = true;
                        
                        if (!UserTransactions.ContainsKey(PendingTransactions[i].From))
                        {
                            UserTransactions[PendingTransactions[i].From] = 0;
                            UserBalance[PendingTransactions[i].From] = Wallets.GetBalance(PendingTransactions[i].From);
                            SpecialTransaction[PendingTransactions[i].From] = false;
                        }
                        
                        if(PendingTransactions[i].Timestamp + 750000 > (ulong)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds())
                        {
                            if(!Wallets.CheckTransactionAlreadyIncluded(PendingTransactions[i]))
                            {
                                if(PendingTransactions[i].CheckTransactionCorrect(UserBalance[PendingTransactions[i].From], Blockchain.CurrentHeight))
                                {
                                    if (UserTransactions[PendingTransactions[i].From] < Blockchain.GetBlock(Blockchain.CurrentHeight).Difficulty)
                                    {
                                        if (!AlreadyAddedTransactions.Contains(Transactions[i].Signature))
                                        {
                                            if(PendingTransactions[i].To.Length == 88 && !SpecialTransaction[PendingTransactions[i].From])
                                            {
                                                UserBalance[PendingTransactions[i].From] -= PendingTransactions[i].Amount;
                                                UserTransactions[PendingTransactions[i].From]++;
                                                Transactions.Add(PendingTransactions[i]);
                                            }
                                            if((PendingTransactions[i].Signature.Length == 205 || PendingTransactions[i].To.Length != 88) && UserTransactions[PendingTransactions[i].From] == 0)
                                            {
                                                UserBalance[PendingTransactions[i].From] -= PendingTransactions[i].Amount;
                                                SpecialTransaction[PendingTransactions[i].From] = true;
                                                UserTransactions[PendingTransactions[i].From]++;
                                                Transactions.Add(PendingTransactions[i]);
                                            }
                                        }
                                        AlreadyAddedTransactions.Add(Transactions[i].Signature);
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
                    MinerThreads[i] = new Thread(Mine);
                    MinerThreads[i].Start(i);
                    
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
                
                if(MonitorMining)
                {
                    Console.WriteLine("Thread " + (Index + 1) + " started!");
                }
                
                while (KeepMining)
                {
                    if (PauseMining || Blockchain.BlockExists(ToBeMined[Index].BlockHeight))
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        byte CustomDifficulty = ToBeMined[Index].Difficulty;
                        if(Pool.PoolAddress.Length > 5) { CustomDifficulty = Pool.CustomDifficulty; }
                        StringBuilder RandomData = new StringBuilder(ImageData + "|" + MessageData + "|");
                    
                        for (int i = RandomData.Length; i < 4096; i++)
                        {
                            RandomData.Append(Chars[Random.Next(0, Chars.Length)]);
                        }
                        
                        ToBeMined[Index].Timestamp = (ulong)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                        ToBeMined[Index].ExtraData = RandomData.ToString();
                        ToBeMined[Index].RecalculateHash(false);

                        if (CheckSolution(ToBeMined[Index].CurrentHash, CustomDifficulty))
                        {
                            if (Program.DebugLogging) { Console.WriteLine("Solution found, verifying now."); }
                            lock (ToBeMined[Index]) { SolutionFound(ToBeMined[Index], CustomDifficulty); }
                        }

                        TotalHashes++;
                        HashrateStats[CurrentHour]++;
                    }
                }
                
                if(MonitorMining)
                {
                    Console.WriteLine("Thread " + (Index + 1) + " stopped!");
                }
            }
            catch(Exception Ex)
            {
                Console.WriteLine(Ex);
            }
        }

        static void SolutionFound(Block ToBeChecked, byte CustomDifficulty)
        {
            lock (ToBeChecked)
            {
                if (ToBeChecked.CheckBlockCorrect(-1, CustomDifficulty))
                {
                    if (Program.DebugLogging) { Console.WriteLine("Solution is correct."); }

                    ValidHashes++;
                    SolutionsStats[CurrentHour]++;
                    string[] BlockData = new string[ToBeChecked.Transactions.Length * 7 + 9];

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
                        BlockData[9 + i * 7] = ToBeChecked.Transactions[i].From;
                        BlockData[10 + i * 7] = ToBeChecked.Transactions[i].To;
                        BlockData[11 + i * 7] = ToBeChecked.Transactions[i].Amount.ToString();
                        BlockData[12 + i * 7] = ToBeChecked.Transactions[i].Fee.ToString();
                        BlockData[13 + i * 7] = ToBeChecked.Transactions[i].Timestamp.ToString();
                        BlockData[14 + i * 7] = ToBeChecked.Transactions[i].Message;
                        BlockData[15 + i * 7] = ToBeChecked.Transactions[i].Signature;
                    }
                    
                    if(Pool.PoolAddress.Length > 5)
                    {
                        BlockData[0] = "Pool";
                        BlockData[1] = "Share";
                        BlockData[7] += "-" + MiningAddress;
                        Network.Send(Pool.PoolNode, null, BlockData);
                    }
                    else
                    {
                        Network.Broadcast(BlockData);
                    }

                    if(MonitorMining)
                    {
                        Thread.Sleep(1000);
                        string[] Rates = GetHashrate(true);
                        string[] Memories = GetMemories();
                        string Balance = Wallets.GetBalance(MiningAddress).ToString();
                        Balance = "0." + new string('0', 24 - Balance.Length) + Balance + " ①";
                        Console.WriteLine("Solution found at height: " + ToBeChecked.BlockHeight);
                        Console.WriteLine("Difficulty: " + ToBeChecked.Difficulty + " Time: " + ToBeChecked.Timestamp);
                        Console.WriteLine("Total founded solutions: " + Rates[0]);
                        Console.WriteLine("Your total speed of all threads is:");
                        Console.WriteLine(Rates[1] + " ≈ " + Rates[2] + " ≈ " + Rates[3]);
                        Console.WriteLine("Your total memory consumption is:");
                        Console.WriteLine(Memories[1] + " ≈ " + Memories[2] + " ≈ " + Memories[3]);
                        Console.WriteLine("You are mining to address: " + MiningAddress[..10] + "..." + MiningAddress[^10..]);
                        Console.WriteLine("Current balance of this address: " + Balance);
                        Console.WriteLine("");
                    }
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
            Console.WriteLine("A - Account information.");
            Console.WriteLine("B - Blockchain explorer.");
            Console.WriteLine("C - Show controls and keys.");
            Console.WriteLine("D - Show block difficulty.");
            Console.WriteLine("E - Calculate estimated earnings.");
            Console.WriteLine("F - Fix corrupted blocks.");
            Console.WriteLine("G - Generate blockchain checksum.");
            Console.WriteLine("H - Show mining hashrate.");
            Console.WriteLine("I - Incorrect transaction generator.");
            //Console.WriteLine("J - .");
            //Console.WriteLine("K - .");
            //Console.WriteLine("L - .");
            Console.WriteLine("M - Show memory usage.");
            Console.WriteLine("N - Show connected nodes.");
            //Console.WriteLine("O - .");
            Console.WriteLine("P - Show previously mined blocks.");
            //Console.WriteLine("Q - .");
            //Console.WriteLine("R - .");
            Console.WriteLine("S - Start or stop mining.");
            Console.WriteLine("T - Change threads count.");
            //Console.WriteLine("U - .");
            Console.WriteLine("V - Show node and miner version.");
            //Console.WriteLine("W - .");
            Console.WriteLine("X - Exit to main menu.");
            //Console.WriteLine("Y - .");
            //Console.WriteLine("Z - .");
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
                
                switch (Key)
                {
                    case ConsoleKey.A:
                    {
                        string Balance = Wallets.GetBalance(MiningAddress).ToString();
                        Balance = "0." + new string('0', 24 - Balance.Length) + Balance + " ①";
                        Console.WriteLine("You are mining to address: " + MiningAddress);
                        Console.WriteLine("Current balance of this address: " + Balance);
                        break;
                    }
                    case ConsoleKey.B:
                    {
                        MonitorMining = false;
                        Blockchain.Explore();
                        PrintControls();
                        MonitorMining = true;
                        break;
                    }
                    case ConsoleKey.C:
                    {
                        PrintControls();
                        break;
                    }
                    case ConsoleKey.D:
                    {
                        uint TempHeight = Blockchain.CurrentHeight;
                        Console.WriteLine("Current block difficulty is: " + Blockchain.GetBlock(TempHeight).Difficulty + "/205");
                        if(Pool.CustomDifficulty < 200)
                        {
                            Console.WriteLine("Pool difficulty is: " + Blockchain.GetBlock(TempHeight).Difficulty + "/205");
                        }
                        ulong TimeDiffrence = 0;
                        for (byte j = 0; j < 10; j++)
                        {
                            TimeDiffrence += Blockchain.GetBlock(TempHeight - j).Timestamp - Blockchain.GetBlock(TempHeight - j - 1).Timestamp; 
                        }
                        Console.WriteLine("Average block time (10 blocks): " + TimeDiffrence/10 + " seconds");
                        for (byte j = 10; j < 100; j++)
                        {
                            TimeDiffrence += Blockchain.GetBlock(TempHeight - j).Timestamp - Blockchain.GetBlock(TempHeight - j - 1).Timestamp; 
                        }
                        Console.WriteLine("Average block time (100 blocks): " + TimeDiffrence/100 + " seconds");
                        break;
                    }
                    case ConsoleKey.E:
                    {
                        Console.WriteLine("Coming soon!");
                        break;
                    }
                    case ConsoleKey.F:
                    {
                        Console.WriteLine("Verifying blocks, please wait...");
                        Task.Run(() => Blockchain.FixCorruptedBlocks());
                        break;
                    }
                    case ConsoleKey.G:
                    {
                        uint Limit = Blockchain.CurrentHeight - 1000;
                        if(Limit > Blockchain.CurrentHeight) { Limit = 1; }
                        string Checksum = "OneCoin";
                        
                        for (uint i = Limit; i < Blockchain.CurrentHeight; i++)
                        {
                            if(Blockchain.BlockExists(i))
                            {
                                Block ToCheckSum = Blockchain.GetBlock(i);
                                Checksum = Hashing.TqHash(ToCheckSum.PreviousHash + Checksum + ToCheckSum.CurrentHash);
                            }
                        }
                        
                        Console.WriteLine("You are at height " + Blockchain.CurrentHeight + ", your checksum is: " + Checksum[..5] + Checksum[^5..]);
                        Console.WriteLine("You can generate checksum to check if other nodes have the same blockchain.");
                        Console.WriteLine("Warning: Checksum changes after each block mined or height changed!");
                        break;
                    }
                    case ConsoleKey.H:
                    {
                        string[] Rates = GetHashrate(true);
                        Console.WriteLine("Your total speed of all threads is:");
                        Console.WriteLine(Rates[1] + " ≈ " + Rates[2] + " ≈ " + Rates[3]);
                        break;
                    }
                    case ConsoleKey.I:
                    {
                        MonitorMining = false;
                        Account.IncorrectTransaction();
                        PrintControls();
                        MonitorMining = true;
                        break;
                    }
                    case ConsoleKey.M:
                    {
                        string[] Memories = GetMemories();
                        Console.WriteLine("Your total memory consumption is:");
                        Console.WriteLine(Memories[1] + " ≈ " + Memories[2] + " ≈ " + Memories[3]);
                        break;
                    }
                    case ConsoleKey.N:
                    {
                        Console.WriteLine("You are currently connected to " + Network.ConnectedNodes() + " nodes:");
                        string[] ConnectedNodes = Network.ConnectedNodesAddresses();
                        for (int i = 0; i < ConnectedNodes.Length; i++) {  Console.WriteLine(ConnectedNodes[i]); }
                        break;
                    }
                    case ConsoleKey.P:
                    {
                        string MinedBlocks = "";
                        Console.WriteLine("You mined these blocks (searching only last 1000):");
                        for (uint i = Blockchain.CurrentHeight; i > Blockchain.CurrentHeight -  1111; i--)
                        {
                            if(Blockchain.GetBlock(i).Transactions[0].To == MiningAddress)
                            {
                                MinedBlocks += i + ", ";
                            }
                        }
                        if(MinedBlocks.Length < 1)
                        {
                            MinedBlocks = "Nothing found... ";
                        }
                        Console.WriteLine(MinedBlocks);
                        break;
                    }
                    case ConsoleKey.S:
                    {
                        StartOrStop(ThreadsCount);
                        break;
                    }
                    case ConsoleKey.T:
                    {
                        KeepMining = false;
                        Thread.Sleep(1000);
                        Console.Write("Enter number of threads (type 0 to exit): ");
                        if(ushort.TryParse(Console.ReadLine(), out ushort TempNumber))
                        {
                            if(TempNumber != 0)
                            {
                                if(TempNumber > 255) { TempNumber = 255; }
                                ThreadsCount = (byte)TempNumber;
                            }
                        }
                        StartOrStop(ThreadsCount);
                        break;
                    }
                    case ConsoleKey.V:
                    {
                        Console.WriteLine("You are running OneCoin v. " + Program.RunningVersion);
                        break;
                    }
                    case ConsoleKey.X:
                    {
                        MonitorMining = !MonitorMining;
                        break;
                    }
                }
            }
        }
    }
}
