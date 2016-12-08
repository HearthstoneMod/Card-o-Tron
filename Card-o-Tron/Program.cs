using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using Discord;

namespace Card_o_Tron
{
    public class Program
    {
        #region Constructor 

        private static Program MainProgram;

        static void Main(string[] args)
        {
            MainProgram = new Program();
            MainProgram.Start();
        }

        #endregion

        private const ulong ServerID = 194099156988461056;

        private DiscordClient Client;
        private Server Server;

        private string AppDirectory;

        private Role DeveloperRole;
        //private Role AdministratorRole;
        private Role ModeratorRole;
        private Role VeteranRole;

        private WebHeaderCollection MashapeHeader = new WebHeaderCollection();

        #region Methods

        public void Start()
        {
            AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

            MashapeHeader.Add("X-Mashape-Key", "kZYPGbYm4SmshzBsE9Ftb3ON5fump1IUQhFjsnrRl51bOGh5tv");

            Client = new DiscordClient();

            Client.MessageReceived += async (obj, args) =>
            {
                await Task.Run(() => ProcessMessage(args));
            };

            Client.ExecuteAndWait(async () =>
            {
                await Client.Connect("Bot MjEyMjg0MDA2NDg0NTQxNDQw.CopoxA.Yna0J5n7tb22-sBYUqA7UAdCMhI");

                await Task.Delay(1000);

                Client.SetGame("Modstone");

                Server = Client.Servers.First(s => s.Id == ServerID);

                DeveloperRole = Server.FindRoles("Developers").FirstOrDefault();
                //AdministratorRole = Server.FindRoles("Administrators").FirstOrDefault();
                ModeratorRole = Server.FindRoles("Moderators").FirstOrDefault();
                VeteranRole = Server.FindRoles("Veterans").FirstOrDefault();

                LogText("Loaded Card-o-Tron bot to server " + Server.Name);
            });
        }

        private void ProcessMessage(MessageEventArgs args)
        {
            Channel channel = args.Channel;
            User user = args.User;
            string fullUser = user.ToString();

            if (args.Message.IsAuthor == false)
            {
                if (args.Server?.Id == ServerID)
                {
                    string fullText = args.Message.Text;

                    if (fullText.StartsWith("!"))
                    {
                        string[] commands = fullText.Split();
                        bool isDeveloper = user.HasRole(DeveloperRole);
                        //bool isAdmin = isDeveloper || user.HasRole(AdministratorRole);
                        bool isModerator = isDeveloper || user.HasRole(ModeratorRole);
                        bool isVeteran = isDeveloper || isModerator || user.HasRole(VeteranRole);

                        switch (commands[0].ToLower())
                        {
                            case "!hello":
                                if (isModerator)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    channel.SendTTSMessage("***HELLO! HELLO! HELLO!***");
                                }
                                break;

                            case "!ping":
                                if (isModerator)
                                {
                                    LogNormalCommand(channel, commands[0], fullUser);
                                    channel.SendMessage("`Latency : " + new Ping().Send("www.discordapp.com").RoundtripTime + " ms`");
                                }
                                break;

                            case "!help":
                                if (commands.Length == 1)
                                {
                                    channel.SendMessage("Use `!help card` to get the full list of Card-o-Tron commands");
                                }
                                else if (commands[1].ToLower() == "card")
                                {
                                    LogNormalCommand(channel, commands[0], fullUser);
                                    channel.SendMessage("**· Normal Commands :**\n " +
                                                        "```!hello - HELLO! (mod+ only)\n" +
                                                        "!ping - Checks bot status (mod+ only)\n" +
                                                        "!help - Shows this message```\n" +

                                                        "**· Card Commands: **\n" +
                                                        "```!card <fullname> - Sends the card as an image\n" +
                                                        "!cardgif <fullname> - Sends the card as a gif```\n" +

                                                        "**· Hero Commands: **\n" +
                                                        "```!hero <fullname> - Sends the hero as an image\n" +
                                                        "!herogif <fullname> - Sends the hero as a gif```\n");
                                }
                                break;

                            case "!card":
                                if (commands.Length > 1)
                                {
                                    LogNormalCommand(channel, commands[0], fullUser);
                                    CardCommand(channel, fullText.Substring(commands[0].Length + 1));
                                }
                                break;

                            case "!cardgif":
                                if (commands.Length > 1)
                                {
                                    LogNormalCommand(channel, commands[0], fullUser);
                                    CardGifCommand(channel, fullText.Substring(commands[0].Length + 1));
                                }
                                break;

                            case "!hero":
                                if (commands.Length > 1)
                                {
                                    LogNormalCommand(channel, commands[0], fullUser);
                                    HeroCommand(channel, fullText.Substring(commands[0].Length + 1));
                                }
                                break;

                            case "!herogif":
                                if (commands.Length > 1)
                                {
                                    LogNormalCommand(channel, commands[0], fullUser);
                                    HeroGifCommand(channel, fullText.Substring(commands[0].Length + 1));
                                }
                                break;
                        }
                    }
                }
            }
        }

        #endregion
        
        #region Card Related Methods

        public async void CardCommand(Channel channel, string cardName)
        {
            WebClient webClient = new WebClient();
            webClient.Proxy = WebRequest.GetSystemWebProxy();
            webClient.Headers.Add(MashapeHeader);

            try
            {
                string jsonCard = webClient.DownloadString("https://omgvamp-hearthstone-v1.p.mashape.com/cards/" + cardName);
                dynamic jsonObject = JsonConvert.DeserializeObject(jsonCard);

                bool foundMatch = false;

                foreach (dynamic cardObject in jsonObject)
                {
                    if (cardObject.type == "Minion" || cardObject.type == "Spell" || cardObject.type == "Weapon")
                    {
                        foundMatch = true;

                        channel.SendMessage(cardObject.img.ToString());
                    }
                }

                if (foundMatch == false)
                {
                    channel.SendMessage(cardName + " **was not found**");
                }
            }
            catch (Exception ex)
            {
                channel.SendMessage(cardName + " **was not found**");
            }
        }

        public async void CardGifCommand(Channel channel, string cardName)
        {
            WebClient webClient = new WebClient();
            webClient.Proxy = WebRequest.GetSystemWebProxy();
            webClient.Headers.Add(MashapeHeader);

            try
            {
                string jsonCard = webClient.DownloadString("https://omgvamp-hearthstone-v1.p.mashape.com/cards/" + cardName);
                dynamic jsonObject = JsonConvert.DeserializeObject(jsonCard);

                bool foundMatch = false;

                foreach (dynamic cardObject in jsonObject)
                {
                    if (cardObject.type == "Minion" || cardObject.type == "Spell" || cardObject.type == "Weapon")
                    {
                        foundMatch = true;

                        channel.SendMessage(cardObject.imgGold.ToString());
                    }
                }

                if (foundMatch == false)
                {
                    channel.SendMessage(cardName + " **was not found**");
                }
            }
            catch (Exception ex)
            {
                channel.SendMessage(cardName + " **was not found**");
            }
        }

        public async void HeroCommand(Channel channel, string heroName)
        {
            WebClient webClient = new WebClient();
            webClient.Proxy = WebRequest.GetSystemWebProxy();
            webClient.Headers.Add(MashapeHeader);

            try
            {
                string jsonCard = webClient.DownloadString("https://omgvamp-hearthstone-v1.p.mashape.com/cards/" + heroName);
                dynamic jsonObject = JsonConvert.DeserializeObject(jsonCard);

                bool foundMatch = false;

                foreach (dynamic cardObject in jsonObject)
                {
                    if (cardObject.type == "Hero")
                    {
                        foundMatch = true;

                        channel.SendMessage(cardObject.img.ToString());
                    }
                }

                if (foundMatch == false)
                {
                    channel.SendMessage(heroName + " **was not found**");
                }
            }
            catch (Exception ex)
            {
                channel.SendMessage(heroName + " **was not found**");
            }
        }

        public async void HeroGifCommand(Channel channel, string heroName)
        {
            WebClient webClient = new WebClient();
            webClient.Proxy = WebRequest.GetSystemWebProxy();
            webClient.Headers.Add(MashapeHeader);

            try
            {
                string jsonCard = webClient.DownloadString("https://omgvamp-hearthstone-v1.p.mashape.com/cards/" + heroName);
                dynamic jsonObject = JsonConvert.DeserializeObject(jsonCard);

                bool foundMatch = false;

                foreach (dynamic cardObject in jsonObject)
                {
                    if (cardObject.type == "Hero")
                    {
                        foundMatch = true;

                        channel.SendMessage(cardObject.imgGold.ToString());
                    }
                }

                if (foundMatch == false)
                {
                    channel.SendMessage(heroName + " **was not found**");
                }
            }
            catch (Exception ex)
            {
                channel.SendMessage(heroName + " **was not found**");
            }
        }

        #endregion

        #region Log Methods

        public void LogText(string text)
        {
            Console.WriteLine("<white>" + text + "</white>");
        }

        public void LogNormalCommand(Channel channel, string cmd, string user)
        {
            Console.WriteLine("<cyan>" + cmd + " requested in #" + channel.Name + " by " + user + "</cyan>");
        }

        public void LogAdminCommand(Channel channel, string cmd, string user)
        {
            Console.WriteLine("<green>" + cmd + " requested in #" + channel.Name + " by " + user + "</green>");
        }

        #endregion
    }
}
