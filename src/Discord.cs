using System.Threading;
using DiscordRPC;

namespace OneCoin
{
    class Discord
    {
        static DiscordRpcClient RichPresence = null;
        
        public static void StartService()
        {
            if(Settings.DiscordStatus)
            {
                RichPresence = new DiscordRpcClient("426857409839890442");
                RichPresence.Initialize();
            }
        }
        
        public static void StopService()
        {
            if(Settings.DiscordStatus)
            {
                RichPresence.Dispose();
                RichPresence = null;
            }
        }
        
        public static void UpdateService()
        {
            if(RichPresence != null)
            {
                Mining.GetHashrate();
                Thread.Sleep(10000);
                
                RichPresence DiscordRpc = new();
                DiscordRpc.Assets = new();
                DiscordRpc.Assets.LargeImageKey = "onecoin";
                DiscordRpc.Assets.LargeImageText = "OneCoin is new decentralized cryptocurrency - check it out:  https://github.com/TheQffel/OneCoin   https://discord.gg/SbsFcxFYsg";
                if(Account.PublicKey != null)
                {
                    string Address = Wallets.AddressToShort(Account.PublicKey);
                    string NicknameWithTag = Wallets.GetName(Address);
                    
                    DiscordRpc.Assets.SmallImageKey = "account";
                    DiscordRpc.Assets.SmallImageText = Address;
                    
                    if(NicknameWithTag.Length > 1)
                    {
                        DiscordRpc.Details = "Nickname: " + NicknameWithTag.Replace("|", " # ");
                    }
                }
                if(Mining.KeepMining)
                {
                    DiscordRpc.State = "Status: Mining - " + Mining.GetHashrate()[2];
                }
                else
                {
                    DiscordRpc.State = "Status: Using Console App";
                }
                RichPresence.SetPresence(DiscordRpc);
            }
        }
    }
}
