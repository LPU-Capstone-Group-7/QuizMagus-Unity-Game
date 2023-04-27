using UnityEngine;
using TMPro;
using System.Runtime.InteropServices;


//TODO CHECK IF PLAYER CAN PLAY 
public class UnityReactManager : MonoBehaviour
{
    [DllImport("__Internal")] private static extern void SendReactGame (string message);

    [Header("Game Data")]
    private UnityReactGame unityReactGame;

    private void TriggerMessageCall(string message){
        #if UNITY_WEBGL == true && UNITY_EDITOR == false
            SendReactGame(message);
        #endif
    }

    private void Start() {
        string message = "Hello, this is a message from unity";
        TriggerMessageCall(message);
    }

    public void StartGame(string unityReactGameJSON)
    {
        unityReactGame = JsonUtility.FromJson<UnityReactGame>(unityReactGameJSON);
        DataManager.instance.LoadGame(unityReactGame);
    }

}

public class UnityReactGame{
    public string gameType;
    public string uniqueGameSettings;
    public int allowedAttempts;
}