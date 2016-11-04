using System.Configuration;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfigManager.Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestInitialize]
        public void Initialize()
        {
            ConfigurationManager.RefreshSection("appSettings");
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        [TestMethod]
        public void File_Does_Not_Exist()
        {
            Assert.IsFalse(ConfigManager.Load("C:\\asdf.xml"));
        }

        [TestMethod]
        public void Invalid_XML()
        {
            var filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.FullName + "\\Invalid.config";
            Assert.IsFalse(ConfigManager.Load(filePath));
        }

        [TestMethod]
        public void Load()
        {
            var filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.FullName + "\\Local.config";
            Assert.IsTrue(ConfigManager.Load(filePath));

            Assert.AreEqual("local", ConfigurationManager.AppSettings["test1"]);
            Assert.AreEqual("local", ConfigurationManager.AppSettings["test2"]);
            Assert.AreEqual("app", ConfigurationManager.AppSettings["test3"]);

            Assert.AreEqual("local", ConfigurationManager.ConnectionStrings["conn1"].ConnectionString);
            Assert.AreEqual("app", ConfigurationManager.ConnectionStrings["conn2"].ConnectionString);
            Assert.AreEqual("local", ConfigurationManager.ConnectionStrings["conn3"].ConnectionString);
        }

        [TestMethod]
        public void Load_ActiveConfig()
        {
            var activeConfig = ConfigurationManager.AppSettings["BuildConfiguration"];
            var filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.FullName + $"\\Local.{activeConfig}.config";
            Assert.IsTrue(ConfigManager.Load(filePath));

            Assert.AreEqual("app", ConfigurationManager.AppSettings["test1"]);
            Assert.AreEqual("app", ConfigurationManager.AppSettings["test2"]);
            Assert.AreEqual("active", ConfigurationManager.AppSettings["test3"]);
            Assert.AreEqual("active", ConfigurationManager.AppSettings["test4"]);
        }
    }
}