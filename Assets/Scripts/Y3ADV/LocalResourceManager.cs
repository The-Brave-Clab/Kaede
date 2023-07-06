using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DiffPatch;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Y3ADV
{
    public static class LocalResourceManager
    {
        static LocalResourceManager()
        {
            loadedMap = new Dictionary<string, Dictionary<string, LoadedData>>
            {
                [""] = new ()
            };

            LoadLocalDataVersion();
        }

        private static string LocalResourceFileName => $"{Application.persistentDataPath}/local_resource.json";
        private static LocalDataVersionList localData;

        private static void LoadLocalDataVersion()
        {
            if (!File.Exists(LocalResourceFileName))
            {
                localData = new LocalDataVersionList
                {
                    results = new List<LocalDataVersion>()
                };
                SaveLocalDataVersion();
            }
            else
            {
                localData = JsonUtility.FromJson<LocalDataVersionList>(File.ReadAllText(LocalResourceFileName));
            }
        }

        private static void SaveLocalDataVersion()
        {
            File.WriteAllText(LocalResourceFileName, JsonUtility.ToJson(localData));
        }

        private static bool CheckLocalData(string key, string etag)
        {
            return localData.results.Any(v => v.key == key && v.etag == etag);
        }

        private static void SaveLocalData(string key, string etag, byte[] content)
        {
            if (CheckLocalData(key, etag)) return;

            localData.results.Add(new LocalDataVersion
            {
                key = key,
                etag = etag
            });
            
            SaveLocalDataVersion();

            string filename = Path.Combine(Application.persistentDataPath, key);
            Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
            
            File.WriteAllBytes(filename, content);
        }

        public delegate T WebRequestToObjectCallback<out T>(UnityWebRequest www) where T : Object;

        public delegate void ProcessObjectCallback<in T>(T obj) where T : Object;

        public delegate void ProcessBytesCallback(byte[] bytes);

        public delegate void Callback();
        private struct LoadedData // option between unity asset and plain bytes
        {
            public Object Asset;
            public byte[] Bytes;

            public bool Valid(bool isAsset)
            {
                return isAsset ? Asset != null : Bytes != null;
            }
        }

        private static Dictionary<string, Dictionary<string, LoadedData>> loadedMap;

        private static Dictionary<string, LoadedData> GetOrInitializeScriptLoadedAsset(string scriptName)
        {
            if (loadedMap.ContainsKey(scriptName))
            {
                return loadedMap[scriptName];
            }

            loadedMap[scriptName] = new Dictionary<string, LoadedData>();
            return loadedMap[scriptName];
        }

        public static void RemoveScriptLoadedAsset(string scriptName)
        {
            loadedMap.Remove(scriptName);
        }

        // Basic load asset from WebRequest function
        // Handles Unity Assets, byte array, text (as TextAsset)
        private static IEnumerator LoadFromFile<T>(bool isAsset, bool isSound,
            string[] paths, AWSSettingsAsset.Bucket bucket, 
            bool cacheAsLocalFile,
            WebRequestToObjectCallback<T> wrcb, ProcessObjectCallback<T> ocb, ProcessBytesCallback bcb)
            where T : Object
        {
            Dictionary<string, LoadedData> currentlyLoadedAssets =
                GetOrInitializeScriptLoadedAsset(GameManager.ScriptName);

            string assetKey = paths[0];

            if (currentlyLoadedAssets.ContainsKey(assetKey) && currentlyLoadedAssets[assetKey].Valid(isAsset))
            {
                if (isAsset)
                    ocb?.Invoke((T) currentlyLoadedAssets[assetKey].Asset);
                else
                    bcb?.Invoke(currentlyLoadedAssets[assetKey].Bytes);
                yield break;
            }

            UnityWebRequest successfulRequest = null;

            if (StartupSettings.TestMode)
                cacheAsLocalFile = false;

            foreach (var path in paths)
            {
                int retryCount = 5;
                var url = GameManager.GetObjectURL(bucket, path);
                do
                {
#if !WEBGL_BUILD
                    string etag = "";

                    if (cacheAsLocalFile)
                    {
                        var checkMetadataTask = GameManager.GetObjectMetadata(bucket, path);
                        yield return new WaitUntil(() => checkMetadataTask.IsCompleted);
                        var metadata = checkMetadataTask.Result;
                        if (metadata.HttpStatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
                            break;
                        if (metadata.HttpStatusCode != HttpStatusCode.OK)
                        {
                            --retryCount;
                            continue;
                        }

                        etag = metadata.ETag.Trim('"');

                        if (CheckLocalData(path, etag))
                            url = $"file://{Application.persistentDataPath}/{path}";
                    }
#endif

                    var webRequest = UnityWebRequest.Get(url);
                    if (isSound)
                        webRequest.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.WAV);
                    
                    webRequest.SetRequestHeader("referer", "https://scenario.yuyuyui.org");


                    string displayName = Path.GetFileNameWithoutExtension(path);
                    GameManager.RegisterLoading(displayName, () => webRequest.downloadProgress);
                    yield return webRequest.SendWebRequest();
                    GameManager.UnregisterLoading(displayName);

                    if (webRequest.result == UnityWebRequest.Result.Success && webRequest.responseCode == 200)
                    {
                        successfulRequest = webRequest;

                        Profiler.BeginSample("Invoke Callback");
                        if (isAsset)
                        {
                            if (wrcb != null)
                            {
                                Object loadedAsset = wrcb.Invoke(successfulRequest);
                                currentlyLoadedAssets[assetKey] = new LoadedData
                                {
                                    Asset = loadedAsset,
                                    Bytes = null
                                };
                                ocb?.Invoke((T) loadedAsset);
                            }
                        }
                        else
                        {
                            byte[] loadedBytes = successfulRequest.downloadHandler.data;
                            currentlyLoadedAssets[assetKey] = new LoadedData
                            {
                                Asset = null,
                                Bytes = loadedBytes
                            };
                            bcb?.Invoke(loadedBytes);
                        }
                        Profiler.EndSample();

#if !WEBGL_BUILD
                        if (cacheAsLocalFile)
                        {
                            Profiler.BeginSample("Save to local file");
                            SaveLocalData(path, etag, successfulRequest.downloadHandler.data);
                            Profiler.EndSample();
                        }
#endif

                        yield break;
                    }

                    if (webRequest.responseCode is 404 or 403)
                    {
                        currentlyLoadedAssets[assetKey] = new LoadedData
                        {
                            Asset = null,
                            Bytes = null
                        };
                        break;
                    }

                    --retryCount;
                } while (retryCount > 0);
            }

            yield return null;
        }

        private static IEnumerator LoadAssetFromFile<T>(bool isSound, string[] paths, AWSSettingsAsset.Bucket bucket,
            bool cacheAsLocalFile,
            WebRequestToObjectCallback<T> wrcb = null, ProcessObjectCallback<T> ocb = null)
            where T : Object
        {
            var realPaths = isSound ? 
                paths.Select(p => $"sound/{p.Trim('/')}").ToArray() : 
                paths.Select(p => $"assets/ios/_yuyuyuassetbundles/resources/{p.Trim('/')}").ToArray();
            yield return LoadFromFile(true, isSound, realPaths, bucket, cacheAsLocalFile, wrcb, ocb, null);
        }

        public static IEnumerator LoadTexture2DFromFile(string[] paths,
            ProcessObjectCallback<Texture2D> cb = null)
        {
            yield return LoadAssetFromFile(false, paths, GameManager.AWSSettings.extractedBucket, true,
                www =>
                {
                    if (www == null) return null;

                    Texture2D loadedTexture = new Texture2D(2, 2, GraphicsFormat.R8G8B8A8_SRGB, 0, TextureCreationFlags.None);
                    loadedTexture.LoadImage(www.downloadHandler.data);
                    loadedTexture.filterMode = FilterMode.Trilinear;
                    return loadedTexture;
                }, cb);
        }

        public static IEnumerator LoadAssetBundleFromFile(string path,
            ProcessObjectCallback<AssetBundle> cb = null)
        {
            yield return LoadFromFile(true, false,
                new[] {path},
                GameManager.AWSSettings.assetBundleBucket, true,
                www =>
                {
                    if (www == null) return null;

                    return AssetBundle.LoadFromMemory(www.downloadHandler.data);
                }, cb, null);
        }

        public static IEnumerator LoadTextFromFile(string[] paths, bool cacheAsFile,
            ProcessObjectCallback<TextAsset> cb = null)
        {
            yield return LoadAssetFromFile(false, paths, GameManager.AWSSettings.extractedBucket, cacheAsFile,
                www =>
                {
                    if (www == null) return null;

                    return new TextAsset(www.downloadHandler.text);
                }, cb);
        }

        public static IEnumerator LoadTextAndPatchFromFile(string[] paths, bool cacheAsFile,
            ProcessObjectCallback<TextAsset> cb = null)
        {
            TextAsset loadedText = null;
            yield return LoadTextFromFile(paths, cacheAsFile, asset => loadedText = asset);

            if (loadedText == null) yield break;
            bool patchAvailable = false;
            yield return LoadFromFile(true, false, paths, GameManager.AWSSettings.patchBucket, false,
                www =>
                {
                    if (www == null) return null;
                    return new TextAsset(www.downloadHandler.text);
                },
                asset =>
                {
                    try
                    {
                        string patch = asset.text;
                        string loaded = loadedText.text;

                        var diffs = DiffParserHelper.Parse(patch);
                        foreach (var diff in diffs)
                        {
                            loaded = PatchHelper.Patch(loaded, diff.Chunks, "\n");
                        }

                        TextAsset patchedText = new TextAsset(loaded);
                        patchAvailable = true;
                        Debug.Log($"Patch for {paths[0]} found and applied!\n{patch}");
                        cb?.Invoke(patchedText); // Potential bug here: asset returned by wrcb is not the same
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }, null);
            if (!patchAvailable)
            {
                cb?.Invoke(loadedText);
            }
        }

        public static IEnumerator LoadTranslation(string scenarioName, string languageCode,
            ProcessObjectCallback<TextAsset> cb = null)
        {
            if (StartupSettings.OverrideTranslation)
            {
                var filePath = StartupSettings.OverrideTranslationFile;
                if (File.Exists(filePath))
                {
                    string jsonContent = File.ReadAllText(filePath);
                    TextAsset jsonTextAsset = new TextAsset(jsonContent);

                    cb?.Invoke(jsonTextAsset);
                    yield break;
                }
            }

            yield return LoadFromFile(true, false,
            new []{$"{languageCode}/{scenarioName}/{scenarioName}.json"}, GameManager.AWSSettings.translationBucket,
            false,
            www => www == null ? null : new TextAsset(www.downloadHandler.text),
            cb, null);
        }

        public static IEnumerator LoadAudioClipFromFile(string[] paths, bool cacheAsLocalFile, bool analyzeLoopInfo,
            ProcessObjectCallback<AudioClip> cb = null)
        {
            yield return LoadAssetFromFile(true, paths, GameManager.AWSSettings.extractedBucket, cacheAsLocalFile,
                request =>
                {
                    AudioClip newClip = DownloadHandlerAudioClip.GetContent(request);

#if WEBGL_BUILD
                    if (analyzeLoopInfo)
                    {
                        byte[] binaryData = request.downloadHandler.data;
                        SoundManager.AnalyzeLoopInfo(newClip, binaryData);
                    }
#endif

                    return newClip;
                },
                cb);
        }

        public static IEnumerator LoadBytesFromFile(string[] paths,
            ProcessBytesCallback cb = null)
        {
            var realPaths = 
                paths.Select(p => $"assets/ios/_yuyuyuassetbundles/resources/{p.Trim('/')}").ToArray();
            yield return LoadFromFile<TextAsset>(false, false, realPaths,
                GameManager.AWSSettings.extractedBucket, true,
                null, null, cb);
        }

        public static IEnumerator DownloadToFile(Uri url, string fileName, Callback cb = null)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            string savePath = $"{Application.persistentDataPath}/{fileName}";
            File.WriteAllBytes(savePath, www.downloadHandler.data);

            cb?.Invoke();
        }
        
        
        
        public delegate bool IsInterestedData(LocalDataDownload data);
        public static IEnumerator DownloadLocalResource(string url, 
            string dataVersionFile, IsInterestedData isInterestedData)
        {
            Debug.Log("Updating required local resources...");

            string localDataVersionFile = Path.Combine(Application.persistentDataPath, dataVersionFile);

            // Get remote data
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            string responseStr = www.downloadHandler.text;

            LocalDataResult localDataResult = JsonUtility.FromJson<LocalDataResult>(responseStr);

            // Get local data
            IEnumerable<LocalDataDownload> needUpdate;

            if (File.Exists(localDataVersionFile))
            {
                // If we already have a local version file,
                // compare it with the remote result and 
                // update the ones with different etag

                string versionContent = File.ReadAllText(localDataVersionFile);
                LocalDataVersionList versionList = JsonUtility.FromJson<LocalDataVersionList>(versionContent)!;

                needUpdate =
                    from version in versionList.results
                    from download in localDataResult.results
                    where version.key == download.key && version.etag != download.etag
                    select download;
            }
            else
            {
                // If we don't have a local version file,
                // we need to download all
                needUpdate = localDataResult.results;
            }

            int count = 0;
            foreach (var data in needUpdate)
            {
                // We are only interested in these two databases
                if (!isInterestedData(data))
                    continue;

                // Download each file
                string databaseFilename = Path.GetFileName(data.key.Replace('/', Path.DirectorySeparatorChar));

                var fileUrl = data.url;

                Debug.Log($"Downloading {databaseFilename}...");

                yield return DownloadToFile(new Uri(fileUrl), databaseFilename);
                ++count;
            }

            if (count == 0)
            {
                Debug.Log("All data files are up to date.");
            }
            else
            {
                Debug.Log($"Updated {count} file(s).");

                // Save the new version list to local storage
                LocalDataVersionList newVersionList = new LocalDataVersionList
                {
                    results = localDataResult.results.Select(
                        r => new LocalDataVersion
                        {
                            etag = r.etag,
                            key = r.key
                        }).ToList()
                };

                File.WriteAllText(localDataVersionFile, JsonUtility.ToJson(newVersionList));
            }
        }
        
        
        
        
        
        

        [Serializable]
        public class LocalDataResult
        {
            public List<LocalDataDownload> results = new();
        }
        
        [Serializable]
        public class LocalDataDownload
        {
            public string key = "";
            public string etag = "";
            public long size;
            public string url = "";
        }

        [Serializable]
        public class LocalDataVersionList
        {
            public List<LocalDataVersion> results = new();
        }

        [Serializable]
        public class LocalDataVersion
        {
            public string key = "";
            public string etag = "";
        }
    }
}