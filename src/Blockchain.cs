using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace OneCoin
{
    class Blockchain
    {
        public static uint CurrentHeight = 1;
        public static bool SyncMode = true;
        public static bool TryAgain = false;
        public static short SyncSpeed = 100;
        
        static Dictionary<uint, Block> BlocksInMemory = new();
        static readonly object FileSystemRead = new();
        static readonly object FileSystemWrite = new();
        static readonly object WholeBlockchain = new();

        public static string DatabaseHost = "";
        public static string DatabaseUser = "";
        public static string DatabasePass = "";
        public static string DatabaseDb = "";
        
        public static string TempAvatarsPath = "";
        public static string TempQrCodesPath = "";
        public static string TempBlocksPath = "";
        
        public static List<string> AllKnownAddresses = new();
        
        public static Dictionary<long, Dictionary<uint, Block>> NodesChain = new();
        public static Dictionary<long, bool> NodesLock = new();
        public static Dictionary<long, uint> NodesMin = new();
        public static Dictionary<long, uint> NodesMax = new();

        public static void DatabaseSync()
        {
            try
            { 
                Database.Connect(DatabaseHost, DatabaseUser, DatabasePass, DatabaseDb);
                Database.SpecialCommand("SET @@sql_mode = '';"); // To prevent error on empty strings.
                
                string Tmp = Database.Get("blocks", "BlockHeight", "", "BlockHeight DESC", 1)[0][0];
                if(Tmp.Length < 1)
                {
                    Tmp = "0";
                    Database.Add("blocks", "BlockHeight, PreviousHash, CurrentHash", "'0', '1ONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOIN0', '1ONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOIN0'");
                }
                uint DbHeight = uint.Parse(Tmp) + 1;

                while(SyncMode)
                {
                    Thread.Sleep(1000);
                }
                
                for (uint i = 0; i < CurrentHeight; i++)
                {
                    Block ToInsert = GetBlock(i);
                    
                    for (int j = 0; j < ToInsert.Transactions.Length; j++)
                    {      
                        if(ToInsert.Transactions[j].To.Length == 88)
                        {
                            if(!AllKnownAddresses.Contains(ToInsert.Transactions[j].To))
                            {
                                AllKnownAddresses.Add(ToInsert.Transactions[j].To);
                            }
                        }
                    }
                }
                
                while(DatabaseHost.Length > 1)
                {
                    for(int i = 0; i < AllKnownAddresses.Count; i++)
                    {
                        if(Program.DebugLogging)
                        {
                            Console.WriteLine("Updating user: " + AllKnownAddresses[i]);
                        }
                        
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
                        
                        Thread.Sleep(1000);
                    }
                    
                    AllKnownAddresses.Clear();

                    for (; DbHeight < CurrentHeight; DbHeight++)
                    {
                        Block ToInsert = GetBlock(DbHeight);
                        
                        string Hash = Database.Get("blocks", "CurrentHash", "BlockHeight='" + (DbHeight - 1) + "'")[0][0];
                        
                        if(Hash == ToInsert.PreviousHash)
                        {
                            if(Program.DebugLogging)
                            {
                                Console.WriteLine("Adding block and transactions to database: " + DbHeight);
                            }
                            
                            Database.Add("blocks", "BlockHeight, PreviousHash, CurrentHash, Timestamp, Difficulty, Message, ExtraData", "'" + ToInsert.BlockHeight + "', '" + ToInsert.PreviousHash + "', '" + ToInsert.CurrentHash + "', '" + ToInsert.Timestamp + "', '" + ToInsert.Difficulty + "', '" + ToInsert.ExtraData.Split('|')[1] + "', '" + ToInsert.ExtraData.Split('|')[2] + "'");
                            
                            if(TempBlocksPath.Length > 1)
                            {
                                Media.TextToImage(ToInsert.ExtraData.Split('|')[0]).Save(TempBlocksPath + "/" + ToInsert.BlockHeight + ".png", ImageFormat.Png);
                            }

                            for (int j = 0; j < ToInsert.Transactions.Length; j++)
                            {
                                Thread.Sleep(100);
                            
                                Database.Add("transactions", "BlockHeight, AddressFrom, AddressTo, Amount, Fee, Timestamp, Message, Signature", "'" + ToInsert.BlockHeight + "', '" + ToInsert.Transactions[j].From + "', '" + ToInsert.Transactions[j].To + "', '" + ToInsert.Transactions[j].Amount + "', '" + ToInsert.Transactions[j].Fee + "', '" + ToInsert.Transactions[j].Signature + "', '" + ToInsert.Transactions[j].Message + "', '" + ToInsert.Transactions[j].Signature + "'");
                                
                                if(ToInsert.Transactions[j].From.Length == 88)
                                {
                                    if(!AllKnownAddresses.Contains(ToInsert.Transactions[j].From))
                                    {
                                        AllKnownAddresses.Add(ToInsert.Transactions[j].From);
                                    }
                                }
                                
                                if(ToInsert.Transactions[j].To.Length == 88)
                                {
                                    if(!AllKnownAddresses.Contains(ToInsert.Transactions[j].To))
                                    {
                                        AllKnownAddresses.Add(ToInsert.Transactions[j].To);
                                    }
                                }
                            }
                        }
                        else
                        {
                            DbHeight--;
                            
                            if(Program.DebugLogging)
                            {
                                Console.WriteLine("Removing previous block with wrong hash: " + DbHeight);
                            }
                            
                            Database.Del("blocks", "BlockHeight='" + DbHeight + "'");
                            Database.Del("transactions", "BlockHeight='" + DbHeight + "'");
                            
                            DbHeight--;
                        }
                        
                        Thread.Sleep(1000);
                    }
                }
                
                Database.Disconnect();
                
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex);
            }
        }
        
        public static void FixCorruptedBlocks()
        {
            bool DeleteNow = false;
            uint TempHeight = CurrentHeight;
            uint TempDelete = CurrentHeight;

            for (uint i = 1; i <= TempDelete; i++)
            {
                if(!DeleteNow)
                {
                    if(BlockExists(i))
                    {
                        if(!GetBlock(i).CheckBlockCorrect())
                        {
                            TempHeight = i - 1;
                            DeleteNow = true;
                            
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("Incorrect block was found, fixing now...");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    else
                    {
                        TempHeight = i - 1;
                        DeleteNow = true;
                        
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("Missing block was found, fixing now...");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    
                }
                if(DeleteNow)
                {
                    if(BlockExists(i))
                    {
                        DelBlock(i);
                    }
                }
            }
            
            CurrentHeight = TempHeight;
        }
        
        public static void SyncBlocks()
        {
            SyncMode = true;
            byte Timeout = 0;
            
            bool[] Nodes = new bool[256];
            bool FirstRun = CurrentHeight < 10;
            
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("You are not synced at height: " + CurrentHeight);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            
            for (uint i = CurrentHeight + 1; i < uint.MaxValue; i++)
            {
                for (int j = 0; j < Nodes.Length; j++)
                {
                    Nodes[j] = false;
                }
                
                while(!BlockExists(i) && Timeout <= 100)
                {
                    TcpClient RandomNode = Network.RandomClient(out byte NodeIndex);
                    
                    if(!Nodes[NodeIndex])
                    {
                        Nodes[NodeIndex] = !Nodes[NodeIndex];
                        Task.Run(() => Network.Send(RandomNode, null, new [] { "Block", "Get", i.ToString() } ));
                    }

                    Thread.Sleep(SyncSpeed);
                    
                    Timeout++;
                }
                if(Timeout > 100)
                {
                    break;
                }
                Timeout = 0;
                
                if(i % 1000 == 0)
                {
                    Console.WriteLine("Current synchronisation height: " + i);
                }
            }
            
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("You are now synced to height: " + CurrentHeight);
            
            SyncMode = false;
            FixCorruptedBlocks();
            
            if(TryAgain)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("At least one block was incorrect.");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Trying again, please wait...");
                Thread.Sleep(10000);
                DelBlock(CurrentHeight);
                TryAgain = false;
                SyncBlocks();
            }
            
            if(FirstRun)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("It looks like you are using this app first time.");
                Console.WriteLine("Check if you are synchronised to right height.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            
            Console.ForegroundColor = ConsoleColor.White;
        }
        
        public static uint CheckOtherNodes(long NodeId)
        {
            uint Result = 0;
            
            uint Lowest = NodesMin[NodeId];
            uint Highest = NodesMax[NodeId];
            
            if(Highest > CurrentHeight)
            {
                for (uint i = Highest; i + 250 > Highest && i != 0; i--)
                {
                    if(NodesChain[NodeId].ContainsKey(i))
                    {
                        if(NodesChain[NodeId].ContainsKey(i+1))
                        {
                            if(NodesChain[NodeId][i+1].PreviousHash != NodesChain[NodeId][i].CurrentHash)
                            {
                                Result = Highest;
                                NodesChain[NodeId] = new Dictionary<uint, Block>();
                                NodesMin[NodeId] = uint.MaxValue;
                                NodesMax[NodeId] = uint.MinValue;
                                break;
                            }
                        }
                        
                        if(BlockExists(i))
                        {
                            if(NodesChain[NodeId][i].CurrentHash == GetBlock(i).CurrentHash && NodesChain[NodeId][i].PreviousHash == GetBlock(i).PreviousHash)
                            {
                                lock(WholeBlockchain)
                                {
                                    bool AllCorrect = true;
                                    
                                    for (uint j = i; j <= Highest; j++)
                                    {
                                        if(!GetBlock(j, NodeId).CheckBlockCorrect(NodeId))
                                        {
                                            AllCorrect = false;
                                        }
                                    }
                                    
                                    if(AllCorrect)
                                    {
                                        for (uint j = i; j <= Highest; j++)
                                        {
                                            if(BlockExists(j))
                                            {
                                                DelBlock(j);
                                            }
                                            SetBlock(GetBlock(j, NodeId));
                                        }
                                        Mining.PrepareToMining(GetBlock(Highest));
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        Result = i;
                        break;
                    }
                }
                
                while(Lowest + 250 < CurrentHeight && BlockExists(Lowest))
                {
                    if(NodesChain[NodeId].ContainsKey(Lowest))
                    {
                        NodesChain[NodeId].Remove(Lowest);
                    }
                    Lowest++;
                }
            }
            
            return Result;
        }
        
        public static bool BlockExists(uint Height, long NodeId = -1)
        {
            bool AvailableOnNodes = Height == 0;
            if(NodeId > -1) { AvailableOnNodes = NodesChain[NodeId].ContainsKey(Height); }
            return BlocksInMemory.ContainsKey(Height) || File.Exists(Settings.BlockchainPath + Height + ".dat") || AvailableOnNodes;
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

        public static Block GetBlock(uint Height, long NodeId = -1)
        {
            if(NodeId > -1)
            {
                if(NodesChain[NodeId].ContainsKey(Height))
                {
                    return NodesChain[NodeId][Height];
                }
            }
            if (BlocksInMemory.ContainsKey(Height))
            {
                if(BlocksInMemory[Height] == null)
                {
                    BlocksInMemory[Height] = LoadBlock(Height);
                }
            }
            else
            {
                BlocksInMemory[Height] = LoadBlock(Height);
            }
            return BlocksInMemory[Height];
        }

        public static void SetBlock(Block Block)
        {
            BlocksInMemory[Block.BlockHeight] = Block;
            SaveBlock(Block);
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
            
            if(Height <= CurrentHeight)
            {
                CurrentHeight = Height - 1;
            }
        }

        static Block LoadBlock(uint Height)
        {
            Block OneBlock = null;
            
            if(Height == 0)
            {
                OneBlock = new();
                OneBlock.BlockHeight = Height;
                OneBlock.CurrentHash = "1ONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOIN0";
                return OneBlock;
            }

            byte Timeout = 0;
            while ((!File.Exists(Settings.BlockchainPath + Height + ".dat") || !File.Exists(Settings.TransactionsPath + Height + ".dat")) && Timeout++ < 100)
            {
                Task.Run(() => Network.Send(Network.RandomClient(out _), null, new [] { "Block", "Get", Height.ToString() } ));
                
                Thread.Sleep(100);
            }
            
            lock (FileSystemRead)
            {
                using BinaryReader BlockFile = new(File.Open(Settings.BlockchainPath + Height + ".dat", FileMode.Open));
                using BinaryReader TransactionsFile = new(File.Open(Settings.TransactionsPath + Height + ".dat", FileMode.Open));

                OneBlock = new();
                OneBlock.BlockHeight = Height;
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
                uint NewHeight;
                Console.WriteLine("Current blockchain height: " + CurrentHeight);
                Console.Write("Enter block number (type 0 to exit): ");
                while(!uint.TryParse(Console.ReadLine(), out NewHeight))
                {
                    Console.Write("Incorrect block number, please try again: ");
                }
                if(NewHeight == 0)
                {
                    break;
                }
                if(NewHeight > CurrentHeight)
                {
                    NewHeight = CurrentHeight;
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
                        else
                        {
                            NewHeight = 1;
                        }
                    }
                    if(Key == ConsoleKey.PageUp)
                    {
                        if(NewHeight < CurrentHeight - 1000)
                        {
                            NewHeight += 1000;
                        }
                        else
                        {
                            NewHeight = CurrentHeight;
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
