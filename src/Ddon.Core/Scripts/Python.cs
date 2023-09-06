using System;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace Ddon.Core.Scripts
{
    public class Python
    {
        private readonly string PythonPath;

        public Python(IOptions<ScriptsOptions> options)
        {
            PythonPath = options.Value.Python
                ?? throw new Exception("没有设置python环境信息");
        }

        public void Run(string pyPath)
        {
            Process p = new Process();
            p.StartInfo.FileName = PythonPath;
            p.StartInfo.Arguments = pyPath;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
        }
    }
}
