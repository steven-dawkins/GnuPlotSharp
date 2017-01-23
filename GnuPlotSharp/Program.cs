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
    public class Program
    {
        public static void Main(string[] args)
        {
            var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(assemblyFile.Directory.FullName, "gnuplot", "gnuplot.exe");

            var fi = new FileInfo(path);
            if (!fi.Exists)
            {
                throw new Exception("Unable to locate gnuplot at: " + fi.FullName);
            }

            var scriptFile = @"c:\temp\plt.txt";
            var dataFile = @"c:/temp/plt.dat";
            var outputfile = "c:/temp/printme3.png";
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
            Console.WriteLine(scriptContent);
            File.WriteAllText(dataFile, data);

            File.WriteAllText(scriptFile, scriptContent);

            var p = new Process();
            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = @"-c " + scriptFile;
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
            //Process.Start(outputfile);

            //Console.ReadLine();
        }
    }
}
