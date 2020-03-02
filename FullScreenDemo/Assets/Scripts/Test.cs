using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour {

    public Text text;
    // Use this for initialization
    void Awake () {

        Debug.Log("Screen:" + Screen.width + "-" + Screen.height);
        Debug.Log("SafeArea:" + Screen.safeArea.ToString());
        StartCoroutine(Adaptor());
    }

    IEnumerator Adaptor()
    {
        while (!NotchUtils.Instance().IsInit)
        {
            yield return null;
        }
        if (NotchUtils.Instance().HasNotch)
        {
            text.text = NotchUtils.Instance().NotchSize.ToString();
        }
    }
}
