using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Crypto.Digests;

namespace OneCoin
{
    class Hashing
    {
        public static string[] HashEncoding = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "W", "Z" };
        public static short[] Primes = { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797, 809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887, 907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997, 1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069, 1087, 1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181, 1187, 1193, 1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291, 1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373, 1381, 1399, 1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499, 1511, 1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583, 1597, 1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657, 1663, 1667, 1669, 1693, 1697, 1699, 1709, 1721, 1723, 1733, 1741, 1747, 1753, 1759, 1777, 1783, 1787, 1789, 1801, 1811, 1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877, 1879, 1889, 1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979, 1987, 1993, 1997, 1999, 2003, 2011, 2017, 2027, 2029, 2039, 2053, 2063, 2069, 2081, 2083, 2087, 2089, 2099, 2111, 2113, 2129, 2131, 2137, 2141, 2143, 2153, 2161, 2179, 2203, 2207, 2213, 2221, 2237, 2239, 2243, 2251, 2267, 2269, 2273, 2281, 2287, 2293, 2297, 2309, 2311, 2333, 2339, 2341, 2347, 2351, 2357, 2371, 2377, 2381, 2383, 2389, 2393, 2399, 2411, 2417, 2423, 2437, 2441, 2447, 2459, 2467, 2473, 2477, 2503, 2521, 2531, 2539, 2543, 2549, 2551, 2557, 2579, 2591, 2593, 2609, 2617, 2621, 2633, 2647, 2657, 2659, 2663, 2671, 2677, 2683, 2687, 2689, 2693, 2699, 2707, 2711, 2713, 2719, 2729, 2731 };
        // Explanation: Number 1 (at index 0) is not prime number, but i want to start indexing from 1. Now, 1st prime number is Primes[1] (instead of Primes[0]).

        public static string BinaryToEncoding(string Binary)
        {
            string Result = "";
            if (Binary.Length % 5 == 0)
            {
                for (int i = 0; i < Binary.Length / 5; i++)
                {
                    int Index = Convert.ToInt32(Binary.Substring(i * 5, 5), 2);
                    Result += HashEncoding[Index];
                }
            }
            return Result;
        }

        public static string EncodingToBinary(string Encoded)
        {
            string Result = "";
            for (int i = 0; i < Encoded.Length; i++)
            {
                for (byte j = 0; j < 32; j++)
                {
                    if (Encoded[i].ToString() == HashEncoding[j])
                    {
                        Result += Convert.ToString(j, 2).PadLeft(5, '0');
                    }
                }
            }
            return Result;
        }

        public static byte HashEncodingIndex(char Encoded)
        {
            switch (Encoded)
            {
                case '0': return 0;  
                case '1': return 1;  
                case '2': return 2;  
                case '3': return 3;  
                case '4': return 4;  
                case '5': return 5;  
                case '6': return 6;  
                case '7': return 7;  
                case '8': return 8;  
                case '9': return 9;  
                case 'A': return 10; 
                case 'B': return 11; 
                case 'C': return 12; 
                case 'D': return 13; 
                case 'E': return 14; 
                case 'F': return 15; 
                case 'G': return 16; 
                case 'H': return 17; 
                case 'I': return 18; 
                case 'J': return 19; 
                case 'K': return 20; 
                case 'L': return 21; 
                case 'M': return 22; 
                case 'N': return 23; 
                case 'O': return 24; 
                case 'P': return 25; 
                case 'R': return 26; 
                case 'S': return 27; 
                case 'T': return 28; 
                case 'U': return 29; 
                case 'W': return 30; 
                case 'Z': return 31; 
            }
            return 32;
        }

        public static string BytesToBinary(byte[] Data)
        {
            string Binary = "";
            for (int i = 0; i < Data.Length; i++)
            {
                Binary += Convert.ToString(Data[i], 2).PadLeft(8, '0');
            }
            return Binary;
        }

        public static string TqHash(string Value)
        {
            // This combinations is 1024 bits length + parity bit, so total length is 1025 bits.
            // It may change to sha4-1024 + parity bit in future, to keep total length the same.
            // For now it doesn't exists, but it may appear, when sha3-512 will not be secure enough.
            
            var AlgorithmA = new Sha3Digest(512); // SHA3-512
            var AlgorithmB = new Sha256Digest(); // SHA2-256
            var AlgorithmC = new MD5Digest(); // MD5-128
            var AlgorithmD = new MD4Digest(); // MD4-128
            byte[] Raw = Encoding.UTF8.GetBytes(Value);
            AlgorithmA.BlockUpdate(Raw, 0, Raw.Length);
            AlgorithmB.BlockUpdate(Raw, 0, Raw.Length);
            AlgorithmC.BlockUpdate(Raw, 0, Raw.Length);
            AlgorithmD.BlockUpdate(Raw, 0, Raw.Length);
            byte[] Bytes = new byte[128];
            AlgorithmA.DoFinal(Bytes, 0);
            AlgorithmB.DoFinal(Bytes, 64);
            AlgorithmC.DoFinal(Bytes, 96);
            AlgorithmD.DoFinal(Bytes, 112);
            string Binary = BytesToBinary(Bytes);
            bool ParityBit = false;
            for (int i = 0; i < Binary.Length; i++)
            {
                if (Binary[i] == '1') { ParityBit = !ParityBit; }
            }
            if (ParityBit) { Binary += "1"; }
            else { Binary += "0"; }
            return BinaryToEncoding(Binary);
        }
        
        public static ushort SumHash(string Hash)
        {
            ushort Result = 0;
            
            for (byte i = 0; i < Hash.Length; i++)
            {
                Result += HashEncodingIndex(Hash[i]);   
            }
            
            return Result;
        }

        public static string BytesToHex(byte[] Data)
        {
            return string.Concat(Data.Select(x => x.ToString("x2")));
        }

        public static byte[] HexToBytes(string HexString)
        {
            HexString = HexString.ToLower();
            if(CheckStringFormat(HexString, 2, 0, int.MaxValue))
            {
                return Enumerable.Range(0, HexString.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(HexString.Substring(x, 2), 16)).ToArray();
            }
            return null;
        }

        public static bool CheckStringFormat(string Text, byte Mode, int Min, int Max)
        {
            // Mode Types: 1: Only length checking.
            // 2: Hex (Lower), 3: Base32 (Upper),
            // 4: Base64 (No Spaces, No Separators),
            // 5: Base64 (Spaces, No Separators),
            // 6: Base64 (No Spaces, Separators),
            // 7: Base64 (Spaces, Separators)

            string RegexFormula = "^.*$";
            if (Mode == 2) { RegexFormula = "^[a-f0-9]*$"; }
            if (Mode == 3) { RegexFormula = "^[A-PR-WZ0-9]*$"; }
            if (Mode == 4) { RegexFormula = "^[a-zA-Z0-9]*$"; }
            if (Mode == 5) { RegexFormula = "^[a-zA-Z0-9 ]*$"; }
            if (Mode == 6) { RegexFormula = "^[a-zA-Z0-9|]*$"; }
            if (Mode == 7) { RegexFormula = "^[a-zA-Z0-9 |]*$"; }

            Regex RegexCheck = new(RegexFormula);

            return RegexCheck.IsMatch(Text) && Text.Length >= Min && Text.Length <= Max;
        }
    }
}
