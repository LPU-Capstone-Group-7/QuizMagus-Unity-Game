using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TickerItem : MonoBehaviour
{
    float tickerWidth;
    float itemSpeed;
    string message;
    RectTransform rt;
    bool canMove = false;

    public float GetXPosition {get {return rt.anchoredPosition.x;}}
    public float GetWidth{get {return rt.rect.width; }}
    public string GetMessage{get {return message;}}

    public void Initialize(float tickerWidth, float itemSpeed, string message)
    {
        rt = GetComponent<RectTransform>();
        GetComponent<TextMeshProUGUI>().text = message;

        this.tickerWidth = tickerWidth;
        this.itemSpeed = itemSpeed;
        this.message = message;
        this.canMove = tickerWidth < GetComponentInParent<RectTransform>().rect.width;

        rt.anchoredPosition = canMove? new Vector2(tickerWidth, 0) : Vector2.zero;

    }

    // Update is called once per frame
    void Update()
    {
        if(canMove)
        {
            rt.position += Vector3.left * itemSpeed;
        }
        
        if(GetXPosition <= 0 - tickerWidth - GetWidth)
        {
            Destroy(gameObject);
        }
    }
}
