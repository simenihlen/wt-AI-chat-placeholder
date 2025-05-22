using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject.Server.Baseclasses;
using TestProject.Server.Injection;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TestProject.Server.Tests.EntityTests
{
    public class TestBaseImplementation2 : TestBase, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        DatabaseContext _context;

        public TestBaseImplementation2(CustomWebApplicationFactory<Program> factory, ITestOutputHelper logger) : base(factory, logger)
        {
            _context = GetService<DatabaseContext>();
        }
        [Fact]
        public void CanCreate()
        {
            var username = "CanCreate";
            var getEntity = () => _context.UserEntities.FirstOrDefault(e => e.username == username);
            Assert.Null(getEntity());
            CreateUser(_context, username);
            Assert.NotNull(getEntity());
        }



        public static UserEntity CreateUser(DatabaseContext _context, string username)
        {
            var user = new UserEntity
            {
                sub = "sub",
                username = username
            };
            _context.UserEntities.Add(user);
            _context.SaveChanges();
            return user;
        }
    }
}
