using Grpc.Net.Client;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text;
using TransferLibrary.Export;
using TransferLibrary.NetworkTransfer;
using HttpProxyService.GrpsTransfer;
using System.Reflection;
using Grpc.Core;

namespace HttpProxyService.ProxyService
{
    using MessageJson = IRabbitTransfer.MessageJson;

    public sealed class HttpProxyException : System.Exception
    {
        public System.Guid ExceptionID { get; private set; } = Guid.NewGuid();
        public System.DateTime ExceptionTime { get; private set; } = DateTime.Now;

        public HttpProxyException(System.String message) : base(message) { }
    }

    public class HttpProxy : BackgroundService
    {
        private record MessageBroker(MessageJson message_body, string response_path, string request_type);
        private System.Threading.Timer RefreshTimer { get; set; } = default!;

        protected IRabbitTransfer RabbitTransfer { get; private set; } = default!;

        private System.Int32 HandlerTimer = default, HttpTimeout = default;
        protected ILogger<ProxyService.HttpProxy> Logger { get; private set; } = default!;

        public HttpProxy(ILogger<ProxyService.HttpProxy> logger/*, IRabbitTransfer rabbit_transfer*/) /*: this(logger)*/
        {
            //this.RabbitTransfer = rabbit_transfer;
            this.Logger = logger;
            try { 
                this.HandlerTimer = int.Parse(System.Environment.GetEnvironmentVariable("HANDLER_TIMER")!); 
                this.HttpTimeout = int.Parse(System.Environment.GetEnvironmentVariable("HTTP_TIMEOUT")!);
            }
            catch (System.Exception) { this.HandlerTimer = 10_000; this.HttpTimeout = 5000; }

            this.Logger.LogInformation($"HANDLER_TIMER: {this.HandlerTimer}");
        }
        //protected HttpProxy(ILogger<ProxyService.HttpProxy> logger) : base() { this.Logger = logger; }

        private HttpProxy.MessageBroker MessageBrokerListen(string in_queue, CancellationToken cancel)
        {
            var result_message = this.RabbitTransfer.GetMessageQueue(in_queue, cancel).Result;
            this.RefreshTimer.Change(Timeout.Infinite, Timeout.Infinite);

            if (result_message == null || result_message.JsonRecord.Count < 2)
            { throw new System.Exception($"WARNING: Empty message read"); }

            string? response_path = default, request_type = default;
            result_message.JsonRecord.ForEach(delegate(Dictionary<string, object> record)
            {
                if (record.ContainsKey("response_path")) response_path = (string)record["response_path"];
                if (record.ContainsKey("request_type")) request_type = (string)record["request_type"];
            });
            if (response_path == null || request_type == null)
            { throw new HttpProxyException($"ERROR: Message key [message_id] or [request_type]"); }

            this.Logger.LogInformation($"MESSAGE_TYPE: {request_type}");
            return new MessageBroker(result_message, response_path, request_type);
        }

        private async Task<MessageJson> SendGrpsRequest(string request_type, string request_code)
        {
            var request_result = new IRabbitTransfer.MessageJson() { JsonRecord = new() { new() } };
            using (var client_channel = GrpcChannel.ForAddress("http://localhost:5236"/*ExportTransfer.HttpHostname*/))
            {
                var client_instance = new FetchData.FetchDataClient(client_channel);
                var service_method = client_instance.GetType().GetMethod(request_type, new Type[] 
                { typeof(RequestData), typeof(Grpc.Core.Metadata), typeof(DateTime?), typeof(CancellationToken) });

                if (service_method == null) throw new IHttpTransfer.HttpTransferException("ERROR: NOT FOUND");
                dynamic service_call = service_method.Invoke(client_instance, new object?[] {
                    new RequestData() { StudentGradeBook = request_code }, null, null, default(CancellationToken)})!;

                var service_result = await service_call.ResponseAsync;
                var service_headers = (Grpc.Core.Metadata)(await service_call.ResponseHeadersAsync);

                foreach (var property in service_result.GetType().GetProperties(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    request_result.JsonRecord[0].Add(property.Name, property.GetValue(service_result) ?? "");
                }
                request_result.JsonRecord.Add( new() { ["responce_time"] = DateTime.Now });
            }
            if (request_result.JsonRecord == null || request_result.JsonRecord.Count == 0)
            { throw new IHttpTransfer.HttpTransferException("ERROR: SERVER PROCESS ERROR"); }

            return request_result;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var out_exchange = System.Environment.GetEnvironmentVariable("OUTPUT_PATH");
            var in_queue = System.Environment.GetEnvironmentVariable("INPUT_PATH");

            //ProxyService.HttpProxy.MessageBroker result_message = default!;

            //if (out_exchange == null || in_queue == null) throw new HttpProxyException("ERROR: ENV-VAR NOT SET");
            while (!stoppingToken.IsCancellationRequested)
            {
                //var timer_sourse = new CancellationTokenSource();
                //using (RefreshTimer = new Timer(_ => timer_sourse.Cancel(), null, HandlerTimer, Timeout.Infinite))
                //{
                //    try { result_message = this.MessageBrokerListen(in_queue, timer_sourse.Token); }
                //    catch (System.Exception error) { this.Logger.LogWarning(error.Message); continue; }
                //}
                IRabbitTransfer.MessageJson http_result = new() { JsonRecord = new() { } };
                var request_code = "418318"/*(string)result_message.message_body.JsonRecord[0]["Код"]*/;

                try { http_result = this.SendGrpsRequest("GetProfileDataAsync"/*result_message.request_type*/, request_code).Result; }
                catch (IHttpTransfer.HttpTransferException error)
                {
                    http_result.JsonRecord.Add(new() { ["error"] = $"[{error.ExceptionTime}]: {error.Message}" });
                    this.Logger.LogWarning(error.Message);
                }
                //catch (System.Exception error) { this.Logger.LogError(error.Message); }

                //this.RabbitTransfer.SendMessage(http_result, out_exchange, result_message.response_path!);
                //this.Logger.LogInformation($"RESPONSE_PATH: {result_message.response_path}");

                foreach(var item in http_result.JsonRecord)
                {
                    foreach (var i in item) Console.WriteLine(i);
                }
                this.Logger.LogInformation("DELEY");
                Task.Delay(1000).Wait();
            }
            return Task.CompletedTask;
        }
    }
}