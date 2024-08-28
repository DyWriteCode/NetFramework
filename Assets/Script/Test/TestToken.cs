using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Game.Log;
using Game.Manager;
using Game.Net;
using Game.Net.SyncVar;
using Game.Net.TokenAuth;
using Proto;
using UnityEngine;

namespace Game.Test
{
    public class TestToken : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            //Dictionary<string, object> tokens = new Dictionary<string, object>
            //{
            //    { "a", "a" },
            //    { "b", "b" },
            //    { "c", "c" },
            //    { "d", "d" },
            //};
            //string test = TokenManager.Instance.GenerateToken("1001", TokenType.Flash, tokens);
            //Debug.LogWarning(test);
            //Debug.LogWarning(TokenManager.Instance.ValidateToken(test));
            //TokenManager.Instance.InvalidateToken(test);
            //Debug.LogWarning(TokenManager.Instance.ValidateToken(test));
        }

        // Update is called once per frame
        async void Update()
        {
            //if (Input.GetKeyDown(KeyCode.A))
            //{
            //    (string fresh, string longs) = await TokenManager.Instance.GetToken(9999, 3);
            //    LogUtils.Warn(fresh);
            //    LogUtils.Warn(longs);
            //    LogUtils.Warn("------------------------------------------------------------");
            //    (fresh, longs) = await TokenManager.Instance.UpdateToken(fresh, longs, 3);
            //    LogUtils.Warn(fresh);
            //    LogUtils.Warn(longs);
            //}
            if (GameApp.NetClient.Running == true)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        GameApp.SyncVarManager.GetVar("i", 10, (string id, object result) =>
                        {
                            LogUtils.Warn($"{id} ------------ {result}");
                        });
                    }
                    //(string test1, string test2) = await TokenManager.Instance.GetToken(6666, 3);
                    //LogUtils.Warn(test1);
                    //LogUtils.Warn(test2);
                    //(test1, test2) = await TokenManager.Instance.UpdateToken(test1, test2, 3);
                    //LogUtils.Warn(test1);
                    //LogUtils.Warn(test2);
                }
            }
        }
    }
}