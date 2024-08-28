using Game.Log;
using Game.Manager;
using Game.Net;
using Game.Net.SyncVar;
using UnityEngine;

namespace Game.Test
{
    public class testSyncVar
    {
        [SyncVar("test1")]
        public int test = 10;

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
            if (GameApp.NetClient.Running == true)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    await GameApp.SyncVarManager.GetVar("token_long", 3, (string id, object result) =>
                    {
                        LogUtils.Warn(result);
                    });
                    LogUtils.Warn(await GameApp.SyncVarManager.GetVar("token_string", 3));
                }
            }
        }
    }
}
