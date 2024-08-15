using System.Collections;
using System.Collections.Generic;
using Game.Log;
using Game.Net;
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
            
        }

        // Update is called once per frame
        async void Update()
        {
            if (NetClient.Instance.Running == true)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    LogUtils.Warn(await SyncVarManager.Instance.GetVar("token_long", 3));
                    LogUtils.Warn(await SyncVarManager.Instance.GetVar("token_string", 3));
                }
            }
        }
    }
}
