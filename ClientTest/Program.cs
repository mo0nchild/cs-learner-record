using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TransferLibrary.Export;
using TransferLibrary.NetworkTransfer;

namespace ClientTest
{
    public class Program : System.Object
    {
        public static void Main(string[] args)
        {
            IServiceProvider service_provider = Host.CreateDefaultBuilder(args)
                .ConfigureServices((IServiceCollection service_collection) =>
                {
                    service_collection.AddLogging().AddHttpClient().AddNetworkTransfer();
                }).Build().Services;

            var rabbit_client = service_provider.GetService<IRabbitTransfer>()!;

            var message = new Dictionary<string, object>()
            {
                { "request_type", "authorization" },
                { "�����", "uraniaa" },
                { "������", "12345asdasdasd" }
            };
            using var cancel_sourse = new CancellationTokenSource();
            try {
                var result = rabbit_client.SendMessage("InputExchange", "OutputExchange", message,
                    cancel_sourse.Token);

                cancel_sourse.Cancel();
                if (result.Result == null) { Console.WriteLine("\n\tCANCELED\n"); return; }

                Console.WriteLine("\n");
                foreach (var item in result.Result.JsonRecord)
                {
                    foreach (var record in item) Console.WriteLine($"{record.Key}: {record.Value}");
                    Console.WriteLine("\n");
                }
            }
            catch(AggregateException error) when (error.InnerException is TransferException)
            {
                Console.WriteLine($"{error.Message}");
            }
        }
    }
}