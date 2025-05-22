using System.Net;
using System.Net.Http.Json;
using System.Text;
using TestProject.Server.Baseclasses;
using TestProject.Server.Injection;
using Testrepository.Server.Exceptions;
using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Models.Requests;
using Testrepository.Server.Utils;
using Xunit.Abstractions;

namespace TestProject.Server.Tests.ControllerTests
{
    public class UserControllerTests : TestBase, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UserControllerTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper logger) : base(factory, logger)
        {
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task GetUserById_InvalidId_Returns_404()
        {   
            string sub = "446753a7-f493-4f60-9a64-85b5d4f994d7";
            var response = await _client.GetAsync($"api/user/{sub}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var expectedResult = new NoSuchUserException(sub).Message;
            var actualResult = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResult, actualResult);
        }
        [Fact]
        public async Task GetUserById_ValidId_Returns_200()
        {
            string sub = "446753a7-f493-4f60-9a64-85b5d4f994d7";
            var response = CreateUser(_client, sub);
            Assert.True(response.IsSuccessStatusCode);
            response = await _client.GetAsync($"api/user/{sub}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var user = await GetDTO<UserEntity>(response);
            Assert.Equal(sub, user.sub);
        }
        [Fact]
        public async Task Handshake_NewUser_Returns_200()
        {
            string sub = "446753a7-f493-4f60-9a64-85b5d4f994d7";
            var response = await _client.PostAsJsonAsync("api/user/handshake", sub);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var expectedResponse = new HandshakeDTO
            {
                sub = sub,
                currentSession = new SessionDTO
                {
                    id = 1,
                    user_id = sub,
                    Messages = new List<ChatMessageDTO>()
                },
                sessionIds = new List<int> { 1 }
            };
            var actualResponse = await GetDTO<HandshakeDTO>(response);
            Assert.Equal(expectedResponse.sub, actualResponse.sub);
            Assert.Equal(expectedResponse.currentSession.id, actualResponse.currentSession.id);
            Assert.Equal(expectedResponse.sessionIds, actualResponse.sessionIds);
        }
        [Fact]
        public async Task Handshake_ExistingUser_Returns_200()
        {
            string sub = "446753a7-f493-4f60-9a64-85b5d4f994d7";
            var response = await _client.PostAsJsonAsync("api/user/handshake", sub);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            response = await _client.PostAsync($"sessions/create?projectId={1}", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
             response = await _client.PostAsJsonAsync("api/user/handshake", sub);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var expectedResponse = new HandshakeDTO
            {
                sub = sub,
                currentSession = new SessionDTO
                {
                    id = 1,
                    user_id = sub,
                    Messages = new List<ChatMessageDTO>()
                },
                sessionIds = new List<int> { 1,2 }
            };
            var actualResponse = await GetDTO<HandshakeDTO>(response);
            Assert.Equal(expectedResponse.sub, actualResponse.sub);
            Assert.Equal(expectedResponse.currentSession.id, actualResponse.currentSession.id);
            Assert.Equal(expectedResponse.sessionIds, actualResponse.sessionIds);
        }

        public static HttpResponseMessage CreateUser(HttpClient client, string userId)
        {
            var content = new StringContent($"\"{userId}\"", Encoding.UTF8, "application/json");
            return client.PostAsync($"api/user/handshake", content).Result;
        }
    }
}
