using Microsoft.VisualStudio.TestTools.UnitTesting;
using RodizioSmartRestuarant;

namespace TestProject1
{
    [TestClass]
    public class AppxamlTests
    {
        [TestMethod]
        public void SendEmail_Normal_Sent()
        {
            var _app = new RodizioSmartRestuarant.Helpers.SendGridTester();

            var result = _app.SendEmail("an exception").Result;

            Assert.IsTrue(result);
        }
    }
}