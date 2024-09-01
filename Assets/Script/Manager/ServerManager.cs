using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ServerManager : BaseManager<ServerManager>
{
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;
    private CancellationTokenSource cancellationTokenSource;

    private string serverIP = "127.0.0.1";
    private int serverPort = 12345;

    protected override void _InitManager()
    {
        //ConnectToServer().Forget(); // for test
    }

    async UniTaskVoid ConnectToServer()
    {
        try
        {
            socket = new TcpClient();
            await socket.ConnectAsync(serverIP, serverPort);  // 비동기 연결 시도
            if (socket.Connected)
            {
                stream = socket.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                Debug.Log("Connected to server");

                StartReceiving();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to server: {e.Message}");
        }
    }

    async void StartReceiving()
    {
        try
        {
            while (true)
            {
                string message = await reader.ReadLineAsync().AsUniTask();
                if (message != null)
                {
                    Debug.Log("Received message: " + message);
                }
                else
                {
                    Debug.Log("Server closed the connection.");
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to receive data from the server: " + e.Message);
        }
    }

    async UniTask SendMessageToServer(string message)
    {
        if (socket != null && socket.Connected)
        {
            await writer.WriteLineAsync(message);
            Debug.Log("Message sent: " + message);
        }
        else
        {
            Debug.LogError("No connection to server.");
        }
    }

    void OnDestroy()
    {
        // Clean up network resources
        writer?.Close();
        reader?.Close();
        stream?.Close();
        socket?.Close();
    }
}