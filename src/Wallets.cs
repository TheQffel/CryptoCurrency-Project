using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using ZXing;

namespace OneCoin
{
    class Wallets
    {
        public static string[] AddressEncoding = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", " ", "|" };
        public static ulong[] MinerRewards = { 423539247696576513, 288230376151711744, 144115188075855872, 72057594037927936, 36028797018963968, 18014398509481984, 9007199254740992, 4503599627370496, 2251799813685248, 1125899906842624, 562949953421312, 281474976710656, 140737488355328, 70368744177664, 35184372088832, 17592186044416, 8796093022208, 4398046511104, 2199023255552, 1099511627776, 549755813888, 274877906944, 137438953472, 68719476736, 34359738368, 17179869184, 8589934592, 4294967296, 2147483648, 1073741824, 536870912, 268435456, 134217728, 67108864, 33554432, 16777216, 8388608, 4194304, 2097152, 1048576, 524288, 262144, 131072, 65536, 32768, 16384, 8192, 4096, 2048, 1024, 512, 256, 128, 64, 32, 16, 8, 4, 2, 1, 0, 0, 0, 0 };
        
        public static uint LastCacheHeight = 0;
        public static bool CacheUpdateInProgress = false;
        
        public static Dictionary<string, KeyValuePair<string, uint>> NicknameCache = new();
        public static Dictionary<string, KeyValuePair<string, uint>> AvatarsCache = new();
        
        public static void UpdateNicknamesAvatarsCache()
        {
            if(!CacheUpdateInProgress)
            {
                CacheUpdateInProgress = true;

                for (uint i = LastCacheHeight; i < Blockchain.CurrentHeight - 10; i++)
                {
                    Block OneCoinBlock = Blockchain.GetBlock(i);

                    for (int j = 1; j < OneCoinBlock.Transactions.Length; j++)
                    {
                        if(OneCoinBlock.Transactions[j].To.Length < 50)
                        {
                            NicknameCache[OneCoinBlock.Transactions[j].From] = new KeyValuePair<string, uint>(OneCoinBlock.Transactions[j].To, i);
                        }
                        if(OneCoinBlock.Transactions[j].To.Length > 100)
                        {
                            AvatarsCache[OneCoinBlock.Transactions[j].From] = new KeyValuePair<string, uint>(OneCoinBlock.Transactions[j].To, i);
                        }
                    }
                }
                
                CacheUpdateInProgress = false;
            }
        }
        
        public static string GetName(string Address)
        {
            string Nickname = "";
            UpdateNicknamesAvatarsCache();
            
            if(NicknameCache.ContainsKey(Address))
            {
                if(GetBalance(Address) > 0)
                {
                    Nickname = NicknameCache[Address].Key;
                    byte Tag = 1;

                    foreach(KeyValuePair<string, KeyValuePair<string, uint>> Entry in NicknameCache)
                    {
                        if(Entry.Value.Key.Replace(" ", "").ToLower() == Nickname.Replace(" ", "").ToLower())
                        {
                            if(Entry.Value.Value < NicknameCache[Address].Value)
                            {
                                Tag++;
                            }
                        }
                    }
                    
                    Nickname = Nickname + "|" + Tag;
                }
            }
            return Nickname;
        }
        
        public static string GetAvatar(string Address)
        {
            string Avatar = "";
            UpdateNicknamesAvatarsCache();
            
            if(AvatarsCache.ContainsKey(Address))
            {
                if(GetBalance(Address) > 0)
                {
                    Avatar = AvatarsCache[Address].Key;
                }
            }
            return Avatar;
        }
        
        public static string GetAddress(string Name, byte Tag = 1)
        {
            string Address = "";
            UpdateNicknamesAvatarsCache();
            List<KeyValuePair<string, uint>> Addresses = new();

            foreach(KeyValuePair<string, KeyValuePair<string, uint>> Entry in NicknameCache)
            {
                if(Entry.Value.Key.Replace(" ", "").ToLower() == Name.Replace(" ", "").ToLower())
                {
                    Addresses.Add(new KeyValuePair<string, uint>(Entry.Key, Entry.Value.Value));
                }
            }
            if(Addresses.Count >= Tag)
            {
                Addresses.Sort((x, y) => x.Value.CompareTo(y.Value));
                Address = Addresses[Tag-1].Key;
            }
            return Address;
        }

        public static BigInteger GetBalance(string Address, long NodeId = -1, uint BlockHeight = 0)
        {
            if(BlockHeight == 0)
            {
                BlockHeight = Blockchain.CurrentHeight;
            }
            
            BigInteger Balance = 0;

            for (uint i = 1; i <= BlockHeight; i++)
            {
                Block OneCoinBlock = Blockchain.GetBlock(i, NodeId);

                for (int j = 0; j < OneCoinBlock.Transactions.Length; j++)
                {
                    if (OneCoinBlock.Transactions[j].From == Address)
                    {
                        Balance -= OneCoinBlock.Transactions[j].Amount;
                    }

                    if (OneCoinBlock.Transactions[j].To == Address)
                    {
                        Balance += OneCoinBlock.Transactions[j].Amount;
                    }

                    if (OneCoinBlock.Transactions[0].To == Address)
                    {
                        if (OneCoinBlock.Transactions[j].To.Length != 88)
                        {
                            Balance += OneCoinBlock.Transactions[j].Amount;
                        }
                        Balance += OneCoinBlock.Transactions[j].Fee;
                    }
                }
            }
            return Balance;
        }

        public static uint GetLastUsedBlock(string Address, long NodeId = -1, uint BlockHeight = 0)
        {
            if(BlockHeight == 0)
            {
                BlockHeight = Blockchain.CurrentHeight;
            }
            
            uint LastUse = 0;

            for (uint i = 1; i <= BlockHeight; i++)
            {
                Block OneCoinBlock = Blockchain.GetBlock(i, NodeId);
                
                for (int j = 0; j < OneCoinBlock.Transactions.Length; j++)
                {
                    if (OneCoinBlock.Transactions[j].From == Address)
                    {
                        LastUse = i;
                    }

                    if (OneCoinBlock.Transactions[j].To == Address)
                    {
                        if(LastUse == 0)
                        {
                            LastUse = i;
                        }
                    }
                }
            }
            return LastUse;
        }

        public static bool CheckTransactionAlreadyIncluded(Transaction Transaction, long NodeId = -1, uint BlockHeight = 0)
        {
            if(BlockHeight == 0)
            {
                BlockHeight = Blockchain.CurrentHeight;
            }
            
            for (uint i = BlockHeight; i > 0; i--)
            {
                Block OneCoinBlock = Blockchain.GetBlock(i, NodeId);

                if (OneCoinBlock.Timestamp < Transaction.Timestamp) { return false; }

                for (int j = 0; j < OneCoinBlock.Transactions.Length; j++)
                {
                    if(OneCoinBlock.Transactions[j].Hash() == Transaction.Hash())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string AddressToShort(string Address)
        {
            if(Address == null) { Address = " "; }
            if(Address.Length > 9)
            {
                int Total = 0;
                String Binary = "";
                Address = Address[2..];
                for(short i = 0; i < Address.Length; i++)
                {
                    char Letter = Address[i];
                    if(Letter == '0') { Binary += "0000"; Total += i * 0; }
                    if(Letter == '1') { Binary += "0001"; Total += i * 1; }
                    if(Letter == '2') { Binary += "0010"; Total += i * 2; }
                    if(Letter == '3') { Binary += "0011"; Total += i * 3; }
                    if(Letter == '4') { Binary += "0100"; Total += i * 4; }
                    if(Letter == '5') { Binary += "0101"; Total += i * 5; }
                    if(Letter == '6') { Binary += "0110"; Total += i * 6; }
                    if(Letter == '7') { Binary += "0111"; Total += i * 7; }
                    if(Letter == '8') { Binary += "1000"; Total += i * 8; }
                    if(Letter == '9') { Binary += "1001"; Total += i * 9; }
                    if(Letter == 'a') { Binary += "1010"; Total += i * 10; }
                    if(Letter == 'b') { Binary += "1011"; Total += i * 11; }
                    if(Letter == 'c') { Binary += "1100"; Total += i * 12; }
                    if(Letter == 'd') { Binary += "1101"; Total += i * 13; }
                    if(Letter == 'e') { Binary += "1110"; Total += i * 14; }
                    if(Letter == 'f') { Binary += "1111"; Total += i * 15; }
                }
                Binary += Convert.ToString(Total % 65536, 2).PadLeft(16, '0');
                String Result = "";
                for(int i = 0; i < 88; i++)
                {
                    String Letter = Binary.Substring(i*6, 6);
                    Result += AddressEncoding[Convert.ToInt16(Letter, 2)];
                }
                return Result;
            }
            return " ";
        }

        public static string AddressToLong(string Address)
        {
            if(Hashing.CheckStringFormat(Address, 4, 88, 88))
            {
                int Total = 0;
                String Binary = "";
                for(int i = 0; i < Address.Length; i++)
                {
                    char Letter = Address[i];
                    for (short j = 0; j < AddressEncoding.Length; j++)
                    {
                        if(Letter == AddressEncoding[j][0]) { Binary += Convert.ToString(j, 2).PadLeft(6, '0'); }
                    }
                }
                String Result = "04";
                for(int i = 0; i < 128; i++)
                {
                    String Letters = "0000" + Binary.Substring(i*4, 4);
                    short Value = Convert.ToInt16(Letters, 2);
                    Result += AddressEncoding[Value];
                    Total += Value * i;
                }
                if(Convert.ToString(Total % 65536, 2).PadLeft(16, '0') == Binary[512..])
                {
                    return Result.ToLower();
                }
            }
            return " ";
        }
        
        public static bool CheckAddressCorrect(string Address)
        {
            return AddressToLong(Address).Length > 9;
        }

        public static void PrintQrCode(string Address)
        {
            Bitmap QrCode = GenerateQrCode(Address);
            Console.ForegroundColor = ConsoleColor.Black;
            for (int i = 10; i <= QrCode.Width - 10; i += 2)
            {
                Console.Write("[]");
                for (int j = 10; j <= QrCode.Height - 10; j += 2)
                {
                    if (QrCode.GetPixel(i, j) == Color.FromArgb(255, 255, 255, 255))
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("[]");
                    }
                    if (QrCode.GetPixel(i, j) == Color.FromArgb(255, 0, 0, 0))
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("[]");
                    }
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.WriteLine("[]");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        
        public static Bitmap GenerateQrCode(string Address)
        {
            BarcodeWriter Barcode = new();
            Barcode.Format = BarcodeFormat.QR_CODE;
            return Barcode.Write(Address);
        }

        public static bool VerifySignature(string Message, string PublicKey, string Signature)
        {
            if(!CheckAddressCorrect(AddressToShort(PublicKey))) { return false; }
            var Curve = SecNamedCurves.GetByName("secp256k1");
            var Domain = new ECDomainParameters(Curve.Curve, Curve.G, Curve.N, Curve.H);
            var PublicKeyBytes = Hashing.HexToBytes(PublicKey);
            var Q = Curve.Curve.DecodePoint(PublicKeyBytes);
            var KeyParameters = new ECPublicKeyParameters(Q, Domain);
            ISigner Signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            Signer.Init(false, KeyParameters);
            Signer.BlockUpdate(Encoding.ASCII.GetBytes(Message), 0, Message.Length);
            var SignatureBytes = Hashing.HexToBytes(Signature);
            if(SignatureBytes == null) { return false; }
            return Signer.VerifySignature(SignatureBytes);
        }

        public static string GenerateSignature(string PrivateKey, string Message)
        {
            var Curve = SecNamedCurves.GetByName("secp256k1");
            var Domain = new ECDomainParameters(Curve.Curve, Curve.G, Curve.N, Curve.H);
            var KeyParameters = new ECPrivateKeyParameters(new Org.BouncyCastle.Math.BigInteger(PrivateKey, 16), Domain);
            ISigner Signer = SignerUtilities.GetSigner("SHA-256withECDSA");
            Signer.Init(true, KeyParameters);
            Signer.BlockUpdate(Encoding.ASCII.GetBytes(Message), 0, Message.Length);
            return Hashing.BytesToHex(Signer.GenerateSignature());
        }
    }
}
