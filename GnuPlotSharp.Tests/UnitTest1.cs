using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ApprovalTests;
using ApprovalTests.Reporters;
using System.IO;

namespace GnuPlotSharp.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [UseReporter(typeof(ImageReporter), typeof(ClipboardReporter))]
        public void TestMethod1()
        {
            var outputfile = Path.Combine(Path.GetTempFileName() + ".png").Replace("\\", "/");

            new GnuPlotScript("Hello World!!!").Render(outputfile);
            
            Approvals.VerifyFile(outputfile);
        }
    }
}
