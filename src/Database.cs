using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using MySqlConnector;

namespace OneCoin
{
    class Database
    {
        
        public static string DatabaseHost = "";
        public static string DatabaseUser = "";
        public static string DatabasePass = "";
        public static string DatabaseDb = "";
        
        static MySqlConnection Connection;
        static object MySqlLock = new();
        
        public static bool StoreUsersData = false;
        public static bool StoreBlocksData = false;
        public static bool StorePoolData = false;
        
        public static void Connect()
        {
            lock(MySqlLock)
            {
                string ConnectionString = "Server=" + DatabaseHost + ";" + "UserId=" + DatabaseUser + ";" + "Password=" + DatabasePass + ";" + "Database=" + DatabaseDb + ";" + "SslMode=None";
                Connection = new MySqlConnection(ConnectionString);
                Connection.Open();
            }
        }
        
        public static void Disconnect()
        {
            lock(MySqlLock)
            {
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }
        }
        
        public static void SpecialCommand(string Command)
        {
            lock(MySqlLock)
            {
                MySqlCommand Cmd = Connection.CreateCommand();
                Cmd.CommandText = Command;
                Cmd.ExecuteNonQuery();
            }
        }
        
        public static string[][] Get(string Table, string Column = "*", string When = "", string Order = "", byte Limit = 0)
        {
            DataTable Result = new();
            
            lock(MySqlLock)
            {
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "SELECT " + Column + " FROM " + Table;
                if (When != "")
                {
                    Command.CommandText += " WHERE " + When;
                }
                if (Order != "")
                {
                    Command.CommandText += " ORDER BY " + Order;
                }
                if (Limit > 0)
                {
                    Command.CommandText += " LIMIT " + Limit;
                }
                Result.Load(Command.ExecuteReader());
            }
            if (Result.Rows.Count > 0 && Result.Columns.Count > 0)
            {
                object[][] Objects = Result.AsEnumerable().Select(x => x.ItemArray).ToArray();
                string[][] Results = new string[Objects.Length][];
                for (int i = 0; i < Objects.Length; i++)
                {
                    Results[i] = new string[Objects[i].Length];
                    for (int j = 0; j < Objects[i].Length; j++)
                    {
                        Results[i][j] = Objects[i][j].ToString();
                    }
                }
                return Results;
            }
            return new[] { new []{ "" } };
        }

        public static int Set(string Table, string ColumnValues, string When)
        {
            int Result;
            lock(MySqlLock)
            {
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "UPDATE " + Table + " SET " + ColumnValues + " WHERE " + When;
                Result = Command.ExecuteNonQuery();
            }
            return Result;
        }

        public static int Add(string Table, string Columns, string Values)
        {
            int Result;
            lock(MySqlLock)
            {
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "INSERT INTO " + Table + " (" + Columns + ") VALUES (" + Values + ")";
                Result = Command.ExecuteNonQuery();
            }
            return Result;
        }

        public static int Del(string Table, string Where)
        {
            int Result;
            lock(MySqlLock)
            {
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "DELETE FROM " + Table + " WHERE " + Where;
                Result = Command.ExecuteNonQuery();
            }
            return Result;
        }
        
        public static void DatabaseSync()
        {
            try
            { 
                Connect();
                SpecialCommand("SET @@sql_mode = '';"); // To prevent error on empty strings.
                
                uint DbHeight = 0;
                
                if(StoreBlocksData)
                {
                    string Tmp = Get("blocks", "BlockHeight", "", "BlockHeight DESC", 1)[0][0];
                    if(Tmp.Length < 1)
                    {
                        Tmp = "0";
                        Add("blocks", "BlockHeight, PreviousHash, CurrentHash", "'0', '1ONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOIN0', '1ONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOINONECOIN0'");
                    }
                    DbHeight = uint.Parse(Tmp) + 1;
                }

                while(Blockchain.SyncMode)
                {
                    Thread.Sleep(1000);
                }
                
                for (uint i = 0; i < Blockchain.CurrentHeight; i++)
                {
                    Block ToInsert = Blockchain.GetBlock(i);
                    
                    for (int j = 0; j < ToInsert.Transactions.Length; j++)
                    {      
                        if(ToInsert.Transactions[j].To.Length == 88)
                        {
                            if(!Blockchain.AllKnownAddresses.Contains(ToInsert.Transactions[j].To))
                            {
                                Blockchain.AllKnownAddresses.Add(ToInsert.Transactions[j].To);
                            }
                        }
                    }
                }
                
                while(DatabaseHost.Length > 1)
                {
                    if(StoreUsersData)
                    {
                        for(int i = 0; i < Blockchain.AllKnownAddresses.Count; i++)
                        {
                            if(Program.DebugLogging)
                            {
                                Console.WriteLine("Updating user: " + Blockchain.AllKnownAddresses[i]);
                            }
                            
                            string[] Temp = (Wallets.GetName(Blockchain.AllKnownAddresses[i]) + "|1").Split("|");
                            Temp = new [] { Wallets.GetBalance(Blockchain.AllKnownAddresses[i]).ToString(), Temp[0], Temp[1], Wallets.GetAvatar(Blockchain.AllKnownAddresses[i]) };
                            
                            if(Set("onecoin", "Balance='" + Temp[0] + "', Nickname='" + Temp[1] + "', Tag='" + Temp[2] + "', Avatar='" + Temp[3] + "'", "Address='" + Blockchain.AllKnownAddresses[i] + "'") < 1)
                            {
                                Add("onecoin", "Address, Balance, Nickname, Tag, Avatar", "'" + Blockchain.AllKnownAddresses[i] + "', '" + Temp[0] + "', '" + Temp[1] + "', '" + Temp[2] + "', '" + Temp[3] + "'");
                            }
                            
                            if(Blockchain.TempAvatarsPath.Length > 1 && Temp[3].Length > 9)
                            {
                                Media.TextToImage(Temp[3]).Save(Blockchain.TempAvatarsPath + "/" + Blockchain.AllKnownAddresses[i] + ".png", ImageFormat.Png);
                            }
                            if(Blockchain.TempQrCodesPath.Length > 1)
                            {
                                Bitmap QrCode = Wallets.GenerateQrCode(Blockchain.AllKnownAddresses[i]);
                                new Bitmap(QrCode, QrCode.Width*4, QrCode.Height*4).Save(Blockchain.TempQrCodesPath + "/" + Blockchain.AllKnownAddresses[i] + ".png", ImageFormat.Png);
                            }
                            
                            Thread.Sleep(1000);
                        }
                    
                        Blockchain.AllKnownAddresses.Clear();
                    }
                    
                    if(StoreBlocksData)
                    {
                        for (; DbHeight < Blockchain.CurrentHeight; DbHeight++)
                        {
                            Block ToInsert = Blockchain.GetBlock(DbHeight);
                            
                            string Hash = Get("blocks", "CurrentHash", "BlockHeight='" + (DbHeight - 1) + "'")[0][0];
                            
                            if(Hash == ToInsert.PreviousHash)
                            {
                                if(Program.DebugLogging)
                                {
                                    Console.WriteLine("Adding block and transactions to database: " + DbHeight);
                                }
                                
                                Add("blocks", "BlockHeight, PreviousHash, CurrentHash, Timestamp, Difficulty, Message, ExtraData", "'" + ToInsert.BlockHeight + "', '" + ToInsert.PreviousHash + "', '" + ToInsert.CurrentHash + "', '" + ToInsert.Timestamp + "', '" + ToInsert.Difficulty + "', '" + ToInsert.ExtraData.Split('|')[1] + "', '" + ToInsert.ExtraData.Split('|')[2] + "'");
                                
                                if(Blockchain.TempBlocksPath.Length > 1)
                                {
                                    Media.TextToImage(ToInsert.ExtraData.Split('|')[0]).Save(Blockchain.TempBlocksPath + "/" + ToInsert.BlockHeight + ".png", ImageFormat.Png);
                                }

                                for (int j = 0; j < ToInsert.Transactions.Length; j++)
                                {
                                    Thread.Sleep(100);
                                
                                    Add("transactions", "BlockHeight, AddressFrom, AddressTo, Amount, Fee, Timestamp, Message, Signature", "'" + ToInsert.BlockHeight + "', '" + ToInsert.Transactions[j].From + "', '" + ToInsert.Transactions[j].To + "', '" + ToInsert.Transactions[j].Amount + "', '" + ToInsert.Transactions[j].Fee + "', '" + ToInsert.Transactions[j].Signature + "', '" + ToInsert.Transactions[j].Message + "', '" + ToInsert.Transactions[j].Signature + "'");
                                    
                                    if(ToInsert.Transactions[j].From.Length == 88)
                                    {
                                        if(!Blockchain.AllKnownAddresses.Contains(ToInsert.Transactions[j].From))
                                        {
                                            Blockchain.AllKnownAddresses.Add(ToInsert.Transactions[j].From);
                                        }
                                    }
                                    
                                    if(ToInsert.Transactions[j].To.Length == 88)
                                    {
                                        if(!Blockchain.AllKnownAddresses.Contains(ToInsert.Transactions[j].To))
                                        {
                                            Blockchain.AllKnownAddresses.Add(ToInsert.Transactions[j].To);
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
                                
                                Del("blocks", "BlockHeight='" + DbHeight + "'");
                                Del("transactions", "BlockHeight='" + DbHeight + "'");
                                
                                DbHeight--;
                            }
                            
                            Thread.Sleep(1000);
                        } 
                    }
                    
                    if(StorePoolData)
                    {
                        if(Pool.ShareHeight != Blockchain.CurrentHeight)
                        {
                            if(Pool.ShareHeight > 1)
                            {
                                Pool.SynchronisingNow = true;

                                foreach(KeyValuePair<string, byte> Shares in Pool.MinersShares)
                                {
                                    Add("shares", "Address, BlockHeight, ValidShares, StaleShares, InvalidShares", "'" + Shares.Key + "', '" + (Pool.ShareHeight + 1) + "', '" + Shares.Value + "', '0', '0'");
                                }
                                Pool.MinersShares.Clear();

                                Pool.SynchronisingNow = false;
                            }
                            Pool.ShareHeight = Blockchain.CurrentHeight;
                            
                            Thread.Sleep(1000);
                        }
                    }
                }
                
                Disconnect();
                
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex);
            }
        }
    }
}