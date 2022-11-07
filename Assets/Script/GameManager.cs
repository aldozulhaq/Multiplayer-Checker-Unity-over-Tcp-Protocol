using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { set; get; } //static instance so we can call it from any scripts

    public GameObject mainMenu;
    public GameObject serverMenu;
    public GameObject connectMenu;

    public GameObject serverPrefab;
    public GameObject clientPrefab;

    public TMP_InputField nameInput;

    private void Start()
    {
        instance = this;
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);
        DontDestroyOnLoad(gameObject);
    }

    public void menuConnectButton()
    {
        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
    }
    public void menuHostButton()
    {
        try
        {
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            s.Init();

            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            //c.clientName = nameInput.text;
            c.clientName = DataManager.player.name;
            c.isHost = true;
            if (c.clientName == "")
                c.clientName = "Host";
            c.ConnectToServer("127.0.0.1", 5003); //connect to self thus localhost
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        mainMenu.SetActive(false);
        serverMenu.SetActive(true);
    }
    public void connectToServerButton()
    {
        string hostAddress = GameObject.Find("HostInput").GetComponent<TMP_InputField>().text;
        if (hostAddress == "")
            hostAddress = "127.0.0.1"; //default localhost

        try
        {
            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = nameInput.text;
            if (c.clientName == "")
                c.clientName = "Client";
            c.ConnectToServer(hostAddress, 5003);
            connectMenu.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void backButton()
    {
        mainMenu.SetActive(true);
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);

        Server s = FindObjectOfType<Server>();
        if (s != null)
            Destroy(s.gameObject);

        Client c = FindObjectOfType<Client>();
        if (c != null)
            Destroy(c.gameObject);
    }

    public void singleplayerButton()
    {
        SceneManager.LoadScene("Game");

    }

    public void startGame()
    {
        SceneManager.LoadScene("Game");
    }
}
