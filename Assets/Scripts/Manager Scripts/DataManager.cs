using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    public int allowedAttempts;
    [TextArea(15,20)] [SerializeField] private string uniqueGameSettings;
    
    private void Awake() 
    {
        if(instance == null)
        {
            instance = this;
        }
        else{ Destroy(gameObject);}

        DontDestroyOnLoad(gameObject);
    }

    //TAKE THE UNIQUE PARAMETER AND THEN STORE IT HERE
    public TGameSettings GetGameSettings<TGameSettings>() where TGameSettings : class
    {
        return JsonUtility.FromJson<TGameSettings>(uniqueGameSettings);
    }

   //MAKE A GAME RESULT GENERIC GAMEOBJECT TO STORE RESULTS CLASS FOR EACH GAME
    public void GetGameResults<TGameResults>(TGameResults gameResults)
    {

    }
    
    //FIGURE OUT WHAT GAME SHOULD BE LOADED
    public void LoadGame(UnityReactGame unityReactGame)
    {
        uniqueGameSettings = unityReactGame.uniqueGameSettings;
        allowedAttempts = unityReactGame.allowedAttempts;

        switch (unityReactGame.gameType)
        {
            case "TriviaGame":  GameManager.instance.LoadLevel("TriviaGame");   break;
            case "WordSearch":  GameManager.instance.LoadLevel("WordSearch");   break;
            case "CrossWord":   GameManager.instance.LoadLevel("CrossWord");    break;
            default: break;
        }
    }
}
