using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CreateUI : MonoBehaviour {

    [MenuItem("GameObject/UI/Image")]
    static void CreatImage()
    {
        if (Selection.activeTransform)
        {
            if (Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("Image", typeof(Image));
                var image = go.GetComponent<Image>();
                image.raycastTarget = false;
                //image.rectTransform.localScale = Vector3.one;
                go.transform.SetParent(Selection.activeTransform, false);
            }
        }
    }
    [MenuItem("GameObject/UI/Raw Image")]
    static void CreatRawImage()
    {
        if (Selection.activeTransform)
        {
            if (Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("RawImage", typeof(RawImage));
                var rawImage = go.GetComponent<RawImage>();
                rawImage.raycastTarget = false;
                //image.rectTransform.localScale = Vector3.one;
                go.transform.SetParent(Selection.activeTransform, false);
            }
        }
    }
    [MenuItem("GameObject/UI/Text")]
    static void CreatText()
    {
        if (Selection.activeTransform)
        {
            if (Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("Text", typeof(Text));
                var text = go.GetComponent<Text>();
                
                //image.rectTransform.localScale = Vector3.one;
                go.transform.SetParent(Selection.activeTransform, false);
                text.color = Color.black;
                text.text = "new text";
                text.fontSize = 35;
                text.rectTransform.sizeDelta = new Vector2(150f, 50f);
                text.raycastTarget = false;
            }
        }
    }
}
