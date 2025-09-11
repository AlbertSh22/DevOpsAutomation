using System;
using System.ServiceModel;
using System.Data.SqlClient;
using System.Security.Principal;

using Serilog;

namespace WinSvc
{
    [ServiceContract] // (Namespace="http://RoboSvc. ...")
    public interface ITaskHelper
    {
        [OperationContract]
        void BackupRepo(string url, string path);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TaskHelper : ITaskHelper
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
