using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace OneCoin
{
    class Block
    {
        public uint BlockHeight = 1;
        public string PreviousHash = "";
        public string CurrentHash = "";
        public ulong Timestamp = 1;
        public byte Difficulty = 1;
        public string ExtraData = "||";

        public Transaction[] Transactions = Array.Empty<Transaction>();
        public string TransactionsHash;

        public override string ToString()
        {
            if(CheckBlockCorrect())
            {
                string Temp = ExtraData.Split("|")[1];
                if (Temp.Length < 20) { Temp = new string(' ', 20 - Temp.Length) + Temp; }
                string Text = "╔═════════════════════════════╗\n";
                Text += "║ Height: " + new string(' ', 19 - BlockHeight.ToString().Length) + BlockHeight + " ║\n";
                Text += "║ Timestamp: " + new string(' ', 16 - Timestamp.ToString().Length) + Timestamp + " ║\n";
                Text += "║ Difficulty: " + new string(' ', 15 - Difficulty.ToString().Length) + Difficulty + " ║\n";
                Text += "║ Image: Not possible to view ╚" + new string('═', Temp.Length - 17) + "╗\n";
                Text += "║ Signature: " + Temp + " ╚" + new string('═', 208 - Temp.Length) + "╗\n";
                Text += "║ Previous Hash: " + PreviousHash + " ║\n";
                Text += "║ Current Hash:  " + CurrentHash + " ║\n";
                Text += "╠════════════╤══════════════════════════════════════════════════════════════════════════════════════════╤══════════════════════════════════════════════════════════════════════════════════════════╤═══════════════════════════╩╤══════════════════════╗\n";
                Text += "║ Timestamp: │  Address - From:                                                                         │  Address - To:                                                                           │  Amount:                   │  Fee:                ║\n";
                Text += "╟────────────┼──────────────────────────────────────────────────────────────────────────────────────────┼──────────────────────────────────────────────────────────────────────────────────────────┼────────────────────────────┼──────────────────────╢\n";

                for (int i = 0; i < Transactions.Length; i++)
                {
                    string[] Temps = { Transactions[i].Timestamp.ToString(), Transactions[i].From, Transactions[i].To, Transactions[i].Amount.ToString(), Transactions[i].Fee.ToString(), Transactions[i].Signature };

                    if (Temps[0].Length < 10) { Temps[0] += new string(' ', 10 - Temps[0].Length); }
                    if (Temps[1].Length < 85) { Temps[1] += new string(' ', 88 - Temps[1].Length); }
                    if (Temps[2].Length >= 4 && Temps[2].Length <= 24) { Temps[2] = "[Nickname]: " + Temps[2]; }
                    if (Temps[2].Length >= 128 && Temps[2].Length <= 1048576) { Temps[2] = "[Avatar]: (Avatar Data)"; }
                    if (Temps[2].Length < 85) { Temps[2] += new string(' ', 88 - Temps[2].Length); }
                    if (Temps[3].Length < 24) { Temps[3] = new string('0', 24 - Temps[3].Length) + Temps[3]; }
                    if (Temps[4].Length < 18) { Temps[4] = new string('0', 18 - Temps[4].Length) + Temps[4]; }

                    Text += "║ " + Temps[0] + " │ " + Temps[1] + " │ " + Temps[2] + " │ 0." + Temps[3] + " │ 0." + Temps[4] + " ║\n";
                }

                Text += "╚════════════╧══════════════════════════════════════════════════════════════════════════════════════════╧══════════════════════════════════════════════════════════════════════════════════════════╧════════════════════════════╧══════════════════════╝\n";

                return Text + "* To view block image use another (graphical, not command line) blockchain explorer.";
            }
            return "Block at height " + BlockHeight + " is incorrect!";
        }

        public void RecalculateTransactions()
        {
            TransactionsHash = Transactions[0].Hash();

            for (int i = 1; i < Transactions.Length; i++)
            {
                TransactionsHash = Transactions[i].Hash("", TransactionsHash);
            }
        }

        public void RecalculateHash(bool CalculateTransactionsHash = true)
        {
            if(CalculateTransactionsHash)
            {
                RecalculateTransactions();
            }

            CurrentHash = Hashing.TqHash(BlockHeight + " " + PreviousHash + " " + Timestamp + " " + Difficulty + " " + ExtraData + " " + TransactionsHash);

            long HistoricalHeight = BlockHeight;
            string HistoryHash = CurrentHash;

            for (int i = 0; i < HistoryHash.Length; i++)
            {
                HistoricalHeight -= ((long)Hashing.HashEncodingIndex(HistoryHash[i]) + 1 + i) * Hashing.Primes[Difficulty + i];
                
                if (HistoricalHeight > 0)
                {
                    Block HistoricalBlock = Blockchain.GetBlock((uint)HistoricalHeight);

                    long TransactionNumber = BlockHeight + (long)Hashing.SumHash(CurrentHash);

                    for (int j = 0; j < HistoricalBlock.CurrentHash.Length; j++)
                    {
                        TransactionNumber += (long)Hashing.HashEncodingIndex(HistoricalBlock.CurrentHash[j]) * (j + 1);
                    }
                    TransactionNumber %= HistoricalBlock.Transactions.Length;
                    
                    CurrentHash = HistoricalBlock.Transactions[TransactionNumber].Hash(CurrentHash, "");
                }
                else { break; }
            }
        }


        public bool CheckBlockCorrect()
        {
            // Mode Types: 1: Only length checking.
            // 2: Hex (Lower), 3: Base32 (Upper),
            // 4: Base64 (No Spaces, No Separators),
            // 5: Base64 (Spaces, No Separators),
            // 6: Base64 (No Spaces, Separators),
            // 7: Base64 (Spaces, Separators)

            if (BlockHeight <= 1) { return true; }

            Block PreviousBlock = Blockchain.GetBlock(BlockHeight - 1);
            bool Correct = true;

            RecalculateHash();
            
            if (PreviousHash != PreviousBlock.CurrentHash) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Block " + BlockHeight + " is incorrect: Wrong previous hash."); } }
            if(!Mining.CheckSolution(CurrentHash, Difficulty)) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Block " + BlockHeight + " is incorrect: Wrong current hash."); } }

            if (!Hashing.CheckStringFormat(PreviousHash, 3, 205, 205)) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Block " + BlockHeight + " is incorrect: Wrong previous hash."); } }
            if (!Hashing.CheckStringFormat(CurrentHash, 3, 205, 205)) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Block " + BlockHeight + " is incorrect: Wrong current hash."); } }
            if (!Hashing.CheckStringFormat(ExtraData, 7, 4096, 4096)) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Block " + BlockHeight + " is incorrect: Wrong extra data."); } }

            string[] Extras = ExtraData.Split('|');
            if (Extras.Length != 3) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Block " + BlockHeight + " is incorrect: Wrong extras data number."); } }
            if (!Media.ImageDataCorrect(Extras[0])) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Block " + BlockHeight + " is incorrect: Wrong extra data image."); } }

            if (Timestamp <= PreviousBlock.Timestamp) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Block " + BlockHeight + " is incorrect: Wrong timestamp."); } }

            byte NextDifficulty = PreviousBlock.Difficulty;

            if (BlockHeight < 11) { NextDifficulty = (byte)BlockHeight; }
            else
            {
                ulong[] TimestampDifferences = new ulong[10];
                bool CanBeChanged = true;
                
                for (uint i = 0; i < 10; i++)
                {
                    if(Blockchain.GetBlock(BlockHeight - (i + 1)).Difficulty != Blockchain.GetBlock(BlockHeight - (i + 2)).Difficulty && i != 9)
                    {
                        CanBeChanged = false;
                    }
                    TimestampDifferences[i] = Blockchain.GetBlock(BlockHeight - (i + 1)).Timestamp - Blockchain.GetBlock(BlockHeight - (i + 2)).Timestamp;
                }
                
                if(CanBeChanged)
                {
                    if (TimestampDifferences.Max() < PreviousBlock.Difficulty) { NextDifficulty++; }
                    if (TimestampDifferences.Min() > (ulong)PreviousBlock.Difficulty * (ulong)PreviousBlock.Difficulty) { NextDifficulty--; }
                }
            }

            if (Difficulty != NextDifficulty) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Block " + BlockHeight + " is incorrect: Wrong difficulty. Expected: " + NextDifficulty + ", Is: " + Difficulty); } }

            Transaction MinerReward = new();
            MinerReward.From = Transactions[0].From;
            MinerReward.To = Transactions[0].To;
            MinerReward.Amount = Wallets.MinerRewards[(BlockHeight-1)/1000000];
            MinerReward.Fee = 0;
            MinerReward.Timestamp = BlockHeight;
            MinerReward.Message = "";
            MinerReward.Signature = BlockHeight + "";

            if (MinerReward.Hash() != Transactions[0].Hash()) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Block " + BlockHeight + " is incorrect: Wrong miner reward hash."); } }
            if (Transactions[0].From != "OneCoin") { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Block " + BlockHeight + " is incorrect: Wrong miner from data."); } }
            if (!Wallets.CheckAddressCorrect(Transactions[0].To)) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Block " + BlockHeight + " is incorrect: Wrong miner to data."); } }

            ulong PreviousTransactionTimestamp = 1;

            Dictionary<string, byte> UserTransactions = new();
            Dictionary<string, BigInteger> UserBalance = new();
            Dictionary<string, bool> NicknameAvatarChange = new();
            
            if(Correct)
            {
                for (int i = 1; i < Transactions.Length; i++)
                {
                    bool A = Transactions[i].To.Length <= 25 && BlockHeight < 1000; // Unlock nicknames at 1k
                    bool B = Transactions[i].To.Length >= 99 && BlockHeight < 10000; // Unlock avatars at 10k
                    bool C = Transactions[i].To.Length == 88 && BlockHeight < 100000; // Unlock transactions at 100k
                    if(A || B || C) { Console.WriteLine("Block " + BlockHeight + " contains locked actions."); return false; }
                    
                    if (!UserTransactions.ContainsKey(Transactions[i].From))
                    {
                        UserTransactions[Transactions[i].From] = 0;
                        UserBalance[Transactions[i].From] = Wallets.GetBalance(Transactions[i].From, BlockHeight - 1);
                        NicknameAvatarChange[Transactions[i].From] = false;
                    }
                    UserTransactions[Transactions[i].From]++;
                    if (UserTransactions[Transactions[i].From] > Difficulty) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Transaction " + i + " in " + BlockHeight + " is incorrect: Too much transactions."); } }
                    if (!Transactions[i].CheckTransactionCorrect(UserBalance[Transactions[i].From])) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Transaction " + i + " in " + BlockHeight + " is incorrect: Wrong transaction."); } }
                    UserBalance[Transactions[i].From] -= Transactions[i].Amount;

                    if (Transactions[i].Timestamp + 1000000 < Timestamp) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Transaction " + i + " in " + BlockHeight + " is incorrect: Wrong timestamp."); } }
                    if (Transactions[i].Timestamp < PreviousTransactionTimestamp) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Transaction " + i + " in " + BlockHeight + " is incorrect: Wrong timestamp."); } }
                    if (Transactions[i].Timestamp > Timestamp) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Transaction " + i + " in " + BlockHeight + " is incorrect: Wrong timestamp."); } }
                    PreviousTransactionTimestamp = Transactions[i].Timestamp;
                    
                    if(Transactions[i].To.Length != 88) { NicknameAvatarChange[Transactions[i].From] = true;  }
                    if(NicknameAvatarChange[Transactions[i].From] && UserTransactions[Transactions[i].From] > 1) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Transaction " + i + " in " + BlockHeight + " is incorrect: Account data change with transactions."); } }

                    if (Wallets.CheckTransactionAlreadyIncluded(Transactions[i], BlockHeight-1)) { Correct = false; if (Program.DebugLogging) { Console.WriteLine("Transaction " + i + " in " + BlockHeight + " is incorrect: Already included."); } }
                }
            }

            return Correct;
        }
    }
}
