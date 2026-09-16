using System.Text;
using System.Net.WebSockets;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var clients = new List<Client>();

app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        PlayerType conectPlayerType;

        if (!clients.Exists(client => client.PlayerType == PlayerType.playerA))
        {
            conectPlayerType = PlayerType.playerA;
        }
        else if (!clients.Exists(client => client.PlayerType == PlayerType.playerB))
        {
            conectPlayerType = PlayerType.playerB;
        }
        else { return; }

        var client = new Client
        {
            WebSocket = webSocket,
            PlayerType = conectPlayerType
        };

        clients.Add(client);
        Console.WriteLine($"Connected: {clients.Count}");
        Console.WriteLine($"Connected: {client.PlayerType}");

        var playerTypeMessage = new Message
        {
            Type = "PlayerType",
            PlayerType = client.PlayerType.ToString()
        };

        var json = JsonSerializer.Serialize(playerTypeMessage);
        var bytes = Encoding.UTF8.GetBytes(json);
        await client.WebSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);

        try
        {
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
                    clients.Remove(client);

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
                Console.WriteLine(request?.PlayerType);
                Console.WriteLine(request?.X);
                Console.WriteLine(request?.Z);

                foreach (var targetClient in clients)
                {
                    if (targetClient.WebSocket == webSocket)
                        continue;

                    if (targetClient.WebSocket.State != WebSocketState.Open)
                        continue;

                    var sendBytes = Encoding.UTF8.GetBytes(message);
                    await targetClient.WebSocket.SendAsync(
                        new ArraySegment<byte>(sendBytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);

                    Console.WriteLine("B send OK");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

            clients.Remove(client);
            return;
        }
    }
});

app.MapGet("/", () => "Hello World!");

app.Run();

public class Message
{
    public string Type { get; set; } = "";
    public string PlayerType { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
}

public enum PlayerType
{
    playerA,
    playerB
}

public class Client
{
    public WebSocket WebSocket { get; set; }
    public PlayerType PlayerType { get; set; }
}