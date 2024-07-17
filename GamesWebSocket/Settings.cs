using Microsoft.Extensions.Configuration;

namespace GamesWebSocket
{
    public class Settings
    {
        public static IConfiguration? config;

        public static void Initialize(IConfiguration Configuration)
        {
            config = Configuration;
        }
    }
}
