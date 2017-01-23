using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GnuPlotSharp
{
    public class GnuPlotLauncher
    {
        public void Launch(string arguments)
        {
            var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(assemblyFile.Directory.FullName, "gnuplot", "gnuplot.exe");

            var fi = new FileInfo(path);
            if (!fi.Exists)
            {
                throw new Exception("Unable to locate gnuplot at: " + fi.FullName);
            }

            var p = new Process();
            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();

            Console.WriteLine(p.StandardOutput.ReadToEnd());
            Console.WriteLine(p.StandardError.ReadToEnd());
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                throw new Exception("nonzero exit code");
            }
        }

    }


    public class Program
    {
        public static void Main(string[] args)
        {            
            var scriptFile = Path.GetTempFileName() + ".txt";
            var dataFile = Path.GetTempFileName() + "plt.dat";
            var outputfile = "c:/temp/printme4.png";
            var scriptContent = 
$@"set term png
set output ""{outputfile}""
plot [0.0:0.5] [2:6] ""{dataFile}"" with lines   title ""Hello World!!!""
";

            var data = @"
0.1 5
0.2 4
0.3 3
0.4 4
";            
            File.WriteAllText(dataFile, data);        
            File.WriteAllText(scriptFile, scriptContent);

            var arguments = @"-c " + scriptFile;
            
            Process.Start(outputfile);

            //Console.ReadLine();
        }
    }
}
