using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        private List<string> Admins = new List<string>();

        private WebHeaderCollection MashapeHeader = new WebHeaderCollection();

        #region Methods

        public void Start()
        {
            AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

            LoadFiles();

            MashapeHeader.Add("X-Mashape-Key", "kZYPGbYm4SmshzBsE9Ftb3ON5fump1IUQhFjsnrRl51bOGh5tv");

            Client = new DiscordClient();

            Client.MessageReceived += async (obj, args) =>
            {
                await Task.Run(() => ProcessMessage(args));
            };

            Client.ExecuteAndWait(async () =>
            {
                await Client.Connect("MjEyMjg0MDA2NDg0NTQxNDQw.CopoxA.Yna0J5n7tb22-sBYUqA7UAdCMhI");

                await Task.Delay(1000);

                Client.SetGame("Modstone");

                Server = Client.Servers.First(s => s.Id == ServerID);

                LogText("Loaded Card-o-Tron bot to server " + Server.Name);
            });
        }

        private void LoadFiles()
        {
            if (File.Exists(AppDirectory + "admins.list"))
            {
                string[] admins = File.ReadAllText(AppDirectory + "admins.list").Split(new string[1] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string admin in admins)
                {
                    Admins.Add(admin);
                }

                LogText("Loaded " + admins.Length + " admins");
            }
            else
            {
                File.Create(AppDirectory + "admins.list").Close();

                LogText("Created empty admin list");
            }

            LogText(" ");
        }

        private void ProcessMessage(MessageEventArgs args)
        {
            string fullUser = args.User.ToString();
            Channel channel = args.Channel;

            if (args.Message.IsAuthor == false)
            {
                if (args.Server?.Id == ServerID)
                {
                    string fullText = args.Message.Text;

                    if (fullText.StartsWith("!"))
                    {
                        string[] commands = fullText.Split();
                        bool isAdmin = Admins.Contains(fullUser);

                        switch (commands[0].ToLower())
                        {
                            case "!hello":
                                if (isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    channel.SendTTSMessage("***HELLO! HELLO! HELLO!***");
                                }
                                break;

                            case "!ping":
                                LogNormalCommand(channel, commands[0], fullUser);
                                channel.SendMessage("`Latency : " + new Ping().Send("www.discordapp.com").RoundtripTime + " ms`");
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
                                                        "```!hello - HELLO! (admin only)\n" +
                                                        "!ping - Checks bot status\n" +
                                                        "!help - Shows this message```\n" +

                                                        "**· Admin Commands: **\n" +
                                                        "```!addadmin <fullname> - Adds an admin to the admin list (admin only)\n" +
                                                        "!removeadmin <fullname> - Removes an admin from the admin list (admin only)\n" +
                                                        "!adminlist - Shows the full list of admins```\n" +

                                                        "**· Card Commands: **\n" +
                                                        "```!card <fullname> - Sends the card as an image\n" +
                                                        "!cardgif <fullname> - Sends the card as a gif```\n" +

                                                        "**· Hero Commands: **\n" +
                                                        "```!hero <fullname> - Sends the hero as an image\n" +
                                                        "!herogif <fullname> - Sends the hero as a gif```\n");
                                }
                                break;
                            case "!addadmin":
                                if (commands.Length > 1 && isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    AddAdminCommand(channel, commands[1]);
                                }
                                break;

                            case "!removeadmin":
                                if (commands.Length > 1 && isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    RemoveAdminCommand(channel, commands[1]);
                                }
                                break;

                            case "!adminlist":
                                LogNormalCommand(channel, commands[0], fullUser);
                                ShowAdminListCommand(channel);
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

        #region Admin Related Methods

        private void ShowAdminListCommand(Channel channel)
        {
            if (Admins.Count > 0)
            {
                string adminList = "**Showing current admin list **(" + DateTime.Today.ToShortDateString() + ")** :**\n\n```";

                for (int i = 0; i < Admins.Count; i++)
                {
                    adminList += "· " + Admins[i] + "\n";
                }

                channel.SendMessage(adminList + "```");
            }
            else
            {
                channel.SendMessage("**Admin list is empty.**");
            }
        }

        private void AddAdminCommand(Channel channel, string admin)
        {
            if (Server.Users.Any(u => u.ToString() == admin))
            {
                if (Admins.Contains(admin))
                {
                    channel.SendMessage("@" + admin + "** is already an admin.**");
                }
                else
                {
                    AddAdmin(admin);
                    channel.SendMessage("@" + admin + "** was added to the admin list.**");
                }
            }
            else
            {
                channel.SendMessage(admin + "** was not found in the server.**");
            }
        }

        private void RemoveAdminCommand(Channel channel, string admin)
        {
            if (Admins.Contains(admin))
            {
                RemoveAdmin(admin);
                channel.SendMessage(admin + "** was removed from admins.**");
            }
            else
            {
                channel.SendMessage(admin + "** is not an admin.**");
            }
        }

        private void AddAdmin(string admin)
        {
            Admins.Add(admin);
            SaveAdminFile();
        }

        private void RemoveAdmin(string admin)
        {
            Admins.Remove(admin);
            SaveAdminFile();
        }

        private void SaveAdminFile()
        {
            string adminString = string.Join(";", Admins.ToArray());

            if (adminString.StartsWith(";"))
            {
                adminString = adminString.Substring(1);
            }

            File.WriteAllText(AppDirectory + "admins.list", adminString);
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
                    if (cardObject.type == "Minion" || cardObject.type == "Spell")
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
                    if (cardObject.type == "Minion" || cardObject.type == "Spell")
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
