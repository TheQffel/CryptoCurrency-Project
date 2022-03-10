using System;
using System.Numerics;

namespace OneCoin
{
    class Transaction
    {
        public string From;
        public string To;
        public BigInteger Amount;
        public ulong Fee;
        public ulong Timestamp;
        public string Signature;
        public string Message;

        public string Hash() { return Hash("", ""); }
        
        public override string ToString()
        {
            if(Message == null) { Message = ""; }
            
            if(Message.Length > 0)
            {
                return From + "|" + To + "|" + Amount + "|" + Fee + "|" + Timestamp + "|" + Message;
            }
            else
            {
                return From + "|" + To + "|" + Amount + "|" + Fee + "|" + Timestamp;
            }
        }
        
        public string Hash(string Prefix, string Suffix)
        {
            string ToHash = ToString() + "|" + Signature;
            if (Prefix.Length > 0) { ToHash = Prefix + " " + ToHash; }
            if (Suffix.Length > 0) { ToHash = ToHash + " " + Suffix; }
            return Hashing.TqHash(ToHash);
        }

        public bool VerifySignature()
        {
            return Wallets.VerifySignature(ToString(), Wallets.AddressToLong(From), Signature);
        }

        public void GenerateSignature(string Key)
        {
            Signature = Wallets.GenerateSignature(Key, ToString());
        }
        
        public bool CheckTransactionCorrect(BigInteger Balance, uint Height, long NodeId = -1)
        {
            bool Correct = From != To; if(Program.DebugLogging && !Correct) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Addresses are the same!"); } 
            if(Fee > 1000000000000000000) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Too high fee!"); } }
            if(Fee >= Amount) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Fee higher than amount!"); } }
            if (Amount < 1) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Amount less than 1"); } }
            if (Balance < Amount + Fee) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Not enough balance!"); } }
            if(Message == null) { Message = ""; }
            
            if (!Hashing.CheckStringFormat(Message, 5, 0, 256)) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Message contains invalid characters!"); } }
            if (!Wallets.CheckAddressCorrect(From)) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: From address is incorrect!"); } }
            if (!Wallets.CheckAddressCorrect(To))
            {
                if (Hashing.CheckStringFormat(To, 5, 4, 24))
                {
                    if (Amount != BigInteger.Pow(2, 24 - To.Length)) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Wrong nickname price!"); } }
                    if (Message != null) { if(Message.Length > 0) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Nickname cannot contain message!"); } }}
                }
                else if (Hashing.CheckStringFormat(To, 5, 128, 1048576))
                {
                    if (Amount != To.Length) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Wrong avatar price!"); } }
                    if (!Media.ImageDataCorrect(To)) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Image is incorrect!"); } }
                    if (Message != null) { if(Message.Length > 0) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Avatar cannot contain message!"); } }}
                }
                else { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: To address is incorrect!"); } }
            }

            if(Signature.Length != 205)
            {
                if (!VerifySignature()) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Wrong signature!"); } }
            }
            else
            {
                if(Hashing.TqHash(ToString()) != Signature) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Signature not match hash!"); } }
                uint Inactivity = Height + 1 - Wallets.GetLastUsedBlock(From, NodeId, Height);
                int Difficulty = 250 - (int)(Inactivity / 100000);
                if(Difficulty < 1) { Difficulty = 1; }
                if(!Mining.CheckSolution(Signature, (byte)Difficulty)) { Correct = false; if(Program.DebugLogging) { Console.WriteLine("Transaction " + Signature[..5] + Signature[^5..] + " is incorrect: Solution not good enough!"); } }
            }

            return Correct;
        }
    }
}
