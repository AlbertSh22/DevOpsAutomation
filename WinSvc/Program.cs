using System.ServiceProcess;

using Serilog;

namespace WinSvc
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ConfigureLogging();

            ServiceBase[] ServicesToRun;

            ServicesToRun = new ServiceBase[]
            {
                new ExSvc()
            };

            ServiceBase.Run(ServicesToRun);
        }

        static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo
                .File(@"C:\logs\exsvc-.log", rollingInterval: RollingInterval.Hour) // Creates a new file each hour
                .CreateLogger();

            Log.Information("Logging initialized");
        }
    }
}
