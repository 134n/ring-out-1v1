using System.Text;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var clients = new List<WebSocket>();

app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        clients.Add(webSocket);
        Console.WriteLine($"Connected: {clients.Count}");

        while (webSocket.State == WebSocketState.Open)
        {
            var buffer = new byte[1024];
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
                );

            if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("Client disconnected");
                
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Server closing",
                    CancellationToken.None
                );
                clients.Remove(webSocket);

                return;
            }

            var message = Encoding.UTF8.GetString(
                buffer,
                0,
                result.Count
                );

            Console.WriteLine(message);

            var request =
                JsonSerializer.Deserialize<Message>(message);

            Console.WriteLine(request?.Type);
            Console.WriteLine(request?.X);
            Console.WriteLine(request?.Z);

            foreach(var client in clients)
            {
                if(client == webSocket)
                continue;

                if(client.State != WebSocketState.Open)
                continue;

                var bytes = Encoding.UTF8.GetBytes(message);
                await client.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);

                Console.WriteLine("B send OK");
            }
        }
    }
});

app.MapGet("/", () => "Hello World!");

app.Run();

public class Message
{
    public string Type { get; set; } = "";
    public float X { get; set; }
    public float Z { get; set; }
}