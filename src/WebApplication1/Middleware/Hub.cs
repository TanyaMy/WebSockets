using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace WebApplication1.Middleware
{

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class HubExtensions
    {

        static ConcurrentBag<WebSocket> _sockets = new ConcurrentBag<WebSocket>();
        public static IApplicationBuilder UseMySockets(this IApplicationBuilder builder)
        {
            builder.Use(async (http, next) =>
            {
                if (http.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await http.WebSockets.AcceptWebSocketAsync();
                    while (webSocket.State == WebSocketState.Open)
                    {
                        var token = CancellationToken.None;
                        var buffer = new ArraySegment<Byte>(new Byte[4096]);
                        var received = await webSocket.ReceiveAsync(buffer, token);


                        switch (received.MessageType)
                        {
                            case WebSocketMessageType.Text:
                                var request = Encoding.UTF8.GetString(buffer.Array,
                                                        0,
                                                        received.Count);

                                if (request == "Hi!")
                                {
                                    _sockets.Add(webSocket);
                                }
                                else
                                {
                                    var type = WebSocketMessageType.Text;
                                    
                                    foreach (var socket in _sockets)
                                    {
                                        await socket.SendAsync(buffer, type, true, token);
                                    }
                                }
                                break;
                        }
                    }
                }
                else
                {
                    await next();
                }
            });

            return builder;
        }
    }
}
