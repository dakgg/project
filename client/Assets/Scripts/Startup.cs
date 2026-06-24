using UnityEngine;
using ViewSystem;

public class Startup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeGameInstanceBeforeLoad()
    {
        var go = new GameObject("_ViewManager").AddComponent<ViewManager>();
        DontDestroyOnLoad(go);
    }
}
