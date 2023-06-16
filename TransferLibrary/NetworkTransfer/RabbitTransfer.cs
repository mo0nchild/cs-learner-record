using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Collections;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TransferLibrary.Export;
using System.Diagnostics;
using static TransferLibrary.NetworkTransfer.IRabbitTransfer;

namespace TransferLibrary.NetworkTransfer
{
    public interface IRabbitTransfer : System.IDisposable
    {
        public sealed class MessageJson : System.Object
        {
            [Newtonsoft.Json.JsonPropertyAttribute("Данные")]
            public List<Dictionary<string, object>> JsonRecord { get; set; } = new();

            public override string ToString()
            {
                var resultString = "Данные запроса: [ ";
                foreach(var item in this.JsonRecord)
                {
                    resultString += "{ ";
                    foreach (var record in item) resultString += $"{{ \"{record.Key}\" : \"{record.Value}\" }}, ";
                    resultString += " }, ";
                }
                return resultString + " ]";
            }
        }
        public void SendMessage(MessageJson message, string exchange, string key = "");
        public System.Guid RabbitID { get; }

        public Task<MessageJson> GetMessageExchange(string exchange, CancellationToken token);
        public Task<MessageJson> GetMessageQueue(string queue, CancellationToken token);
    }

    public class RabbitTransfer : System.Object, NetworkTransfer.IRabbitTransfer
    {
        // protected RabbitMQ.Client.ConnectionFactory RabbitFactory { get; private set; } = default!;
        public System.Int32 QueueExpires { get; set; } = 100_000;
        public System.Guid RabbitID { get; private set; } = System.Guid.NewGuid();

        protected RabbitMQ.Client.IConnection RabbitConnection { get; private set; } = default!;
        protected ILogger<RabbitTransfer> Logger { get; private set; } = default!;

        public RabbitTransfer(ILogger<RabbitTransfer> logger, System.String hostname) : this(logger)
        {
            this.RabbitConnection = (new ConnectionFactory() { HostName = hostname }).CreateConnection();
            this.Logger.LogInformation($"RabbitHostname: {hostname}");
        }
        protected RabbitTransfer(ILogger<RabbitTransfer> logger) : base() { this.Logger = logger; }

        void System.IDisposable.Dispose() { this.RabbitConnection.Dispose(); }

        public void SendMessage(IRabbitTransfer.MessageJson message, string exchange, string key = "")
        {
            //using IConnection connection = RabbitFactory;
            using (var channel = this.RabbitConnection.CreateModel())
            {
                message.JsonRecord.Add(new() { ["message_id"] = this.RabbitID.ToString() });
                var encoded_message = JsonConvert.SerializeObject(message!);

                channel.BasicPublish(exchange, key, body: Encoding.UTF8.GetBytes(encoded_message));
                this.Logger.LogInformation($"MESSAGE PUBLISH: {encoded_message}");
            }
        }
        public Task<IRabbitTransfer.MessageJson> GetMessageQueue(string queue, CancellationToken token)
        {
            var cancellation_sourse = new CancellationTokenSource();
            var result_message = default(NetworkTransfer.IRabbitTransfer.MessageJson)!;

            //using IConnection connection = RabbitFactory.CreateConnection();
            RabbitMQ.Client.IModel channel = this.RabbitConnection.CreateModel();
            var message_body = default(string?);

            var consumer = new EventingBasicConsumer(channel) { };
            consumer.Received += (object? model, BasicDeliverEventArgs args) =>
            {
                message_body = Encoding.UTF8.GetString(args.Body.ToArray());
                this.Logger.LogInformation($"MESSAGE RECEIVED: {message_body}");

                try { result_message = JsonConvert.DeserializeObject<MessageJson>(message_body); }
                finally { cancellation_sourse.Cancel(); }
            };
            consumer.ConsumerCancelled += (sender, args) => cancellation_sourse.Cancel();

            channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);
            return Task<IRabbitTransfer.MessageJson>.Run(delegate ()
            {
                var process_end = cancellation_sourse.Token;

                while (!process_end.IsCancellationRequested && (!(token.IsCancellationRequested)
                    || message_body != null)) { Task.Delay(millisecondsDelay: 100).Wait(); }

                try { return result_message; } finally { channel.Dispose(); }
            });
        }
        public Task<IRabbitTransfer.MessageJson> GetMessageExchange(string exchange, CancellationToken token)
        {
            var queue_args = new Dictionary<string, object>() { ["x-expires"] = QueueExpires };
            var queue_name = this.RabbitID.ToString();

            using (var channel = this.RabbitConnection.CreateModel())
            {
                channel.QueueDeclare(queue_name, false, false, true, arguments: null);
                channel.QueueBind(queue_name, exchange, queue_name, arguments: null);

                this.Logger.LogInformation($"QUEUE DECLARED: {queue_name}");
                return this.GetMessageQueue(queue_name, token);
            }
        }
    }
}
