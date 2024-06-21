namespace WireMock.Net.Tests
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using WireMock.RequestBuilders;
    using WireMock.ResponseBuilders;
    using WireMock.Server;

    [TestFixture]
    public class FaultSimulationExampleTests
    {
        private const string BaseUrl = "http://localhost:9876";
        private WireMockServer server;
        private RestClient client;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.client = new RestClient(BaseUrl);
        }

        [SetUp]
        public void SetUp()
        {
            this.server = WireMockServer.Start(9876);
        }

        [Test]
        public async Task StubDelayTest()
        {
            // Arrange
            this.CreateStubReturningDelayedResponse();
            RestRequest request = new RestRequest("/delay", Method.Get);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Act
            RestResponse response = await this.client.ExecuteAsync(request);
            stopwatch.Stop();

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(2000));
        }

        [Test]
        public async Task StubFaultTest()
        {
            // Arrange
            this.CreateStubReturningFault();
            RestRequest request = new RestRequest("/fault", Method.Get);

            // Act
            RestResponse response = await this.client.ExecuteAsync(request);

            // Assert
            Assert.That(response.Content, Is.Not.Null);
            Assert.Throws<JsonReaderException>(() => JObject.Parse(response.Content));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.client.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            this.server.Stop();
            this.server.Dispose();
        }

        private void CreateStubReturningDelayedResponse()
        {
            this.server.Given(
                Request.Create().WithPath("/delay").UsingGet())
            .RespondWith(
                Response.Create()
                .WithStatusCode(200)

                // Returns the response after a 2000ms delay
                .WithDelay(TimeSpan.FromMilliseconds(2000)));
        }

        private void CreateStubReturningFault()
        {
            this.server.Given(
                Request.Create().WithPath("/fault").UsingGet())
            .RespondWith(
                Response.Create()

                // Returns a response with HTTP status code 200
                // and garbage in the response body
                .WithFault(FaultType.MALFORMED_RESPONSE_CHUNK));
        }
    }
}
