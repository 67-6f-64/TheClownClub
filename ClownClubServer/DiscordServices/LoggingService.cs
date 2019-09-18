using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Windows;

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
            string logText = $"{DateTime.UtcNow.ToString("hh:mm:ss")} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            if ((Application.Current.MainWindow as MainWindow) != null) {
                //string logText = $"{DateTime.UtcNow.ToString("hh:mm:ss")} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
                (Application.Current.MainWindow as MainWindow).txtLog.Text += $"\n{logText}";
            }
            MessageBox.Show(logText);
            await Task.Delay(0);
        }
    }
}
