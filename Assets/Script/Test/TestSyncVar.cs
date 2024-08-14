using System.Collections;
using System.Collections.Generic;
using Game.Log;
using Game.Net.SyncVar;
using UnityEngine;

namespace Game.Test
{
    public class testSyncVar
    {
        [SyncVar("test1")]
        public int test = 0;

        [SyncVar("test2")]
        public string test2 = "test";
    }

    public class TestSyncVar : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            //LogUtils.InitSettings(new LogUtilsConfig()
            //{
            //    savePath = Application.streamingAssetsPath,
            //});
            //SyncVarManager.Instance.RegisterAssembly();
        }

        // Update is called once per frame
        void Update()
        {
            //LogUtils.Log(SyncVarManager.Instance.GetSyncVarValue("test1"));
            //LogUtils.Log(SyncVarManager.Instance.GetSyncVarValue("test2"));
        }
    }
}
