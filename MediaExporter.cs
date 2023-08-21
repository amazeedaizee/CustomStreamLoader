using Cysharp.Threading.Tasks;
using HarmonyLib;
using ngov3;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CustomStreamLoader
{
    internal class MediaExporter
    {
        public static Dictionary<string, AssetBundle> LoadedAssetBundlePaths = new Dictionary<string, AssetBundle>();
        public static List<string> LoadedAddressables = new List<string>();
        public static List<string> AddressablePaths = new List<string>();
        public static List<string> CatalogPaths = new List<string>();
        public static Dictionary<string, AnimationClip> AnimClipLoader = new Dictionary<string, AnimationClip>();
        public static Sprite LoadImageFromFile(string path)
        {
            string fileName = Path.GetFileName(path);
            byte[] file = File.ReadAllBytes(path);

            Texture2D tex = new Texture2D(2, 2);
            ImageConversion.LoadImage(tex, file, false);
            tex.filterMode = FilterMode.Point;
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = fileName;
            return sprite;
        }

        public static async UniTask AddExternalCatalogs()
        {
            foreach (var bundle in AddressablePaths)
            {
                await AddAddressBundle(bundle);
            }
            foreach (var catalog in CatalogPaths)
            {
                await AddExternalCatalog(catalog);
            }
        }
        public static async UniTask AddExternalCatalog(string path)
        {
            try
            {
                AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(path, true);
                await UniTask.WaitUntil(() => handle.IsDone);
                Debug.Log("Catalog loaded.");
            }
            catch { }
        }

        public static async UniTask AddAddressBundle(string path)
        {
            string fileName = Path.GetFileName(path);
            string targetStrAssetPath = Path.Combine(Addressables.RuntimePath, "StandaloneWindows64", fileName);
            if (File.Exists(targetStrAssetPath))
                return;
            var bundleData = File.ReadAllBytes(path);
            using FileStream targetStream = File.Create(targetStrAssetPath);
            await targetStream.WriteAsync(bundleData, 0, bundleData.Length);
            LoadedAddressables.Add(targetStrAssetPath);
        }

        public static void LoadAssetBundles(string clipName, string path)
        {
            AssetBundle bundle;
            if (LoadedAssetBundlePaths.ContainsKey(path))
                bundle = LoadedAssetBundlePaths[path];
            else
            {
                bundle = AssetBundle.LoadFromFile(path);
                LoadedAssetBundlePaths.Add(path, bundle);
            }
            if (!AnimClipLoader.ContainsKey(clipName))
            {
                var clip = bundle.LoadAsset<AnimationClip>(clipName);
                AnimClipLoader.Add(clipName, clip);
            }

        }

        internal static void DeleteAddressBundlesFromPath()
        {
            if (LoadedAddressables.Count == 0) { return; }
            foreach (string bundlePath in LoadedAddressables)
            {
                File.Delete(bundlePath);
            }
        }

    }

    [HarmonyPatch]
    internal class AnimationPatcher
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LoadWebcamData), "LoadAnimation")]
        [HarmonyPatch(typeof(LoadLiveViewData), "LoadAnimation")]
        static async UniTask<AnimationClip> SetCustomAnim(UniTask<AnimationClip> value, string address)
        {
            string customId = address.Replace(".anim", "");
            if (MediaExporter.AnimClipLoader.Count == 0)
            {
                return await value;
            }
            try
            {
                AnimationClip customClip = MediaExporter.AnimClipLoader[customId];
                if (customClip == null || customClip.ToString() == "")
                {
                    return await value;
                }
                if (ThatOneLongList.list.Exists(x => x == customClip.name))
                {
                    return await value;
                }
                if (StreamLoader.customAssets.Exists(x => x.fileName == customClip.name && x.customAssetFileType == CustomAssetFileType.AddressableBundle))
                {
                    return await value;
                }
                return customClip;
            }
            catch
            {
            }
            return await value;
        }

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(LoadWebcamData), "LoadAnimation")]
        [HarmonyPatch(typeof(LoadLiveViewData), "LoadAnimation")]
        static Exception SuppressException(Exception __exception)
        {
            return null;
        }
    }
}
