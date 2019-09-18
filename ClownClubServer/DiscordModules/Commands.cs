using ClownClubServer.Classes;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClownClubServer.DiscordModules {
    [Name("Base")]
    [RequireContext(ContextType.Guild | ContextType.DM)]
    public class Commands : ModuleBase<SocketCommandContext> {
        private static Random random = new Random();
        public static string RandomString(int length) {
            const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [Command("register")]
        [Summary("Register user.")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Register(string code) {
            if (DatabaseManager.Users.Exists(x => x.Id.Equals(Context.User.Id))) {
                await ReplyAsync($"{Context.User.Mention} you already have an auth key.");
            }
            else if (DatabaseManager.Invites.Exists(x => x.Id.Equals(code))) {
                Invite invite = DatabaseManager.Invites.FindOne(x => x.Id.Equals(code));
                DatabaseManager.Invites.Delete(code);
                string key = RandomString(32);
                DatabaseManager.Users.Insert(new User(Context.User.Id, key, invite));
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name.Equals("Verified"));
                await (Context.User as IGuildUser).AddRoleAsync(role);
                await Context.User.SendMessageAsync($"**Auth Key:** {key}");
                await ReplyAsync($"{Context.User.Mention} check your DMs.");
            }
            else {
                await ReplyAsync($"{Context.User.Mention} invalid invite.");
            }
        }

        [Command("invite")]
        [Summary("Generates an invite if user has met requirements.")]
        public async Task Invite() {
            if (!DatabaseManager.Users.Exists(x => x.Id.Equals(Context.User.Id))) {
                await ReplyAsync($"{Context.User.Mention} you are not registered.");
                return;
            }

            var account = DatabaseManager.Users.FindOne(x => x.Id.Equals(Context.User.Id));
            if (account.RegistrationDate < DateTime.Now.AddMonths(3)) {
                await ReplyAsync($"{Context.User.Mention} you must be registered for at least 3 months to get an invite.");
                return;
            }
            else if (account.LastInviteDate < DateTime.Now.AddMonths(3)) {
                await ReplyAsync($"{Context.User.Mention} you can only get 1 invite every 3 months.");
                return;
            }
            else if (DatabaseManager.Invites.Exists(x => x.Inviter.Equals(Context.User.Id))) {
                await ReplyAsync($"{Context.User.Mention} you can only have 1 unused invite at a time.");
                return;
            }

            var code = RandomString(16);
            DatabaseManager.Invites.Insert(new Invite(code, Context.User.Id));
            await Context.User.SendMessageAsync($"**Invite Code:** {code}");
            await ReplyAsync($"{Context.User.Mention} check your DMs.");
            account.LastInviteDate = DateTime.Now;
            DatabaseManager.Users.Update(account);
        }

        [Command("adminvite")]
        [Summary("Generates an invite for the user specified by the admin.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AdminviteUser(SocketUser user = null) {
            var code = RandomString(16);
            if (user is null) {
                DatabaseManager.Invites.Insert(new Invite(code, Context.User.Id));
                await Context.User.SendMessageAsync($"**Invite Code:** {code}");
                await ReplyAsync($"{Context.User.Mention} check your DMs.");
                return;
            }
            else if (!DatabaseManager.Users.Exists(x => x.Id.Equals(user.Id))) {
                await ReplyAsync($"{Context.User.Mention} that user is not registered.");
                return;
            }

            DatabaseManager.Invites.Insert(new Invite(code, user.Id));
            await user.SendMessageAsync($"**Invite Code:** {code}");
            await ReplyAsync($"{user.Mention} check your DMs.");
        }

        [Command("invitewave")]
        [Summary("Gives an invite to every user.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task InviteWave() {
            int invites = 0;
            foreach (var user in DatabaseManager.Users.FindAll()) {
                var discordUser = Context.Client.GetUser(user.Id);
                if (discordUser is null)
                    continue;
                var code = RandomString(16);
                DatabaseManager.Invites.Insert(new Invite(code, discordUser.Id));
                await Context.User.SendMessageAsync($"**Invite Code:** {code}");
                invites++;
            }

            await ReplyAsync($"{invites} invite(s) given out.");
        }

        [Command("invitewave")]
        [Summary("Gives an invite to every user until the maximum is reached.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task InviteWave(int max) {
            int invites = 0;
            foreach (var user in DatabaseManager.Users.FindAll()) {
                if (invites >= max)
                    break;
                var discordUser = Context.Client.GetUser(user.Id);
                if (discordUser is null)
                    continue;
                var code = RandomString(16);
                DatabaseManager.Invites.Insert(new Invite(code, discordUser.Id));
                await Context.User.SendMessageAsync($"**Invite Code:** {code}");
                invites++;
            }

            await ReplyAsync($"{invites} invite(s) given out.");
        }

        [Command("clearinvites")]
        [Summary("Deletes all invites.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearInvites(SocketUser user = null) {
            if (user is null) {
                var count = DatabaseManager.Invites.Count();
                DatabaseManager.Database.DropCollection("invites");

                await ReplyAsync($"{count} invite(s) deleted.");
            }
            else {
                var count = DatabaseManager.Invites.Find(x => x.Inviter.Equals(user.Id)).Count();
                DatabaseManager.Invites.Delete(x => x.Inviter.Equals(user.Id));

                await ReplyAsync($"{count} invite(s) deleted.");
            }
        }

        [Command("whoami")]
        [Summary("Returns user's Discord ID.")]
        public async Task WhoAmI() {
            await ReplyAsync($"{Context.User.Mention} {Context.User.Id}");
        }

        [Command("whois")]
        [Summary("Gives information about a specified user.")]
        public async Task WhoIs(SocketUser user = null) {
            if (user is null) {
                await ReplyAsync($"{Context.User.Mention} invalid user.");
                return;
            }
            else if (!DatabaseManager.Users.Exists(x => x.Id.Equals(user.Id))) {
                await ReplyAsync($"{Context.User.Mention} that user is not registered.");
                return;
            }

            var account = DatabaseManager.Users.FindOne(x => x.Id.Equals(user.Id));
            var inviter = Context.Client.GetUser(account.InviteCode.Inviter);

            var embed = new EmbedBuilder().WithAuthor(user)
                .WithFooter(footer => footer.Text = "TheClown.Club")
                .WithColor(Color.Blue)
                .WithTitle("User Information")
                .AddField("Discord ID", user.Id)
                .AddField("Registered", account.RegistrationDate)
                .AddField("Invited By", inviter == null ? account.InviteCode.Inviter.ToString() : inviter.Username)
                .WithCurrentTimestamp()
                .Build();
            await ReplyAsync(embed: embed);
        }
    }
}
