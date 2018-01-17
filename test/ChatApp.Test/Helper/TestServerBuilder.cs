using System;
using System.IO;
using System.Reflection;
using ChatApp.Services;
using ChatApp.Test.Mock;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Test.Helper
{
    public class TestServerBuilder
    {
        protected IWebHostBuilder webHostBuilder;

        public TestServerBuilder()
        {
            webHostBuilder = new WebHostBuilder()
                .UseEnvironment("Test")
                .UseContentRoot(GetProjectPath("src", typeof(Startup).GetTypeInfo().Assembly))
                .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile(Path.GetFullPath("./appsettings.Test.json"), optional: false);
                });
        }

        public TestServer CreateTestServer()
        {
            webHostBuilder.ConfigureServices((service) =>
            {
                service.AddTransient<IEmailSender, EmailSenderMock>();
                service.AddSingleton<IDateTimeService, DateTimeServiceMock>();
            })
            .UseStartup<Startup>();
            return new TestServer(webHostBuilder);
        }

        private string GetProjectPath(string projectRelativePath, Assembly startupAssembly)
        {
            var projectName = startupAssembly.GetName().Name;
            var applicationBasePath = System.AppContext.BaseDirectory;
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                directoryInfo = directoryInfo.Parent;

                var projectDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, projectRelativePath));
                if (projectDirectoryInfo.Exists)
                {
                    var projectFileInfo = new FileInfo(Path.Combine(projectDirectoryInfo.FullName, projectName, $"{projectName}.csproj"));
                    if (projectFileInfo.Exists)
                    {
                        return Path.Combine(projectDirectoryInfo.FullName, projectName);
                    }
                }
            }
            while (directoryInfo.Parent != null);

            throw new Exception($"Project root could not be located using the application root {applicationBasePath}.");
        }
    }
}