using System;
//using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Security.Principal;

using Serilog;

namespace TaskService
{
    // Define a service contract
    [ServiceContract] // (Namespace="http://RoboSvc. ...")
    public interface IGitHelper
    {
        [OperationContract]
        void BackupRepo(string url, string path);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class GitHelper : IGitHelper
    {
        public void BackupRepo(string url, string path)
        {
            try
            {
                Log.Information("Try to backup git ...");

                DumpIdentityInfo();

                var ctx = ServiceSecurityContext.Current;
                var level = ctx.WindowsIdentity.ImpersonationLevel;

                if ((level == TokenImpersonationLevel.Impersonation)
                    || (level == TokenImpersonationLevel.Delegation))
                {
                    // Impersonate.
                    using (ctx.WindowsIdentity.Impersonate())
                    {
                        // Make a system call in the caller's context and ACLs 
                        // on the system resource are enforced in the caller's context. 
                        Log.Information("Impersonating the caller imperatively");

                        DumpIdentityInfo();

                        ExecStoredProcedure();
                    }
                }

                throw new NotImplementedException("TODO: under construction");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "BackupRepo failed");
            }
        }

        static void Main(string[] args)
        {
            ConfigureLogging();

            var exitEvent = new ManualResetEvent(false);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            using (var host = new ServiceHost(typeof(GitHelper)))
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

        static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Logging initialized");
        }

        static void DumpIdentityInfo()
        {
            var identity = WindowsIdentity.GetCurrent();
            
            Log.Information($"\t\tThread Identity            :{identity.Name}");
            Log.Information($"\t\tThread Impersonation level :{identity.ImpersonationLevel}");
            Log.Information($"\t\thToken                     :{identity.Token.ToString()}");
        }

        static void ExecStoredProcedure()
        {
            var connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=RoboSvc;Integrated Security=True;";

            var storedProcedure = "[dbo].[sp_test_impersonation]";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(storedProcedure, connection))
            {
                try
                {
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new InvalidOperationException("Read failed");
                        }

                        var usr = reader.GetString(reader.GetOrdinal("usr"));

                        Log.Information($"usr: {usr}");
                    }
                }
                catch (SqlException ex)
                {
                    Log.Error(ex, "Darabase error");
                }
            }
        }
    }
}