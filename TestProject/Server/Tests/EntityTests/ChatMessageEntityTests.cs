/*
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject.Server.Injection;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;
using Xunit.Sdk;

namespace TestProject.Server.Tests.EntityTests
{
    public class ChatMessageEntityTests : ComponentPoolManager
    {
        private DatabaseContext _context;
        private UserEntity user;
        private SessionEntity session;
        public ChatMessageEntityTests() : base()
        {
            _context = _componentPool.DatabaseContext;
            user = UserEntityTests.CreateUser(_context, "user");
            session = SessionEntityTests.CreateSession(_context, DateTime.UtcNow, user);
        }
        [Fact]
        public void CanCreate()
        {
            var msg = "msgcontent";
            var getEntity = () => _context.ChatMessages.FirstOrDefault(e => e.text== msg);
            Assert.Null(getEntity());
            CreateChatMessage(_context, session, msg);
            Assert.NotNull(getEntity());
        }
        public static ChatMessageEntity CreateChatMessage(DatabaseContext _context, SessionEntity session, string msgContent)
        {
            var msg = new ChatMessageEntity
            {
                sender = "TestProject",
                text = msgContent,
                timestamp = DateTime.UtcNow,
                session = session
            };
            _context.ChatMessages.Add(msg);
            _context.SaveChanges();
            return msg;
        }

        [Fact]
        public void CanRead()
        {
            string msg = "msgcontent";
            CreateChatMessage(_context, session, msg);
            var getEntity = () => _context.ChatMessages.FirstOrDefault(e => e.text== msg);
            Assert.NotNull(getEntity());
        }

        [Fact]
        public void CanUpdate()
        {
            string msg = "msgcontent";
            string newmsg = "newmsgcontent";
            CreateChatMessage(_context, session, msg);

            Update(msg, newmsg);
            var retrieved = _context.ChatMessages.FirstOrDefault(e => e.text == newmsg);
            Assert.NotNull(retrieved);
        }
        private void Update(string msg, string newmsg)
        {
            var entity = _context.ChatMessages.FirstOrDefault(e => e.text == msg);
            Assert.NotNull(entity);
            entity.text = newmsg;
            _context.SaveChanges();
        }

        [Fact]
        public void CanDelete()
        {
            string msg = "msgcontent";
            CreateChatMessage(_context, session, msg);
            Delete(msg);
            var retrieved = _context.ChatMessages.FirstOrDefault(e => e.text == msg);
            Assert.Null(retrieved);
        }


        private void Delete(string msg)
        {
            var entity = _context.ChatMessages.FirstOrDefault(e => e.text == msg);
            Assert.NotNull(entity);
            _context.ChatMessages.Remove(entity);
            _context.SaveChanges();
        }
    }
}
*/