using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class WordSearchTextUI : MonoBehaviour
{
    public static WordSearchTextUI instance;

    [Header("Answer List Grid")]
    [SerializeField] private RectTransform answerListContainerTransform;
    [SerializeField] private int gridColumn = 2;
    [SerializeField] private float maxCellHeight;
    [SerializeField] private GameObject wordSearchTextPrefab;
    [SerializeField] private float strikethroughSpeed;
    private List<GameObject> wordSearchTextObjectList;
    

    private void Awake()
    {
        if(instance == null) instance = this;    
    }

    public void DisplayWordSearchAnswers(List<string> wordSearchContents)
    {
        ClearExistingWordSearchAnswers();
        
        //GET THE WIDTH AND HEIGHT OF THE CONTAINER
        float containerWidth = answerListContainerTransform.sizeDelta.x * answerListContainerTransform.localScale.x;
        float containerHeight = answerListContainerTransform.sizeDelta.y * answerListContainerTransform.localScale.y;

        //GET THE INDIVIDUAL WIDTH AND HEIGHT OF EACH CELL USING THE NUMBER OF CHILD OR INDEXES
        float childWidth = containerWidth / gridColumn;
        float childHeight = Mathf.Min(containerHeight / (wordSearchContents.Count / (float)gridColumn), maxCellHeight);
        answerListContainerTransform.GetComponent<GridLayoutGroup>().cellSize = new Vector2(childWidth, childHeight);

        //SHUFFLE LIST
        wordSearchContents.Sort();

        //INSTANTIATE ALL OF THE WORD SEARCH TEXT
        for (int i = 0; i < wordSearchContents.Count; i++)
        {
            GameObject gameObjectText = Instantiate(wordSearchTextPrefab, Vector3.zero, Quaternion.identity, answerListContainerTransform);

            //ADJUST ALIGNMENT OF TEXT BASED ON ORDER OF THE TEXT
            gameObjectText.GetComponent<TextMeshProUGUI>().text = wordSearchContents[i];

            //ADJUST TEXT POSITION
            gameObjectText.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
            if(i % 2 == 1) gameObjectText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        }
    }

    private void ClearExistingWordSearchAnswers()
    {
        foreach (Transform child in answerListContainerTransform)
        {
            Destroy(child.gameObject);
        }
    }

    public void CrossOutWord(string word)
    {
        foreach (Transform child in answerListContainerTransform)
        {
            TextMeshProUGUI textMeshPro = child.GetComponent<TextMeshProUGUI>();

            if(textMeshPro != null && textMeshPro.text == word)
            { 
                StartCoroutine(PlayStriketrhoughAnimation(textMeshPro));
                break;
            }
        }
    }

    private IEnumerator PlayStriketrhoughAnimation(TextMeshProUGUI text)
    {
        string originalText = text.text;
        float speed = strikethroughSpeed / originalText.Length;

        for (int i = 0; i < originalText.Length; i++)
        {
            text.text = "<s color=#b0305c>" + originalText.Insert(i, "</s>");
            yield return new WaitForSeconds(speed);
        }

        text.text = "<s color=#b0305c>" + originalText + "</s>";
    }

}
