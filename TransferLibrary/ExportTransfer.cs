using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TransferLibrary.NetworkTransfer;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Net;

namespace TransferLibrary.Export
{
    using MessageJson = IRabbitTransfer.MessageJson;
    public sealed class TransferException : System.Exception
    {
        public System.DateTime ExceptionTime { get; private set; } = DateTime.Now;
        public TransferException(System.String message) : base(message) { }
    }

    public class RequestType : System.Object
    {
        public System.String RequestValue { get; private set; } = default!;
        public RequestType(string state) : base() => this.RequestValue = state;


        public readonly static RequestType Authorization = new("authorization");

        public readonly static RequestType Orders = new("orders");

        public readonly static RequestType Statements = new("statements");
    }

    public static class ExportTransfer : System.Object
    {
        private const System.String InputExchange = "InputExchange", OutputExchange = "OutputExchange";

        public static System.String HttpHostname { get; set; } = "http://localhost:8080";
        public static System.String RabbitHostname { get; set; } = "localhost";

        public static System.Guid ExportManagerId { get; private set; } = default;
        static ExportTransfer() { ExportTransfer.ExportManagerId = Guid.NewGuid(); }

        public static IServiceCollection AddNetworkTransfer(this IServiceCollection services_collection)
        {
            return services_collection.AddHttpClient()
                .AddTransient<NetworkTransfer.IRabbitTransfer, NetworkTransfer.RabbitTransfer>((dispatcher) =>
                {
                    var required_logger = dispatcher.GetRequiredService<ILoggerFactory>()
                        .CreateLogger<NetworkTransfer.RabbitTransfer>();
                    return new NetworkTransfer.RabbitTransfer(required_logger, ExportTransfer.RabbitHostname);
                })
                .AddTransient<NetworkTransfer.IHttpTransfer, NetworkTransfer.HttpTransfer>((dispatcher) =>
                {
                    var required_service = dispatcher.GetRequiredService<IHttpClientFactory>();
                    return new NetworkTransfer.HttpTransfer(required_service, ExportTransfer.HttpHostname);
                });
        }
        public static Task<MessageJson?> SendMessage(this IRabbitTransfer rabbit_transfer,
            Dictionary<string, object> message, CancellationToken token, string input_exchange = InputExchange,
            string output_exchange = OutputExchange)
        {
            if (message == null || message.ContainsKey("request_type") == false)
            { throw new Export.TransferException("ERROR: INPUT MESSAGE SET NOT CORRECTLY"); }

            message.Add("response_path", rabbit_transfer.RabbitID);

            var message_json = new IRabbitTransfer.MessageJson() { JsonRecord = new() { message } };
            IRabbitTransfer.MessageJson? request_result = default;

            return Task<IRabbitTransfer.MessageJson?>.Run(delegate () {
                while (!token.IsCancellationRequested)
                {
                    var transfer_result = rabbit_transfer.GetMessageExchange(output_exchange, token);
                    rabbit_transfer.SendMessage(message_json, input_exchange);

                    if (transfer_result.Result != null && transfer_result.Result?.JsonRecord.Count > 1)
                    { request_result = transfer_result.Result; break; }
                }
                if (request_result == null) return null;
                var error_check = request_result!.JsonRecord.Where((record) => record.ContainsKey("error"));

                if (error_check.Count() > 0)
                { throw new TransferException(error_check.ElementAt(0)["error"].ToString()!); }

                return request_result;
            });
        }
        public static Task<MessageJson?> SendMessage(this IRabbitTransfer rabbit_transfer, RequestType type,
            string person_id, CancellationToken token, string input_exchange = InputExchange,
            string output_exchange = OutputExchange)
        {
            return rabbit_transfer.SendMessage(new () { { "request_type", type.RequestValue }, { "Код", person_id } }, 
                token, input_exchange, output_exchange);
        }
    }
}
