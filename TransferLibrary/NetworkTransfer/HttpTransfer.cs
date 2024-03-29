﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TransferLibrary.Export;

namespace TransferLibrary.NetworkTransfer
{
    public interface IHttpTransfer : System.IDisposable
    {
        public TData SendRequest<TData>(Dictionary<System.String, System.Object> request_body, 
            System.String resourse, System.Int32 timeout = 5000);
        public string TestConnection(System.String connection, System.String message);

        public sealed class HttpTransferException : System.Exception
        {
            public System.DateTime ExceptionTime { get; private set; } = DateTime.Now;
            public HttpTransferException(System.String error_message) : base(error_message) { }
        }
    }

    public class HttpTransfer : System.Object, NetworkTransfer.IHttpTransfer
    {
        protected ILogger<HttpTransfer> Logger { get; private set; } = default!;
        protected IHttpClientFactory ClientFactory { get; private set; } = default!;
        protected System.String ConnectionPath { get; private set; } = default!;

        public HttpTransfer(ILogger<HttpTransfer> logger, IHttpClientFactory client_factory, System.String hostname)
        { 
            (this.ConnectionPath, this.ClientFactory) = (hostname, client_factory);
            (this.Logger = logger).LogInformation($"HttpHostname: {this.ConnectionPath}");
        }
        public TData SendRequest<TData>(Dictionary<System.String, System.Object> request_body, 
            string resourse, int timeout = 5000)
        {
            using HttpClient httpClient = this.ClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post, Content = JsonContent.Create(request_body),
                RequestUri = new Uri($@"{this.ConnectionPath}/{resourse}"),
            };
            using (HttpResponseMessage response = httpClient.SendAsync(request).Result)
            {
                if (response.IsSuccessStatusCode != true)
                { throw new IHttpTransfer.HttpTransferException("ERROR: CLIENT BAD REQUEST"); }

                var message_result = response.Content.ReadAsStringAsync().Result!;
                return JsonConvert.DeserializeObject<TData>(message_result)!;
            };
        }
        [System.ObsoleteAttribute]
        public string TestConnection(string conn,  string message)
        {
            var http_client = ClientFactory.CreateClient();
            using var response = http_client.PostAsync(conn, new StringContent(message)).Result;

            return response.Content.ReadAsStringAsync().Result;
        }
        void System.IDisposable.Dispose() { }
    }
}
