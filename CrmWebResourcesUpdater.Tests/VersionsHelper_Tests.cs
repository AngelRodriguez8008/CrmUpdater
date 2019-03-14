using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CrmWebResourcesUpdater.Tests
{
    /// <summary>
    /// Summary description for VersionsHelper
    /// </summary>
    [TestClass]
    public class VersionsHelper_Tests
    {
        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        [TestMethod]
        public void UpdateJS_ContainsExtension()
        {
            bool isJsFile = VersionsHelper.IsFileExtensionSomeOf("test.js", new[] { "js" });
            Assert.IsTrue(isJsFile);
        }
        
        [TestMethod]
        public void UpdateSolution_IncrementSolutionVersion_FromNull()
        {
            DateTime now = new DateTime(2020, 10, 2, 15, 30, 0);
            string version = VersionsHelper.IncrementSolutionVersion(null, now);
            Assert.IsNotNull(version);
            Assert.AreEqual("1.0.201002.1530", version);
        }

        [TestMethod]
        public void UpdateSolution_IncrementSolutionVersion_FromPart0()
        {
            DateTime now = new DateTime(2020, 10, 2, 15, 30, 0);
            string version = VersionsHelper.IncrementSolutionVersion("9", now);
            Assert.IsNotNull(version);
            Assert.AreEqual("9.0.201002.1530", version);
        }

        
        [TestMethod]
        public void UpdateSolution_IncrementSolutionVersion_FromPart1()
        {
            DateTime now = new DateTime(2020, 10, 2, 15, 30, 0);
            string version = VersionsHelper.IncrementSolutionVersion("8.2", now);
            Assert.IsNotNull(version);
            Assert.AreEqual("8.2.201002.1530", version);
        }
        
        [TestMethod]
        public void UpdateSolution_IncrementSolutionVersion_ReplaceTime()
        {
            DateTime now = new DateTime(2020, 10, 2, 15, 30, 0);
            string version = VersionsHelper.IncrementSolutionVersion("8.2.201002.0930", now);
            Assert.IsNotNull(version);
            Assert.AreEqual("8.2.201002.1530", version);
        }

        [TestMethod]
        public void UpdateSolution_IncrementSolutionVersion_ReplaceDateTime()
        {
            DateTime now = new DateTime(2020, 10, 2, 15, 30, 0);
            string version = VersionsHelper.IncrementSolutionVersion("8.2.191224.0930", now);
            Assert.IsNotNull(version);
            Assert.AreEqual("8.2.201002.1530", version);
        }
    }
}
