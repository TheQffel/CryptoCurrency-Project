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
            string ToHash = ToString();
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
        
        public bool CheckTransactionCorrect(BigInteger Balance, uint Height)
        {
            bool Correct = From != To;
            if(Fee > 1000000000000000000) { Correct = false; }
            if(Fee >= Amount) { Correct = false; }
            if (Amount < 1) { Correct = false; }
            if (Balance < Amount) { Correct = false; }
            if(Message == null) { Message = ""; }
            
            if (!Hashing.CheckStringFormat(Message, 5, 0, 64)) { Correct = false; }
            if (!Wallets.CheckAddressCorrect(From)) { Correct = false; }
            if (!Wallets.CheckAddressCorrect(To))
            {
                if (Hashing.CheckStringFormat(To, 5, 4, 24))
                {
                    if (Amount != BigInteger.Pow(2, 24 - To.Length)) { Correct = false; }
                    if (Message != null) { if(Message.Length > 0) { Correct = false; }}
                }
                else if (Hashing.CheckStringFormat(To, 5, 128, 1048576))
                {
                    if (Amount != To.Length) { Correct = false; }
                    if (!Media.ImageDataCorrect(To)) { Correct = false; }
                    if (Message != null) { if(Message.Length > 0) { Correct = false; }}
                }
                else { Correct = false; }
            }

            if(Signature.Length != 205)
            {
                if (!VerifySignature()) { Correct = false; }
            }
            else
            {
                if(Hashing.TqHash(ToString()) != Signature) { Correct = false; }
                uint Inactivity = Height + 1 - Wallets.GetLastUsedBlock(From, Height);
                int Difficulty = 250 - (int)(Inactivity / 100000);
                if(Difficulty < 1) { Difficulty = 1; }
                if(!Mining.CheckSolution(Signature, (byte)Difficulty)) { Correct = false; }
            }

            return Correct;
        }
    }
}
