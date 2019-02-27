using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ProcessingEngineDemo.Console;

namespace console
{
    class Program
    {
        static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddXmlFile("settings.xml", false);
            configBuilder.AddXmlFile("secrets.xml", true);
            var config = configBuilder.Build();

            var app = new Application(config);
            Task.Run(() => app.RunAsync()).Wait();
        }
    }
}
