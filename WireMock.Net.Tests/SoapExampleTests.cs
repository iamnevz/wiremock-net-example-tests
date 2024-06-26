namespace WireMock.Net.Tests
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using SoapHttpClient;
    using SoapHttpClient.Enums;
    using WireMock.RequestBuilders;
    using WireMock.ResponseBuilders;
    using WireMock.Server;

    [TestFixture]
    internal class SoapExampleTests
    {
        private WireMockServer server;
        private SoapClient client;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var httpClientFactory = CustomHttpClientFactory();
            this.client = new SoapClient(httpClientFactory);
        }

        [SetUp]
        public void SetUp()
        {
            this.server = WireMockServer.Start(9876, useSSL: true);
        }

        [Test]
        public async Task SoapStubTest()
        {
            // Arrange
            this.CreateSoapStub();
            var ns = XNamespace.Get("http://helio.spdf.gsfc.nasa.gov/");

            // Act
            var response =
                await this.client.PostAsync(
                    new Uri("https://localhost:9876/WS/helio/1/HeliocentricTrajectoriesService"),
                    SoapVersion.Soap11,
                    body: new XElement(ns.GetName("getAllObjects")));

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.client = null!;
        }

        [TearDown]
        public void TearDown()
        {
            this.server.Stop();
            this.server.Dispose();
        }

        private static IHttpClientFactory CustomHttpClientFactory()
        {
            var serviceProvider = new ServiceCollection();

            serviceProvider
                .AddHttpClient(nameof(SoapClient))
                .ConfigurePrimaryHttpMessageHandler(e =>
                    new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    });

            return serviceProvider.BuildServiceProvider().GetService<IHttpClientFactory>() !;
        }

        private void CreateSoapStub()
        {
            this.server.Given(
                Request.Create().WithPath("/WS/helio/1/HeliocentricTrajectoriesService").UsingPost())
            .RespondWith(
                Response.Create()
                .WithStatusCode(200));
        }
    }
}
