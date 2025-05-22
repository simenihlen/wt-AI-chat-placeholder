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
    //public class UserEntityTests : ComponentPoolManager
    public class UserEntityTests : TestBase, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private DatabaseContext _context;

        public UserEntityTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper logger) : base(factory, logger)
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
        
        [Fact]
        public void CanRead()
        {
            CreateUser(_context, "CanRead");
            var retrieved = _context.UserEntities.FirstOrDefault(e => e.username == "CanRead");
            Assert.NotNull(retrieved);
        }
        
        
        [Fact]
        public void CanUpdate()
        {
            CreateUser(_context, "CanCreate");
            Update("CanCreate", "Updated");
            var retrieved = _context.UserEntities.FirstOrDefault(e => e.username == "Updated");
            Assert.NotNull(retrieved);
        }
        
        [Fact]
        public void CanDelete()
        {
            CreateUser(_context, "CanDelete");
            Update("CanDelete", "StillCan");
            Delete("StillCan");
            var retrieved = _context.UserEntities.FirstOrDefault(e => e.username == "StillCan");
            Assert.Null(retrieved);
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
        
        private void Update(string OldName, string NewName)
        {
            var entity = _context.UserEntities.FirstOrDefault(e => e.username == OldName);
            Assert.NotNull(entity);
            entity.username = NewName;
            _context.SaveChanges();
        }


        private void Delete(string v)
        {
            var entity = _context.UserEntities.FirstOrDefault(e => e.username == v);
            Assert.NotNull(entity);
            _context.UserEntities.Remove(entity);
            _context.SaveChanges();
        }
    }
}
