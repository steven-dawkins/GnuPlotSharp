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
            Program.Main(new string[] { });
            Approvals.VerifyFile(@"c:\temp\printme3.png");
        }
    }
}
