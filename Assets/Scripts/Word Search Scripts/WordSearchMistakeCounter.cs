using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordSearchMistakeCounter : MonoBehaviour
{
    public static WordSearchMistakeCounter instance;

    [Header("Cross Mark UI Components")]
    [SerializeField] private Transform crossMarkPrefab;
    [SerializeField] private float spacingX;

    [Header("Cross Mark List Counter")]
    List<Transform> activeCrossMarkList;
    
    private void Awake()
    {
        if(instance == null) instance = this;    
    }

    public void CreateMistakeCounterUI(int count)
    {
        ClearActiveCrossMarkList();
        activeCrossMarkList = new List<Transform>();

        for (int i = 0; i < count; i++)
        {
            RectTransform crossMarkUI = Instantiate(crossMarkPrefab, transform.position, Quaternion.identity, transform).GetComponent<RectTransform>();
            activeCrossMarkList.Add(crossMarkUI);

            //COMPUTE WORLD POSITION
            crossMarkUI.anchoredPosition = new Vector3(i * spacingX, 0, 0);
        }
    }

    public void ClearActiveCrossMarkList()
    {
        if(activeCrossMarkList == null || activeCrossMarkList.Count == 0) return;

        foreach (Transform crossMarkTransform in activeCrossMarkList)
        {
            Destroy(crossMarkTransform.gameObject);
        }
    }

    public int GetRemainingAllowableMistake()
    {
        return activeCrossMarkList.Count;
    }

    public void DecreaseRemainingAllowableMistake()
    {
        if(activeCrossMarkList != null && activeCrossMarkList.Count > 0)
        {
            Transform crossMarkUI = activeCrossMarkList[activeCrossMarkList.Count - 1];
            crossMarkUI.GetComponent<Animator>().Play("CrossMark_Shake");
            activeCrossMarkList.Remove(crossMarkUI);
            Destroy(crossMarkUI.gameObject,.4f);
        }
    }

    public int GetNumberOfAllowableMistake(string difficulty)
    {
        switch (difficulty)
        {
            case "easy":    return 1;
            case "normal":  return 2;
            case "hard":    return 3;
            default:        return 0;
        }
    }
}
