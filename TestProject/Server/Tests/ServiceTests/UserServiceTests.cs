using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;
using Microsoft.EntityFrameworkCore;
using TestProject.Server.Injection;

using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Testrepository.Server.Services.UserService.Implementations;
using TestProject.Server.Baseclasses;
using Xunit.Abstractions;

namespace TestProject.Server.Tests.ServiceTests
{
    public class UserServiceTests : TestBase, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly UserServiceImpl userService;
        private UserEntity User;

        public UserServiceTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper logger) : base(factory, logger)
        {
            userService = GetService<UserServiceImpl>();
            User = InstantiateUser(GetType().Name);
        }

        [Fact]
        public async Task Handshake_new_user()
        {
            var sub = "sub";
            var response = await userService.Handshake(sub);
            Assert.True(response.sessionIds[0] == 1);
            Assert.True(response.currentSession.id == 1);
            Assert.True(response.sub == sub);
        }
        [Fact]
        public async Task Handshake_existing_user()
        {
            var sub = "sub";
            var response = await userService.Handshake(sub);
            var response2 = await userService.Handshake(sub);
            Assert.True(response2.sessionIds[0] == 1);
            Assert.True(response2.currentSession.id == 1);
            Assert.True(response2.sub == sub);
        }
        [Fact]
        public async Task GetUserBySubAsync_nonexisting_user_returs_null()
        {
            var sub = "sub";
            var response = await userService.GetUserBySubAsync(sub);
            Assert.True(response == null);
        }
        [Fact]
        public async Task GetUserBySubAsync_existing_user_returs_null()
        {
            //add a user by name of sub
            var sub = "sub";
            var response = await userService.GetUserBySubAsync(sub);
            Assert.True(response!.sub == sub);
        }

    }
}
