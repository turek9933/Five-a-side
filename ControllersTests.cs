using Microsoft.AspNetCore.Identity.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Five_a_side.Controllers;
using Five_a_side.Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;


namespace Five_a_side.Tests
{
    [TestClass]
    public class PlayersControllerTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private PlayersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _controller = new PlayersController(_mockHttpClientFactory.Object);
        }

        [TestMethod]
        public void Get_ReturnsPlayersList()
        {
            var result = _controller.Get();

            Assert.IsInstanceOfType(result, typeof(IEnumerable<Player>));
        }

        [TestMethod]
        public void GetTempPlayer_ReturnsTempPlayer()
        {
            var result = _controller.GetTempPlayer();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsInstanceOfType(okResult.Value, typeof(Player));
            var player = okResult.Value as Player;
            Assert.IsNotNull(player);
            Assert.AreEqual("Cristiano Ronaldo", player.Name);
        }

        [TestMethod]
        public async Task PutTempPlayer_ValidPlayer_ReturnsOk()
        {
            var player = new Player("8198", "Cristiano Ronaldo", 39, "Portugal", "Al-Nassr FC", "€15.00m");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"results\":[{\"id\":\"8198\",\"name\":\"Cristiano Ronaldo\",\"age\":\"39\",\"nationalities\":[\"Portugal\"],\"club\":{\"name\":\"Al-Nassr FC\"},\"marketValue\":\"€15.00m\"}]}")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost:8000")
            };
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var result = await _controller.PutTempPlayer(player);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual($"Player:\n\n{player}\n\nsaved successfully.", okResult.Value);
        }

        [TestMethod]
        public void SavePlayer_TempPlayerExists_ReturnsOk()
        {
            var result = _controller.SavePlayer();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetPlayerOnTransfermarkt_ValidPlayerId_ReturnsOk()
        {
            int playerId = 8198;
            var mockClient = new Mock<HttpMessageHandler>();
            mockClient.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":\"8198\",\"name\":\"Cristiano Ronaldo\",\"age\":\"39\",\"citizenship\":[\"Portugal\"],\"club\":{\"name\":\"Al-Nassr\"},\"marketValue\":\"€15.00m\"}")
                });

            var client = new HttpClient(mockClient.Object)
            {
                BaseAddress = new Uri("http://localhost:8000")
            };
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var result = await _controller.GetPlayerOnTransfermarkt(playerId);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsInstanceOfType(okResult.Value, typeof(Player));
            var player = okResult.Value as Player;
            Assert.IsNotNull(player);
            Assert.AreEqual("Cristiano Ronaldo", player.Name);
        }

    }

    [TestClass]
    public class TeamsControllerTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private TeamsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _controller = new TeamsController(_mockHttpClientFactory.Object);
        }

        [TestMethod]
        public void Get_ReturnsTeamsList()
        {
            var result = _controller.Get();

            Assert.IsInstanceOfType(result, typeof(IEnumerable<Team>));
        }

        [TestMethod]
        public void GetTempTeam_ReturnsTempTeam()
        {
            var result = _controller.GetTempTeam();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsInstanceOfType(okResult.Value, typeof(Team));
            var team = okResult.Value as Team;
            Assert.IsNotNull(team);
            Assert.AreEqual("Default Team", team.Name);
        }

        [TestMethod]
        public async Task PutTempTeam_ValidTeam_ReturnsOk()
        {
            var team = new Team
            {
                Id = 1,
                Name = "Test Team",
                Description = "Description",
                GK = new Player("59377", "David de Gea", 33, "Spain", "Without Club", "€5.00m"),
                LB = new Player("117229", "Zbigniew Boniek", 68, "Poland", "Retired", "€0"),
                RB = new Player("418560", "Erling Haaland", 23, "Norway", "Manchester City", "€180.00m"),
                LF = new Player("8198", "Cristiano Ronaldo", 39, "Portugal", "Al-Nassr FC", "€15.00m"),
                RF = new Player("15242", "Grzegorz Rasiak", 45, "Poland", "Retired", "€0")
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"position\":{\"main\":\"Goalkeeper\"}}")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost:8000")
            };
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var result = await _controller.PutTempTeam(team);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("Temporary team saved successfully.", okResult.Value);
        }

        [TestMethod]
        public void SaveTempTeam_TempTeamExists_ReturnsOk()
        {
            var result = _controller.SaveTempTeam();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void GetTeamById_ValidTeamId_ReturnsOk()
        {
            int teamId = 1;

            var result = _controller.GetTeamById(teamId);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }
    }

    [TestClass]
    public class CurrencyControllerTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private CurrencyController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _controller = new CurrencyController(_mockHttpClientFactory.Object);
        }

        [TestMethod]
        public async Task GetCurrencyRates_ReturnsOk()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[{\"table\":\"A\",\"no\":\"085/A/NBP/2023\",\"effectiveDate\":\"2023-05-05\",\"rates\":[{\"currency\":\"US Dollar\",\"code\":\"USD\",\"mid\":3.5}]}]")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://api.nbp.pl")
            };

            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var result = await _controller.GetCurrencyRates();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsInstanceOfType(okResult.Value, typeof(List<CurrencyRate>));
            var rates = okResult.Value as List<CurrencyRate>;
            Assert.IsNotNull(rates);
            Assert.AreEqual(1, rates.Count);
            Assert.AreEqual("USD", rates[0].Code);
            Assert.AreEqual(3.5m, rates[0].Mid);
        }

        [TestMethod]
        public async Task ConvertTempPlayerValue_ValidCurrencyCode_ReturnsOk()
        {
            string targetCurrencyCode = "USD";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[{\"table\":\"A\",\"no\":\"085/A/NBP/2023\",\"effectiveDate\":\"2023-05-05\",\"rates\":[{\"currency\":\"US Dollar\",\"code\":\"USD\",\"mid\":3.5},{\"currency\":\"Euro\",\"code\":\"EUR\",\"mid\":4.5}]}]")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://api.nbp.pl")
            };

            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var result = await _controller.ConvertTempPlayerValue(targetCurrencyCode);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var json = JObject.Parse(JsonConvert.SerializeObject(okResult.Value));
            Assert.AreEqual("Cristiano Ronaldo", json["Player"].ToString());
            Assert.AreEqual("€15.00m", json["OriginalValue"].ToString());
            Assert.AreEqual("19m USD", json["ConvertedValue"].ToString());
        }

        [TestMethod]
        public async Task ConvertPlayerValue_ValidPlayerIdAndCurrencyCode_ReturnsOk()
        {
            string playerId = "8198";
            string targetCurrencyCode = "USD";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[{\"table\":\"A\",\"no\":\"085/A/NBP/2023\",\"effectiveDate\":\"2023-05-05\",\"rates\":[{\"currency\":\"US Dollar\",\"code\":\"USD\",\"mid\":3.5},{\"currency\":\"Euro\",\"code\":\"EUR\",\"mid\":4.5}]}]")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://api.nbp.pl")
            };

            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var result = await _controller.ConvertPlayerValue(playerId, targetCurrencyCode);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var json = JObject.Parse(JsonConvert.SerializeObject(okResult.Value));
            Assert.AreEqual("Cristiano Ronaldo", json["Player"].ToString());
            Assert.AreEqual("€15.00m", json["OriginalValue"].ToString());
            Assert.AreEqual("19m USD", json["ConvertedValue"].ToString());
        }

        [TestMethod]
        public async Task ConvertTempTeamValue_ValidCurrencyCode_ReturnsOk()
        {
            string targetCurrencyCode = "USD";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[{\"table\":\"A\",\"no\":\"085/A/NBP/2023\",\"effectiveDate\":\"2023-05-05\",\"rates\":[{\"currency\":\"US Dollar\",\"code\":\"USD\",\"mid\":3.5},{\"currency\":\"Euro\",\"code\":\"EUR\",\"mid\":4.5}]}]")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://api.nbp.pl")
            };

            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var result = await _controller.ConvertTempTeamValue(targetCurrencyCode);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var json = JObject.Parse(JsonConvert.SerializeObject(okResult.Value));
            Assert.AreEqual("Default Team", json["Team"].ToString());
            Assert.AreEqual("€200m", json["OriginalValue"].ToString());
            Assert.AreEqual("257m USD", json["ConvertedValue"].ToString());
        }

        [TestMethod]
        public async Task ConvertTeamValue_ValidTeamIdAndCurrencyCode_ReturnsOk()
        {
            string teamId = "0";
            string targetCurrencyCode = "USD";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[{\"table\":\"A\",\"no\":\"085/A/NBP/2023\",\"effectiveDate\":\"2023-05-05\",\"rates\":[{\"currency\":\"US Dollar\",\"code\":\"USD\",\"mid\":3.5},{\"currency\":\"Euro\",\"code\":\"EUR\",\"mid\":4.5}]}]")
                });

            var client = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://api.nbp.pl")
            };

            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var result = await _controller.ConvertTeamValue(teamId, targetCurrencyCode);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var json = JObject.Parse(JsonConvert.SerializeObject(okResult.Value));
            Assert.AreEqual("Default Team", json["Team"].ToString());
            Assert.AreEqual("€210m", json["OriginalValue"].ToString());
            Assert.AreEqual("270m USD", json["ConvertedValue"].ToString());
        }
    }
    [TestClass]
    public class LoginControllerTests
    {
        private Mock<IConfiguration> _mockConfiguration;
        private LoginController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.SetupGet(c => c["Jwt:Key"]).Returns("MoimZdaniemNieMaTakZeDobrzeAlboZeNieDobrzeUwazamZeMarchew");
            _mockConfiguration.SetupGet(c => c["Jwt:Issuer"]).Returns("aehit@students.vizja.pl");

            _controller = new LoginController(_mockConfiguration.Object);
        }

        [TestMethod]
        public void PostLogin_ReturnsOkWithToken()
        {
            var loginRequest = new LoginRequest
            {
                Email = "string",
                Password = "string"
            };

            var result = _controller.PostLogin(loginRequest);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsInstanceOfType(okResult.Value, typeof(string));
            var token = okResult.Value as string;
            Assert.IsTrue(token.Split('.').Length == 3);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            Assert.IsNotNull(jwtToken);
            Assert.AreEqual("aehit@students.vizja.pl", jwtToken.Issuer);
        }
    }
}
