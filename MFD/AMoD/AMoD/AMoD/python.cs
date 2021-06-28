using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace O2DESNet.AMoD
{
    class Python
    {
        public string StartProcess(string runFilePath,int N_P,int N_V)
        {

            string args = N_P.ToString() + " " + N_V.ToString();
           
            Process process = new Process();//创建进程对象    
            ProcessStartInfo startInfo = new ProcessStartInfo(runFilePath,args); // 括号里是(程序名,参数)
            process.StartInfo = startInfo;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            string result = null;
            while(!process.StandardOutput.EndOfStream)
            {
                result += process.StandardOutput.ReadLine() + Environment.NewLine;
                
            }
            return result;
        }
    }
}
