using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocketConnect : MonoBehaviour
{
    private ClientWebSocket client;

    [SerializeField]
    private Transform player;

    private CancellationTokenSource cts;

    [Serializable]
    public class Message
    {
        public string Type;
        public float X;
        public float Z;
    }

    async Task Start()
    {
        cts = new CancellationTokenSource();
        client = new ClientWebSocket();

        await client.ConnectAsync(
            new Uri("ws://localhost:5288/ws"),
            cts.Token);

        Debug.Log("connected");

        while (client.State == WebSocketState.Open)
        {
            await SendPositionAsync();
            await Task.Delay(2000, cts.Token);
        }

        var buffer = new byte[1024];
        var result = await client.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            CancellationToken.None
            );

        var response = Encoding.UTF8.GetString(
            buffer,
            0,
            result.Count
        );

        Debug.Log(response);
    }

    private async Task SendPositionAsync()
    {
        var message = new Message
        {
            Type = "Move",
            X = player.transform.position.x,
            Z = player.transform.position.z
        };

        var json = JsonUtility.ToJson(message);

        Debug.Log(json);

        var bytes = Encoding.UTF8.GetBytes(json);

        await client.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    private async void OnDestroy()
    {
        cts?.Cancel();

        if (client != null && client.State == WebSocketState.Open)
        {
            await client.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Unity Stop",
                CancellationToken.None
            );

            client.Dispose();
        }

        Debug.Log("Unity Stop");
    }
}