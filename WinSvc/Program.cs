using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using System.Threading;

namespace WinSvc
{
    internal static class Program
    {
        #region Consts

        const string LogFilePath = @"C:\logs\exsvc-.log";
        
        const string LogFileTemplate = @"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] <{ThreadId}> {Message:lj}{NewLine}{Exception}";
        const string LogConcoleTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] <{ThreadId}> {Message:lj}{NewLine}{Exception}";

        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            ConfigureLogging();

#if DEBUG
            if (!Environment.UserInteractive)
            {
#endif
                Log.Information("Running as service");
                
                using (var service = new ExSvc())
                {
                    var servicesToRun = new ServiceBase[]
                    {
                        service
                    };

                    ServiceBase.Run(servicesToRun);
                }
#if DEBUG
            }
            else
            {
                Log.Information("Running as console application");

                var exitEvent = new ManualResetEvent(false);

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    exitEvent.Set();
                };

                using (var host = new ServiceHost(typeof(TaskHelper)))
                {
                    host.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.UseWindowsGroups;

                    // Open the ServiceHostBase to create listeners and start listening for messages.
                    host.Open();
                    Log.Information("Auto Task Service lisnening");

                    exitEvent.WaitOne();

                    Log.Information("Shuting down ...");
                    host.Close();
                }
            }
#endif

            Log.CloseAndFlush();
        }

        static void ConfigureLogging()
        {
            ;
            
            var config = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#endif
                .WriteTo.File(
                    LogFilePath,
                    rollingInterval: RollingInterval.Hour,
                    outputTemplate: LogFileTemplate,
                    restrictedToMinimumLevel: LogEventLevel.Debug
                ) // Creates a new file each hour
                .Enrich.WithThreadId();

            if (Environment.UserInteractive)
            {
                config.WriteTo.Console(
                    outputTemplate: LogConcoleTemplate,
                    restrictedToMinimumLevel: LogEventLevel.Debug
                );
            }

            Log.Logger = config.CreateLogger();

            Log.Information("Logging initialized");
        }
    }
}
