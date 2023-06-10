using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class SongPrompt : MonoBehaviour
{
    public static SongPrompt instance;
    [SerializeField] SongItem[] songList;
    SongItem currentSong;
    bool isPlayingMusic = false;
    
    [Header("Song Prompt UI")]
    Transform songPromptUI;
    Animator songPromptAnimator;
    TextMeshProUGUI songTitleText;
    TextMeshProUGUI songAuthorText;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    

    void Update()
    {
        if(!isPlayingMusic && songList.Length > 1) PlayRandomBackgroundMusic();
        if(isPlayingMusic && !AudioManager.instance.isPlaying(currentSong.title)) isPlayingMusic = false;
    }

    public void PlayRandomBackgroundMusic()
    {
        if(songPromptUI == null)
        {
            songPromptUI = GameObject.Find("Music Credits").transform;
            
            songPromptAnimator = songPromptUI.GetComponent<Animator>();
            songTitleText = songPromptUI.Find("Title Text").GetComponent<TextMeshProUGUI>();
            songAuthorText = songPromptUI.Find("Author Text").GetComponent<TextMeshProUGUI>();
        }

        isPlayingMusic = true;
        SongItem newSong;

        //KEEP PICKING NEW SONG THAT IS DIFFERENT FROM THE PREVIOUS ONE
        do{
            System.Random random = new System.Random();
            int randomIndex = random.Next(0,songList.Length);
            newSong = songList[randomIndex];
        }
        while(currentSong.title == newSong.title);


        currentSong = newSong;
        AudioManager.instance.Play(newSong.title);
        DisplaySongPrompt(currentSong);
    }

    private void DisplaySongPrompt(SongItem currentSong)
    {
        songTitleText.text = currentSong.title + "♫♪";
        songAuthorText.text = currentSong.author;

        songPromptAnimator.Play("Show_Prompt");
    }
}

[System.Serializable]
struct SongItem{
    public string title;
    public string author;
}
