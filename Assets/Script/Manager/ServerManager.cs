using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

public enum PacketType
{
    MOVE,
}

public class ServerManager : BaseManager<ServerManager>
{
    public delegate void MessageHandler(PacketType packetType, JObject packetBody);

    Dictionary<PacketType, MessageHandler> m_dicMessageHandler = new();

    TcpClient m_socket;
    NetworkStream m_stream;
    StreamReader m_reader;
    StreamWriter m_writer;

    private readonly string SERVER_IP = "127.0.0.1";
    private readonly int SERVER_PORT = 12345;

    protected override async void _InitManager()
    {
        await ConnectToServer();
    }

    public async UniTask ConnectToServer()
    {
        try
        {
            m_socket = new TcpClient();
            await m_socket.ConnectAsync(SERVER_IP, SERVER_PORT);
            if (m_socket.Connected)
            {
                m_stream = m_socket.GetStream();
                m_reader = new StreamReader(m_stream, Encoding.UTF8);
                m_writer = new StreamWriter(m_stream, Encoding.UTF8) { AutoFlush = true };

                Debug.Log("Connected to server");

                StartReceiving().Forget();

                await SendMessageToServer("New Connection!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to server: {e.Message}");
        }
    }

    private async UniTaskVoid StartReceiving()
    {
        try
        {
            while (true)
            {
                string message = await m_reader.ReadLineAsync();
                if (message != null)
                {
                    Debug.Log("Received message: " + message);
                    HandleMessage(message);
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

    private void HandleMessage(string message)
    {
        try
        {
            var json = JObject.Parse(message);
            string type = json["type"].ToString();

            if (type == "MOVE")
            {
                string characterId = json["characterId"].ToString();
                float x = json["position"]["x"].ToObject<float>();
                float y = json["position"]["y"].ToObject<float>();
                float z = json["position"]["z"].ToObject<float>();

                Debug.Log($"Character {characterId} moved to ({x}, {y}, {z})");
            }
            else
            {
                Debug.Log("Unknown message type: " + type);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse message: " + e.Message);
        }
    }

    public async UniTask SendMessageToServer(string message)
    {
        if (m_socket != null && m_socket.Connected)
        {
            await m_writer.WriteLineAsync(message);
            Debug.Log("Message sent: " + message);
        }
        else
        {
            Debug.LogError("No connection to server.");
        }
    }

    public async UniTask SendCharacterMove(string characterId, float x, float y, float z)
    {
        var moveMessage = new JObject
        {
            ["type"] = "MOVE",
            ["characterId"] = characterId,
            ["position"] = new JObject
            {
                ["x"] = x,
                ["y"] = y,
                ["z"] = z
            }
        };

        await SendMessageToServer(moveMessage.ToString(Newtonsoft.Json.Formatting.None));
    }

    public void Disconnect()
    {
        m_writer?.Close();
        m_reader?.Close();
        m_stream?.Close();
        m_socket?.Close();
    }

    ~ServerManager()
    {
        Disconnect();
    }
}