﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple.Client
{
    public class MapleClient
    {
        public ObservableCollection<ServerModel> Servers
        {
            get;
        } = new ObservableCollection<ServerModel>();

        public int ListenPort { get; set; }
        public int ListenTimeout { get; set; }

        public MapleClient(int listenPort = 17756, int listenTimeout = 5000)
        {
            ListenPort = listenPort;
            ListenTimeout = listenTimeout;
        }

        /// <summary>
        /// Starts scanning for Maple servers
        /// </summary>
        public async Task StartScanningForAdvertisingServers()
        {            
            //var hostList = new List<ServerModel>();
            var listener = new UdpClient(ListenPort);
            var ipEndPoint = new IPEndPoint(IPAddress.Any, ListenPort);
            string[] delimeter = new string[] { "::" };

            var timeoutTask = UdpTimeoutTask();

            try
            {
                while (timeoutTask.IsCompleted == false)
                {
                    //Console.WriteLine("Waiting for broadcast");

                    // create two tasks, one that will timeout after a while
                    var tasks = new Task<UdpReceiveResult>[]
                    {
                        timeoutTask,
                        listener.ReceiveAsync()
                    };

                    var completedTask = await Task.WhenAny(tasks);

                    if(completedTask == timeoutTask)
                    {
                        break;
                    }

                    var results = completedTask.Result;

                    /*

                    int index = 0;

                    await Task.Run(() => index = Task.WaitAny(tasks));

                    var results = tasks[index].Result;

                    if (results.RemoteEndPoint == null) { break; }

                    */

                    string host = Encoding.UTF8.GetString(results.Buffer, 0, results.Buffer.Length);
                    string hostIp = host.Split(delimeter, StringSplitOptions.None)[1];

                    //Console.WriteLine($"Raw:{host}, ip:{hostIp}");

                    //Console.WriteLine("Received broadcast from {0} :\n {1}\n", hostIp, host);

                    var server = new ServerModel()
                    {
                        Name = host.Split(delimeter, StringSplitOptions.None)[0],
                        IpAddress = host.Split(delimeter, StringSplitOptions.None)[1]
                    };

                    // if the server doesn't already exist in the list
                    if (!Servers.Any(s => s.IpAddress == hostIp))
                    {   // add it
                        //Console.WriteLine($"Found a server. Name: '{server.Name}', IP: {server.IpAddress} ");
                        Servers.Add(server);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                listener.Dispose();
            }
        }

        async Task<UdpReceiveResult> UdpTimeoutTask()
        {
            await Task.Delay(ListenTimeout);
            return new UdpReceiveResult();
        }

        protected async Task<bool> SendCommandAsync(string command, string hostAddress)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://" + hostAddress + "/"),
                Timeout = TimeSpan.FromSeconds(ListenTimeout)
            };

            try
            {
                var response = await client.PostAsync(command, null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> PostAsync(string hostAddress, int port, string endPoint, string data, string contentType = "text/plain")
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri($"http://{hostAddress}:{port}/{endPoint}");
            client.Timeout = TimeSpan.FromSeconds(ListenTimeout);
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue(contentType));

            try
            {
                var response = await client.PostAsync(endPoint, new StringContent(data));
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<string> GetAsync(string hostAddress, int port, string endpoint, string param, string value)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri($"http://{hostAddress}:{port}/"),
                Timeout = TimeSpan.FromSeconds(ListenTimeout)
            };

            try
            {
                var uri = $"{endpoint}?{param}={value}";

                var response = await client.GetAsync(uri);

                var msg = await response.Content.ReadAsStringAsync();
                return msg;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        public async Task<string> GetAsync(string hostAddress, int port, string endpoint, IDictionary<string, string> parameters)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri($"http://{hostAddress}:{port}/"),
                Timeout = TimeSpan.FromSeconds(ListenTimeout)
            };

            try
            {
                var uri = $"{endpoint}?";

                bool isFirst = true;
                foreach (var param in parameters)
                {
                    if (isFirst) isFirst = false;
                    else uri += "&";

                    uri += $"{param.Key}={param.Value}";
                }

                var response = await client.GetAsync(uri);

                var msg = await response.Content.ReadAsStringAsync();
                return msg;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }
    }
}