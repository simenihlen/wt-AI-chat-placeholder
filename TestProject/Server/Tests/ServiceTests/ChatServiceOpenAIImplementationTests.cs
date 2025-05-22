
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;
using Microsoft.EntityFrameworkCore;
using TestProject.Server.Injection;
using Testrepository.Server.Services;

using System.Reflection;
using TestProject.Server.Baseclasses;
using Testrepository.Server.Services.ChatService;
using Xunit.Abstractions;

namespace TestProject.Server.Tests.ServiceTests
{
    public class ChatServiceOpenAIImplementationTests : TestBase, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly OpenAIChatServiceImpl chatMessageService;

        public ChatServiceOpenAIImplementationTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper logger) : base(factory, logger)
        {
            chatMessageService = GetService<OpenAIChatServiceImpl>();
        }
        [Fact]
        public async Task GetResponseAsync_WhenCalled_ReturnsResponse()
        {
            var embedding = await chatMessageService.GetEmbeddingForPrompt("string hello");
        }
    }
}
