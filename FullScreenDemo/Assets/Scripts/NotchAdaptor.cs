using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class NotchAdaptor : MonoBehaviour {

    RectTransform rectTransform = null;
	// Use this for initialization
	void Start () {
        
        StartCoroutine(Adaptor());
    }

    IEnumerator Adaptor()
    {
        while(!NotchUtils.Instance().IsInit)
        {
            yield return null;
        }
        if(NotchUtils.Instance().HasNotch)
        {
            Rect rect = NotchUtils.Instance().GetSafeRect();
            if(rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            rectTransform.anchorMin = rect.min;
            rectTransform.anchorMax = rect.size;
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if(focus)
        {
            StartCoroutine(Adaptor());
        }
    }
}
