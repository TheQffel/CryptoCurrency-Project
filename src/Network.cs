using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneCoin
{
    class Network
    {
        public static TcpClient[] Peers = new TcpClient[256];
        public static bool[] IsListening = new bool[256];

        public static Dictionary<long, bool> NodesTransmission = new();

        public static void ListenForPackets()
        {
            try
            {
                UdpClient Listener = new UdpClient(10101);

                while (true)
                {
                    IPEndPoint Reciever = new IPEndPoint(IPAddress.Any, 10101);
                    UdpClient Sender = new();
                    byte[] Data = Listener.Receive(ref Reciever);
                    Sender.Connect(Reciever);
                    Action(null, Sender, Encoding.UTF8.GetString(Data, 0, Data.Length).Split("~"));
                    Thread.Sleep(100);
                    Sender.Dispose();
                }
            }
            catch (Exception Ex)
            {
                if (Program.DebugLogging)
                {
                    Console.WriteLine(Ex);
                }
            }
        }

        public static void ListenForConnections()
        {
            try
            {
                TcpListener Listener = new(IPAddress.Any, 10101);
                Listener.Start();

                while (true)
                {
                    TcpClient Client = Listener.AcceptTcpClient();
                    Task.Run(() => Recieve(Client, false));
                }
            }
            catch (Exception Ex)
            {
                if (Program.DebugLogging)
                {
                    Console.WriteLine(Ex);
                }
            }
        }

        public static void Send(TcpClient TcpClient, UdpClient UdpClient, string[] Messages)
        {
            long NodeId = NodeToId(TcpClient, UdpClient);
            if(!NodesTransmission.ContainsKey(NodeId))
            {
                NodesTransmission[NodeId] = false;
            }
            
            if(!NodesTransmission[NodeId])
            {
                NodesTransmission[NodeId] = true;
                
                try
                {
                    string Message = "";
                    for (int i = 0; i < Messages.Length; i++)
                    {
                        Message += "~" + Messages[i];
                    }
                    if (TcpClient != null)
                    {
                        if (TcpClient.Connected)
                        {
                            NetworkStream Stream = TcpClient.GetStream();
                            Stream.Write(Encoding.UTF8.GetBytes(Message[1..]).Concat(new byte[] { 4 }).ToArray());
                        }
                    }
                    if (UdpClient != null)
                    {
                        byte[] Data = Encoding.UTF8.GetBytes(Message[1..]);
                        UdpClient.Send(Data, Data.Length);
                    }
                }
                catch (Exception Ex)
                {
                    if (Program.DebugLogging)
                    {
                        Console.WriteLine(Ex);
                    }
                }
                
                NodesTransmission[NodeId] = false;
            }
        }

        public static void Broadcast(string[] Messages, TcpClient Exclude = null)
        {
            try
            {
                for (int i = 0; i < 256; i++)
                {
                    if (Peers[i] != null)
                    {
                        if (Peers[i].Connected)
                        {
                            if (Peers[i] != Exclude)
                            {
                                Task Transmission = Task.Run(() => Send(Peers[i], null, Messages));
                                while((byte)Transmission.Status < 3) { Thread.Sleep(1); }
                            }
                        }
                        else
                        {
                            Peers[i] = null;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                if (Program.DebugLogging)
                {
                    Console.WriteLine(Ex);
                }
            }
        }

        public static void Recieve(TcpClient Client, bool Listening)
        {
            string Peer = ((IPEndPoint)Client.Client.RemoteEndPoint).Address.MapToIPv4() + ":" + ((IPEndPoint)Client.Client.RemoteEndPoint).Port;
            long NodeId = NodeToId(Client, null);
            
            if(Program.DebugLogging)
            {
                Console.WriteLine("Connected: " + Peer);
            }
            
            Blockchain.NodesLock[NodeId] = false;
            
            try
            {
                byte Index = 255;
                for (byte i = 0; i <= Index; i++)
                {
                    if (Peers[i] == null)
                    {
                        Index = i;
                        break;
                    }
                    else
                    {
                        if (!Peers[i].Connected)
                        {
                            Index = i;
                            break;
                        }
                    }
                }
                Peers[Index] = Client;
                IsListening[Index] = Listening;
                NetworkStream Stream = Peers[Index].GetStream();

                byte[] Buffer = new byte[ushort.MaxValue];
                int ByteIndex = 0;
                int ByteValue = 0;

                while (Peers[Index].Connected)
                {
                    ByteValue = Stream.ReadByte();

                    if (ByteValue != 4)
                    {
                        Buffer[ByteIndex] = (byte)ByteValue;
                        ByteIndex++;
                    }
                    else
                    {
                        Action(Client, null, Encoding.UTF8.GetString(Buffer, 0, ByteIndex).Split("~"));
                        ByteIndex = 0;
                    }
                }
                Peers[Index] = null;
            }
            catch (Exception Ex)
            {
                if (Program.DebugLogging)
                {
                    Console.WriteLine(Ex);
                }
            }
            
            Blockchain.NodesLock[NodeId] = false;
            
            if(Program.DebugLogging)
            {
                Console.WriteLine("Disconnected: " + Peer);
            }
        }

        public static void Discover(string Ip, int Port = 10101, int Delay = -1)
        {
            if(Delay > 0)
            {
                Thread.Sleep(Delay);
            }
            
            try
            {
                if(ConnectedNodes(true) < 16)
                {
                    List<string> NodeIps = new List<string>();
                    foreach (var Address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    {
                        NodeIps.Add(Address.MapToIPv4() + ":0");
                    }
                    
                    byte FullNodesCount = 0;
                    string[] Nodes = NodeIps.Concat(ConnectedNodesAddresses()).ToArray();
                    for (int i = 0; i < Nodes.Length; i++)
                    {
                        if(Nodes[i].Split(':')[0] == Ip)
                        {
                            Ip = "0";
                            break;
                        }
                    }
                    
                    if (Ip.Length > 5)
                    {
                        IPAddress Address = IPAddress.Parse(Ip);
                    
                        if(!IPAddress.IsLoopback(Address))
                        {
                            TcpClient New = new();
                            New.Connect(Address, Port);
                            
                            Recieve(New, true);
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                if(Program.DebugLogging)
                {
                    if(((SocketException)Ex).ErrorCode is not 110 and not 111)
                    {
                        Console.WriteLine(Ex);
                    }
                }
            }
        }

        public static void SearchVerifiedNodes()
        {
            try
            { 
                string LatestNodes = new WebClient().DownloadString("http://one-coin.org/verifiednodes.txt");
                File.WriteAllText(Settings.AppPath + "/nodes.txt", LatestNodes);
            }
            catch (Exception Ex)
            {
                if(Program.DebugLogging) { Console.WriteLine("Cannot refresh nodes list, using local file."); }
            }
            
            string[] Nodes = File.ReadAllLines(Settings.AppPath + "/nodes.txt");
            Random Random = new Random();
            Nodes = Nodes.OrderBy(x => Random.Next()).ToArray();
            
            for (int i = 0; i < Nodes.Length; i++)
            {
                string[] FullNode = Nodes[i].Split(":");
                if(FullNode.Length > 1)
                {
                    Task.Run(() => Discover(FullNode[0], int.Parse(FullNode[1])));
                    Thread.Sleep(100);
                }
            }
            Broadcast(new[] { "Nodes", "List" });
        }

        public static void FlushConnections()
        {
            Broadcast(new[] { "Nodes", "Restart" });
            
            try
            {
                for (int i = 0; i < 256; i++)
                {
                    if (Peers[i] != null)
                    {
                        if (Peers[i].Connected)
                        {
                            Peers[i].Close();
                        }
                        Peers[i] = null;
                    }
                }
            }
            catch (Exception Ex)
            {
                if (Program.DebugLogging)
                {
                    Console.WriteLine(Ex);
                }
            }
        }

        public static TcpClient RandomClient(out byte NodeIndex)
        {
            Random Random = new();
            
            if(ConnectedNodes() == 0)
            {
                NodeIndex = 255;
                return null;
            }

            while(true)
            {
                NodeIndex = (byte)Random.Next(0, 256);

                if (Peers[NodeIndex] != null)
                {
                    if (Peers[NodeIndex].Connected)
                    {
                        return Peers[NodeIndex];
                    }
                }
            }
        }

        public static byte ConnectedNodes(bool OnlyPublic = false)
        {
            byte Nodes = 0;

            for (int i = 0; i < 256; i++)
            {
                if (Peers[i] != null)
                {
                    if (Peers[i].Connected)
                    {
                        if(!OnlyPublic || ((IPEndPoint)Peers[i].Client.RemoteEndPoint).Port == 10101)
                        {
                            Nodes++;
                        }
                    }
                }
            }

            return Nodes;
        }
        
        public static string[] ConnectedNodesAddresses()
        {
            List<string> Nodes = new();

            for (int i = 0; i < 256; i++)
            {
                if (Peers[i] != null)
                {
                    if (Peers[i].Connected)
                    {
                        IPEndPoint Node = (IPEndPoint)Peers[i].Client.RemoteEndPoint;
                        Nodes.Add(Node.Address.MapToIPv4() + ":" + Node.Port);
                    }
                }
            }

            return Nodes.ToArray();
        }
        
        public static long NodeToId(TcpClient TcpClient, UdpClient UdpClient)
        {
            long NodeId = -1;
            
            if (UdpClient != null)
            {
                NodeId = BitConverter.ToUInt32(((IPEndPoint)UdpClient.Client.RemoteEndPoint).Address.MapToIPv4().GetAddressBytes(), 0);
                NodeId <<= 16;
                NodeId += ((IPEndPoint)UdpClient.Client.RemoteEndPoint).Port;
            }
            if (TcpClient != null)
            {
                NodeId = BitConverter.ToUInt32(((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.MapToIPv4().GetAddressBytes(), 0);
                NodeId <<= 16;
                NodeId += ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Port;
            }
            
            return NodeId;
        }

        public static void Action(TcpClient TcpClient, UdpClient UdpClient, string[] Messages)
        {
            long NodeId = NodeToId(TcpClient, UdpClient);

            switch (Messages[0].ToLower())
            {
                case "nodes":
                {
                    if (Messages.Length > 1)
                    {
                        if (Messages[1].ToLower() == "list")
                        {
                            Task.Run(() => Send(TcpClient, UdpClient, new [] { "Nodes", "Response" }.Concat(ConnectedNodesAddresses()).ToArray() ));
                        }
                        if (Messages[1].ToLower() == "response")
                        {
                            Random Random = new Random();
                            string[] Nodes = Messages.Skip(2).OrderBy(x => Random.Next()).ToArray();
                            
                            for (int i = 0; i < Nodes.Length; i++)
                            {
                                Task.Run(() => Discover(Nodes[i].Split(":")[0]));
                                Thread.Sleep(100);
                            }
                        }
                        if (Messages[1].ToLower() == "restart")
                        {
                            if(TcpClient != null)
                            {
                                IPEndPoint NodeAddress = (IPEndPoint)TcpClient.Client.RemoteEndPoint;
                                
                                if(NodeAddress.Port == 10101)
                                {
                                    Task.Run(() => Discover(NodeAddress.Address.MapToIPv4().ToString(), 10101, 100000));
                                }
                            }
                        }
                    }
                    break;
                }
                case "block":
                {
                    if (Messages.Length > 2)
                    {
                        if (Messages[1].ToLower() == "get")
                        {
                            if(Messages[2] == "X") { Messages[2] = Blockchain.CurrentHeight.ToString(); }
                            
                            if (Blockchain.BlockExists(uint.Parse(Messages[2])))
                            {
                                Block OldBlock = Blockchain.GetBlock(uint.Parse(Messages[2]));
                                string[] BlockData = new string[OldBlock.Transactions.Length * 7 + 9];
                                
                                if(OldBlock.CheckBlockCorrect())
                                {
                                    BlockData[0] = "Block";
                                    BlockData[1] = "Set";
                                    BlockData[2] = OldBlock.BlockHeight.ToString();
                                    BlockData[3] = OldBlock.PreviousHash;
                                    BlockData[4] = OldBlock.CurrentHash;
                                    BlockData[5] = OldBlock.Timestamp.ToString();
                                    BlockData[6] = OldBlock.Difficulty.ToString();
                                    BlockData[7] = OldBlock.ExtraData;
                                    BlockData[8] = OldBlock.Transactions.Length.ToString();

                                    for (int i = 0; i < OldBlock.Transactions.Length; i++)
                                    {
                                        BlockData[9 + i * 7] = OldBlock.Transactions[i].From;
                                        BlockData[10 + i * 7] = OldBlock.Transactions[i].To;
                                        BlockData[11 + i * 7] = OldBlock.Transactions[i].Amount.ToString();
                                        BlockData[12 + i * 7] = OldBlock.Transactions[i].Fee.ToString();
                                        BlockData[13 + i * 7] = OldBlock.Transactions[i].Timestamp.ToString();
                                        BlockData[14 + i * 7] = OldBlock.Transactions[i].Message;
                                        BlockData[15 + i * 7] = OldBlock.Transactions[i].Signature;
                                    }
                                    Task.Run(() => Send(TcpClient, UdpClient, BlockData));
                                }
                            }
                        }
                        if (Messages[1].ToLower() == "set")
                        {
                            if (Messages.Length > 15)
                            {
                                Block NewBlock = new();

                                NewBlock.BlockHeight = uint.Parse(Messages[2]);
                                NewBlock.PreviousHash = Messages[3];
                                NewBlock.CurrentHash = Messages[4];
                                NewBlock.Timestamp = ulong.Parse(Messages[5]);
                                NewBlock.Difficulty = byte.Parse(Messages[6]);
                                NewBlock.ExtraData = Messages[7];
                                NewBlock.Transactions = new Transaction[ushort.Parse(Messages[8])];

                                for (int i = 0; i < NewBlock.Transactions.Length; i++)
                                {
                                    NewBlock.Transactions[i] = new();
                                    NewBlock.Transactions[i].From = Messages[9 + i * 7];
                                    NewBlock.Transactions[i].To = Messages[10 + i * 7];
                                    NewBlock.Transactions[i].Amount = BigInteger.Parse(Messages[11 + i * 7]);
                                    NewBlock.Transactions[i].Fee = ulong.Parse(Messages[12 + i * 7]);
                                    NewBlock.Transactions[i].Timestamp = ulong.Parse(Messages[13 + i * 7]);
                                    NewBlock.Transactions[i].Message = Messages[14 + i * 7];
                                    NewBlock.Transactions[i].Signature = Messages[15 + i * 7];
                                }
                                
                                while(NewBlock.Timestamp > (ulong)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds())
                                {
                                    Thread.Sleep(100);
                                }
                                
                                if(Blockchain.SyncMode)
                                {
                                    if(!Blockchain.BlockExists(NewBlock.BlockHeight))
                                    {
                                        if(NewBlock.CheckBlockCorrect())
                                        {
                                            Blockchain.SetBlock(NewBlock);
                                        }
                                        else
                                        {
                                            Blockchain.TryAgain = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if(!Blockchain.NodesChain.ContainsKey(NodeId))
                                    {
                                        Blockchain.NodesChain[NodeId] = new Dictionary<uint, Block>();
                                        Blockchain.NodesLock[NodeId] = false;
                                        Blockchain.NodesMin[NodeId] = uint.MaxValue;
                                        Blockchain.NodesMax[NodeId] = uint.MinValue;
                                    }
                                    
                                    if(!Blockchain.NodesLock[NodeId])
                                    {
                                        Blockchain.NodesLock[NodeId] = true;
                                        Blockchain.NodesChain[NodeId][NewBlock.BlockHeight] = NewBlock;
                                        
                                        if(Blockchain.NodesMin[NodeId] > NewBlock.BlockHeight)
                                        {
                                            Blockchain.NodesMin[NodeId] = NewBlock.BlockHeight;
                                        }
                                        if(Blockchain.NodesMax[NodeId] < NewBlock.BlockHeight)
                                        {
                                            Blockchain.NodesMax[NodeId] = NewBlock.BlockHeight;
                                        }
                                        
                                        uint Height = Blockchain.CurrentHeight;
                                        uint Missing = Blockchain.CheckOtherNodes(NodeId);
                                        Blockchain.NodesLock[NodeId] = false;
                                        
                                        if(Missing > 0)
                                        {
                                            Task.Run(() => Send(TcpClient,  UdpClient, new[] { "Block", "Get", Missing.ToString() }));
                                        }
                                        else
                                        {
                                            if(Height != Blockchain.CurrentHeight)
                                            {
                                                if (Blockchain.BlockExists(Blockchain.CurrentHeight))
                                                {
                                                    Block OldBlock = Blockchain.GetBlock(Blockchain.CurrentHeight);
                                                    string[] BlockData = new string[OldBlock.Transactions.Length * 7 + 9];
                                                    
                                                    if(OldBlock.CheckBlockCorrect())
                                                    {
                                                        BlockData[0] = "Block";
                                                        BlockData[1] = "Set";
                                                        BlockData[2] = OldBlock.BlockHeight.ToString();
                                                        BlockData[3] = OldBlock.PreviousHash;
                                                        BlockData[4] = OldBlock.CurrentHash;
                                                        BlockData[5] = OldBlock.Timestamp.ToString();
                                                        BlockData[6] = OldBlock.Difficulty.ToString();
                                                        BlockData[7] = OldBlock.ExtraData;
                                                        BlockData[8] = OldBlock.Transactions.Length.ToString();

                                                        for (int i = 0; i < OldBlock.Transactions.Length; i++)
                                                        {
                                                            BlockData[9 + i * 7] = OldBlock.Transactions[i].From;
                                                            BlockData[10 + i * 7] = OldBlock.Transactions[i].To;
                                                            BlockData[11 + i * 7] = OldBlock.Transactions[i].Amount.ToString();
                                                            BlockData[12 + i * 7] = OldBlock.Transactions[i].Fee.ToString();
                                                            BlockData[13 + i * 7] = OldBlock.Transactions[i].Timestamp.ToString();
                                                            BlockData[14 + i * 7] = OldBlock.Transactions[i].Message;
                                                            BlockData[15 + i * 7] = OldBlock.Transactions[i].Signature;
                                                        }
                                                        Broadcast(BlockData);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
                case "transaction":
                {
                    if (Messages.Length > 7)
                    {
                        bool A = Messages[2].Length != 88 && Blockchain.CurrentHeight < 1000; // Unlock nicknames/avatars at 1k
                        bool B = Messages[2].Length == 88 && Blockchain.CurrentHeight < 10000; // Unlock transactions at 10k
                        bool C = Messages[4] != "0" && Blockchain.CurrentHeight < 100000; // No fee for transactions to 100k
                        
                        if(!A && !B && !C)
                        {
                            Transaction Transaction = new();
                            Transaction.From = Messages[1];
                            Transaction.To = Messages[2];
                            Transaction.Amount = ulong.Parse(Messages[3]);
                            Transaction.Fee = ulong.Parse(Messages[4]);
                            Transaction.Timestamp = ulong.Parse(Messages[5]);
                            Transaction.Message = Messages[6];
                            Transaction.Signature = Messages[7];
                            
                            while(Transaction.Timestamp > (ulong)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds())
                            {
                                Thread.Sleep(1);
                            }

                            lock (Mining.PendingTransactions)
                            {
                                if(Transaction.Timestamp + 500000 > (ulong)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds())
                                {
                                    if(!Mining.PendingTransactions.Exists(Pending => Pending.Signature == Transaction.Signature))
                                    {
                                        if(Transaction.CheckTransactionCorrect(Wallets.GetBalance(Transaction.From), Blockchain.CurrentHeight))
                                        {
                                            if(!Wallets.CheckTransactionAlreadyIncluded(Transaction))
                                            {
                                                Mining.PendingTransactions.Add(Transaction);
                                                Mining.PendingTransactions.Sort((x, y) => x.Timestamp.CompareTo(y.Timestamp));
                                                Mining.UpdateTransactions();
                                                
                                                Broadcast(Messages/*, TcpClient*/);  // #TOBEREMOVED#
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
                case "account":
                {
                    if (Messages.Length > 3)
                    {
                        if (Messages[1].ToLower() == "info")
                        {
                            if (Messages[2].ToLower() == "get")
                            {
                                Task.Run(() => Send(TcpClient, UdpClient, new[] { "Account", "Info", "Set", Wallets.GetBalance(Messages[3]).ToString(), Wallets.GetName(Messages[3]), Wallets.GetAvatar(Messages[3])}));
                            }
                        }
                        if (Messages[1].ToLower() == "search")
                        {
                            if (Messages[2].ToLower() == "request")
                            {
                                if (Messages[3].ToLower() == "nickname")
                                {
                                    Task.Run(() => Send(TcpClient, UdpClient, new[] { "Account", "Search", "Result", "Nickname", Messages[4], Messages[5], Wallets.GetAddress(Messages[4], byte.Parse(Messages[5]))}));
                                }
                            }
                        }
                    }
                    break;
                }
                case "service":
                {
                    if (Messages.Length > 3)
                    {
                        if (Messages[1].ToLower() == "info")
                        {
                            if (Messages[2].ToLower() == "get")
                            {
                                Task.Run(() => Send(TcpClient, UdpClient, new[] { "Service", "Info", "Set", Messages[3], Wallets.GetName(Messages[3]), Wallets.GetAvatar(Messages[3]) }));
                            }
                        }
                    }
                    break;
                }
            }
        }

    }
}
