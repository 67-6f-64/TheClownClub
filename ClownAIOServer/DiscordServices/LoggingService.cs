using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace ClownClubServer.DiscordServices {
    public class LoggingService {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;

        // DiscordSocketClient and CommandService are injected automatically from the IServiceProvider
        public LoggingService(DiscordSocketClient discord, CommandService commands) {
            _discord = discord;
            _commands = commands;

            _discord.Log += OnLog;
            _commands.Log += OnLog;
        }

        private async Task OnLog(LogMessage msg) {
            var logText = $"{DateTime.UtcNow:hh:mm:ss} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";

            await Task.Delay(0);
        }
    }
}
