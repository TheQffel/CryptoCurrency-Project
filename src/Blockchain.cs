using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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
        public static bool NoBootStrap = false;
        
        static Dictionary<uint, Block> BlocksInMemory = new();
        static readonly object FileSystemRead = new();
        static readonly object FileSystemWrite = new();
        static readonly object WholeBlockchain = new();

        public static string TempBlocksPath = "";

        public static Dictionary<long, Dictionary<uint, Block>> NodesChain = new();
        public static Dictionary<long, bool> NodesLock = new();
        public static Dictionary<long, uint> NodesMin = new();
        public static Dictionary<long, uint> NodesMax = new();

        public static void DatabaseSync()
        {
            while(SyncMode || TryAgain) { Thread.Sleep(1000); }
            
            string Tmp = Database.Get("blocks", "BlockHeight", "", "BlockHeight DESC", "", 1)[0][0];
            if(Tmp.Length < 1)
            {
                Tmp = "0";
                Database.Add("blocks", "BlockHeight, PreviousHash, CurrentHash", "'0', '1ONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOIN0', '1ONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOIN0'");
            }
            uint DbHeight = uint.Parse(Tmp) + 1;
            
            while (Database.DatabaseHost.Length > 5)
            {
                for (; DbHeight < CurrentHeight; DbHeight++)
                {
                    Thread.Sleep(1000);
                    
                    Block ToInsert = GetBlock(DbHeight);
                    
                    if(ToInsert.CheckBlockCorrect())
                    {
                        string Hash = Database.Get("blocks", "CurrentHash", "BlockHeight='" + (DbHeight - 1) + "'")[0][0];
                        
                        if(Hash == ToInsert.PreviousHash)
                        {
                            if(Program.DebugLogging)
                            {
                                Console.WriteLine("Adding block and transactions to database: " + DbHeight);
                            }
                            
                            Database.Add("blocks", "BlockHeight, PreviousHash, CurrentHash, Timestamp, Difficulty, Message, Image, ExtraData", "'" + ToInsert.BlockHeight + "', '" + ToInsert.PreviousHash + "', '" + ToInsert.CurrentHash + "', '" + ToInsert.Timestamp + "', '" + ToInsert.Difficulty + "', '" + ToInsert.ExtraData.Split('|')[1] + "', '" + ToInsert.ExtraData.Split('|')[0] + "', '" + ToInsert.ExtraData.Split('|')[2] + "'");
                            
                            if(TempBlocksPath.Length > 1)
                            {
                                Media.TextToImage(ToInsert.ExtraData.Split('|')[0]).Save(TempBlocksPath + "/" + ToInsert.BlockHeight + ".png", ImageFormat.Png);
                            }

                            for (int j = 0; j < ToInsert.Transactions.Length; j++)
                            {
                                Thread.Sleep(100);
                            
                                Database.Add("transactions", "BlockHeight, AddressFrom, AddressTo, Amount, Fee, Timestamp, Message, Signature", "'" + ToInsert.BlockHeight + "', '" + ToInsert.Transactions[j].From + "', '" + ToInsert.Transactions[j].To + "', '" + ToInsert.Transactions[j].Amount + "', '" + ToInsert.Transactions[j].Fee + "', '" + ToInsert.Transactions[j].Timestamp + "', '" + ToInsert.Transactions[j].Message + "', '" + ToInsert.Transactions[j].Signature + "'");
                                
                                if(Wallets.AddressToUpdate != null)
                                {
                                    if(ToInsert.Transactions[j].From.Length == 88)
                                    {
                                        if(!Wallets.AddressToUpdate.Contains(ToInsert.Transactions[j].From))
                                        {
                                            Wallets.AddressToUpdate.Add(ToInsert.Transactions[j].From);
                                        }
                                    }
                                    
                                    if(ToInsert.Transactions[j].To.Length == 88)
                                    {
                                        if(!Wallets.AddressToUpdate.Contains(ToInsert.Transactions[j].To))
                                        {
                                            Wallets.AddressToUpdate.Add(ToInsert.Transactions[j].To);
                                        }
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
                    }
                }
                
                Thread.Sleep(10000);
            }
        }
        
        public static void ArchiveBlocks(string ArchivePath)
        {
            if(File.Exists(ArchivePath + "/BlockchainTemp.zip"))
            {
                File.Delete(ArchivePath + "/BlockchainTemp.zip");
            }
            
            if(!File.Exists(ArchivePath + "/Blockchain.zip"))
            {
                File.Create(ArchivePath + "/Blockchain.zip");
            }
            
            while (true)
            {
                Thread.Sleep(10000000);
                
                List<string> AllFiles = new();
                string[] DirectoriesM = Directory.GetDirectories(Settings.BlockchainPath);
                DirectoriesM = DirectoriesM.OrderBy(p => p.Length).ThenBy(r => r).ToArray();
                for (int i = 0; i < DirectoriesM.Length; i++)
                {
                    string[] DirectoriesK = Directory.GetDirectories(DirectoriesM[i]);
                    DirectoriesK = DirectoriesK.OrderBy(p => p.Length).ThenBy(r => r).ToArray();
                    DirectoriesM[i] = Path.GetFileName(DirectoriesM[i]);
                    for (int j = 0; j < DirectoriesK.Length; j++)
                    {
                        string[] Files = Directory.GetFiles(DirectoriesK[j]);
                        Files = Files.OrderBy(p => p.Length).ThenBy(r => r).ToArray();
                        DirectoriesK[j] = Path.GetFileName(DirectoriesK[j]);
                        for (int k = 0; k < Files.Length; k++)
                        {
                            AllFiles.Add("/" + DirectoriesM[i] + "/" + DirectoriesK[j] + "/" + Path.GetFileName(Files[k]));
                        }
                    }
                }
                    
                ZipArchive Archive = ZipFile.Open(ArchivePath + "/BlockchainTemp.zip", ZipArchiveMode.Create);

                for (int i = 0; i < AllFiles.Count - 1000; i++)
                {
                    Archive.CreateEntryFromFile(Settings.BlockchainPath + AllFiles[i], "blockchain" + AllFiles[i], CompressionLevel.Optimal);
                    Archive.CreateEntryFromFile(Settings.TransactionsPath + AllFiles[i], "transactions" + AllFiles[i], CompressionLevel.Optimal);
                }
                Archive.Dispose();
                
                File.Delete(ArchivePath + "/Blockchain.zip");
                File.Move(ArchivePath + "/BlockchainTemp.zip", ArchivePath + "/Blockchain.zip");
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
            if(CurrentHeight == 0 && !NoBootStrap)
            {
                string FileName = Settings.AppPath + "/Blockchain.zip";
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("It looks like you are synchronising from genesis block.");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Bootstap will be downloaded to speed up synchronisation.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Downloading genesis blocks...");
                WebClient WebClient = new();
                
                int DownloadPercent = 0;
                bool DownloadDone = false;
                WebClient.DownloadFileCompleted += (Sender, Args) => { DownloadDone = true; };
                WebClient.DownloadProgressChanged += (Sender, Args) => { DownloadPercent = Args.ProgressPercentage; };
                WebClient.DownloadFileAsync(new Uri("http://one-coin.org/blockchain.zip"), FileName);
                while (!DownloadDone)
                {
                    Thread.Sleep(100);
                    Console.CursorLeft = 0;
                    Console.Write("Downloading genesis blocks: " + DownloadPercent + "%");
                }
                Console.WriteLine();
                Console.WriteLine("Extracting downloaded blockchain...");
                ZipFile.ExtractToDirectory(FileName, Settings.AppPath, true);
                File.Move(FileName, FileName + ".backup");
                Console.WriteLine("Checking if all blocks are correct...");
                CurrentHeight = LastBlockExists();
                FixCorruptedBlocks();
            }
            
            SyncMode = true;
            byte Timeout = 0;
            bool[] Nodes = new bool[256];
            
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("You are not synced at height: " + CurrentHeight);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Thread.Sleep(1000);
            
            double StartBlock = CurrentHeight - 1;
            double StartTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            
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
                        Network.Send(RandomNode, null, new [] { "Block", "Get", i.ToString() } );
                    }

                    Thread.Sleep(SyncSpeed);
                    
                    Timeout++;
                }
                if(Timeout > 100)
                {
                    break;
                }
                Timeout = 0;
                
                Console.CursorLeft = 0;
                Console.Write("Current synchronisation height: " + i);
            }
            
            double StopBlock = CurrentHeight + 1;
            double StopTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            
            Console.CursorLeft = 0;
            Console.WriteLine("Average speed: " + Math.Round((StopBlock-StartBlock)/(StopTime-StartTime), 2) + " blocks per second.");
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
                Thread.Sleep(1000);
                DelBlock(CurrentHeight);
                TryAgain = false;
                SyncBlocks();
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
        
        public static string BlockPath(uint Height, bool Main, bool CheckPath = true)
        {
            string BlockOrTransaction;
            string FilePath = "00000000" + Height;
            if(Main) { BlockOrTransaction = Settings.BlockchainPath; }
            else { BlockOrTransaction = Settings.TransactionsPath; }
            FilePath = BlockOrTransaction + short.Parse(FilePath[^9..^6]) + "M/" + short.Parse(FilePath[^6..^3]) + "K/";
            if(CheckPath) { if(!Directory.Exists(FilePath)) { Directory.CreateDirectory(FilePath); } }
            return FilePath + Height + ".dat";
        }
        
        public static bool BlockExists(uint Height, long NodeId = -1)
        {
            bool AvailableOnNodes = Height == 0;
            if(NodeId > -1) { AvailableOnNodes = NodesChain[NodeId].ContainsKey(Height); }
            return BlocksInMemory.ContainsKey(Height) || File.Exists(BlockPath(Height, true, false)) || AvailableOnNodes;
        }

        public static uint LastBlockExists()
        {
            uint Result = 1073741824;
            uint Change = 1073741824;

            for (int i = 0; i < 32; i++)
            {
                Change /= 2;

                if(File.Exists(BlockPath(Result, true, false)))
                {
                    Result += Change;
                }
                else
                {
                    Result -= Change;
                }
            }
            
            Result++;
            while(!File.Exists(BlockPath(Result, true, false)) && Result > 0)
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
            if (File.Exists(BlockPath(Height, true)))
            {
                File.Delete(BlockPath(Height, true));
            }
            if(File.Exists(BlockPath(Height, false)))
            {
                File.Delete(BlockPath(Height, false));
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
            while ((!File.Exists(BlockPath(Height, true, false)) || !File.Exists(BlockPath(Height, false, false))) && Timeout++ < 100)
            {
                Network.Send(Network.RandomClient(out _), null, new [] { "Block", "Get", Height.ToString() } );
                
                Thread.Sleep(100);
            }
            
            lock (FileSystemRead)
            {
                using BinaryReader BlockFile = new(File.Open(BlockPath(Height, true), FileMode.Open));
                using BinaryReader TransactionsFile = new(File.Open(BlockPath(Height, false), FileMode.Open));

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
                using BinaryWriter BlockFile = new(File.Open(BlockPath(OneBlock.BlockHeight, true), FileMode.Create));
                using BinaryWriter TransactionsFile = new(File.Open(BlockPath(OneBlock.BlockHeight, false), FileMode.Create));

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
