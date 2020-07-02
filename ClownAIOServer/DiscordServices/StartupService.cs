using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ClownClubServer.DiscordServices {
    public class StartupService {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public StartupService(
            IServiceProvider provider,
            DiscordSocketClient discord,
            CommandService commands) {
            _provider = provider;
            _discord = discord;
            _commands = commands;
        }

        public async Task StartAsync() {
            string discordToken = "NjIwNzg5NzI4OTU4MDg3MTc4.XXky-g.jy_NTbu2w24KFFTVmrjyXFMMcDI";

            await _discord.LoginAsync(TokenType.Bot, discordToken);     // Login to discord
            await _discord.StartAsync();                                // Connect to the websocket

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);     // Load commands and modules into the command service
        }
    }
}
