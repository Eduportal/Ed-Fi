using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common
{
    using System.Diagnostics;

    // ReSharper disable once InconsistentNaming
    public static class IISExpressHelper
    {
        public static Process Start(string path, int port)
        {
            var programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var iisProcess = new Process
            {
                StartInfo =
                {
                    FileName = programfiles + "\\IIS Express\\iisexpress.exe",
                    Arguments = string.Format("/path:\"{0}\" /port:{1}", path, port),
                    UseShellExecute = true
                }
            };
            try
            {
                iisProcess.Start();
            }
            catch
            {
                iisProcess.CloseMainWindow();
                iisProcess.Dispose();
            }
            return iisProcess;
        }

        public static void Stop(Process iisProcess)
        {
            if (iisProcess != null)
            {
                if (!iisProcess.HasExited)
                {
                    iisProcess.Kill();
                }
                iisProcess.Dispose();
            }
        }
    }
}
