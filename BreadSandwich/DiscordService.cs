using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BreadSandwich.Core
{
    public class DiscordService : IDiscordService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DiscordService> _logger;
        private CommandService _commands;
        private DiscordSocketClient _client;
        private readonly string _token;
        private readonly string _botPrefix;
        
        public DiscordService(IServiceProvider serviceProvider, ILogger<DiscordService> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _token = configuration["DiscordAuthToken"];
            _botPrefix = configuration["DiscordBotPrefix"];
        }

        /// <summary>
        /// Start new client and log in
        /// </summary>
        public async Task StartAsync()
        {
            await StopAsync(); // In case there is a client already running
            
            _commands = new CommandService();
            await _commands.AddModulesAsync(assembly: Assembly.GetAssembly(typeof(DiscordService)), _serviceProvider);
            _logger.LogInformation("Commands client successfully started");
            
            _client = new DiscordSocketClient();
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.SetStatusAsync(UserStatus.Online);
            await _client.SetActivityAsync(new Game("sandwich consumption"));
            await _client.StartAsync();
            _client.MessageReceived += MessageUpdated;
            _logger.LogInformation("Discord client successfully started");
        }

        /// <summary>
        /// Logout and dispose of current client if one exists
        /// </summary>
        public async Task StopAsync()
        {
            if (_client != null)
            {
                await _client.LogoutAsync();
                _client.Dispose();
                _logger.LogInformation("Discord client successfully logged out and stopped");
            }
        }
        
        private async Task MessageUpdated(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix(char.Parse(_botPrefix), ref argPos) || 
                  message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);
            
            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context, 
                argPos: argPos,
                services: _serviceProvider);
        }
    }
}