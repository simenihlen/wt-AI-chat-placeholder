/*
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestProject.Server.Injection;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;
using Xunit.Sdk;

namespace TestProject.Server.Tests.EntityTests
{
    public class ChatMessageEmbeddingEntityTests : ComponentPoolManager
    {
        private DatabaseContext _context;
        public ChatMessageEmbeddingEntityTests() : base()
        {
            _context = _componentPool.DatabaseContext;
        }
        
        [Fact]
        public void CanCreate()
        {
            var username = "CanCreate";
            var msgcontent = "msg";
            var embeddingData = new double[] { 1.0, 2.0, 3.0 };
            var getEntity = () => _context.ChatMessageEmbeddings.FirstOrDefault(e => e.Embedding == embeddingData);
            Assert.Null(getEntity());
            var user = UserEntityTests.CreateUser(_context, username);
            var session = SessionEntityTests.CreateSession(_context, DateTime.UtcNow, user);
            var ChatMessageEntity = ChatMessageEntityTests.CreateChatMessage(_context, session, msgcontent);
            CreateChatMessageEmbedding(_context,ChatMessageEntity, embeddingData);
            Assert.NotNull(getEntity());
            Assert.Equal(getEntity().Embedding, embeddingData);
        }
        [Fact]
        public void CanRead()
        {
            var username = "CanRead";
            var msgcontent = "msg";
            var embeddingData = new double[] { 1.0, 2.0, 3.0 };
            var user = UserEntityTests.CreateUser(_context, username);
            var session = SessionEntityTests.CreateSession(_context, DateTime.UtcNow, user);
            var getEntity = () => _context.ChatMessageEmbeddings.FirstOrDefault(e => e.Embedding == embeddingData);
            Assert.Null(getEntity());
            var ChatMessageEntity = ChatMessageEntityTests.CreateChatMessage(_context, session, msgcontent);            
            CreateChatMessageEmbedding(_context, ChatMessageEntity, embeddingData);
            Assert.NotNull(getEntity());
            Assert.Equal(getEntity().Embedding, embeddingData);
        }
        [Fact]
        public void CanUpdate()
        {
            var username = "CanUpdate";
            var msgcontent = "msg";
            var embeddingData = new double[] { 1.0, 2.0, 3.0 };
            var user = UserEntityTests.CreateUser(_context, username);
            var session = SessionEntityTests.CreateSession(_context, DateTime.UtcNow, user);
            var ChatMessageEntity = ChatMessageEntityTests.CreateChatMessage(_context, session, msgcontent);
            var newEmbeddingData = new double[] { 4.0, 5.0, 6.0 };
            Func<double[], ChatMessageEmbeddingEntity?> getEntity = (embeddingData) =>
                _context.ChatMessageEmbeddings.FirstOrDefault(e => e.Embedding.SequenceEqual(embeddingData));
            var embedding = CreateChatMessageEmbedding(_context, ChatMessageEntity, embeddingData);
            Assert.NotNull(getEntity(embeddingData));
            Assert.Null(getEntity(newEmbeddingData));
            embedding.Embedding = newEmbeddingData;
            _context.SaveChanges();
            Assert.NotNull(getEntity(newEmbeddingData));
            Assert.Equal(getEntity(newEmbeddingData)?.Embedding, newEmbeddingData);
        }
        [Fact]
        public void CanDelete()
        {
            var username = "CanDelete";
            var msgcontent = "msg";
            var embeddingData = new double[] { 1.0, 2.0, 3.0 };
            var user = UserEntityTests.CreateUser(_context, username);
            var session = SessionEntityTests.CreateSession(_context, DateTime.UtcNow, user);
            var ChatMessageEntity = ChatMessageEntityTests.CreateChatMessage(_context, session, msgcontent);
            var getEntity = () => _context.ChatMessageEmbeddings.FirstOrDefault(e => e.Embedding == embeddingData);
            Assert.Null(getEntity());
            var embedding = CreateChatMessageEmbedding(_context, ChatMessageEntity, embeddingData);
            Assert.NotNull(getEntity());
            _context.ChatMessageEmbeddings.Remove(embedding);
            _context.SaveChanges();
            Assert.Null(getEntity());
        }

        public static ChatMessageEmbeddingEntity CreateChatMessageEmbedding(DatabaseContext _context, ChatMessageEntity msg, double[] embeddingdata)
        {
            var embedding = new ChatMessageEmbeddingEntity
            {
                ChatMessageId = msg.id,
                Embedding = embeddingdata
            };
            _context.ChatMessageEmbeddings.Add(embedding);
            _context.SaveChanges();
            return embedding;
        }
        /*
        [Fact]
        public void CanRead()
        {
            CreateChatMessageEmbedding(_context, "CanRead");
            var retrieved = _context.UserEntities.FirstOrDefault(e => e.username == "CanRead");
            Assert.NotNull(retrieved);
        }
        
        
        [Fact]
        public void CanUpdate()
        {
            CreateChatMessageEmbedding(_context, "CanCreate");
            Update("CanCreate", "Updated");
            var retrieved = _context.UserEntities.FirstOrDefault(e => e.username == "Updated");
            Assert.NotNull(retrieved);
        }
        
        [Fact]
        public void CanDelete()
        {
            CreateChatMessageEmbedding(_context, "CanDelete");
            Update("CanDelete", "StillCan");
            Delete("StillCan");
            var retrieved = _context.UserEntities.FirstOrDefault(e => e.username == "StillCan");
            Assert.Null(retrieved);
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
 */