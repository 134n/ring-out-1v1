using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class WebSocketConnect : MonoBehaviour
{
    private ClientWebSocket client;

    [Serializable]
    public class Message
    {
        public string Type;
    }

    async void Start()
    {
        client = new ClientWebSocket();

        await client.ConnectAsync(
            new Uri("ws://localhost:5288/ws"),
            CancellationToken.None);

        Debug.Log("connected");

        var message = new Message
        {
            Type = "connect"
        };

        var json = JsonUtility.ToJson(message);
        
        Debug.Log(json);

        var bytes = Encoding.UTF8.GetBytes(json);

        await client.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);

        Debug.Log("Send Hello");

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
}