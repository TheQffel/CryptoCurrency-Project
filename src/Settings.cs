using System.IO;

namespace OneCoin
{
    class Settings
    {
        public static string WordsPath = "./words/";
        public static string ImagesPath = "./images/";
        public static string MessagesPath = "./texts/";
        public static string BlockchainPath = "./blockchain/";
        public static string TransactionsPath = "./transactions/";
        public static string SettingsPath = "./settings/";
        public static string WalletsPath = "./wallets/";
        public static string ServicesPath = "./services/";
        public static string ExtrasPath = "./extras/";

        public static uint MemoryLimit = uint.MaxValue;
        public static sbyte ConsoleSize = 0;
        public static bool DiscordStatus = true;

        public static void CheckPaths()
        {
            if (!Directory.Exists(WalletsPath)) { Directory.CreateDirectory(WordsPath); }
            if (!Directory.Exists(ImagesPath)) { Directory.CreateDirectory(ImagesPath); }
            if (!Directory.Exists(MessagesPath)) { Directory.CreateDirectory(MessagesPath); }
            if (!Directory.Exists(BlockchainPath)) { Directory.CreateDirectory(BlockchainPath); }
            if (!Directory.Exists(TransactionsPath)) { Directory.CreateDirectory(TransactionsPath); }
            if (!Directory.Exists(SettingsPath)) { Directory.CreateDirectory(SettingsPath); }
            if (!Directory.Exists(WalletsPath)) { Directory.CreateDirectory(WalletsPath); }
            if (!Directory.Exists(ServicesPath)) { Directory.CreateDirectory(ServicesPath); }
            if (!Directory.Exists(ExtrasPath)) { Directory.CreateDirectory(ExtrasPath); }
        }

        public static void Load()
        {
            string[] Settings = List();
            for (int i = 0; i < Settings.Length; i++)
            {
                if (File.Exists(SettingsPath + Settings[i].ToLower() + ".txt"))
                {
                    Set(Settings[i].ToLower(), File.ReadAllText(SettingsPath + Settings[i].ToLower() + ".txt"));
                }
            }
        }

        public static void Save()
        {
            string[] Settings = List();
            for (int i = 0; i < Settings.Length; i++)
            {
                File.WriteAllText(SettingsPath + Settings[i].ToLower() + ".txt", Get(Settings[i].ToLower()));
            }
        }

        public static string Get(string Name)
        {
            Name = Name.ToLower();
            if (Name == "consolesize") { return ConsoleSize.ToString(); }
            if (Name == "memorylimit") { return MemoryLimit.ToString(); }
            if (Name == "discordstatus") { return DiscordStatus.ToString(); }
            return null;
        }

        public static bool Set(string Name, string Value)
        {
            Name = Name.ToLower();
            if (Name == "consolesize") { ConsoleSize = sbyte.Parse(Value); return true; }
            if (Name == "memorylimit") { MemoryLimit = uint.Parse(Value); return true; }
            if (Name == "discordstatus") { DiscordStatus = bool.Parse(Value); return true; }
            return false;
        }

        public static string[] List()
        {
            return new[] { "ConsoleSize", "MemoryLimit", "DiscordStatus" };
        }
    }
}
