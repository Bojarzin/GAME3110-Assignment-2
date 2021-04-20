using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;

public class SocketManager : MonoBehaviour
{
    // Scene Objects
    public TMP_Text result;
    public TMP_Text playerChoice;
    public TMP_Text yourID;

    public string playerID;

    public bool sentChoice;

    public int test;

    public UdpClient udp;
    const int PORT = 12345;

    void Start()
    {
        udp = new UdpClient();
        udp.Connect("ec2-18-188-63-98.us-east-2.compute.amazonaws.com", PORT);

        SendConnectMessage();
        SendHeartbeat();

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("SendHeartbeat", 1, 1);
    }

    private void Update()
    {
        Debug.Log("Times Finished: " + test);
        Debug.Log("Sent Choice: " + sentChoice);
    }

    void SendConnectMessage()
    {
        var payload = new ConnectClientMessage
        {
            header = SocketMessageType.CONNECT
        };
        var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload));
        udp.Send(data, data.Length);
    }

    void OnReceived(IAsyncResult _result)
    {
        UdpClient socket = _result.AsyncState as UdpClient;
        IPEndPoint source = new IPEndPoint(0, 0);
        byte[] message = socket.EndReceive(_result, ref source);

        string returnData = Encoding.ASCII.GetString(message);
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);

        HandleMessagePayload(returnData);
    }

    void HandleMessagePayload(string _data)
    {
        var payload = JsonUtility.FromJson<BaseSocketMessage>(_data);
        switch (payload.header)
        {
            case SocketMessageType.CONNECT:
                {
                    var connectPayload = JsonUtility.FromJson<ConnectClientMessage>(_data);
                    playerID = connectPayload.id;
                    yourID.text = "Your ID: " + playerID.ToString();
                    Debug.Log(playerID);
                    break;
                }
            case SocketMessageType.GAME_RULES:
                {
                    var gameRulesPayload = JsonUtility.FromJson<GameRulesServerMessage>(_data);
                    break;
                }
            case SocketMessageType.RESULT:
                {
                    sentChoice = false;
                    var resultPayload = JsonUtility.FromJson<ResultServerMessage>(_data);
                    result.text = resultPayload.message;
                    Debug.Log(resultPayload.message);
                    break;
                }
        }
    }

    public void SendPlayerChoiceToServer(string _choice)
    {
        var payload = new ChoiceClientMessage
        {
            header = SocketMessageType.CHOICE,
            choice = _choice,
            id = playerID
        };

        Debug.Log(playerID);

        var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload));
        udp.Send(data, data.Length);
    }

    void SendHeartbeat()
    {
        var beat = new HeartBeat
        {
            header = SocketMessageType.HEARTBEAT,
            heartBeat = "heartbeat"
        };

        var sendHeartBeat = Encoding.ASCII.GetBytes(JsonUtility.ToJson(beat));
        udp.Send(sendHeartBeat, sendHeartBeat.Length);
    }
}

[System.Serializable]
enum SocketMessageType
{
    NONE = 0,

    CONNECT = 1,        // Sent from client to server
    CHOICE = 2,         // Sent from client to server

    GAME_RULES = 3,     // Sent from server to client
    RESULT = 4,         // Sent from server to client

    HEARTBEAT = 5       // Sent from client to server
};

[System.Serializable]
class BaseSocketMessage
{
    public SocketMessageType header;
}

[System.Serializable]
class HeartBeat : BaseSocketMessage
{
    public string heartBeat = "heartbeat";
}

[System.Serializable]
class GameRulesServerMessage : BaseSocketMessage
{
    
}

[System.Serializable]
class ResultServerMessage : BaseSocketMessage
{
    public string message;
}

[System.Serializable]
class ConnectClientMessage : BaseSocketMessage
{
    public string id;
}

[System.Serializable]
class ChoiceClientMessage : BaseSocketMessage
{
    public string choice;
    public string id;
}
