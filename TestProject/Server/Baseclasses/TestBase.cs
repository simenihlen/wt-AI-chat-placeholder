using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestProject.Server.Injection;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;
using Testrepository.Server.Utils;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TestProject.Server.Baseclasses
{
    public class TestBase : IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly IServiceProvider _serviceProvider;
        private ITestOutputHelper _logger;

        public TestBase(CustomWebApplicationFactory<Program> factory, ITestOutputHelper logger)
        {
            this._logger = logger;
            factory.TestClassName = GetType().Name;
            _scope = factory.Services.CreateScope();
            _serviceProvider = _scope.ServiceProvider;
            GetService<DatabaseContext>().Database.EnsureCreated();
        }
        public void WriteLine(string message)
        {
            _logger.WriteLine(message);
        }

        public T GetService<T>()
        {
            var service = (T?)_serviceProvider.GetService(typeof(T));

            if (service == null)
            {
                throw new InvalidOperationException($"Service of type {typeof(T)} not registered.");
            }

            return service;
        }
        public void Dispose()
        {
            GetService<DatabaseContext>().Database.EnsureDeleted();
            _scope.Dispose();
        }

        public static UserEntity InstantiateUser(string username)
        {
            return new UserEntity
            {
                username = username,
            };
        }
        protected async Task<T> GetDTO<T>(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new JsonException("Deserialization returned null.");
        }
    }
}
