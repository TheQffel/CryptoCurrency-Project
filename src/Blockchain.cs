using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace OneCoin
{
    class Blockchain
    {
        public static uint CurrentHeight = 1;
        public static bool Synchronising = true;
        
        static Dictionary<uint, Block> BlocksInMemory = new();
        static readonly object FileSystemRead = new();
        static readonly object FileSystemWrite = new();

        public static Dictionary<string, BigInteger> BalanceCache = new();
        public static Dictionary<string, string> NicknameCache = new();
        public static Dictionary<string, string> AvatarsCache = new();
        public static Dictionary<string, uint> NameSetCache = new();
        public static Dictionary<string, uint> LastUseCache = new();

        public static uint CacheHeight = 0;
        public static bool CacheUpdateInProgress = false;
        public static string CacheHash = GetBlock(0).CurrentHash;
        
        public static string DatabaseHost = "";
        public static string DatabaseUser = "";
        public static string DatabasePass = "";
        public static string DatabaseDb = "";
        
        public static string TempAvatarsPath = "";
        public static string TempQrCodesPath = "";
        public static string TempBlocksPath = "";
        
        public static List<string> AllKnownAddresses = new();

        public static void UpdateCache()
        {
            if(!CacheUpdateInProgress)
            {
                CacheUpdateInProgress = true;
                
                while (CacheHeight < CurrentHeight)
                {
                    CacheHeight++;
                    
                    Block OneCoinBlock = GetBlock(CacheHeight);
                    string ThisBlockMiner = OneCoinBlock.Transactions[0].To;
                    CheckIfExistsInCache(ThisBlockMiner, CacheHeight);
                    BalanceCache[ThisBlockMiner] += OneCoinBlock.Transactions[0].Amount;
                    LastUseCache[ThisBlockMiner] = CacheHeight;
                    CacheHash = Hashing.TqHash(CacheHash + " | " + OneCoinBlock.CurrentHash);

                    for (int i = 1; i < OneCoinBlock.Transactions.Length; i++)
                    {
                        CheckIfExistsInCache(OneCoinBlock.Transactions[i].From, CacheHeight);
                        
                        if(OneCoinBlock.Transactions[i].To.Length < 77)
                        {
                            BalanceCache[OneCoinBlock.Transactions[i].From] -= OneCoinBlock.Transactions[i].Amount;
                            BalanceCache[ThisBlockMiner] += OneCoinBlock.Transactions[i].Amount;
                            LastUseCache[OneCoinBlock.Transactions[i].From] = CacheHeight;
                            NicknameCache[OneCoinBlock.Transactions[i].From] = OneCoinBlock.Transactions[i].To;
                            NameSetCache[OneCoinBlock.Transactions[i].From] = CacheHeight;
                        }
                        if(OneCoinBlock.Transactions[i].To.Length == 88)
                        {
                            CheckIfExistsInCache(OneCoinBlock.Transactions[i].To, CacheHeight);
                            
                            BalanceCache[OneCoinBlock.Transactions[i].From] -= OneCoinBlock.Transactions[i].Amount;
                            BalanceCache[OneCoinBlock.Transactions[i].To] += OneCoinBlock.Transactions[i].Amount;
                            LastUseCache[OneCoinBlock.Transactions[i].From] = CacheHeight;
                        }
                        if(OneCoinBlock.Transactions[i].To.Length > 99)
                        {
                            BalanceCache[OneCoinBlock.Transactions[i].From] -= OneCoinBlock.Transactions[i].Amount;
                            BalanceCache[ThisBlockMiner] += OneCoinBlock.Transactions[i].Amount;
                            LastUseCache[OneCoinBlock.Transactions[i].From] = CacheHeight;
                            AvatarsCache[OneCoinBlock.Transactions[i].From] = OneCoinBlock.Transactions[i].To;
                        }
                    }
                    
                    if(Program.DebugLogging)
                    {
                        if(CacheHeight % 1000000 == 0)
                        {
                            Console.WriteLine("Cache at height " + CacheHeight + "...");
                        }
                    }
                }
                
                CacheUpdateInProgress = false;
            }
        }
        
        public static void DatabaseSync()
        {
            Database.Connect(DatabaseHost, DatabaseUser, DatabasePass, DatabaseDb);
            Database.SpecialCommand("SET @@sql_mode = '';"); // To prevent error on empty strings.
            
            string Tmp = Database.Get("blocks", "BlockHeight", "", "BlockHeight DESC", 1)[0][0];
            if(Tmp.Length < 1)
            {
                Tmp = "0";
                Database.Add("blocks", "BlockHeight, PreviousHash, CurrentHash", "'0', '1ONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOIN0', '1ONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOIN0'");
            }
            uint LatestStoredBlock = uint.Parse(Tmp);
            
            while(DatabaseHost.Length > 1)
            {
                for(int i = 0; i < AllKnownAddresses.Count; i++)
                {
                    string[] Temp = (Wallets.GetName(AllKnownAddresses[i]) + "|1").Split("|");
                    Temp = new [] { Wallets.GetBalance(AllKnownAddresses[i]).ToString(), Temp[0], Temp[1], Wallets.GetAvatar(AllKnownAddresses[i]) };
                    
                    if(Database.Set("onecoin", "Balance='" + Temp[0] + "', Nickname='" + Temp[1] + "', Tag='" + Temp[2] + "', Avatar='" + Temp[3] + "'", "Address='" + AllKnownAddresses[i] + "'") < 1)
                    {
                        Database.Add("onecoin", "Address, Balance, Nickname, Tag, Avatar", "'" + AllKnownAddresses[i] + "', '" + Temp[0] + "', '" + Temp[1] + "', '" + Temp[2] + "', '" + Temp[3] + "'");
                    }
                    
                    if(TempAvatarsPath.Length > 1 && Temp[3].Length > 9)
                    {
                        Media.TextToImage(Temp[3]).Save(TempAvatarsPath + "/" + AllKnownAddresses[i] + ".png", ImageFormat.Png);
                    }
                    if(TempQrCodesPath.Length > 1)
                    {
                        Bitmap QrCode = Wallets.GenerateQrCode(AllKnownAddresses[i]);
                        new Bitmap(QrCode, QrCode.Width*4, QrCode.Height*4).Save(TempQrCodesPath + "/" + AllKnownAddresses[i] + ".png", ImageFormat.Png);
                    }
                    
                    Thread.Sleep(10000);
                }

                for (uint i = LatestStoredBlock + 1; i < CurrentHeight; i++)
                {
                    Block ToInsert = GetBlock(i);
                    
                    string Hash = Database.Get("blocks", "CurrentHash", "BlockHeight='" + (i-1) + "'")[0][0];
                    
                    if(Hash == ToInsert.PreviousHash )
                    {
                        Database.Add("blocks", "BlockHeight, PreviousHash, CurrentHash, Timestamp, Difficulty, Message, ExtraData", "'" + ToInsert.BlockHeight + "', '" + ToInsert.PreviousHash + "', '" + ToInsert.CurrentHash + "', '" + ToInsert.Timestamp + "', '" + ToInsert.Difficulty + "', '" + ToInsert.ExtraData.Split('|')[1] + "', '" + ToInsert.ExtraData.Split('|')[2] + "'");
                        
                        if(TempBlocksPath.Length > 1)
                        {
                            Media.TextToImage(ToInsert.ExtraData.Split('|')[0]).Save(TempBlocksPath + "/" + ToInsert.BlockHeight + ".png", ImageFormat.Png);
                        }

                        for (int j = 0; j < ToInsert.Transactions.Length; j++)
                        {
                            Thread.Sleep(1000);
                        
                            Database.Add("transactions", "BlockHeight, AddressFrom, AddressTo, Amount, Fee, Timestamp, Message, Signature", "'" + ToInsert.BlockHeight + "', '" + ToInsert.Transactions[j].From + "', '" + ToInsert.Transactions[j].To + "', '" + ToInsert.Transactions[j].Amount + "', '" + ToInsert.Transactions[j].Fee + "', '" + ToInsert.Transactions[j].Signature + "', '" + ToInsert.Transactions[j].Message + "', '" + ToInsert.Transactions[j].Signature + "'");
                        }
                    }
                    else
                    {
                        Database.Del("blocks", "BlockHeight='" + (i-1) + "'");
                        Database.Del("transactions", "BlockHeight='" + (i-1) + "'");
                        
                        i -= 2;
                    }
                    
                    Thread.Sleep(10000);
                }
            }
            Database.Disconnect();
        }
        
        public static bool VerifyCache()
        {
            string CalculatedCacheHash = GetBlock(0).CurrentHash;
                
            for (uint i = 1; i < CacheHeight; i++)
            {
                CacheHash = Hashing.TqHash(CacheHash + " | " + GetBlock(i).CurrentHash);
            }
            
            if(Program.DebugLogging)
            {
                Console.WriteLine("Invalid hash detected!");
            }
            
            return CalculatedCacheHash == CacheHash;
        }
        
        public static void ClearCache()
        {
            while(CacheUpdateInProgress)
            {
                Thread.Sleep(100);
            }
            BalanceCache = new();
            NicknameCache = new();
            AvatarsCache = new();
            NameSetCache = new();
            LastUseCache = new();
            CacheHeight = 0;
        }
        
        public static void CheckIfExistsInCache(string Address, uint Height)
        {
            if(!BalanceCache.ContainsKey(Address))
            {
                BalanceCache[Address] = 0;
                NicknameCache[Address] = "";
                AvatarsCache[Address] = "";
                NameSetCache[Address] = 0;
                LastUseCache[Address] = Height;
            }
            if(DatabaseHost.Length > 1)
            {
                if(!AllKnownAddresses.Contains(Address))
                {
                    AllKnownAddresses.Add(Address);
                }
            }
        }
        
        public static bool BlockExists(uint Height)
        {
            return BlocksInMemory.ContainsKey(Height) || File.Exists(Settings.BlockchainPath + Height + ".dat");
        }

        public static void SyncBlocks(bool Force = false)
        {
            if(!Synchronising || Force)
            {
                Synchronising = true;

                CurrentHeight = 0;

                while (CurrentHeight < 1)
                {
                    Network.Send(Network.RandomClient(), null, new[] { "Block", "Height", CurrentHeight.ToString() });
                    Thread.Sleep(100);
                }
                for (uint i = LastBlockExists() + 1; i < CurrentHeight; i++)
                {
                    while (!BlockExists(i))
                    {
                        Network.Send(Network.RandomClient(), null, new[] { "Block", "Get", i.ToString() });
                        Thread.Sleep(100);
                    }
                    if(Program.DebugLogging)
                    {
                        if(CurrentHeight % 1000 == 0)
                        {
                            Console.WriteLine("Current progress: " + i + " / " + CurrentHeight);
                        }
                    }
                }
                Task.Run(() => UpdateCache());

                Mining.PrepareToMining(GetBlock(CurrentHeight));

                Synchronising = false;
            }
        }

        public static uint LastBlockExists()
        {
            uint Result = 1073741824;
            uint Change = 1073741824;

            for (int i = 0; i < 32; i++)
            {
                Change /= 2;

                if(File.Exists(Settings.BlockchainPath + Result + ".dat"))
                {
                    Result += Change;
                }
                else
                {
                    Result -= Change;
                }
            }
            
            Result++;
            while(!File.Exists(Settings.BlockchainPath + Result + ".dat") && Result > 0)
            {
                Result--;
            }
            return Result;
        }

        public static Block GetBlock(uint Height)
        {
            if (!BlocksInMemory.ContainsKey(Height))
            {
                BlocksInMemory[Height] = LoadBlock(Height);
            }
            return BlocksInMemory[Height];
        }

        public static void SetBlock(Block Block)
        {
            if (!BlocksInMemory.ContainsKey(Block.BlockHeight))
            {
                BlocksInMemory[Block.BlockHeight] = Block;
            }
            SaveBlock(Block);
            
            Task.Run(() => UpdateCache());
        }

        public static void DelBlock(uint Height)
        {
            if (BlocksInMemory.ContainsKey(Height))
            {
                BlocksInMemory.Remove(Height);
            }
            if (File.Exists(Settings.BlockchainPath + Height + ".dat"))
            {
                File.Delete(Settings.BlockchainPath + Height + ".dat");
            }
            if(File.Exists(Settings.TransactionsPath + Height + ".dat"))
            {
                File.Delete(Settings.TransactionsPath + Height + ".dat");
            }
            
            Task.Run(() => ClearCache());
        }

        static Block LoadBlock(uint Height)
        {
            Block OneBlock = new();
            OneBlock.BlockHeight = Height;
            
            if(Height < 1)
            {
                OneBlock.CurrentHash = "1ONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOIN0";
                return OneBlock;
            }

            if (File.Exists(Settings.BlockchainPath + Height + ".dat") && File.Exists(Settings.TransactionsPath + Height + ".dat"))
            {
                lock (FileSystemRead)
                {
                    using BinaryReader BlockFile = new(File.Open(Settings.BlockchainPath + Height + ".dat", FileMode.Open));
                    using BinaryReader TransactionsFile = new(File.Open(Settings.TransactionsPath + Height + ".dat", FileMode.Open));

                    OneBlock.PreviousHash = BlockFile.ReadString();
                    OneBlock.CurrentHash = BlockFile.ReadString();
                    OneBlock.Timestamp = BlockFile.ReadUInt64();
                    OneBlock.Difficulty = BlockFile.ReadByte();

                    OneBlock.ExtraData = Convert.ToBase64String(BlockFile.ReadBytes(3072)).Replace('+', ' ').Replace('/', '|');

                    OneBlock.Transactions = new Transaction[BlockFile.ReadUInt16()];

                    for (int i = 0; i < OneBlock.Transactions.Length; i++)
                    {
                        OneBlock.Transactions[i] = new();
                        OneBlock.Transactions[i].From = TransactionsFile.ReadString();
                        OneBlock.Transactions[i].To = TransactionsFile.ReadString();
                        OneBlock.Transactions[i].Amount = BytesToBigInt(TransactionsFile.ReadBytes(12));
                        OneBlock.Transactions[i].Fee = TransactionsFile.ReadUInt64();
                        OneBlock.Transactions[i].Timestamp = TransactionsFile.ReadUInt64();
                        OneBlock.Transactions[i].Message = TransactionsFile.ReadString();
                        OneBlock.Transactions[i].Signature = TransactionsFile.ReadString();
                    }
                    
                    BlockFile.Close();
                    TransactionsFile.Close();
                }
            }
            
            if(OneBlock.Transactions.Length < 1 && Height > 0)
            {
                for (int i = 0; i < 100; i++)
                {
                    Network.Send(Network.RandomClient(), null, new[] { "Block", "Get", Height.ToString() });
                    Thread.Sleep(100);

                    if (BlockExists(Height))
                    {
                        return LoadBlock(Height);
                    }
                }
            }
            return OneBlock;
        }

        static void SaveBlock(Block OneBlock)
        {
            lock (FileSystemWrite)
            {
                using BinaryWriter BlockFile = new(File.Open(Settings.BlockchainPath + OneBlock.BlockHeight + ".dat", FileMode.Create));
                using BinaryWriter TransactionsFile = new(File.Open(Settings.TransactionsPath + OneBlock.BlockHeight + ".dat", FileMode.Create));

                BlockFile.Write(OneBlock.PreviousHash);
                BlockFile.Write(OneBlock.CurrentHash);
                BlockFile.Write(OneBlock.Timestamp);
                BlockFile.Write(OneBlock.Difficulty);

                BlockFile.Write(Convert.FromBase64String(OneBlock.ExtraData.Replace(' ', '+').Replace('|', '/')));

                BlockFile.Write((ushort)OneBlock.Transactions.Length);

                for (long i = BlockFile.BaseStream.Length; i < 4096; i++) { BlockFile.Write((byte)i); } // Fixed Size: 4KB (4096 BYTES)

                for (int i = 0; i < OneBlock.Transactions.Length; i++)
                {
                    if(OneBlock.Transactions[i].Message == null) { OneBlock.Transactions[i].Message = ""; }
                    
                    TransactionsFile.Write(OneBlock.Transactions[i].From);
                    TransactionsFile.Write(OneBlock.Transactions[i].To);
                    TransactionsFile.Write(BigIntToBytes(OneBlock.Transactions[i].Amount));
                    TransactionsFile.Write(OneBlock.Transactions[i].Fee);
                    TransactionsFile.Write(OneBlock.Transactions[i].Timestamp);
                    TransactionsFile.Write(OneBlock.Transactions[i].Message);
                    TransactionsFile.Write(OneBlock.Transactions[i].Signature);
                }
                for (long j = TransactionsFile.BaseStream.Length; j%1024 != 0; j++) { TransactionsFile.Write((byte)j); } // Rounded Size: 1KB (1024 BYTES)
                
                BlockFile.Close();
                TransactionsFile.Close();
            }
            if(OneBlock.BlockHeight > CurrentHeight)
            {
                CurrentHeight = OneBlock.BlockHeight;
            }
        }

        static byte[] BigIntToBytes(BigInteger Number)
        {
            byte[] Result = new byte[12];
            byte[] Bytes = Number.ToByteArray();
            for (int i = 0; i < 12; i++)
            {
                if(i < Bytes.Length) { Result[11 - i] = Bytes[i]; }
                else { Result[11 - i] = 0; }
            }
            return Result;
        }
        
        static BigInteger BytesToBigInt(byte[] Number)
        {
            byte[] Result = new byte[12];
            for (int i = 0; i < 12; i++)
            {
                if(i < Number.Length) { Result[11 - i] = Number[i]; }
                else { Result[11 - i] = 0; }
            }
            return new(Result);
        }
        
        public static void Explore()
        {
            while(true)
            {
                Console.Write("Enter block number (type 0 to exit): ");
                uint NewHeight = uint.Parse(Console.ReadLine());
                if(NewHeight == 0)
                {
                    break;
                }
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine(GetBlock(NewHeight));
                    Console.WriteLine("Use arrows keys to explore blockchain.");
                    Console.WriteLine("Press escape key to return.");
                    ConsoleKey Key = Console.ReadKey().Key;
                    
                    if(Key == ConsoleKey.DownArrow || Key == ConsoleKey.LeftArrow)
                    {
                        if(NewHeight > 1)
                        {
                            NewHeight--;
                        }
                    }
                    if(Key == ConsoleKey.UpArrow || Key == ConsoleKey.RightArrow)
                    {
                        if(NewHeight < CurrentHeight)
                        {
                            NewHeight++;
                        }
                    }
                    if(Key == ConsoleKey.PageDown)
                    {
                        if(NewHeight > 1000)
                        {
                            NewHeight -= 1000;
                        }
                    }
                    if(Key == ConsoleKey.PageUp)
                    {
                        if(NewHeight < CurrentHeight - 1000)
                        {
                            NewHeight += 1000;
                        }
                    }
                    if(Key == ConsoleKey.Home)
                    {
                        NewHeight = 1;
                    }
                    if(Key == ConsoleKey.End)
                    {
                        NewHeight = CurrentHeight;
                    }
                    if(Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                }
                Console.WriteLine(" ");
            }
        }
    }
}
