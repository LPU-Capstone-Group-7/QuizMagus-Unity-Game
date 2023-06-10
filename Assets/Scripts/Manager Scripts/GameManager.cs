using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

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

    void Start()
    {
        if (SceneManager.sceneCount.Equals(1))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            SceneManager.LoadSceneAsync("BaseLevel", LoadSceneMode.Additive);
        }

    }

    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadLevelCoroutine(sceneName));
    }

    IEnumerator LoadLevelCoroutine(string sceneName)
    {
        SetScreenTransition(true);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneName);
        SceneManager.LoadScene("BaseLevel",LoadSceneMode.Additive);
    }

    void SetScreenTransition(bool playTransition)
    {
        Animator screenAnim = GameObject.FindGameObjectWithTag("Screen Transition").GetComponent<Animator>();
        if(playTransition)
        {
            screenAnim.Play("ScreenTransition_Close");
        }
        else
        {
            screenAnim.Play("ScreenTransition_Open");
        } 
        
    }


    //UTITLITY SCRIPTS
    public int findIndexOf<TArrayType>(TArrayType[] arrayList, TArrayType element)
    {
        for (int i = 0; i < arrayList.Length; i++)
        {
            if(element.Equals(arrayList[i]))
            {
                return i;
            }
        }

        Debug.LogError("Cannot find index inside the array");
        return 0;
    }

    //SHUFFLES THE TRIVIA GAME SETTINGS USING FISHER-YATES SHUFFLE ALGORITHM
    public TriviaQuestion[] ShuffleTriviaQuestions(TriviaQuestion[] triviaQuestions)
    {
        System.Random random = new System.Random();

        for (int i = triviaQuestions.Length-1; i > 0; i--)
        {
            //GETS A RANDOM INDEX
            int randomIndex = random.Next(0, i+1);

            //SWAPS THE ELEMENTS BETWEEN THE CURRENT INDEX AND RANDOM INDEX
            TriviaQuestion temp = triviaQuestions[i];
            triviaQuestions[i] = triviaQuestions[randomIndex];
            triviaQuestions[randomIndex] = temp;
        }

        return triviaQuestions;
    }

    public string HideAnswerContents(string answer)
    {
        string hiddenAnswer = "";
        foreach (char letter in answer)
        {
            if(letter != ' ')
            {
                hiddenAnswer += "_";
            }
            else{hiddenAnswer += letter;}
        }

        return hiddenAnswer;
    }


    


}
