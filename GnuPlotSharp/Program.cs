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
    class Program
    {
        static void Main(string[] args)
        {
            var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(assemblyFile.Directory.FullName, "gnuplot.exe");

            var fi = new FileInfo(path);
            if (!fi.Exists)
            {
                throw new Exception("Unable to locate gnuplot at: " + fi.FullName);
            }

            var scriptFile = @"c:\temp\plt.txt";
            var dataFile = @"c:\temp\plt.dat";
            var outputfile = "c:/temp/printme3.png";
            var scriptContent = 
$@"set term png
set output ""{outputfile}""
plot sin(x) * x - 10
";

            var data = @"
0.1 5
0.2 4
0.3 3
0.4 4
";
            File.WriteAllText(dataFile, data);

            File.WriteAllText(scriptFile, scriptContent);

            Process.Start(path, @"-c " + scriptFile);

            Process.Start(outputfile);            
        }
    }
}
