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
using System.Configuration.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Console;

namespace TransferLibrary.Export
{
    using MessageJson = IRabbitTransfer.MessageJson;
    public sealed class TransferException : System.Exception
    {
        public System.DateTime ExceptionTime { get; private set; } = DateTime.Now;
        public TransferException(System.String message) : base(message) { }
    }

    public sealed class TransferServiceProvider : object
    {
        public IRabbitTransfer RabbitTransfer { get; set; } = default!;
        public IHttpTransfer HttpTransfer { get; set; } = default!;

        public TransferServiceProvider() : base() { }
    }

    public class RequestType : System.Object
    {
        public System.String RequestValue { get; protected set; } = default!;

        public System.String InputPath { get; protected set; } = default!;
        public System.String OutputPath { get; protected set; } = default!;

        public RequestType(string state, string input, string output) : base()
        {
            this.RequestValue = state; this.InputPath = input; this.OutputPath = output;
        }
    }

    public sealed class StudentRequestType : RequestType
    {
        public StudentRequestType(string request, string input, string output) : base(request, input, output) { }

        public readonly static StudentRequestType Profile = new("student_info/profile", "InputExchange", "OutputExchange");

        public readonly static StudentRequestType Orders = new("student_info/orders", "InputExchange", "OutputExchange");

        public readonly static StudentRequestType Statements = new("student_info/statements", "InputExchange", "OutputExchange");
    }

    public sealed class EmployeeRequestType : RequestType
    {
        public EmployeeRequestType(string request, string input, string output) : base(request, input, output) { }

        public readonly static EmployeeRequestType Profile = new("employee_info/profile", "EmployeeInputExchange", "EmployeeOutputExchange");

        public readonly static EmployeeRequestType Attestation = new("employee_info/attestation", "EmployeeInputExchange", "EmployeeOutputExchange");
    }

    public static class ExportTransfer : System.Object
    {
        private const System.String InputExchange = "InputExchange", OutputExchange = "OutputExchange";

        public static System.Guid ExportManagerId { get; private set; } = default;
        static ExportTransfer() { ExportTransfer.ExportManagerId = Guid.NewGuid(); }

        public static IServiceCollection AddNetworkTransfer(this IServiceCollection services_collection)
        {
            return services_collection.AddHttpClient()
                .AddTransient<NetworkTransfer.IRabbitTransfer, NetworkTransfer.RabbitTransfer>((dispatcher) =>
                {
                    var configuration = dispatcher.GetRequiredService<IConfiguration>();

                    var required_logger = dispatcher.GetRequiredService<ILoggerFactory>()
                        .CreateLogger<NetworkTransfer.RabbitTransfer>();
                    return new NetworkTransfer.RabbitTransfer(required_logger, configuration["RABBITMQ_HOST"]!);
                })
                .AddTransient<NetworkTransfer.IHttpTransfer, NetworkTransfer.HttpTransfer>((dispatcher) =>
                {
                    var required_service = dispatcher.GetRequiredService<IHttpClientFactory>();
                    var configuration = dispatcher.GetRequiredService<IConfiguration>();
                    var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<HttpTransfer>();

                    return new NetworkTransfer.HttpTransfer(loggerFactory, required_service, configuration["HTTP_HOST"]!);
                })
                .AddTransient<TransferServiceProvider>(dispatcher => 
                {
                    return new TransferServiceProvider()
                    {
                        RabbitTransfer = dispatcher.GetRequiredService<IRabbitTransfer>(),
                        HttpTransfer = dispatcher.GetRequiredService<IHttpTransfer>(),
                    };
                });
        }
        public static Task<MessageJson?> SendMessage(this IRabbitTransfer rabbit_transfer, Dictionary<string, object> message, 
            CancellationToken token, string input_exchange, string output_exchange)
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
        public static Task<MessageJson?> GetStudent(this IRabbitTransfer rabbit_transfer, StudentRequestType type,
            string person_id, CancellationToken token)
        {
            return rabbit_transfer.SendMessage(new () { { "request_type", type.RequestValue }, { "Код", person_id } }, 
                token, type.InputPath, type.OutputPath);
        }

        public static Task<MessageJson?> GetEmployee(this IRabbitTransfer rabbit_transfer, EmployeeRequestType type,
            string person_id, CancellationToken token)
        {
            return rabbit_transfer.SendMessage(new() { { "request_type", type.RequestValue }, { "Код", person_id } },
                token, type.InputPath, type.OutputPath);
        }

        public sealed class MarkType : Object
        {
            public string MarkValue { get; private set; } = default!;
            public MarkType(string markValue) : base() { this.MarkValue = markValue; }

            public static MarkType NotBad { get; set; } = new("Удовлетворительно");
            public static MarkType Good { get; set; } = new("Хорошо");
            public static MarkType Perfect { get; set; } = new("Отлично");
        }

        public sealed record class MarkData(MarkType Type, string StatementNumber, string GradeBook);
        public static Task<MessageJson?> SetMark(this IRabbitTransfer rabbitTransfer, MarkData markData, CancellationToken token)
        {
            return rabbitTransfer.SendMessage(new() 
            { 
                { "request_type", "employee_info/setmark" }, { "НомерВедомости", markData.StatementNumber },
                { "ЗачетнаяКнига", markData.GradeBook }, { "Оценка", markData.Type.MarkValue }
            }, 
            token, "EmployeeInputExchange", "EmployeeOutputExchange");
        }
    }
}
