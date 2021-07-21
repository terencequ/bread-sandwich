using System.Threading;
using System.Threading.Tasks;
using BreadSandwich.Core;
using Microsoft.Extensions.Hosting;

namespace BreadSandwich.API.Listeners
{
    public class CommandListener : IHostedService
    {
        private readonly IDiscordService _discordService;

        public CommandListener(IDiscordService discordService)
        {
            _discordService = discordService;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _discordService.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _discordService.StopAsync();
        }
    }
}