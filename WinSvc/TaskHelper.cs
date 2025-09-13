using System;
using System.ServiceModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Principal;
using System.Collections.Generic;

using Serilog;

namespace WinSvc
{
    #region Interface

    [ServiceContract] // (Namespace="http://RoboSvc. ...")
    public interface ITaskHelper
    {
        #region Obsolete

        //[OperationContract]
        //void BackupRepo(string url, string path);

        #endregion

        [OperationContract]
        Tuple<int, Dictionary<string, object>, object[]> ExecSp(string spName, Dictionary<string, object> prms);
    }

    #endregion

    #region Implementation

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TaskHelper : ITaskHelper
    {
        #region Consts

        const string ConnectionStringName = "DatabaseConnection";

        #endregion

        #region Obsolete

        //public void BackupRepo(string url, string path)
        //{
        //    try
        //    {
        //        Log.Debug("Try to backup git repo ...");

        //        DumpIdentityInfo();

        //        var ctx = ServiceSecurityContext.Current;
        //        var lvl = ctx.WindowsIdentity.ImpersonationLevel;

        //        if ((lvl == TokenImpersonationLevel.Impersonation)
        //            || (lvl == TokenImpersonationLevel.Delegation))
        //        {
        //            // Impersonate.
        //            using (ctx.WindowsIdentity.Impersonate())
        //            {
        //                // Make a system call in the caller's context and ACLs 
        //                // on the system resource are enforced in the caller's context. 
        //                Log.Debug("Impersonating the caller imperatively");

        //                DumpIdentityInfo();

        //                ExecStoredProcedure();
        //            }
        //        }

        //        throw new NotImplementedException("TODO: under construction");
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "BackupRepo failed");
        //    }
        //}

        #endregion

        public Tuple<int, Dictionary<string, object>, object[]> ExecSp(string spName, Dictionary<string, object> prms)
        {
            try
            {
                Log.Debug($"Try to exec: {spName} ...");

                DumpIdentityInfo();

                var ctx = ServiceSecurityContext.Current;
                var lvl = ctx.WindowsIdentity.ImpersonationLevel;

                if ((lvl == TokenImpersonationLevel.Impersonation)
                    || (lvl == TokenImpersonationLevel.Delegation)
                )
                {
                    // Impersonate.
                    using (ctx.WindowsIdentity.Impersonate())
                    {
                        // Make a system call in the caller's context and ACLs 
                        // on the system resource are enforced in the caller's context. 
                        Log.Debug("Impersonating the caller imperatively");

                        DumpIdentityInfo();

                        return ExecSpInternal(spName, prms);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ExecSp failed");
            }

            return null; //  string.Empty;
        }

        static void DumpIdentityInfo()
        {
            var identity = WindowsIdentity.GetCurrent();

            Log.Debug($"\t\tThread Identity            :{identity.Name}");
            Log.Debug($"\t\tThread Impersonation level :{identity.ImpersonationLevel}");
            Log.Debug($"\t\thToken                     :{identity.Token}");
        }

        #region Internals

        #region Obsolete

        //static void ExecStoredProcedure()
        //{
        //    var connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;

        //    Log.Debug($"connection string: {connectionString}");

        //    // "Data Source=.\\SQLEXPRESS;Initial Catalog=RoboSvc;Integrated Security=True;";

        //    var storedProcedure = "[dbo].[sp_test_impersonation]";

        //    using (var connection = new SqlConnection(connectionString))
        //    using (var command = new SqlCommand(storedProcedure, connection))
        //    {
        //        try
        //        {
        //            connection.Open();

        //            using (var reader = command.ExecuteReader())
        //            {
        //                if (!reader.Read())
        //                {
        //                    throw new InvalidOperationException("Read failed");
        //                }

        //                object[] vals = new object[reader.FieldCount];

        //                int fieldCount = reader.GetValues(vals);

        //                var usr = reader.GetString(reader.GetOrdinal("usr"));

        //                Log.Debug($"usr: {usr}");
        //            }
        //        }
        //        catch (SqlException ex)
        //        {
        //            Log.Error(ex, "Darabase error");
        //        }
        //    }
        //}

        #endregion

        static Tuple<int, Dictionary<string, object>, object[]> ExecSpInternal(string spName, Dictionary<string, object> prms)
        {
            int retVal = 0;
            var outVars = new Dictionary<string, object>();
            var results = new List<object>();

            var connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;

            Log.Debug($"connection string: {connectionString}");

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(string.Empty, connection)) 
            {
                command.CommandText = spName;
                command.CommandType = CommandType.StoredProcedure;

                // TODO: to add params
                // ...

                var returnParameter = command.Parameters.Add("@ReturnVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    // read sp resultset
                    reader.Read();

                    var vals = new object[reader.FieldCount];

                    reader.GetValues(vals);

                    // next result to return value result
                    reader.NextResult();

                    retVal = (int)returnParameter.Value;

                    return Tuple.Create(retVal, outVars, vals);
                }
            }
        }

        #endregion
    }

    #endregion
}