using System.Threading.Tasks;

namespace BreadSandwich.Core
{
    public interface IDiscordService
    {
        Task StartAsync();
        Task StopAsync();
    }
}