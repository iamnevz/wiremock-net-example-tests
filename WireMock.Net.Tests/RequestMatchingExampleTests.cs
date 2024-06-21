namespace WireMock.Net.Tests
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using RestSharp;
    using WireMock.Matchers;
    using WireMock.RequestBuilders;
    using WireMock.ResponseBuilders;
    using WireMock.Server;

    [TestFixture]
    internal class RequestMatchingExampleTests
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
        public async Task StubHeaderMatchingTest()
        {
            // Arrange
            this.CreateStubHeaderMatching();
            RestRequest request = new RestRequest("/header-matching", Method.Get);
            request.AddHeader("Content-Type", "application/json");

            // Act
            RestResponse response = await this.client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.EqualTo("Header matching successful"));
        }

        [Test]
        public async Task StubRequestBodyMatchingTest()
        {
            // Arrange
            this.CreateStubRequestBodyMatching();

            var requestBody = new
            {
                cars = new[]
                {
                    new { make = "Alfa Romeo" },
                    new { make = "Lancia" },
                }.ToList(),
            };

            RestRequest request = new RestRequest("/request-body-matching", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(requestBody);

            // Act
            RestResponse response = await this.client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
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

        private void CreateStubHeaderMatching()
        {
            this.server.Given(
                Request.Create().WithPath("/header-matching").UsingGet()

                // This makes the mock only respond to requests that contain
                // a 'Content-Type' header with the exact value 'application/json'
                .WithHeader("Content-Type", new ExactMatcher("application/json"))

                // This makes the mock only respond to requests that do not contain
                // the 'ShouldNotBeThere' header
                .WithHeader("ShouldNotBeThere", ".*", matchBehaviour: MatchBehaviour.RejectOnMatch))
            .RespondWith(
                Response.Create()
                .WithStatusCode(200)
                .WithBody("Header matching successful"));
        }

        private void CreateStubRequestBodyMatching()
        {
            this.server.Given(
                Request.Create().WithPath("/request-body-matching").UsingPost()

                // This makes the mock only respond to requests with a JSON request body
                // that produces a match for the specified JSON path expression
                .WithBody(new JsonPathMatcher("$.cars[?(@.make == 'Alfa Romeo')]")))
            .RespondWith(
                Response.Create()
                .WithStatusCode(201));
        }
    }
}
