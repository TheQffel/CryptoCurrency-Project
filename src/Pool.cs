using System;
using System.Collections.Generic;
using System.Net.Sockets;
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
        public static Dictionary<string, byte> MinersShares = new();

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
            bool ShareCorrect = Share.CheckBlockCorrect(-1, CustomDifficulty);
            if(Share.Transactions[0].To != PoolWallet) { ShareCorrect = false; }

            if(ShareCorrect)
            {
                while(SynchronisingNow)
                {
                    Thread.Sleep(100);
                }
                
                if(!MinersShares.ContainsKey(Miner))
                {
                    MinersShares.Add(Miner, 0);
                }
                if(MinersShares[Miner] < 255)
                {
                    MinersShares[Miner]++;
                }
            }
            
            return ShareCorrect;
        }
    }
}
