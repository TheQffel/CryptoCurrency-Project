using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;

namespace OneCoin
{
    class Pool
    {
        public static string PoolKey = "";
        public static string PoolAddress = "";
        public static int PoolPort = 10101;
        public static byte CustomDifficulty = 250;
        public static string PoolWallet = "";
        
        public static string PoolMessage = "";
        public static string PoolImage = "";
        
        public static TcpClient PoolNode = null;
        
        public static uint ShareHeight = 0;
        public static bool SynchronisingNow = false;
        public static Dictionary<string, byte[]> MinersShares = new();
        public static Dictionary<string, ulong> MinerTimestamps = new();

        public static void DatabaseSync()
        {
            while(Blockchain.SyncMode || Blockchain.TryAgain) { Thread.Sleep(1000); }
            
            while (Database.DatabaseHost.Length > 5)
            {
                if(ShareHeight != Blockchain.CurrentHeight)
                {
                    if(ShareHeight > 1)
                    {
                        SynchronisingNow = true;

                        foreach(KeyValuePair<string, byte[]> Shares in MinersShares)
                        {
                            Thread.Sleep(100);
                                    
                            Database.Add("shares", "Address, BlockHeight, Difficulty, ValidShares, StaleShares, InvalidShares", "'" + Shares.Key + "', '" + (ShareHeight + 1) + "', '" + CustomDifficulty + "', '" + Shares.Value[0] + "', '" + Shares.Value[1] + "', '" + Shares.Value[2] + "'");
                        }
                        MinersShares.Clear();

                        SynchronisingNow = false;
                    }
                    ShareHeight = Blockchain.CurrentHeight;
                    
                    Thread.Sleep(1000);
                    
                    string LastShareHeight = Database.Get("shares", "BlockHeight", "", "BlockHeight", "", 1)[0][0];
                    if(LastShareHeight.Length < 4) { LastShareHeight = "999"; }
                    
                    string[] MinedBlock = Database.Get("transactions", "BlockHeight, Amount", "AddressTo = '" + PoolWallet + "' AND BlockHeight BETWEEN '" + LastShareHeight + "' AND '" + (ShareHeight-1000) + "'", "BlockHeight", "", 1)[0];
                    if(uint.TryParse(MinedBlock[0], out uint MinedBlockHeight))
                    {
                        uint TotalShares = 0;
                        string[][] Shares = Database.Get("shares", "Address, SUM(ValidShares)", "BlockHeight <= '" + MinedBlockHeight + "'", "", "Address");
                        
                        for (int i = 0; i < Shares.Length; i++)
                        {
                            TotalShares += uint.Parse(Shares[i][1]);
                        }
                        string RewardPerShare = Hashing.CalculateBigNumbers(MinedBlock[1], TotalShares.ToString(), '/');
                        
                        if(Program.DebugLogging)
                        {
                            Console.WriteLine("Payout for block " + MinedBlockHeight + ": " + RewardPerShare + " Ones / Share - Total Shares: " + TotalShares);
                        }

                        for (int i = 0; i < Shares.Length; i++)
                        {
                            if(Database.Get("onecoin", "Address", "Address = '" + Shares[i][0] + "'")[0][0].Length < 9)
                            {
                                Database.Add("onecoin", "Address, Balance", "'" + Shares[i][0] + "', '0'");
                            }
                            string PoolBalance = Database.Get("onecoin", "Pool_Balance", "Address = '" + Shares[i][0] + "'")[0][0];
                            if(PoolBalance.Length < 1) { PoolBalance = "0"; }
                            Shares[i][1] = Hashing.CalculateBigNumbers(Hashing.CalculateBigNumbers(Shares[i][1], RewardPerShare, '*'), PoolBalance, '+');
                        }
                        
                        Database.Del("shares", "BlockHeight <= '" + MinedBlockHeight + "'");

                        for (int i = 0; i < Shares.Length; i++)
                        {
                            Database.Set("onecoin", "Pool_Balance = '" + Shares[i][1] + "'", "Address = '" + Shares[i][0] + "'");
                        }
                    }
                    
                    Thread.Sleep(1000);
                
                    string[] PendingTransaction = Database.Get("payouts", "AddressTo, Amount", "Timestamp < 9 AND AddressTo = Signature", "", "", 1)[0];
            
                    if(PendingTransaction.Length > 1)
                    {
                        if(BigInteger.TryParse(PendingTransaction[1], out BigInteger Amount))
                        {
                            Transaction Transaction = new();
                            Transaction.From = PoolWallet;
                            Transaction.To = PendingTransaction[0];
                            Transaction.Amount = Amount;
                            Transaction.Fee = 0;
                            Transaction.Timestamp = (ulong)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                            Transaction.GenerateSignature(PoolKey);
                            
                            string BalanceText = Database.Get("onecoin", "Pool_Balance", "Address = '" + Transaction.To + "'")[0][0];
                            if(BigInteger.TryParse(BalanceText, out BigInteger Balance))
                            {
                                if(Balance < Amount || Amount < 1000000000000000000)
                                {
                                    Database.Del("payouts", "Signature = '" + Transaction.To + "'");
                                }
                                else
                                {
                                    Database.Set("onecoin", "Pool_Balance = '" + (Balance - Amount) + "'", "Address = '" + Transaction.To + "'");

                                    Database.Set("payouts", "AddressFrom = '" + Transaction.From + "', AddressTo = '" + Transaction.To + "', Amount = '" + Transaction.Amount + "', Fee = '" + Transaction.Fee + "', Timestamp = '" + Transaction.Timestamp + "', Signature = '" + Transaction.Signature + "'", "Signature = '" + Transaction.To + "'");
                                    
                                    Network.Broadcast(new[] { "Transaction", Transaction.From, Transaction.To, Transaction.Amount.ToString(), Transaction.Fee.ToString(), Transaction.Timestamp.ToString(), Transaction.Message, Transaction.Signature });
                                    
                                    if(Program.DebugLogging)
                                    {
                                        Console.WriteLine("Payout for user " + Transaction.To + ": " + Transaction.Amount + " Ones");
                                    }
                                }
                            }
                        }
                    }
                }
                
                Thread.Sleep(1000);
            }
        }
        
        public static void ConnectionLoop()
        {
            Thread.Sleep(1000);
            
            while(true)
            {
                try
                {
                    Connect();
                    
                    NetworkStream Stream = PoolNode.GetStream();
                    byte[] Buffer = new byte[ushort.MaxValue];
                    int ByteIndex = 0;
                    int ByteValue = 0;
                    
                    while (PoolNode.Connected)
                    {
                        ByteValue = Stream.ReadByte();

                        if (ByteValue != 4)
                        {
                            Buffer[ByteIndex] = (byte)ByteValue;
                            ByteIndex++;
                        }
                        else
                        {
                            Network.Action(PoolNode, null, Encoding.UTF8.GetString(Buffer, 0, ByteIndex).Split("~"));
                            ByteIndex = 0;
                        }
                    }
                    
                    Disconnect();
                }
                catch(Exception Ex)
                {
                    if(Mining.MonitorMining)
                    {
                        Console.WriteLine("Error with pool network connection!");
                        if(Program.DebugLogging)
                        {
                            Console.WriteLine(Ex);
                        }
                        Console.WriteLine("Trying to reconnect in few minutes.");
                    }
                    Thread.Sleep(100000);
                    PoolNode = null;
                }
            }
        }
        
        public static void Connect()
        {
            if(PoolNode == null)
            {
                PoolNode = new();
                PoolNode.Connect(PoolAddress, PoolPort);
            }
            else
            {
                if(!PoolNode.Connected)
                {
                    PoolNode = new();
                    PoolNode.Connect(PoolAddress, PoolPort);
                }
            }
            
            if(Mining.MonitorMining)
            {
                Console.WriteLine("Connected to pool: " + PoolAddress + ":" + PoolPort);
            }
        }
        
        public static void Disconnect()
        {
            if(PoolNode != null)
            {
                if(PoolNode.Connected)
                {
                    PoolNode.Close();
                    PoolNode.Dispose();
                }
                PoolNode = null;
            }
            
            if(Mining.MonitorMining)
            {
                Console.WriteLine("Disconnected from pool: " + PoolAddress + ":" + PoolPort);
            }
        }
        
        public static bool ProcessShare(Block Share, string Miner)
        {
            if(!MinerTimestamps.ContainsKey(Miner))
            {
                MinerTimestamps.Add(Miner, 1);
            }
            if(!MinersShares.ContainsKey(Miner))
            {
                MinersShares.Add(Miner, new byte[] { 0, 0, 0 });
            }
            
            bool ShareCorrect = Share.CheckBlockCorrect(-1, CustomDifficulty);
            if(Share.Transactions[0].To != PoolWallet) { ShareCorrect = false; }
            if(Share.Timestamp <= MinerTimestamps[Miner]) { ShareCorrect = false; }
            
            while(SynchronisingNow)
            {
                Thread.Sleep(100);
            }
            if(Wallets.CheckAddressCorrect(Miner))
            {
                if(ShareCorrect)
                {
                    MinerTimestamps[Miner] = Share.Timestamp;
                    
                    if(Share.BlockHeight > Blockchain.CurrentHeight)
                    {
                        if(MinersShares[Miner][0] < 255)
                        {
                            MinersShares[Miner][0]++;
                        }
                    }
                    else
                    {
                        if(MinersShares[Miner][1] < 255)
                        {
                            MinersShares[Miner][1]++;
                        }
                    }
                }
                else
                {
                    if(MinersShares[Miner][2] < 255)
                    {
                        MinersShares[Miner][2]++;
                    }
                }
            }
            
            return ShareCorrect;
        }
    }
}
