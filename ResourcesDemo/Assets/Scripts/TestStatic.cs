using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TestStatic
{
    public class TestA
    {
        public TestA()
        {
            Debug.Log("TestA ctor");
        }
    }
    public static int a = 0;
    public static TestA testA = new TestA();
    public static TestStatic testS = new TestStatic();
    static TestStatic()
    {
        Debug.Log("TestStatic cctor");
    }

    public TestStatic()
    {
        Debug.Log("TestStatic ctor");
    }
}
