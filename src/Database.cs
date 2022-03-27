using System;
using System.Data;
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
        
        public static void ConnectionLoop()
        {
            bool Loop = true;
            
            while(Loop)
            {
                try
                {
                    Connect();
                    SpecialCommand("SET @@sql_mode = '';");
                    while (DatabaseHost.Length > 5)
                    {
                        Thread.Sleep(1000);
                    }
                    Loop = false;
                    Disconnect();
                }
                catch(Exception Ex)
                {
                    if(Program.DebugLogging)
                    {
                        Console.WriteLine(Ex);
                    }
                }
            }
        }
        
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
                try
                {
                    MySqlCommand Cmd = Connection.CreateCommand();
                    Cmd.CommandText = Command;
                    Cmd.ExecuteNonQuery();
                }
                catch (Exception Ex)
                {
                    if(Program.DebugLogging)
                    {
                        Console.WriteLine(Ex);
                    }
                }
            }
        }
        
        public static string[][] Get(string Table, string Column = "*", string When = "", string Order = "", byte Limit = 0)
        {
            DataTable Result = new();
            
            lock(MySqlLock)
            {
                try
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
                catch (Exception Ex)
                {
                    if(Program.DebugLogging)
                    {
                        Console.WriteLine(Ex);
                    }
                }
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
            int Result = -1;
            lock(MySqlLock)
            {
                try
                {
                    MySqlCommand Command = Connection.CreateCommand();
                    Command.CommandText = "UPDATE " + Table + " SET " + ColumnValues + " WHERE " + When;
                    Result = Command.ExecuteNonQuery();
                }
                catch (Exception Ex)
                {
                    if(Program.DebugLogging)
                    {
                        Console.WriteLine(Ex);
                    }
                }
            }
            return Result;
        }

        public static int Add(string Table, string Columns, string Values)
        {
            int Result = -1;
            lock(MySqlLock)
            {
                try
                {
                    MySqlCommand Command = Connection.CreateCommand();
                    Command.CommandText = "INSERT INTO " + Table + " (" + Columns + ") VALUES (" + Values + ")";
                    Result = Command.ExecuteNonQuery();
                }
                catch (Exception Ex)
                {
                    if(Program.DebugLogging)
                    {
                        Console.WriteLine(Ex);
                    }
                }
            }
            return Result;
        }

        public static int Del(string Table, string Where)
        {
            int Result = -1;
            lock(MySqlLock)
            {
                try
                {
                    MySqlCommand Command = Connection.CreateCommand();
                    Command.CommandText = "DELETE FROM " + Table + " WHERE " + Where;
                    Result = Command.ExecuteNonQuery();
                }
                catch (Exception Ex)
                {
                    if(Program.DebugLogging)
                    {
                        Console.WriteLine(Ex);
                    }
                }
            }
            return Result;
        }
    }
}