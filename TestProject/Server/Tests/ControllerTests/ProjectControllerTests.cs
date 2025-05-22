using System.Net;
using System.Net.Http.Json;
using System.Runtime.Intrinsics.Arm;
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
    public class ProjectControllerTests : TestBase, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProjectControllerTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper logger) : base(factory, logger)
        {
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task CreateProject_ValidUserId_Returns_200()
        {   
            string sub = "446753a7-f493-4f60-9a64-85b5d4f994d7";
            var response = UserControllerTests.CreateUser(_client, sub);
            ProjectDTO project = new ProjectDTO
            {
                title = "Test Project",
                sub = sub
            };
            var jsonContent = JsonContent.Create(project);
            response = await _client.PostAsync($"project/CreateProject",jsonContent);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task CreateProject_InvalidUserId_Returns_403()
        {
            string sub = "testuser";
            ProjectDTO project = new ProjectDTO
            {
                title = "Test Project",
                sub = sub
            };
            var jsonContent = JsonContent.Create(project);
            var response = await _client.PostAsync($"project/CreateProject", jsonContent);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var actualResponse = await response.Content.ReadAsStringAsync();
            var expectedResponse = new NoSuchUserException(sub);
        }
        [Fact]
        public async Task CreateProject_InvalidTitle_Returns_403()
        {
            string title = "";
            UserControllerTests.CreateUser(_client, title);
            ProjectDTO project = new ProjectDTO
            {
                title = title,
                sub = "testuser"
            };
            var jsonContent = JsonContent.Create(project);
            var response = await _client.PostAsync($"project/CreateProject", jsonContent);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var actualResponse = await response.Content.ReadAsStringAsync();
            var expectedResponse = new NoTitleException(title).Message;
            Assert.Equal(expectedResponse, actualResponse);
        }

        [Fact]
        public async Task GetProjects_ValidId_Returns_200()
        {
            string userId = "446753a7-f493-4f60-9a64-85b5d4f994d7";
            await CreateUser(userId);
            var response = await _client.GetAsync($"project/GetProjectsByUserId/{userId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var projects = await DeserializeResponse<List<ProjectDTO>>(response);
            Assert.NotNull(projects);
            Assert.True(projects.Count == 1);
        }
        [Fact]
        public async Task GetProjects_InValidId_Returns_403()
        {
            string userId = "446753a7-f493-4f60-9a64-85b5d4f994d7";
            var response = await _client.GetAsync($"project/GetProjectsByUserId/{userId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var expectedErrorMsg = new NoSuchUserException(userId).Message;
            var actualErrorMsg = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedErrorMsg, actualErrorMsg);
        }
        [Fact]
        public async Task UpdateProject_ValidId_returns_200()
        {
            string userId = "446753a7-f493-4f60-9a64-85b5d4f994d7";
            await CreateUser(userId);
            ProjectDTO project = new ProjectDTO
            {
                id = 1,
                sub = userId,
                title = "ASDLKJASDLKJAS",
            };
            var resp = await _client.PutAsJsonAsync("project/UpdateProject", project);
            var returnedDto = await DeserializeResponse<ProjectDTO>(resp);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.NotNull(returnedDto);
            Assert.Equal(project.title, returnedDto.title);
        }
        [Fact]
        public async Task UpdateProject_InValidId_returns_403()
        {
            var id = 2;
            await CreateUser("user");
            ProjectDTO project = new ProjectDTO
            {
                id = id,
                sub = "user",
                title = "ASDLKJASDLKJAS",
            };
            var response = await _client.PutAsJsonAsync("project/UpdateProject", project);
            Assert.True(response.StatusCode == HttpStatusCode.NotFound);

            var actualErrorMsg = await response.Content.ReadAsStringAsync();
            var expectedErrorMsg = new NoSuchProjectException(id).Message;
            Assert.True(actualErrorMsg == expectedErrorMsg);
        }
        [Fact]
        public async Task UpdateProject_InValidTitle_returns_500()
        {
            await CreateUser("user");
            var newtitle = "";
            ProjectDTO project = new ProjectDTO
            {
                id = 1,
                sub = "user",
                title = newtitle,
            };
            var response = await _client.PutAsJsonAsync("project/UpdateProject", project);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var actualErrorMsg = await response.Content.ReadAsStringAsync();
            var expectedErrorMsg = new NoTitleException(newtitle).Message;
            Assert.Equal(expectedErrorMsg, actualErrorMsg);
        }
        [Fact]
        public async Task DeleteProject_ValidId_returns_200()
        {
            string userId = "446753a7-f493-4f60-9a64-85b5d4f994d7";
            await CreateUser(userId);
            ProjectDTO project = new ProjectDTO
            {
                sub = userId,
                title = "newproject",
            };
            var saveResponse = await _client.PostAsJsonAsync("project/CreateProject", project);
            Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);
            var savedProject = await DeserializeResponse<ProjectDTO>(saveResponse);
            Assert.NotNull(savedProject);
            var resp = await _client.DeleteAsync($"project/DeleteProject/{savedProject.id}");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }
        [Fact]
        public async Task DeleteProject_InValidId_returns_403()
        {
            string userId = "446753a7-f493-4f60-9a64-85b5d4f994d7";
            await CreateUser(userId);
            var invalidProjectId = 22;

            var response = await _client.DeleteAsync($"project/DeleteProject/{invalidProjectId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var actualErrorMsg = await response.Content.ReadAsStringAsync();
            var expectedErrorMsg = new NoSuchProjectException(invalidProjectId).Message;
            Assert.Equal(expectedErrorMsg, actualErrorMsg);
        }

        private async Task CreateUser(string userId)
        {
            var content = new StringContent($"\"{userId}\"", Encoding.UTF8, "application/json");
            await _client.PostAsync($"api/User/handshake", content);
        }
        private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var responseStream = await response.Content.ReadAsStreamAsync();
            return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(responseStream);
        }
    }
}