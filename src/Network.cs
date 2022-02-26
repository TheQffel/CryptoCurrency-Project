using System;
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

        public static string[] VerifiedNodes = Array.Empty<string>();

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
        
        public static UdpClient AddressToClient(string IpAddress)
        {
            UdpClient Client = new UdpClient();
            Client.Connect(IpAddress, 10101);
            return Client;
        }

        public static void Send(TcpClient TcpClient, UdpClient UdpClient, string[] Messages)
        {
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
                                Send(Peers[i], null, Messages);
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
            string Peer = ((IPEndPoint)Client.Client.RemoteEndPoint).Address.MapToIPv4().ToString();
            
            if(Program.DebugLogging)
            {
                Console.WriteLine("Connected: " + Peer);
            }
            
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
            
            if(Program.DebugLogging)
            {
                Console.WriteLine("Disconnected: " + Peer);
            }
        }

        public static void Discover(string Ip, int Port = 10101)
        {
            try
            {
                if (Ip.Length > 5)
                {
                    TcpClient New = new();
                    New.Connect(IPAddress.Parse(Ip), Port);
                        
                    Recieve(New, true);
                }
            }
            catch (Exception e)
            {
                if(Program.DebugLogging)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static void SearchVerifiedNodes()
        {
            WebClient WebClient = new();
            VerifiedNodes = WebClient.DownloadString("http://one-coin.org/verifiednodes.txt").Split("\n");
            for (int i = 0; i < VerifiedNodes.Length; i++)
            {
                string[] FullNode = VerifiedNodes[i].Split(":");
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

        public static TcpClient RandomClient()
        {
            Random Random = new();

            while(true)
            {
                int RandomPeer = Random.Next(0, 256);

                if (Peers[RandomPeer] != null)
                {
                    if (Peers[RandomPeer].Connected)
                    {
                        return Peers[RandomPeer];
                    }
                }
            }
        }

        public static string ConnectedNodes()
        {
            byte Nodes = 0;

            for (int i = 0; i < 256; i++)
            {
                if (Peers[i] != null)
                {
                    if (Peers[i].Connected)
                    {
                        Nodes++;
                    }
                }
            }

            return Nodes + " / 256";
        }

        public static void Action(TcpClient TcpClient, UdpClient UdpClient, string[] Messages)
        {
            switch (Messages[0].ToLower())
            {
                case "nodes":
                {
                    if (Messages.Length > 1)
                    {
                        if (Messages[1].ToLower() == "list")
                        {
                            string Nodes = "Nodes;Response";
                            for (int i = 0; i < 256; i++)
                            {
                                if (Peers[i] != null)
                                {
                                    if (Peers[i].Connected)
                                    {
                                        if (IsListening[i])
                                        {
                                            string Address = ((IPEndPoint)Peers[i].Client.RemoteEndPoint).Address.MapToIPv4().ToString();
                                            int Port = ((IPEndPoint)Peers[i].Client.RemoteEndPoint).Port;
                                            if (Address.Length > 5)
                                            {
                                                Nodes += ";" + Address + ":" + Port;
                                            }
                                        }
                                    }
                                }
                            }
                            Send(TcpClient, UdpClient, Nodes.Split(";"));
                        }
                        if (Messages[1].ToLower() == "response")
                        {
                            for (int i = 2; i < Messages.Length; i++)
                            {
                                //Task.Run(() => Discover(Messages[i].Split(":")[0], int.Parse(Messages[i].Split(":")[1])));
                                Thread.Sleep(100);
                            }
                        }
                    }
                    break;
                }
                case "block":
                {
                    if (Messages.Length > 2)
                    {
                        if (Messages[1].ToLower() == "height")
                        {
                            uint RemoteHeight = uint.Parse(Messages[2]);
                            if (RemoteHeight < Blockchain.CurrentHeight)
                            {
                                Send(TcpClient, UdpClient, new[] { "Block", "Height", Blockchain.CurrentHeight.ToString() });
                            }
                            if (RemoteHeight > Blockchain.CurrentHeight)
                            {
                                if(Blockchain.Synchronising)
                                {
                                    Blockchain.CurrentHeight = RemoteHeight;
                                }
                            }
                        }
                        if (Messages[1].ToLower() == "hash")
                        {
                            uint Requested = uint.Parse(Messages[2]);
                            if (Messages.Length > 2)
                            {
                                if(Blockchain.GetBlock(Requested).CurrentHash == Messages[3])
                                {
                                    Blockchain.SyncBlocks();
                                }
                                else
                                {
                                    Blockchain.DelBlock(Requested);
                                    Send(RandomClient(), null, new[] { "Block", "Hash", (Requested - 1).ToString() });
                                }
                            }
                            else
                            {
                                if (Blockchain.BlockExists(Requested))
                                {
                                    Send(TcpClient, UdpClient, new[] { "Block", "Hash", Blockchain.GetBlock(Requested).CurrentHash });
                                }
                            }
                        }
                        if (Messages[1].ToLower() == "get")
                        {
                            if (Blockchain.BlockExists(uint.Parse(Messages[2])))
                            {
                                Block OldBlock = Blockchain.GetBlock(uint.Parse(Messages[2]));
                                string[] BlockData = new string[OldBlock.Transactions.Length * 7 + 9];

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
                                Send(TcpClient, UdpClient, BlockData);
                            }
                        }
                        if (Messages[1].ToLower() == "set")
                        {
                            if (Messages.Length > 2)
                            {
                                lock (Mining.PendingTransactions)
                                {
                                    if (!Blockchain.BlockExists(uint.Parse(Messages[2])))
                                    {
                                        if(Blockchain.BlockExists(uint.Parse(Messages[2])-1))
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

                                            bool LatestBlock = NewBlock.BlockHeight == Blockchain.CurrentHeight + 1;

                                            if (NewBlock.CheckBlockCorrect())
                                            {
                                                if (LatestBlock)
                                                {
                                                    while(NewBlock.Timestamp > (ulong)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds())
                                                    {
                                                        Thread.Sleep(100);
                                                    }
                                                    
                                                    if(NewBlock.Timestamp + NewBlock.Difficulty > (ulong)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds())
                                                    {
                                                        Blockchain.SetBlock(NewBlock);
                                                        Broadcast(Messages/*, TcpClient*/); // #TOBEREMOVED#
                                                        
                                                        if (!Mining.PauseMining && !Blockchain.Synchronising)
                                                        {
                                                            Mining.PrepareToMining(NewBlock);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Blockchain.SetBlock(NewBlock); 
                                                }
                                            }
                                            else
                                            {
                                                Send(RandomClient(), null, new[] { "Block", "Hash", NewBlock.BlockHeight.ToString() });
                                            }
                                        }
                                        else
                                        {
                                            Task.Run(() => Blockchain.SyncBlocks());
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
                        bool A = Messages[2].Length <= 25 && Blockchain.CurrentHeight >= 1000; // Unlock nicknames at 1k
                        bool B = Messages[2].Length >= 99 && Blockchain.CurrentHeight >= 10000; // Unlock avatars at 10k
                        bool C = Messages[2].Length == 88 && Blockchain.CurrentHeight >= 100000; // Unlock transactions at 100k
                        
                        if(A || B || C)
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
                                Send(TcpClient, UdpClient, new[] { "Account", "Info", "Set", Wallets.GetBalance(Messages[3]).ToString(), Wallets.GetName(Messages[3]), Wallets.GetAvatar(Messages[3])});
                            }
                        }
                        if (Messages[1].ToLower() == "search")
                        {
                            if (Messages[2].ToLower() == "request")
                            {
                                if (Messages[3].ToLower() == "nickname")
                                {
                                    Send(TcpClient, UdpClient, new[] { "Account", "Search", "Result", "Nickname", Messages[4], Messages[5], Wallets.GetAddress(Messages[4], byte.Parse(Messages[5]))});
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
                                Send(TcpClient, UdpClient, new[] { "Service", "Info", "Set", Messages[3], Wallets.GetName(Messages[3]), Wallets.GetAvatar(Messages[3]) });
                            }
                        }
                    }
                    break;
                }
            }
        }

    }
}
