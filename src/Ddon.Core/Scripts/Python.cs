using System;
using System.Diagnostics;
using System.IO;
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

        public int Run(string script)
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(script),
                FileName = PythonPath,
                Arguments = script,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(startInfo);

            while (process?.HasExited is false)
            { }

            return process?.ExitCode ?? -1;
        }
    }
}
