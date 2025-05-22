using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
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
    public class SessionControllerTests : TestBase, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public SessionControllerTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper logger) : base(factory, logger)
        {
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task Test_CreateSession_Nonexisting_Project_returns_404()
        {
            int projectId = 3;
            var response = await _client.PostAsync($"sessions/create?projectId={projectId}", null);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var expectedErrormessage = new NoSuchProjectException(projectId).Message;
            var actualResponsemessage = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrormessage, actualResponsemessage);
        }

        [Fact]
        public async Task Test_CreateSession_Existing_Project_returns_200()
        {
            //TODO: Gotta fix handdshake to account for project.
            //Handshake needs to return sub, currentproject, projectids. currentproject has a currentsession and sessionid's
            string userId = "446753a7-f493-4f60-9a64-85b5d4f994d7";
            UserControllerTests.CreateUser(_client, userId);
            int projectId = 1;
            var response = await _client.PostAsync($"sessions/create?projectId={projectId}", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var expectedResponse = new SessionEntity 
            {
                session_id = 2,
                user_id = userId,
            };
            var actualResponse = await GetDTO<SessionEntity>(response);
            Assert.NotNull(actualResponse);
            Assert.Equal(expectedResponse.session_id, actualResponse.session_id);
        }
        [Fact]
        public async Task Test_GetSessionById_Nonexisting_Session_returns_404()
        {
            int sessionId = 15;
            var response = await _client.GetAsync($"sessions/{sessionId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var expectedErrormessage = new NoSuchSessionException(sessionId).Message;
            var actualResponsemessage = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrormessage, actualResponsemessage);
        }
        [Fact]
        public async Task Test_GetSessionById_Existing_Session_returns_200()
        {
            var userId = "user";
            UserControllerTests.CreateUser(_client, userId);
            var response = await _client.GetAsync($"sessions/{1}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var expectedResponse = new SessionDTO
            {
                id = 1,
                user_id = userId,
            };
            var actualResponse = await GetDTO<SessionDTO>(response);
            Assert.NotNull(actualResponse);
            Assert.Equal(expectedResponse.user_id, actualResponse.user_id);
        }
        [Fact]
        public async Task Test_GetSessionsByUserId_Nonexisting_User_returns_404()
        {
            string userId = "testuser";
            var response = await _client.GetAsync($"sessions/user/{userId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var expectedResponse = new NoSuchUserException(userId).Message;
            var actualResponse = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, actualResponse);
        }
        [Fact]
        public async Task Test_GetSessionsByUserId_Existing_User_returns_200()
        {
            string userId = "testuser";
            UserControllerTests.CreateUser(_client, userId);
            var response = await _client.GetAsync($"sessions/user/{userId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var expectedResponse = new List<SessionDTO>
            {
                new SessionDTO
                {
                    id = 1,
                    user_id = userId,
                }
            };
            var actualResponse = await GetDTO<List<SessionDTO>>(response);
            Assert.NotNull(actualResponse);
            Assert.True(actualResponse.Count == 1);
            Assert.Equal(expectedResponse[0].user_id, actualResponse[0].user_id);
            Assert.Equal(expectedResponse[0].id, actualResponse[0].id);
        }

        [Fact]
        public async Task Test_UpdateSession_Nonexisting_Session_returns_404()
        {
            int sessionId = 27;
            var sub = "asdf";
            UserControllerTests.CreateUser(_client, sub);

            var msgs = new List<ChatMessageDTO>
    {
        new ChatMessageDTO(new ChatMessageEntity
        {
            user_id = sub,
            bot = false,
            session_id = sessionId,
            text = "userMessage",
            timestamp = DateTime.UtcNow
        }),
        new ChatMessageDTO(new ChatMessageEntity
        {
            user_id = null,
            bot = true,
            session_id = sessionId,
            text = "botMessage",
            timestamp = DateTime.UtcNow
        })
    };

            var content = JsonContent.Create(msgs); // More elegant than manually serializing
            var response = await _client.PutAsync($"sessions/{sessionId}?userId={sub}", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var expectedResponse = new NoSuchSessionException(sessionId).Message;
            var actualResponse = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, actualResponse);
        }
        [Fact]
        public async Task Test_UpdateSession_Existing_Session_returns_200()
        {
            int sessionId = 1;
            var sub = "asdf";
            UserControllerTests.CreateUser(_client, sub);

            var msgs = new List<ChatMessageDTO>
    {
        new ChatMessageDTO(new ChatMessageEntity
        {
            user_id = sub,
            bot = false,
            session_id = sessionId,
            text = "userMessage",
            timestamp = DateTime.UtcNow
        }),
        new ChatMessageDTO(new ChatMessageEntity
        {
            user_id = null,
            bot = true,
            session_id = sessionId,
            text = "botMessage",
            timestamp = DateTime.UtcNow
        })
    };

            var content = JsonContent.Create(msgs); // More elegant than manually serializing
            var response = await _client.PutAsync($"sessions/{sessionId}?userId={sub}", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var expectedResponse = new List<ChatMessageDTO>
            {
                new ChatMessageDTO(new ChatMessageEntity
                {
                    user_id = sub,
                    bot = false,
                    session_id = sessionId,
                    text = "userMessage",
                    timestamp = DateTime.UtcNow
                }),
                new ChatMessageDTO(new ChatMessageEntity
                {
                    user_id = null,
                    bot = true,
                    session_id = sessionId,
                    text = "botMessage",
                    timestamp = DateTime.UtcNow
                })
            };
            var actualResponse = await GetDTO<List<ChatMessageDTO>>(response);
            for (int i = 0; i < expectedResponse.Count; i++)
            {
                Assert.Equal(expectedResponse[i].text, actualResponse[i].text);
                Assert.Equal(expectedResponse[i].session_id, actualResponse[i].session_id);
                Assert.Equal(expectedResponse[i].sender, actualResponse[i].sender);
            }
        }
        [Fact]
        public async Task Test_RemoveSessionAsync_Nonexisting_Session_returns_404()
        {
            int sessionId = 15;
            string sub = "user";
            UserControllerTests.CreateUser(_client, sub);
            var response = await _client.DeleteAsync($"sessions/delete/{sessionId}/{sub}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var expectedResponse = new NoSuchSessionException(sessionId).Message;
            var actualResponse = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, actualResponse);
        }
        [Fact]
        public async Task Test_RemoveSessionAsync_Nonexisting_User_returns_404()
        {
            int sessionId = 15;
            string userId = "user";
            var response = await _client.DeleteAsync($"sessions/delete/{sessionId}/{userId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var expectedResponse = new NoSuchUserException(userId).Message;
            var actualResponse = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, actualResponse);
        }
        [Fact]
        public async Task Test_RemoveSessionAsync_Existing_User_Session_returns_200()
        {
            int sessionId = 1;
            string userId = "user";
            UserControllerTests.CreateUser(_client, userId);
            var response = await _client.DeleteAsync($"sessions/delete/{sessionId}/{userId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var expectedResponse = true;
            var actualResponse = await GetDTO<bool>(response);
            Assert.Equal(expectedResponse, actualResponse);
        }
    }
}
