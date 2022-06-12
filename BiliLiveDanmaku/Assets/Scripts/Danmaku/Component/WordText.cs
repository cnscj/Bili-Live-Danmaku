using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordText : MonoBehaviour
{
    public Text wordText;
    public Vector2 size;    //TODO:

    public void SetText(string text)
    {
        wordText.text = text;
    }

    public void InitPos()
    {
        var rectTransform = (RectTransform)transform;
        var textHeight = rectTransform.rect.height;
        var maxYParts = size.y / textHeight;
        var yIndex = Random.Range(0, maxYParts);

        var randomX = size.x;
        var randomY = yIndex * textHeight;

        randomY = Mathf.Max(textHeight/2, randomY);
        randomY = Mathf.Min(size.x - (textHeight/2), randomY);

        rectTransform.localPosition = new Vector3(randomX, randomY, 0);
    }

    private void OnDanmaku(object args)
    {
        var msg = (string)args;

        InitPos();
        SetText(msg);
    }
}
