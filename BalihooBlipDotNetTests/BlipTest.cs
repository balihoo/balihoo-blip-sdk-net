using BalihooBlipDotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;

namespace BalihooBlipDotNetTests
{
    [TestClass]
    public class BlipTest
    {
        [TestMethod]
        public void TestBlipResponseSuccessfullyConvertsHttpResponseMessage()
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Successful Test")
            };
            var blipResponse = new BlipResponse((int)httpResponse.StatusCode, httpResponse.Content.ToString());

            Assert.AreEqual(blipResponse.StatusCode, (int)httpResponse.StatusCode);
            Assert.AreEqual(blipResponse.Body, httpResponse.Content.ToString());
        }
    }
}
