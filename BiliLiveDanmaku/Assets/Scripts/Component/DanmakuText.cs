using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DanmakuText : MonoBehaviour
{
    public Text wordText;
    void Start()
    {

    }

    public void SetText(string text)
    {
        wordText.text = text;
    }
}
