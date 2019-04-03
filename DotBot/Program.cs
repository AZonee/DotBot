using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DotBot
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        public bool togglebackup = false;

        public async Task MainAsync()
        {
            Console.WriteLine("Starting DotBot version 3.1.11");
            _client = new DiscordSocketClient();

            _client.Log += Log;
            _client.MessageReceived += Backup;
            _client.MessageUpdated += Update;
            _client.ReactionAdded += Pin;
            _client.UserBanned += MattGotBanned;
            
            string token = File.ReadAllLines(@"C:\Users\Zonee\source\repos\DotBot\DotBot\BOTTOKEN.btk")[0];
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            while (true)
            {
                var b = Console.ReadLine();
                if (b.StartsWith(".echo "))
                {
                    Console.WriteLine("...");
                    await (_client.GetChannel(539219280944824320) as ISocketMessageChannel).SendMessageAsync(b.Remove(0, 5));
                    Console.WriteLine("Sent");
                }
                else if (b == "embed")
                {
                    Console.WriteLine("\"none\" to skip a step.");
                    var eb = new EmbedBuilder();
                    Console.WriteLine("Title:");
                    var title = Console.ReadLine();
                    Console.WriteLine("Description:");
                    var desc = Console.ReadLine();
                    Console.WriteLine("Color (y/n):");
                    var color = Console.ReadLine();
                    Console.WriteLine("Link:");
                    var link = Console.ReadLine();
                    Console.WriteLine("Footer:");
                    var footer = Console.ReadLine();

                    if (title != "none") { eb.WithTitle(title); }
                    if (desc != "none") { eb.WithDescription(desc); }
                    if (color != "n") { eb.WithColor(Color.Purple); }
                    if (link != "none") { eb.WithUrl(link); }
                    if (footer != "none") { eb.WithFooter(footer); }

                    await (_client.GetChannel(539219280944824320) as ISocketMessageChannel).SendMessageAsync("", false, eb.Build());
                    Console.WriteLine("Sent!");
                }
            }
        }

        public async Task MattGotBanned(SocketUser user, SocketGuild guild)
        {
            if (user.Id == 279262254292271106 && !File.Exists(@"C:\Users\Zonee\source\repos\DotBot\OVERRULEMATTBAN.dbt"))
            {
                await guild.RemoveBanAsync(user.Id);
            }

        }

        public async Task Pin(Cacheable<IUserMessage, ulong> test, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if ((reaction.Emote as Emote).Id == 458664596299841547)
            {
                var id = reaction.MessageId; // Gets the message ID of the pin message
                var sender = reaction.User.Value as SocketGuildUser; // Gets the user who requested the pin as a SocketGuildUser
                var msg = await channel.GetMessageAsync(id);
                string userreal = RealName(msg.Author.Id.ToString());
                var backup = _client.GetChannel(458481007646212108) as ISocketMessageChannel;
                #region Test for duplicate
                var backupmsgs = await backup.GetMessagesAsync(1).FlattenAsync();
                foreach (var message in backupmsgs)
                {
                    if (message.Embeds.Count == 1) { if (message.Embeds.First().Description == msg.Content && message.Embeds.First().Author.Value.Name.StartsWith($"{userreal}")) // Finds identical message
                        {
                            //var newemb = message.Embeds.First() as EmbedBuilder;
                            //if (message.Embeds.First().Author.Value.Name.StartsWith($"{userreal}'s message pinned by {RealName(sender.Id.ToString())}")) { return; } // Same user who pinned 
                            //if (message.Embeds.First().Title == null)
                            //{
                            //    newemb.WithTitle($"Also pinned by: {RealName(sender.Id.ToString())}");
                            //}
                            //else
                            //{
                            //    newemb.WithTitle($"Also pinned by: {RealName(sender.Id.ToString())}, " + message.Embeds.First().Title.Remove(0, 16));
                            //}
                            //await (message as Discord.Rest.RestUserMessage).ModifyAsync(x => x.Embed = newemb.Build());
                            return;
                        } }
                }
                #endregion
                var eb = new EmbedBuilder();
                eb.WithAuthor($"{userreal}'s message pinned by {RealName(sender.Id.ToString())} in the channel {msg.Channel.Name} at time {DateTime.Now.ToString("HH:mm")} on day {DateTime.Now.ToString("MM/dd/yy")}:").Author.WithIconUrl(msg.Author.GetAvatarUrl());
                eb.WithDescription(msg.Content);
                eb.WithColor(Color.Purple);
                foreach (var attach in msg.Attachments)
                {
                    eb.WithImageUrl(attach.Url);
                }
                if ((msg.Author as SocketGuildUser).Nickname == null)
                {
                    eb.WithFooter($"At that time, they had the name \"{msg.Author.Username}\"");
                }
                else
                {
                    eb.WithFooter($"At that time, they had the name \"{msg.Author.Username}\" and the nickname \"{(msg.Author as SocketGuildUser).Nickname}\"");
                }
                await backup.SendMessageAsync($"", false, eb.Build());
            }
        }

        public async Task Update(Cacheable<IMessage, ulong> test, SocketMessage message, ISocketMessageChannel channel)
        {
            if (message.Content.ToLower() == ".togglebackup" && message.Author.Id == 198110625065467905) if (togglebackup == false) togglebackup = true; else togglebackup = false;
            var b = message.Channel as SocketGuildChannel;
            if (b.Guild.Id != 406657890921742336 && togglebackup == false)
            {
                string user = message.Author.Id.ToString();
                string userreal = RealName(user);
                var backup = _client.GetChannel(406675171521462272) as ISocketMessageChannel;
                var eb = new EmbedBuilder();
                eb.WithAuthor($"Message sent by {userreal} in the channel {message.Channel.Name} at time {DateTime.Now.ToString("HH:mm")} on day {DateTime.Now.ToString("MM/dd/yy")}:").Author.WithIconUrl(message.Author.GetAvatarUrl());
                eb.WithDescription(message.Content);
                eb.WithColor(Color.DarkRed);
                foreach (var attach in message.Attachments)
                {
                    eb.WithImageUrl(attach.Url);
                }
                eb.WithFooter($"At that time, they had the nickname \"{message.Author.Username}\"");
                await backup.SendMessageAsync("", false, eb.Build());
            }
        }

        private async Task Backup(SocketMessage message)
        {
            List<ulong> roleIds = (message.Author as IGuildUser).RoleIds.ToList();
            ulong[] uuidsListAdmins = { 278700293162663938, 198110625065467905, 242428180332412929 };
            if (message.Content.ToLower().Contains("@everyone") && !uuidsListAdmins.ToList().Contains(message.Author.Id))
            {
                await message.Channel.SendMessageAsync("L");
                await (_client.GetChannel(561318454179659776) as ISocketMessageChannel).SendMessageAsync(RealName(message.Author.Id.ToString()) + " took the L.");
                if (File.Exists(@"C:\Users\Zonee\source\repos\DotBot\Ls\" + message.Author.Id))
                {
                    var line = File.ReadAllLines(@"C:\Users\Zonee\source\repos\DotBot\Ls\" + message.Author.Id)[0];
                    var linum = Convert.ToInt64(line);
                    linum++;
                    string[] lout = { linum.ToString() };
                    File.WriteAllLines(@"C:\Users\Zonee\source\repos\DotBot\Ls\" + message.Author.Id, lout);
                }
                else
                {
                    string[] lout = { "0" };
                    File.WriteAllLines(@"C:\Users\Zonee\source\repos\DotBot\Ls\" + message.Author.Id, lout);
                }
            }

            if (message.Content.ToLower() == ".leaderboard")
            {
                var emb = new EmbedBuilder();
                emb.WithTitle("The L-eaderboard");
                List<LList> ls = new List<LList>();
                foreach (var fle in Directory.GetFiles(@"C:\Users\Zonee\source\repos\DotBot\Ls\"))
                {
                    string name = fle.Remove(0,@"C:\Users\Zonee\source\repos\DotBot\Ls\".Count()).Trim();
                    var count = Convert.ToInt16(File.ReadAllText(fle)) + 1;
                    LList list = new LList
                    {
                        Name = name,
                        Count = count.ToString()
                    };

                    ls.Add(list);
                }

                ls.Sort((x, y) => Convert.ToInt16(y.Count).CompareTo(Convert.ToInt16(x.Count)));

                foreach (LList l in ls)
                {
                    emb.AddField(RealName(l.Name) + ":", l.Count + " L's.");
                }

                emb.WithColor(Color.Green);

                await message.Channel.SendMessageAsync("Here is the current ***L***eaderboard",false,emb.Build());
            }

            if (message.Content.ToLower() == ".togglebackup" && message.Author.Id == 198110625065467905) if (togglebackup == false) togglebackup = true; else togglebackup = false;
            if (message.Content.ToLower().StartsWith(".whois"))
            {
                if (message.MentionedUsers.Count == 0 || message.MentionedUsers.Count > 1) { return; }
                await message.Channel.SendMessageAsync(RealName(message.MentionedUsers.First().Id.ToString()));
            }
            // Old, only keeping in for fun tbh. Using emotes now.
            if (message.Content.ToLower().StartsWith(".pin") && message.Content.Split(' ').Length == 2)
            {
                ulong id = 0;
                try
                {
                    id = Convert.ToUInt64(message.Content.Split(' ')[1]);
                }
                catch (Exception)
                {
                    await message.Channel.SendMessageAsync("Please put in a valid message ID.");
                }
                if (id == 0) { } else // ID updated
                {
                    var msg = await message.Channel.GetMessageAsync(id);
                    string userreal = RealName(msg.Author.Id.ToString());
                    var backup = _client.GetChannel(458481007646212108) as ISocketMessageChannel;
                    var eb = new EmbedBuilder();
                    eb.WithAuthor($"Message sent by {userreal} and pinned by {RealName(message.Author.Id.ToString())} in the channel {msg.Channel.Name} at time {DateTime.Now.ToString("HH:mm")} on day {DateTime.Now.ToString("MM/dd/yy")}:").Author.WithIconUrl(msg.Author.GetAvatarUrl());
                    eb.WithDescription(msg.Content);
                    eb.WithColor(Color.Purple);
                    foreach (var attach in msg.Attachments)
                    {
                        eb.WithImageUrl(attach.Url);
                    }
                    if ((msg.Author as SocketGuildUser).Nickname == null)
                    {
                        eb.WithFooter($"At that time, they had the name \"{msg.Author.Username}\"");
                    }
                    else
                    {
                        eb.WithFooter($"At that time, they had the name \"{msg.Author.Username}\" and the nickname \"{(msg.Author as SocketGuildUser).Nickname}\"");
                    }
                    await backup.SendMessageAsync($"", false, eb.Build());
                    await message.Channel.SendMessageAsync("Message pinned in the pins channel.");
                }
            }


            //Matt being banned backup system.
            if (message.Author.Id == 279262254292271106 && message.Content.ToLower() == ".unban" && !File.Exists(@"C:\Users\Zonee\source\repos\DotBot\OVERRULEMATTBAN.dbt"))
            {
                Console.WriteLine("asdiuytasdas");
                var guild = (message.Author as IGuildUser).Guild;
                string[] giveRoles = { "Admin", "56702", "DJ", "Member", "War Thunder Players", "CS:GO Players", "Minecraft Players", "Terraria", "D&D", "8A", "Prep", "STClass 18'" };
                var roles = guild.Roles.Where(x => giveRoles.Contains(x.Name));
                await (message.Author as IGuildUser).AddRolesAsync(roles);
            }

            if ((message.Author.Id == 278700293162663938 || message.Author.Id == 198110625065467905) && message.Content.ToLower() == ".togglebanmatt")
            {
                if (File.Exists(@"C:\Users\Zonee\source\repos\DotBot\OVERRULEMATTBAN.dbt")) { File.Delete(@"C:\Users\Zonee\source\repos\DotBot\OVERRULEMATTBAN.dbt"); }
                else { var matt = File.Create(@"C:\Users\Zonee\source\repos\DotBot\OVERRULEMATTBAN.dbt"); matt.Close(); }
            }

            // Actually do the backup.
            var b = message.Channel as SocketGuildChannel;
            if (b.Guild.Id != 406657890921742336 && togglebackup == false)
            {
                string user = message.Author.Id.ToString();
                string userreal = RealName(user);
                var backup = _client.GetChannel(406675171521462272) as ISocketMessageChannel;
                var eb = new EmbedBuilder();
                eb.WithAuthor($"Message sent by {userreal} in the channel {message.Channel.Name} at time {DateTime.Now.ToString("HH:mm")} on day {DateTime.Now.ToString("MM/dd/yy")}:").Author.WithIconUrl(message.Author.GetAvatarUrl());
                eb.WithDescription(message.Content);
                eb.WithColor(Color.Green);
                foreach (var attach in message.Attachments)
                {
                    eb.WithImageUrl(attach.Url);
                }
                if ((message.Author as SocketGuildUser).Nickname == null)
                {
                    eb.WithFooter($"At that time, they had the name \"{message.Author.Username}\"");
                }
                else
                {
                    eb.WithFooter($"At that time, they had the name \"{message.Author.Username}\" and the nickname \"{(message.Author as SocketGuildUser).Nickname}\"");
                }
                await backup.SendMessageAsync($"", false, eb.Build());
            }
        }

        public static string RealName(string user)
        {
            string userreal;
            switch (user)
            {
                case "198110625065467905":
                    userreal = "Ethan";
                    break;
                case "278700293162663938":
                    userreal = "Preston";
                    break;
                case "270832719905161216":
                    userreal = "Jack";
                    break;
                case "279262254292271106":
                    userreal = "Matt";
                    break;
                case "182245902633664514":
                    userreal = "Hung-Yen";
                    break;
                case "270929187647258624":
                    userreal = "Lucas";
                    break;
                case "216737343879643137":
                    userreal = "That al-zero guy";
                    break;
                case "312727985402675201":
                    userreal = "Broc";
                    break;
                case "317738814481367042":
                    userreal = "Brayden";
                    break;
                case "333674207881986058":
                    userreal = "Connor";
                    break;
                case "170694758434471938":
                    userreal = "Darryl";
                    break;
                case "367102737953062912":
                    userreal = "Eli";
                    break;
                case "337800397135544322":
                    userreal = "Lane";
                    break;
                case "400068636204859424":
                    userreal = "Nathan";
                    break;
                case "242428180332412929":
                    userreal = "Larry";
                    break;
                case "330541320193966090":
                    userreal = "Tessa";
                    break;
                case "334082651998650370":
                    userreal = "Ryan P";
                    break;
                case "448253209694437377":
                    userreal = "Ryan C";
                    break;
                case "263788929621295116":
                    userreal = "Quinn";
                    break;
                case "334469066913873941":
                    userreal = "Katie";
                    break;
                case "349029334809444363":
                    userreal = "Lanessa";
                    break;
                case "506840516466442251":
                    userreal = "Lane";
                    break;
                default:
                    userreal = "Unknown User";
                    break;
            }
            return userreal;
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
    class LList
    {
        public string Name { get; set; }
        public string Count { get; set; }
    }
}
