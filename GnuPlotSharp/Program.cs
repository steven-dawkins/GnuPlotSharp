using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

    public class Row<T1, T2>
    {
        public readonly KeyValuePair<T1, T2>[] Data;
        public readonly string Title;

        public Row(string title, T1[] xdata, T2[] ydata)
            : this(title, xdata.Zip(ydata, (a,b) => new KeyValuePair<T1, T2>(a, b)).ToArray())
        {

        }

        public Row(string title, KeyValuePair<T1, T2>[] data)
        {
            this.Title = title;
            this.Data = data;
        }
    }

    public class GnuPlotScript
    {
        private readonly string title;

        public GnuPlotScript(string title)
        {
            this.title = title;
        }        

        public string Render<T1, T2>(Row<T1, T2> row)
        {
            var outputfile = Path.Combine(Path.GetTempFileName() + ".png");

            Render(outputfile, row);

            return outputfile;
        }

        public void Render<T1, T2>(string outputfile, Row<T1, T2> row)
        {
            var scriptFile = Path.GetTempFileName() + ".txt";
            var dataFile = (Path.GetTempFileName() + "plt.dat");

            var scriptContent =
$@"set term png
set output ""{outputfile.Replace("\\", "/")}""
";

            // $@"plot [0.0:0.5] [2:6] ""{dataFile.Replace("\\", "\\\\")}"" with lines   title ""{title}""";

            var xmin = row.Data.Select(v => v.Key).Min();
            var xmax = row.Data.Select(v => v.Key).Max();
            var ymin = row.Data.Select(v => v.Value).Min();
            var ymax = row.Data.Select(v => v.Value).Max();

            scriptContent += $@"plot [{xmin}:{xmax}] [{ymin}:{ymax}] ""{dataFile.Replace("\\", "\\\\")}"" with lines   title ""{row.Title}""";

            var data = String.Join("\n", row.Data.Select(kv => $"{kv.Key}\t{kv.Value}"));                    

            File.WriteAllText(dataFile, data);
            File.WriteAllText(scriptFile, scriptContent);

            var arguments = @"-c " + scriptFile;

            new GnuPlotLauncher().Launch(arguments);
        }
    }

    internal class Program
    {
        internal static void Main(string[] args)
        {                        
            var outputfile = "c:/temp/printme4.png";

            new GnuPlotScript("Hello World2!!!").Render(outputfile, new Row<double, int>("Hello World!!!", new[] { 0.1, 0.2, 0.3, 0.4 }, new[] { 5, 4, 3, 4 }));            

            Process.Start(outputfile);

            //Console.ReadLine();
        }
    }
}
