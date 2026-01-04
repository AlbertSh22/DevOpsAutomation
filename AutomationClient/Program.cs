using System;
using System.ServiceModel;
using System.Security.Principal;
using System.Collections.Generic;

namespace AutomationClient
{
    using WinSvcReference;

    internal class Program
    {
        #region Consts

        const string EndpointConfigurationName = "NetTcpBinding_ITaskHelper";

        #endregion

        static void Main(string[] args)
        {
            var spName = "[dbo].[sp_test_impersonation]";
            
            using (var client = new TaskHelperClient(EndpointConfigurationName))
            {
                try
                {
                    client.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

                    var prms = client.GetSpParams(spName);

                    var dict = new Dictionary<string, object>
                    {
                        { "typ", "D" }
                    };

                    var res = client.ExecSp(spName, dict);

                    Console.WriteLine($"ret val: {res.ReturnValue}, usr: {res.DataSet[0][0]}");
                }
                catch (FaultException<TaskFault> ex)
                {
                    var detail = ex.Detail;

                    Console.WriteLine($"FaultException<MathFault>: Math fault while doing {detail.Operation}. Problem: {detail.ProblemType}");

                    client.Abort();
                }
                catch (FaultException ex)
                {
                    Console.WriteLine($"Unknown FaultException: {ex.GetType().Name} - {ex.Message}");

                    client.Abort();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"EXCEPTION: {ex.GetType().Name}  - {ex.Message}");

                    client.Abort();
                }
            }
        }
    }
}
