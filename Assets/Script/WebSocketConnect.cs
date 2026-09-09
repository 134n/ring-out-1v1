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

    [SerializeField]
    private Transform otherPlayer;

    private string playerType;

    private CancellationTokenSource cts;

    public class Message
    {
        public string Type;
        public string PlayerType;
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

        _ = ReceiveLoop();

        while (client.State == WebSocketState.Open)
        {
            await SendPositionAsync();
            await Task.Delay(2000, cts.Token);
        }
    }

    private async Task SendPositionAsync()
    {
        if (playerType == null) return;

        var message = new Message
        {
            Type = "Move",
            PlayerType = playerType,
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

    async Task ReceiveLoop()
    {
        var buffer = new byte[1024];

        while (client.State == WebSocketState.Open)
        {
            var result = await client.ReceiveAsync
            (
                new ArraySegment<byte>(buffer),
                cts.Token);

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            Debug.Log(message);
            var receiveMessage = JsonUtility.FromJson<Message>(message);

            if (receiveMessage.Type == "PlayerType")
            {
                playerType = receiveMessage.PlayerType.ToString();
            }
            Debug.Log(receiveMessage.PlayerType);
            if (receiveMessage.Type != "Move") continue;

            //var playerType = receiveMessage.PlayerType;//受け取ったタイプ
            if (receiveMessage.PlayerType != playerType)/*順番で決められたAorB*//*自分(AorB)*/
            {//受け取った位置情報のタイプが自身と違うならばotherPlayerに割り当てる
                otherPlayer.position = new Vector3(
                receiveMessage.X, otherPlayer.position.y, receiveMessage.Z);
            }
            else { continue; }
        }
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