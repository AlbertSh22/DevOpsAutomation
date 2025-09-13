using System;
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
            try
            {
                var client = new TaskHelperClient(EndpointConfigurationName);

                client.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

                var dict = new Dictionary<string, object>();
                
                var res = client.ExecSp("[dbo].[sp_test_impersonation]", dict);

                Console.WriteLine($"ret cod: {res.Item1}, usr: {res.Item3[0]}");
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
