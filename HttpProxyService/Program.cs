using System;
using System.Collections.Generic;
using HttpProxyService.ProxyService;
using TransferLibrary.Export;

internal sealed class Program : System.Object
{
    public static void Main(string[] args) => Program.ConfigureHost().Run();
    public static IHost ConfigureHost()
    {
        ExportTransfer.RabbitHostname = Environment.GetEnvironmentVariable("RABBITMQ_HOST")!;
        ExportTransfer.HttpHostname = Environment.GetEnvironmentVariable("1CHTTP_HOST")!;

        IHostBuilder host_builder = Host.CreateDefaultBuilder()
            .ConfigureServices((IServiceCollection services_collection) =>
           {
               services_collection.AddNetworkTransfer().AddHostedService<HttpProxy>();
           });
        return host_builder.Build();
    }
}


