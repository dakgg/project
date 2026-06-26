using UnityEngine.UI;
using ViewSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[ViewLoad("InitialSceneRootPage@View")]
public class InitialSceneRootPage : PageView
{
    public Button StartButton;

    void Awake()
    {
        StartButton.onClick.AddListener(haha);
    }

    public async void haha()
    {
        UnityEngine.Debug.Log("[InitialScene] haha clicked");
        StartButton.interactable = false;

        // LoadSceneAsync 는 로드 + 활성화(전환)까지 자동으로 수행한다.
        // await 가 끝나는 순간 이미 LobbyScene 으로 전환된 상태.
        var handle = Addressables.LoadSceneAsync("LobbyScene@Scene");
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            UnityEngine.Debug.LogError($"Scene load failed: {handle.OperationException}");
            StartButton.interactable = true;
        }
    }
}