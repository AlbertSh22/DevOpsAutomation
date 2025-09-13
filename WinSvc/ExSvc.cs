using System;
using System.Timers;
using System.Diagnostics;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Description;

using Serilog;

namespace WinSvc
{
    #region Interop

    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

    #endregion

    public partial class ExSvc : ServiceBase
    {
        #region Extern

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        #endregion

        #region Consts

        const string SourceName = "ExSource";
        const string LogName = "ExLog";

        #endregion

        #region Fields

        Timer _timer;
        int _eventId = 1;
        ServiceHost _host;

        #endregion

        #region Ctor

        public ExSvc()
        {
            InitializeComponent();

            if (!EventLog.SourceExists(SourceName))
            {
                EventLog.CreateEventSource(SourceName, LogName);
            }

            eventLog.Source = SourceName;
            eventLog.Log = LogName;

            // Set up a timer that triggers every minute.
            _timer = new Timer
            {
                Interval = 60000 // 60 seconds
            };

            _timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            _timer.Start();
        }

        #endregion

        #region Events

        protected override void OnStart(string[] args)
        {
            try
            {
                var message = "In OnStart.";

                Log.Information(message);

                eventLog.WriteEntry(message);

                // Update the service state to Start Pending.
                var serviceStatus = new ServiceStatus
                {
                    dwCurrentState = ServiceState.SERVICE_START_PENDING,
                    dwWaitHint = 100000
                };

                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                _host = new ServiceHost(typeof(TaskHelper));
                _host.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.UseWindowsGroups;

                // Open the ServiceHostBase to create listeners and start listening for messages.
                _host.Open();

                Log.Information("Auto Task Service lisnening");

                // Update the service state to Running.
                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;

                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OnStart failed.");

                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                var message = "In OnStop.";

                Log.Information(message);

                eventLog.WriteEntry(message);

                // Update the service state to Stop Pending.
                var serviceStatus = new ServiceStatus
                {
                    dwCurrentState = ServiceState.SERVICE_STOP_PENDING,
                    dwWaitHint = 100000
                };

                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                Log.Information("Shuting down ...");
                _host.Close();

                // Update the service state to Stopped.
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;

                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OnStop failed.");

                throw;
            }
        }

        protected override void OnContinue()
        {
            try
            {
                var message = "In OnContinue.";

                Log.Information(message);

                eventLog.WriteEntry(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OnContinue failed.");

                throw;
            }
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            try
            {
                var message = "Monitoring the System";

                // TODO: Insert monitoring activities here.
                // ...

                Log.Information($"{message}, eventId: {_eventId}");

                eventLog.WriteEntry(message, EventLogEntryType.Information, _eventId++);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OnTimer failed.");

                throw;
            }
        }

        #endregion
    }
}
