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
                })
                .Build().Services;

            var rabbit_client = service_provider.GetService<IRabbitTransfer>()!;
            using var cancel_sourse = new CancellationTokenSource();
            try {
                var result = rabbit_client.GetEmployee(EmployeeRequestType.Attestation, "0000000001", cancel_sourse.Token);
                // var result = rabbit_client.SetMark(new ExportTransfer.MarkData(ExportTransfer.MarkType.Perfect, "1", "100234"), cancel_sourse.Token);

                //cancel_sourse.Cancel();
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
                Console.WriteLine("\n");

                Console.WriteLine($"{error.Message}");
            }
        }
    }
}