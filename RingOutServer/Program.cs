using System.Text;
using System.Net.WebSockets;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

app.Map("/ws", async context =>
{

    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        var buffer = new byte[1024];
        var result = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            CancellationToken.None
            );

        var message = Encoding.UTF8.GetString(
            buffer,
            0,
            result.Count
            );

        Console.WriteLine(message);

        var request =
            JsonSerializer.Deserialize<Message>(message);

        Console.WriteLine(request?.Type);

        var response = "OK";
        var bytes = Encoding.UTF8.GetBytes(response);

        await webSocket.SendAsync(
        new ArraySegment<byte>(bytes),
        WebSocketMessageType.Text,
        true,
        CancellationToken.None);
        Console.WriteLine("send ok");

        await Task.Delay(-1);
    }
});

app.MapGet("/", () => "Hello World!");

app.Run();

public class Message
{
    public string Type { get; set; } = "";
}