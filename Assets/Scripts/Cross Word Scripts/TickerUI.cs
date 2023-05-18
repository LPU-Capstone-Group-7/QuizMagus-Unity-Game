using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickerUI : MonoBehaviour
{
    [Range(1f, 10f)] public float itemDuration = 3f;
    private float width;
    private float pixelsPerSecond;
    public Transform tickerItemPrefab;
    private TickerItem currentItem;
    
    // Start is called before the first frame update
    void Start()
    {
        width = GetComponent<RectTransform>().rect.width;
        pixelsPerSecond = width / itemDuration;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentItem != null && currentItem.GetXPosition <= -currentItem.GetWidth)
        {
            Debug.Log("lolz");
            CreateNewTicker(currentItem.GetMessage);
        }
    }

    public float GetCanvasWidth()
    {
        return GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
    }

    public void CreateNewTicker(string question)
    {
        //DESTROY CURRENT TICKER
        if(currentItem != null) Destroy(currentItem.gameObject);

        //REPLACE TICKER WITH THIS NEW QUESTION
        Transform ticker = Instantiate(tickerItemPrefab, Vector3.zero, Quaternion.identity, transform);
        width = GetComponent<RectTransform>().rect.width;

        currentItem = ticker.gameObject.GetComponent<TickerItem>();
        currentItem.Initialize(width, pixelsPerSecond, question);

        //CHECK TICKER WIDTH IF ITS GREATER THAN THE CANVAS SIZE OR NOT
    }
}
