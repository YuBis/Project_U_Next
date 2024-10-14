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
    ECHO,

    MOVE,
}

public class PacketDataType
{
    public const string TYPE = "TYPE";
    public const string UID = "UID";
    public const string CID = "CID";

    public const string POSITION = "POSITION";
    public const string X = "X";
    public const string Y = "Y";
    public const string Z = "Z";

    public const string MOVESPEED = "MOVESPEED";

}

public partial class ServerManager : BaseManager<ServerManager>
{
    public delegate void MessageHandler(JObject packetBody);
    Dictionary<PacketType, MessageHandler> m_dicMessageHandler = new();

    TcpClient m_socket;
    NetworkStream m_stream;
    StreamReader m_reader;
    StreamWriter m_writer;

    private readonly string SERVER_IP = "127.0.0.1";
    private readonly int SERVER_PORT = 12345;

    private string TEMP_UID;

    protected override async void _InitManager()
    {
        TEMP_UID = Guid.NewGuid().ToString();
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
                    _HandleMessage(message);
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
            Debug.LogError($"Failed to receive data from the server: {e.Message}");
        }
    }

    void _HandleMessage(string message)
    {
        try
        {
            var json = JObject.Parse(message);
            var type = (PacketType)json[PacketDataType.TYPE].Value<int>();
            if (m_dicMessageHandler.TryGetValue(type, out var handler))
            {
                handler?.Invoke(json);
            }
            else
            {
                Debug.Log($"Unknown message type: {type}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse message: {e.Message}");
        }
    }

    public async UniTask SendMessageToServer(string message)
    {
        if (m_socket != null && m_socket.Connected)
        {
            await m_writer.WriteLineAsync(message);
            Debug.Log($"Message sent: {message}");
        }
        else
        {
            Debug.LogError("No connection to server.");
        }
    }

    public async UniTask SendCharacterMove(string characterId, Vector2 pos, float moveSpeed)
    {
        var msg = _GetDefaultPacketObject(PacketType.MOVE);

        msg[PacketDataType.CID] = characterId;
        msg[PacketDataType.POSITION] = new JObject
        {
            [PacketDataType.X] = pos.x,
            [PacketDataType.Y] = pos.y,
            [PacketDataType.Z] = 0
        };
        msg[PacketDataType.MOVESPEED] = moveSpeed;

        await SendMessageToServer(msg.ToString());
    }

    public void Disconnect()
    {
        m_writer?.Close();
        m_reader?.Close();
        m_stream?.Close();
        m_socket?.Close();
    }

    void _InitHandlerDict()
    {
        //_AddHandler(PacketType.MOVE, _Move);
    }

    void _AddHandler(PacketType messageType, MessageHandler handler)
    {
        m_dicMessageHandler.TryAdd(messageType, handler);
    }

    JObject _GetDefaultPacketObject(PacketType packetType)
    {
        return new JObject()
        {
            [PacketDataType.TYPE] = (int)packetType,
            [PacketDataType.UID] = TEMP_UID
        };
    }

    ~ServerManager()
    {
        Disconnect();
    }
}