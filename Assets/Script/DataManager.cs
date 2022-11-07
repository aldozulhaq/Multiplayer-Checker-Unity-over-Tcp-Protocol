using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json;

[SerializeField]
public class Player
{
    public int id { get; set; }
    public string name { get; set; }
    public int win { get; set; }
    public int lose { get; set; }
}
public class DataManager : MonoBehaviour
{
    static public Player player;
    private string URL = "https://localhost:5001/Player";
    public TMP_InputField loginName;

    public TMP_Text nameText;
    public TMP_Text winValue;
    public TMP_Text loseValue;

    private void Update()
    {
        if (nameText && player.name != "")
        {
            nameText.text = player.name;
        }

        if (winValue)
        {
            winValue.text = player.win.ToString();
        }
        
        if (loseValue)
        {
            loseValue.text = player.lose.ToString();
        }
    }

    public IEnumerator GetData()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(URL + "/" + loginName.text))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
                Debug.Log("GET Success");

            player = new Player();

            player = JsonConvert.DeserializeObject<Player>(request.downloadHandler.text);

            Debug.Log(request.downloadHandler.text);
            
        }
    }
    
    public IEnumerator PostData(Player player)
    {
        var jsonObject = JsonConvert.SerializeObject(player);

        WWWForm objectForm = new WWWForm();
        objectForm.AddField("Id", 0);
        objectForm.AddField("Name", player.name);
        objectForm.AddField("Win", player.win);
        objectForm.AddField("Lose", player.lose);

        Debug.Log(jsonObject);
        using (UnityWebRequest request = UnityWebRequest.Put(URL, jsonObject))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
                Debug.Log("POST Success");
        }

        StartCoroutine(GetData());
    }

    public IEnumerator PutData(Player player)
    {
        using (UnityWebRequest request = UnityWebRequest.Put(URL + "/" + player.id, JsonConvert.SerializeObject(player)))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
                Debug.Log("PUT Success");
        }
    }

    public IEnumerator Login()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(URL + "/" + loginName.text))
        {
            yield return request.SendWebRequest();

            if (request.responseCode == (long)System.Net.HttpStatusCode.NotFound)
            {
                Debug.Log("Data not found, create a new one");
                player = new Player();
                player.id = 0;
                player.name = loginName.text;
                player.lose = 0;
                player.win = 0;

                StartCoroutine(PostData(player));


            }
            else
            {
                StartCoroutine(GetData());
            }
        }
    }

    public void OnLoginPressed()
    {
        StartCoroutine(Login());        
    }

    public void AddWin()
    {
        player.win++;
        StartCoroutine(PutData(player));
    }

    public void AddLose()
    {
        player.lose++;
        StartCoroutine(PutData(player));
    }

    public void LogOut()
    {
        player.name = "";
        player.id = 0;
        player.lose = 0;
        player.win = 0;
    }
}
