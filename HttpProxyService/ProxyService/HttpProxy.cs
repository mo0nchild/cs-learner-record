using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics;
using System.Net;
using TransferLibrary.Export;
using TransferLibrary.NetworkTransfer;

namespace HttpProxyService.ProxyService
{
    using MessageJson = IRabbitTransfer.MessageJson;

    public sealed class HttpProxyException : System.Exception
    {
        public System.Guid ExceptionID { get; private set; } = Guid.NewGuid();
        public System.DateTime ExceptionTime { get; private set; } = DateTime.Now;

        public HttpProxyException(System.String message) : base(message) { }
    }
    public sealed class HttpProxyProperties : Object
    {
        public System.String InputPath { get; set; } = default!;
        public System.String OutputPath { get; set; } = default!;
    }
    public static class HttpProxyExtension : Object
    {
        public static IServiceCollection AddHttpProxy(this IServiceCollection collection, string input, string output)
        {
            return collection.AddTransient<IHostedService>(dispatcher =>
            {
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<HttpProxy>();
                var optionsService = dispatcher.GetRequiredService<IConfiguration>();

                return new HttpProxy(loggerFactory, dispatcher.GetRequiredService<TransferServiceProvider>(),
                    dispatcher.GetRequiredService<IDbContextFactory<DatabaseContext>>())
                {
                    TransferProperties = new HttpProxyProperties() {  InputPath = optionsService[input]!, OutputPath = optionsService[output]!, }
                };
            });
        }
    }
    public class HttpProxy : BackgroundService
    {
        private record MessageBroker(MessageJson message_body, string response_path, string request_type);
        private System.Threading.Timer RefreshTimer { get; set; } = default!;

        protected TransferServiceProvider TransferProvider { get; private set; } = default!;
        protected IDbContextFactory<DatabaseContext> DatabaseFactory { get; private set; } = default!;
        public HttpProxyProperties TransferProperties { get; set; } = default!;

        private System.Int32 HandlerTimer = default, HttpTimeout = default;
        protected ILogger<ProxyService.HttpProxy> Logger { get; private set; } = default!;

        public HttpProxy(ILogger<ProxyService.HttpProxy> logger, TransferServiceProvider transferProvider,
            IDbContextFactory<DatabaseContext> factory) : this(logger)
        {
            this.TransferProvider = transferProvider; this.DatabaseFactory = factory;
            try { 
                this.HandlerTimer = int.Parse(Environment.GetEnvironmentVariable("HANDLER_TIMER")!); 
                this.HttpTimeout = int.Parse(Environment.GetEnvironmentVariable("HTTP_TIMEOUT")!);
            }
            catch (System.Exception) { this.HandlerTimer = 10_000; this.HttpTimeout = 5000; }

            this.Logger.LogInformation($"HANDLER_TIMER: {this.HandlerTimer}");
        }
        protected HttpProxy(ILogger<ProxyService.HttpProxy> logger) : base() { this.Logger = logger; }

        private HttpProxy.MessageBroker MessageBrokerListen(string in_queue, CancellationToken cancel)
        {
            var result_message = this.TransferProvider.RabbitTransfer.GetMessageQueue(in_queue, cancel).Result;
            this.RefreshTimer.Change(Timeout.Infinite, Timeout.Infinite);

            if (result_message == null || result_message.JsonRecord.Count < 2)
            { 
                throw new System.Exception($"WARNING: Empty message read"); 
            }
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
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (this.TransferProperties.InputPath == null || this.TransferProperties.OutputPath == null)
            {
                throw new HttpProxyException("ERROR: ENV-VAR NOT SET");
            }
            ProxyService.HttpProxy.MessageBroker result_message = default!;
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var timer_sourse = new CancellationTokenSource();
                    using (RefreshTimer = new Timer(_ => timer_sourse.Cancel(), null, HandlerTimer, Timeout.Infinite))
                    {
                        try { result_message = this.MessageBrokerListen(this.TransferProperties.InputPath, timer_sourse.Token); }
                        catch (System.Exception error) { this.Logger.LogWarning(error.Message); continue; }
                    }
                    using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
                    {
                        var methodId = await dbcontext.MethodInfos.FirstOrDefaultAsync(item => item.RequestPath == result_message.request_type);
                        var collisionCount = await dbcontext.AccessLogs.Where(item => item.LogName == result_message.response_path).CountAsync();

                        if (methodId != null && collisionCount == 0) await dbcontext.AccessLogs.AddAsync(new Models.AccessLog()
                        {
                            LogName = result_message.response_path, AccessTime = DateTime.UtcNow,
                            MethodInfoId = methodId.MethodInfoId, LogData = result_message.message_body.ToString(),
                        });
                        await dbcontext.SaveChangesAsync();
                    }
                    IRabbitTransfer.MessageJson http_result = new() { JsonRecord = new() { } };
                    try
                    {
                        http_result = this.TransferProvider.HttpTransfer.SendRequest<IRabbitTransfer.MessageJson>(
                            result_message.message_body.JsonRecord[0], result_message.request_type, HttpTimeout);

                        http_result.JsonRecord.Add(new() { ["responce_time"] = DateTime.Now });

                        if (http_result.JsonRecord == null || http_result.JsonRecord.Count == 0)
                        { throw new IHttpTransfer.HttpTransferException("ERROR: SERVER PROCESS ERROR"); }
                    }
                    catch (IHttpTransfer.HttpTransferException error)
                    {
                        http_result.JsonRecord.Add(new() { ["error"] = $"[{error.ExceptionTime}]: {error.Message}" });
                        this.Logger.LogWarning(error.Message);
                    }
                    catch (System.Exception error) { this.Logger.LogError(error.Message); }

                    this.TransferProvider.RabbitTransfer.SendMessage(http_result, this.TransferProperties.OutputPath, result_message.response_path!);
                    this.Logger.LogInformation($"RESPONSE_PATH: {result_message.response_path}");
                }
            });
        }
    }
}