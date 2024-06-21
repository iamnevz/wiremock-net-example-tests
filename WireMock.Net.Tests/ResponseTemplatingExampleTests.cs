namespace WireMock.Net.Tests
{
    using System.Net;
    using System.Threading.Tasks;
    using RestSharp;
    using WireMock.RequestBuilders;
    using WireMock.ResponseBuilders;
    using WireMock.Server;

    [TestFixture]
    internal class ResponseTemplatingExampleTests
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

        [TestCase(Method.Get, "GET", TestName = "Check that GET method is echoed successfully")]
        [TestCase(Method.Post, "POST", TestName = "Check that POST method is echoed successfully")]
        [TestCase(Method.Delete, "DELETE", TestName = "Check that DELETE method is echoed successfully")]
        public async Task StubEchoHttpMethodTest(Method method, string expectedResponseMethod)
        {
            // Arrange
            this.CreateStubEchoHttpMethod();
            RestRequest request = new RestRequest("/echo-http-method", method);

            // Act
            RestResponse response = await this.client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.EqualTo($"HTTP method used was {expectedResponseMethod}"));
        }

        [TestCase("Pillars of the Earth", "Ken Follett", TestName = "Check for Pillars of the Earth")]
        [TestCase("The Secret History", "Donna Tartt", TestName = "Check for The Secret History")]
        public async Task StubEchoRequestJSONBodyElementValueTest(string title, string author)
        {
            // Arrange
            this.CreateStubEchoJsonRequestElement();
            var requestBody = new
            {
                book = new
                {
                    title = title,
                    author = author,
                },
            };

            RestRequest request = new RestRequest("/echo-json-request-element", Method.Post);
            request.AddJsonBody(requestBody);

            // Act
            RestResponse response = await this.client.ExecuteAsync(request);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.EqualTo($"The specified book title is {title}"));
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

        private void CreateStubEchoHttpMethod()
        {
            this.server.Given(
                Request.Create().WithPath("/echo-http-method").UsingAnyMethod())
            .RespondWith(
                Response.Create()
                .WithStatusCode(200)

                // The {{request.method}} handlebar extracts the HTTP method from the request
                .WithBody("HTTP method used was {{request.method}}")

                // This enables response templating for this specific mock response
                .WithTransformer());
        }

        private void CreateStubEchoJsonRequestElement()
        {
            this.server.Given(
                Request.Create().WithPath("/echo-json-request-element").UsingPost())
            .RespondWith(
                Response.Create()
                .WithStatusCode(200)

                // This extracts the book.title element from the JSON request body
                // (using a JsonPath expression) and repeats it in the response body
                .WithBody("The specified book title is {{JsonPath.SelectToken request.body \"$.book.title\"}}")
                .WithTransformer());
        }
    }
}
