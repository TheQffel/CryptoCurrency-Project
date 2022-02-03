
# OneCoin - A decentralized cryptocurrency.
[![DiscordBanner](https://discordapp.com/api/guilds/299932181038366720/widget.png?style=banner2)](https://discord.gg/SbsFcxFYsg)

## What is one coin?
OneCoin is new decentralized cryptocurrency. As name suggest, total supply of this crypto is 1 coin, but don’t worry, it has 24 decimal places. Now you don’t have to calculate how many zeroes is after decimal. One Coin is name for whole coin, but for normal use you can use Ones - 1 Ones is smallest amount you can have, so instead of sending 0,0000​0000​0000​0000​0000​1750 OneCoin you just send 1750 Ones. OneCoin uses ProofOfWorkWithMemory for block confirmation - this means you need to have full blockchain, to be able to "mine" next block and your "mining speed" depends on how big part of blockchain is stored in RAM (because RAM is often faster than SSD, especially randomly accessed). It is also 100% anty burnable, so there always be 1 coin in circulation. We also want to make OneCoin as simple as possible. This means One Coin will never support creating smart contracts, tokens or anything else - but instead it is great tool to buy, sell and transfer money with low fees. Just because we don't support smart contracts doesn't mean that you can't any service or app on OneCoin. You can integrate OneCoin Network Login with your app, so users can interact with your app with OneCoin account, use OneCoin Nickname, OneCoin Avatar, etc. You can even create your token, decentralized or not, and use OneCoin Addresses as wallets. For more info, about OneCoin project check out our wiki page.

## Technical Specifications:
Consensus: PoWwM (Proof of Work with Memory),
Supply: 1 OneCoin (1000000000000000000000000 Ones),
Decimals: 24 (1 Ones = 0.000000000000000000000001 OneCoin),
Algorithm: TqHash (SHA512+SHA256+MD5+MD4),
Rewards: Depends (halved every 1000000 blocks),
Ticker: OneCoin © (letter C in letter O),
Block Time: Depends on network difficulty.

## How to use?
Recommended for advanced users: Download latest release of CommandLine App from this github repository and launch it. Then select "create wallet" and you are good to go. This version contains all features and syncs with whole blockchain. You need this version to be able to "mine". It works on desktops in command line prompts and terminals.

Recommended for normal users: If you prefer Desktop App or Mobile App, you can download it from Applications Store, like Google Play or from our official website. This version is "lite" version, that means it doesn't store whole blockchain on your device, but it have almost all features. You can't "mine" with this app. It works on desktops and smartphones.

If you want to compile command line app by yourself, download this github repository, open folder called "src" in terminal and type: ```dotnet publish -r RUNTIME```, where RUNTIME depends on Operating System you use, e.g.:
Windows: ```dotnet publish -r win-x64```
Linux: ```dotnet publish -r linux-x64```
You need to have .NET SDK installed to compile by yourself.
If you compile yourself don't forget to copy "words" folder to output folder.

## Apps built with OneCoin Network:
As previously stated, OneCoin doesn't support Smart Contracts or Decentralized Apps directly, but they can be created as well with One Coin Network Login. Here are some examples:
[WEB WALLET](http://one-coin.org/wallet) - Generate new account or add existing account from mnemonic words. You can change your account data or send transactions directly in broswer, no app needed. 
[BLOCKCHAIN EXPLORER](http://one-coin.org/explorer) - Exblore blockchain, display information about blocks, accounts and transactions. You can browse all informations directly in browser, no app needed.
[ONE COIN DNS](http://one-coin.org/dns) - Get a free domain. You can assign an IP address to YOURNICKNAME.OCN and use it for your web / ssh / ftp / game server or whatever you want. You can also host simple html page here for free. This service uses "OneCoin Login" to work.
[More Examples Coming Soon!](https://discord.gg/SbsFcxFYsg) - Do you want your app or website to be listed here? Contact me on Discord (if you already make it)! 

## Any questions?
We invite you to join our official Discord Server (you can find invitation link at the top of this readme file) - here you can ask questions, write you ideas, report bugs, etc. You can also browse our wiki, it probably has some answers to your questions.