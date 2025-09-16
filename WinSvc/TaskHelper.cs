using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.ServiceModel;
using WinSvc.Dal;
using WinSvc.Models;

namespace WinSvc
{
    #region Interface

    [ServiceContract]
    public interface ITaskHelper
    {
        #region Obsolete

        //[OperationContract]
        //void BackupRepo(string url, string path);

        #endregion

        [OperationContract]
        [FaultContract(typeof(TaskFault))]
        SpResult ExecSp(string spName, Dictionary<string, object> prms);
    }

    #endregion

    #region ErrorHandling

    [DataContract]
    public class TaskFault
    {
        #region Properties

        [DataMember]
        public string Operation { get; set; }

        [DataMember]
        public string ProblemType { get; set; }

        #endregion
    }

    #endregion

    #region Models

    [DataContract]
    public class SpResult
    { 
        [DataMember]
        public int ReturnValue { get; set; }

        [DataMember]
        public Dictionary<string, object> OutputParams { get; set; }

        [DataMember]
        public string[] Columns { get; set; }

        [DataMember]
        public object[][] DataSet { get; set; }
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

        public SpResult ExecSp(string spName, Dictionary<string, object> prms)
        {
            var method = MethodBase.GetCurrentMethod().Name;

            try
            {
                Log.Debug($"Try to exec: {spName} ...");

                DumpIdentityInfo();

                var ctx = ServiceSecurityContext.Current;
                var lvl = ctx.WindowsIdentity.ImpersonationLevel;

                if ((lvl != TokenImpersonationLevel.Impersonation)
                    && (lvl != TokenImpersonationLevel.Delegation)
                )
                {
                    var err = $"This level ({lvl}) of impersonation is not allowed";
                    
                    Log.Error(err);
                    
                    var fault = new TaskFault 
                    { 
                        Operation = method,
                        ProblemType = err
                    };

                    throw new FaultException<TaskFault>(fault);
                }

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
            catch (FaultException<TaskFault>)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{method} failed");

                var fault = new TaskFault
                {
                    Operation = method,
                    ProblemType = ex.Message
                };

                throw new FaultException<TaskFault>(fault);
            }
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

        static SpResult ExecSpInternal(string spName, Dictionary<string, object> prms)
        {

            List<SysParam> sysPrms = GetParams(spName);

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
                if (prms != null && prms.Any())
                {
                    foreach (var entry in prms)
                    {
                        var key = entry.Key;

                        var sysPrm = sysPrms.Where(x => x.ParameterName == $"@{key}").SingleOrDefault();

                        if (sysPrm == default)
                        {
                            throw new InvalidOperationException($"pram not found");
                        }

                        var typ = sysPrm.DataType;

                        var sqlParam = new SqlParameter(sysPrm.ParameterName, entry.Value);

                        switch (typ)
                        {
                            case "char":
                                sqlParam.SqlDbType = SqlDbType.Char;
                                sqlParam.Size = sysPrm.MaxLength;
                                break;

                            // etc ...
                            
                            default:
                                throw new InvalidOperationException("Not supported type");
                        }

                        command.Parameters.Add(sqlParam);
                    }
                }

                var returnParameter = command.Parameters.Add("@ReturnVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    var cols = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();

                    // read sp resultset
                    var rows = new List<object[]>();

                    while (reader.Read())
                    { 
                        var row = new object[reader.FieldCount];

                        reader.GetValues(row);

                        rows.Add(row);
                    }

                    // next result to return value result
                    reader.NextResult();

                    retVal = (int)returnParameter.Value;

                    var res = new SpResult
                    {
                        ReturnValue = retVal,
                        OutputParams = outVars,
                        Columns = cols,
                        DataSet = rows.ToArray()
                    };

                    return res;
                }
            }
        }

        static List<SysParam> GetParams(string spName)
        {
            using (var context = new RoboSvcEntities())
            {
#if DEBUG
                context.Database.Log = s => Log.Debug(s);
#endif

                var prms = context.Database.SqlQuery<SysParam>($@"select [name] ParameterName, TYPE_NAME(user_type_id) DataType, max_length AS MaxLength 
from sys.parameters 
where object_id = OBJECT_ID('{spName}')").ToList();

                if (prms.Any())
                { 
                    Log.Debug($"param name: {prms.First().ParameterName}");
                }

                return prms;
            }
        }

        #endregion
    }

    #endregion
}