using System;
using System.Security.Principal;

namespace AutomationClient
{
    //using TaskServiceReference;
    using AutomationClient.WinSvcReference;

    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var client = new 
                    //GitHelperClient("NetTcpBinding_IGitHelper")
                    TaskHelperClient("NetTcpBinding_ITaskHelper")
                    ;

                client.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

                client.BackupRepo(@"http://about:blank", @"C:\\temp");
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
