using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGC : MonoBehaviour
{
    public TestGC()
    {
        Debug.Log("TestGC");
    }


    ~TestGC()
    {
        Debug.Log("~TestGC");
    }
    private void Awake()
    {
        Debug.Log("TestGC Awake");
    }


    private void OnEnable()
    {
        Debug.Log("TestGC OnEnable");
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("TestGC Start");
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("TestGC Update");
    }


    private void LateUpdate()
    {
        //Debug.Log("TestGC LateUpdate");
    }

    private void OnDisable()
    {
        Debug.Log("TestGC OnDisable");
    }

    private void OnDestroy()
    {
        Debug.LogFormat("OnDestroy {0}", gameObject.name);
    }

}
