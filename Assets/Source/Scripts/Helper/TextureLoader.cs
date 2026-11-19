using HT.Framework;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LB
{
    static class TextureLoader
    {
        public static void Load(string url, Action<(string Url, Texture Texture)> onSuccess) =>
                  Loader.Instance.Load(url, onSuccess);
        public static void RemoveCache(string url) => Loader.Instance.RemoveCache(url);

        class Loader : HTBehaviour
        {
            //启用自动化
            protected override bool IsAutomate => true;

            readonly Dictionary<string, Texture> cache = new Dictionary<string, Texture>();

            static Loader instance;

            public static Loader Instance => instance ??
                (instance = FindObjectOfType<Loader>() ??
                    new GameObject(typeof(TextureLoader).Name).AddComponent<Loader>());

            public void Load(string url, Action<(string Url, Texture Texture)> onSuccess)
            {
                if (cache.TryGetValue(url, out var cachedTexture))
                {
                    onSuccess((url, cachedTexture));
                    return;
                }

                StartCoroutine(DownloadTexture(url, result =>
                {
                    cache[result.Url] = result.Texture;
                    onSuccess(result);
                }));
            }

            IEnumerator DownloadTexture(string url, Action<(string Url, Texture Texture)> onSuccess)
            {

#if UNITY_EDITOR||UNITY_STANDALONE
                using (UnityWebRequest request =
                    UnityWebRequestTexture.GetTexture(url)
                    )
#elif UNITY_ANDROID              
                using (UnityWebRequest request =
                UnityWebRequestTexture.GetTexture("jar:file://" + url)
                    )
#endif
                // using (var request = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogErrorFormat("Error: {0}", request.error);
                        yield break;
                    }

                    onSuccess((
                        url,
                        DownloadHandlerTexture.GetContent(request)
                    // ((DownloadHandlerTexture) request.downloadHandler).texture
                    ));
                }
            }
            public void RemoveCache(string url)
            {
                if (cache.ContainsKey(url))
                {
                    cache.Remove(url);
                }
            }
            protected override void OnDestroy()
            {
                foreach (var kv in cache)
                {
                    Destroy(kv.Value);
                }

                instance = null;
            }
        }
    }
}
