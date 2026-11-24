using DTFApp.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DTFApp.Tests
{
    [TestClass]
    public class DtfApiServiceTests
    {
        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
            public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            {
                _responder = responder;
            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_responder(request));
            }
        }

        private static DtfApiService CreateService(HttpStatusCode status, string body)
        {
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(status)
            {
                Content = new StringContent(body ?? "")
            });
            return new DtfApiService(new HttpClient(handler));
        }

        [TestMethod]
        public async Task GetNewsAsync_ParsesResponse()
        {
            var service = CreateService(HttpStatusCode.OK, "{\"result\":{\"news\":[{\"type\":\"entry\",\"data\":{\"id\":5,\"title\":\"T\"}}],\"lastId\":5}}");

            var result = await service.GetNewsAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.News.Length);
            Assert.AreEqual(5, result.News[0].Data.Id);
            Assert.AreEqual(5, result.LastId);
        }

        [TestMethod]
        public async Task GetNewsAsync_NonSuccessStatus_Throws()
        {
            var service = CreateService(HttpStatusCode.InternalServerError, "server error");

            await Assert.ThrowsExceptionAsync<HttpRequestException>(() => service.GetNewsAsync());
        }

        [TestMethod]
        public async Task GetCommentsAsync_HonorsCancellation()
        {
            var service = CreateService(HttpStatusCode.OK, "{\"result\":{\"items\":[]}}");

            await Assert.ThrowsExceptionAsync<TaskCanceledException>(
                () => service.GetCommentsAsync(1, new CancellationToken(canceled: true)));
        }

        [TestMethod]
        public async Task LoginAsync_NonSuccessStatus_Throws()
        {
            var service = CreateService(HttpStatusCode.Unauthorized, "{\"message\":\"wrong password\"}");

            await Assert.ThrowsExceptionAsync<HttpRequestException>(() => service.LoginAsync("a@b.c", "pw"));
        }

        [TestMethod]
        public async Task GetEntryAsync_UsesContentEndpointWithId()
        {
            string requestedUri = null;
            var handler = new StubHttpMessageHandler(req =>
            {
                requestedUri = req.RequestUri.ToString();
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"result\":{\"title\":\"x\"}}") };
            });
            var service = new DtfApiService(new HttpClient(handler));

            var response = await service.GetEntryAsync(77);

            StringAssert.Contains(requestedUri, "/v2.0/content?id=77");
            Assert.AreEqual("x", response.Result.Title);
        }
    }
}
