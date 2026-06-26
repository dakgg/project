using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ViewSystem;

public class InitialScene : MonoBehaviour
{
    IEnumerator Start()
    {
        var initReq = Addressables.InitializeAsync();
        while (!initReq.IsDone)
        {
            yield return null;
        }
        ViewRequest.Open("InitialSceneRootPage@View", null, true);
    }
}
