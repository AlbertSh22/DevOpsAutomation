using System;
using System.Diagnostics;
using System.ServiceModel;

namespace Gpb.TaskService.Models
{
    /* [ServiceContract(Namespace = "http://Gpb.TaskService.Models")]
    public interface IGitHelper
    {
        [OperationContract]
        void BackupRepo(string url, string path);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class GitHelper : IGitHelper
    {
        #region Consts

        internal string GitAppName = "git.exe";

        #endregion

        #region Api

        public void BackupRepo(string url, string path)
        {
            try
            {
                GitClone(url, path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
            // TODO: under construction
            // ...
            
            //throw new NotImplementedException();
        }

        #endregion

        #region Implementation

        internal void GitClone(string url, string path)
        {
            //try
            //{
            // TODO: under construction
            // ...
            //}
            //catch (Exception ) 
            //{ 
            //    // ...
            //}

            var startInfo = new ProcessStartInfo
            {
                FileName = "git.exe",

                // UseShellExecute = false,
                CreateNoWindow = true,
            };

            // ...

            using (var process = new Process
            {
                StartInfo = startInfo
            }
            )
            {
                process.Start();

                // ...

                process.WaitForExit();
            }

            throw new NotImplementedException();
        }

        #endregion
    } */
}
