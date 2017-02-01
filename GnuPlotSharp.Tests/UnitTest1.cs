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
            var outputfile = new GnuPlotScript("Hello World!!!").Render(new Row<double, int>("Hello World!!!", new[] { 0.1, 0.2, 0.3, 0.4 }, new[] { 5, 4, 3, 4 }));
            
            Approvals.VerifyFile(outputfile.Outputfile);
        }
    }
}
