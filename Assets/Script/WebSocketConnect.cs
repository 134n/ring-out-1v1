using System;
using System.Net.WebSockets;
using System.Threading;
using UnityEngine;

public class WebSocketConnect : MonoBehaviour
{
    private ClientWebSocket client;

    async void Start()
    {
        client = new ClientWebSocket();

        await client.ConnectAsync(
            new Uri("ws://localhost:5288/ws"),
            CancellationToken.None);
        
        Debug.Log("connected");
    }
}
