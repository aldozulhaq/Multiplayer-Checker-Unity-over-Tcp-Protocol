using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public string clientName;
    public bool isHost = false;

    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public List<GameClient> players = new List<GameClient>();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public bool ConnectToServer(string host, int port)
    {
        if (socketReady)
            return false;

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error : " + e.Message);
        }

        return socketReady;
    }
    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                    OnIncomingData(data);
            }
        }
    }


    //send data to server
    public void Send(string data)
    {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    //read data from server
    private void OnIncomingData(string data)
    {
        Debug.Log("Client : " + data);
        string[] theData = data.Split('|');

        switch (theData[0])
        {
            case "5W":
                for (int i = 1; i < theData.Length - 1; i++)
                {
                    clientConnected(theData[i], false);
                }
                Send("CN|" + clientName + "|" + ((isHost) ? 1 : 0).ToString());
                break;
            case "5C":
                clientConnected(theData[1], false);
                break;
            case "5move":
                boardManager.Instance.tryMove(int.Parse(theData[1]), int.Parse(theData[2]), int.Parse(theData[3]), int.Parse(theData[4]));
                break;
        }
    }

    private void clientConnected(string name, bool isHost)
    {
        GameClient c = new GameClient();
        c.name = name;

        players.Add(c);

        if (players.Count == 2)
            GameManager.instance.startGame();
    }

    private void OnApplicationQuit()
    {
        closeSocket();
    }
    private void OnDisable()
    {
        closeSocket();
    }
    private void closeSocket()
    {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }
}

public class GameClient
{
    public string name;
    public bool isHost;

}