using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestProject.Server.Baseclasses;
using TestProject.Server.Injection;
using Xunit.Abstractions;

namespace TestProject.Server.Tests.ControllerTests
{
    public class StoryControllerTests : TestBase, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public StoryControllerTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper logger) : base(factory, logger)
        {
            _factory = factory;
        }
        [Fact]
        public void StoryControllertestsplaceholder()
        {
            Assert.Fail("Test not implemented");
        }
    }
}
