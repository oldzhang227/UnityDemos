using IFix.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game
{
  
    public class InjectDemo : MonoBehaviour
    {

        public delegate void MyDelegate(string s);

        // Use this for initialization
        void Start()
        {
            LoadPatch();
            int a = 9;
            int b = 10;
            Debug.LogFormat("{0} + {1} = {2}", a, b, Add(a, b));
        }

        void LoadPatch()
        {
            string patchFile = Application.streamingAssetsPath + "/patch/Assembly-CSharp.patch.bytes";
            WWW www = new WWW(patchFile);
            while (!www.isDone) ;
            if(www.bytes != null && www.bytes.Length > 0)
            {
                PatchManager.Load(new MemoryStream(www.bytes));
            }
            www.Dispose();
        }


#if !INJECTFIX_PATCH_ENABLE
        int Add(int a, int b)
        {
            return a * b;
        }
#else
        [IFix.Patch]
        int Add(int a, int b)
        {
            return a + b;
        }
#endif


    }

}

