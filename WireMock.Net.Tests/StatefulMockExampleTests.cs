namespace WireMock.Net.Tests
{
    using System.Net;
    using System.Threading.Tasks;
    using RestSharp;
    using WireMock.RequestBuilders;
    using WireMock.ResponseBuilders;
    using WireMock.Server;

    [TestFixture]
    internal class StatefulMockExampleTests
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
        public async Task StatefulStubTest()
        {
            // Arrange
            this.CreateStatefulStub();
            RestRequest request = new RestRequest("/todo/items", Method.Get);
            RestResponse response = await this.client.ExecuteAsync(request);
            Assert.That(response.Content, Is.EqualTo("Buy milk"));

            request = new RestRequest("/todo/items", Method.Post);
            response = await this.client.ExecuteAsync(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            // Act
            request = new RestRequest("/todo/items", Method.Get);
            response = await this.client.ExecuteAsync(request);

            // Assert
            Assert.That(response.Content, Is.EqualTo("Buy milk;Cancel newspaper subscription"));
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

        private void CreateStatefulStub()
        {
            this.server.Given(
                Request.Create().WithPath("/todo/items").UsingGet())

            // In this scenario, when the current state is 'TodoList State Started',
            // a call to an HTTP GET will only return 'Buy milk'
           .InScenario("To do list")
           .WillSetStateTo("TodoList State Started")
           .RespondWith(
                Response.Create().WithBody("Buy milk"));

            this.server.Given(
                Request.Create().WithPath("/todo/items").UsingPost())

            // In this scenario, when the current state is 'TodoList State Started',
            // a call to an HTTP POST will trigger a state transition to new state
            // 'Cancel newspaper item added'
            .InScenario("To do list")
            .WhenStateIs("TodoList State Started")
            .WillSetStateTo("Cancel newspaper item added")
            .RespondWith(
                Response.Create().WithStatusCode(201));

            this.server.Given(
                Request.Create().WithPath("/todo/items").UsingGet())

            // In this scenario, when the current state is 'Cancel newspaper item added',
            // a call to an HTTP GET will return 'Buy milk;Cancel newspaper subscription'
            .InScenario("To do list")
            .WhenStateIs("Cancel newspaper item added")
            .RespondWith(
                Response.Create().WithBody("Buy milk;Cancel newspaper subscription"));
        }
    }
}
