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

        [TestMethod]
        [UseReporter(typeof(ImageReporter), typeof(ClipboardReporter))]
        public void MultipleSeries()
        {
            var row1 = new Row<double, int>("Row1", new[] { 0.1, 0.2, 0.3, 0.4 }, new[] { 5, 4, 3, 4 });
            var row2 = new Row<double, int>("Row2", new[] { 0.0, 0.2, 0.3, 0.7 }, new[] { 1, 2, 7, 10 });

            var outputfile = new GnuPlotScript("Hello World!!!").Render(new[] { row1, row2 });

            Approvals.VerifyFile(outputfile.Outputfile);
        }

        [TestMethod]
        [UseReporter(typeof(ImageReporter), typeof(ClipboardReporter))]
        public void StringsOnXAxis()
        {
            var outputfile = new GnuPlotScript("With string labels").Render(new Row<string, int>("With string labels", new[] { "a", "b", "c", "d" }, new[] { 5, 4, 3, 4 }));

            Approvals.VerifyFile(outputfile.Outputfile);
        }
    }
}
