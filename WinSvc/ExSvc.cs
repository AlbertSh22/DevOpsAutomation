using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using System.Timers;

namespace WinSvc
{
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
            // Update the service state to Start Pending.
            var serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_START_PENDING,
                dwWaitHint = 100000
            };

            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // using (var _host = new ServiceHost(typeof(TaskHelper)))
            _host = new ServiceHost(typeof(TaskHelper));
            // {
                _host.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.UseWindowsGroups;

                // Open the ServiceHostBase to create listeners and start listening for messages.
                _host.Open();
                //Log.Information("Auto Task Service lisnening");
            // }

            eventLog.WriteEntry("In OnStart");

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;

            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            // Update the service state to Stop Pending.
            var serviceStatus = new ServiceStatus 
            {
                dwCurrentState = ServiceState.SERVICE_STOP_PENDING,
                dwWaitHint = 100000
            };
            
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            _host.Close();

            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;

            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog.WriteEntry("In OnStop.");
        }

        protected override void OnContinue()
        {
            //base.OnContinue();

            eventLog.WriteEntry("In OnContinue.");
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            eventLog.WriteEntry("Monitoring the System", EventLogEntryType.Information, _eventId++);
        }

        #endregion
    }
}
