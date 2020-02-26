using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Test : MonoBehaviour {



    public List<GameObject> objList = new List<GameObject>();

    // Use this for initialization
    void Start () {


        DontDestroyOnLoad(gameObject);

    }


    IEnumerator coLoadScene()
    {
        AssetLoader assetLoader = ResourcesManager.Instance.LoadScene("Assets/Resources/scene/test.unity", true, true, true);
        yield return null;
        while(!assetLoader.isDone)
        {
            Debug.Log(assetLoader.progress);
            yield return null;
        }

        Debug.Log(assetLoader.progress);

        yield return null;
    }

	
	// Update is called once per frame
	void Update () {

        //ResourcesManager.Instance.Update(); 
	}

    private void LateUpdate()
    {
        ResourcesManager.Instance.Update();
    }


    private void OnGUI()
    {
        if(GUI.Button(new Rect(0, 0, 100, 50),"LoadObject"))
        {

            ResourcesManager.Instance.LoadAssetAsync("Assets/Resources/0/Cube.prefab", true, (assetInfo) =>
            {
                if (assetInfo != null && assetInfo.Exist())
                {
                    GameObject testObj = assetInfo.Instantiate();
                    testObj.transform.position = Vector3.zero;
                    int x = UnityEngine.Random.Range(-7, 7);
                    int y = UnityEngine.Random.Range(-4, 6);
                    testObj.transform.position = new Vector3(x, y, 0);
                    testObj.transform.rotation = Quaternion.Euler(0, 90, 0);


                    objList.Add(testObj);
                }
            });
        }

        if (GUI.Button(new Rect(0, 100, 100, 50), "LoadScene"))
        {
            ResourcesManager.Instance.LoadScene("Assets/Resources/scene/test.unity", true, true, true);
        }

        if (GUI.Button(new Rect(0, 200, 100, 50), "LoadScene2"))
        {
            ResourcesManager.Instance.LoadScene("Assets/Resources/scene/test2.unity", true, true, true);
        }

        if (GUI.Button(new Rect(0, 300, 100, 50), "DestroyObject"))
        {
            if(objList.Count > 0)
            {
                if(objList[0] != null)
                {
                    objList[0].DestroyUsePool(true);
                }
                objList.RemoveAt(0);
            }
        }

  
        if (GUI.Button(new Rect(0, 400, 100, 50), "UnLoad"))
        {
            AssetManager.Instance.UnloadunusedAssetAsync();
        }

    }
}
