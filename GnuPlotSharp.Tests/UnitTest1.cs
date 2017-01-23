using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ApprovalTests;
using ApprovalTests.Reporters;

namespace GnuPlotSharp.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [UseReporter(typeof(ImageReporter), typeof(ClipboardReporter))]
        public void TestMethod1()
        {
            var outputfile = "c:/temp/printme4.png";

            new GnuPlotScript("Hello World2!!!").Render(outputfile);
            
            Approvals.VerifyFile(outputfile);
        }
    }
}
