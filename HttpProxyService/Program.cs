using System;
using System.Collections.Generic;
using HttpProxyService;
using HttpProxyService.ProxyService;
using Microsoft.EntityFrameworkCore;
using TransferLibrary.Export;

internal sealed class Program : System.Object
{
    public static void Main(string[] args) => Program.ConfigureHost().Run();
    public static IHost ConfigureHost()
    {
        IHostBuilder host_builder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder => builder.AddEnvironmentVariables())
            .ConfigureServices((IServiceCollection services_collection) =>
            {
                var configurationService = services_collection.BuildServiceProvider()
                    .GetRequiredService<IConfiguration>();

                services_collection.AddNetworkTransfer().AddDbContextFactory<DatabaseContext>(builder =>
                {
                    builder.UseNpgsql(configurationService["DATABASE_CONNECTION"]!);
                })
                    .AddHttpProxy("STUDENT_INPUT_PATH", "STUDENT_OUTPUT_PATH")
                    .AddHttpProxy("EMPLOYEE_INPUT_PATH", "EMPLOYEE_OUTPUT_PATH");
            });
        return host_builder.Build();
    }
}


