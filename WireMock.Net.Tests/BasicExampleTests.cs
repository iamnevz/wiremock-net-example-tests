namespace WireMock.Net.Tests
{
    using System.Net;
    using System.Threading.Tasks;
    using RestSharp;
    using WireMock.RequestBuilders;
    using WireMock.ResponseBuilders;
    using WireMock.Server;

    [TestFixture]
    internal class BasicExampleTests
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
        public async Task HelloWorldStubTest()
        {
            // Arrange
            this.CreateHelloWorldStub();
            RestRequest request = new RestRequest("/hello-world", Method.Get);

            // Act
            RestResponse response = await this.client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.ContentType, Is.EqualTo("text/plain"));
            Assert.That(response.Content, Is.EqualTo("Hello, world!"));
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

        private void CreateHelloWorldStub()
        {
            this.server.Given(
                Request.Create().WithPath("/hello-world").UsingGet())
            .RespondWith(
                Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "text/plain")
                .WithBody("Hello, world!"));
        }
    }
}
