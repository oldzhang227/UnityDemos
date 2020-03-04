using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour {

    public Image image;
    // Use this for initialization
    void Awake () {

        StartCoroutine(Download());
    }

 

    IEnumerator Download()
    {
        WWW www = new WWW("http://q.qlogo.cn/qqapp/1108151222/33A60F8F7BC44EDF8E01BC293C296BFB/100");
        while(!www.isDone)
        {
            yield return null;
        }
        if(www.bytes != null)
        {
            byte[] bytes = www.bytes;
            string path = Application.persistentDataPath + "/head.png";
            File.WriteAllBytes(path, bytes);
            SetSprite(path, image);
        }
    }


    void SetSprite(string path, Image image)
    {
        if (File.Exists(path))
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = null;
            if (bytes[0] == 'G' && bytes[1] == 'I' && bytes[2] == 'F')
            {
                tex = GIFDecoder.Decode(bytes, false);
            }
            else
            {
                tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                tex.LoadImage(bytes);
            }

            if (tex != null)
            {
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                image.sprite = sprite;
            }
        }
    }
}
