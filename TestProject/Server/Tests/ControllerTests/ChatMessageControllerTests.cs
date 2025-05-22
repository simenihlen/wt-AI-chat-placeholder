using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TestProject.Server.Baseclasses;
using TestProject.Server.Injection;
using Testrepository.Server.Exceptions;
using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Requests;
using Testrepository.Server.Utils;
using Xunit.Abstractions;

namespace TestProject.Server.Tests.ControllerTests
{
    public class ChatMessageControllerTests : TestBase, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ChatMessageControllerTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper logger) : base(factory, logger)
        {
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task Throws_404_if_User_nonexistent()
        {
            var request = new ChatRequest
            {
                SessionId = 1,
                UserId = "noexist",  
                Prompt = "Hello, how are you?"
            };
            var response = await _client.PostAsJsonAsync("api/openai-chat/chat", request);
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound);

            var expectedResult = new NoSuchUserException(request.UserId).Message;
            var actualResult = await response.Content.ReadAsStringAsync();

            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(expectedResult, actualResult);
        }
        [Fact]
        public async Task Throws_404_if_sessionID_nonexistent()
        {
            var sub = "user";
            var response = UserControllerTests.CreateUser(_client, sub);
            var user = await GetDTO<HandshakeDTO>(response);

            var request = new ChatRequest
            {
                SessionId = 2,  
                UserId = sub,  
                Prompt = "Hello, how are you?"
            };

            response = await _client.PostAsJsonAsync("api/openai-chat/chat", request);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);

            var actualResult = await response.Content.ReadAsStringAsync();
            var expectedResult = new NoSuchSessionException(request.SessionId).Message;
            Assert.Equal(expectedResult, actualResult);
        }
        [Fact]
        public async Task Throws_400_if_Prompt_empty()
        {
            var sub = "user";
            var response = UserControllerTests.CreateUser(_client, sub);

            var user = await GetDTO<HandshakeDTO>(response);
            var prompt = "";
            var request = new ChatRequest
            {
                SessionId = 1,
                UserId = user.sub,
                Prompt = prompt
            };
            response = await _client.PostAsJsonAsync("api/openai-chat/chat", request);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

            var expectedErrorMsg = await response.Content.ReadAsStringAsync();
            var actualErrorMsg = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMsg, actualErrorMsg);
        }
        [Fact]
        public async Task Returns200_if_request_OK()
        {
            var response = UserControllerTests.CreateUser(_client, "user");
            response.EnsureSuccessStatusCode();
            var user = await GetDTO<HandshakeDTO>(response);
            var prompt = "this is a valid prompt";
            var request = new ChatRequest
            {
                SessionId = 1,
                UserId = "user",
                Prompt = prompt
            };
            response = await _client.PostAsJsonAsync("api/openai-chat/chat", request);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var stringResponse = await response.Content.ReadAsStringAsync();
            Assert.True(stringResponse.Length >0);
        }
        public static HttpResponseMessage CreateMessage(string prommpt)
        {
            return new HttpResponseMessage();
        }
    }
}

 
