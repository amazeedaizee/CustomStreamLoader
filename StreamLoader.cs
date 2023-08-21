using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ngov3;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomStreamLoader
{

    internal class StreamLoader
    {
        internal static List<CustomAsset> customAssets = new List<CustomAsset>();

        internal static List<Playing> customPlayingList;
        internal static StreamSettings customStreamSettings;
        internal static bool hasStreamPlayed;

        internal static async UniTask GetCustomStream()
        {
            KAngelSays dummyAnim;
            bool addDummyAnim = false;
            customPlayingList = new List<Playing>();
            List<string> files = Directory.GetFiles(Path.GetDirectoryName(Initializer.PInfo.Location)).ToList();
            string jsonFile = File.ReadAllText(files.FirstOrDefault(f => f.EndsWith(".json")));
            string newJson = jsonFile.Replace("CustomStreamMaker", "CustomStreamLoader");
            var jsonSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
            customStreamSettings = JsonConvert.DeserializeObject<StreamSettings>(newJson, jsonSettings);
            InitializeCustomAnim(customStreamSettings.CustomStartingAnimation);
            dummyAnim = new KAngelSays(customStreamSettings.StartingAnimation, "");
            for (int i = 0; i < customStreamSettings.PlayingList.Count; i++)
            {
                var playingObj = customStreamSettings.PlayingList[i];
                if (i == 0 && (playingObj.PlayingType == PlayingType.KAngelSays) && string.IsNullOrEmpty((playingObj as KAngelSays).AnimName))
                    (playingObj as KAngelSays).AnimName = customStreamSettings.StartingAnimation;
                else if (i == 0 && playingObj.PlayingType == PlayingType.KAngelSays && (playingObj as KAngelSays).AnimName != customStreamSettings.StartingAnimation)
                    customStreamSettings.StartingAnimation = (playingObj as KAngelSays).AnimName;
                else if (i == 0 && !(playingObj.PlayingType == PlayingType.KAngelSays))
                {
                    addDummyAnim = true;
                }
                AddPlayingObjsToPlayingList(playingObj);
            }
            if (addDummyAnim)
            {
                customPlayingList.InsertRange(0, ConvertToKAngelDialogue(dummyAnim));
            }
            await MediaExporter.AddExternalCatalogs();
        }

        internal static void InitializeCustomAnim(CustomAsset customAsset)
        {
            if (customAsset == null)
                return;
            if (customAssets.Exists(a => a.fileName == customAsset.fileName && a.filePath == customAsset.filePath && a.customAssetFileType == customAsset.customAssetFileType))
                return;
            switch (customAsset.customAssetFileType)
            {
                case CustomAssetFileType.AssetBundle:
                    MediaExporter.LoadAssetBundles(customAsset.fileName, customAsset.filePath);
                    break;
                case CustomAssetFileType.AddressableBundle:
                    if (!string.IsNullOrEmpty(customAsset.catalogPath) && !MediaExporter.CatalogPaths.Contains(customAsset.catalogPath))
                        MediaExporter.CatalogPaths.Add(customAsset.catalogPath);
                    if (MediaExporter.AddressablePaths.Contains(customAsset.filePath))
                        break;
                    MediaExporter.AddressablePaths.Add(customAsset.filePath);
                    break;
            }
            customAssets.Add(customAsset);
        }

        internal static void AddPlayingObjsToPlayingList(PlayingObject obj)
        {
            switch (obj.PlayingType)
            {
                case PlayingType.KAngelSays:
                case PlayingType.KAngelCallout:
                    customPlayingList.AddRange(ConvertToKAngelDialogue(obj as KAngelSays));
                    break;
                case PlayingType.ChatSays:
                case PlayingType.ChatSuper:
                case PlayingType.ChatBad:
                    customPlayingList.Add(ConvertToChatComment(obj as ChatSays));
                    break;
                case PlayingType.PlaySE:
                case PlayingType.PlayBGM:
                    customPlayingList.Add(ConvertToAudio(obj as PlaySound));
                    break;
                case PlayingType.PlayEffect:
                    customPlayingList.Add(ConvertToChanceEffect(obj as PlayEffect));
                    break;
                default:
                    customPlayingList.Add(ConvertToOtherPlayingObj(obj as ChatGeneral));
                    break;
            }
        }

        internal static List<Playing> ConvertToKAngelDialogue(KAngelSays kAngelSays)
        {
            var kList = new List<Playing>();
            InitializeCustomAnim(kAngelSays.customAnim);
            if (kAngelSays.PlayingType == PlayingType.KAngelCallout)
            {
                var kCallout = kAngelSays as KAngelCallout;
                var hatCom = new Playing(true, "", StatusType.Tension, 1, 0, "", "", "", true, SuperchatType.White, true, kCallout.HaterComment);
                kList.Add(hatCom);
            }
            var kChat = new Playing(true, kAngelSays.Dialogue, StatusType.Tension, 1, 0, "", "", kAngelSays.AnimName);
            kList.Add(kChat);
            return kList;
        }

        internal static Playing ConvertToChatComment(ChatSays chatSays)
        {
            if (customStreamSettings.ChatSettings != StreamChatSettings.Normal || chatSays.PlayingType == PlayingType.ChatSays)
                return new Playing(false, chatSays.Comment);
            if (chatSays.PlayingType == PlayingType.ChatBad)
                return new Playing(false, true, chatSays.Comment);
            if (chatSays.PlayingType == PlayingType.ChatSuper)
            {
                var kAnimList = new List<string>();
                var kAngelReplies = new List<string>();
                for (int i = 0; i < chatSays.Replies.Count; i++)
                {
                    InitializeCustomAnim(chatSays.Replies[i].customAnim);
                    kAnimList.Add(chatSays.Replies[i].AnimName);
                    kAngelReplies.Add(chatSays.Replies[i].Dialogue);
                }
                var kAnims = string.Join("___", kAnimList);
                var kReplies = string.Join("___", kAngelReplies);
                return new Playing(false, chatSays.Comment, StatusType.Tension, 0, 10, kReplies, kAnims, "", false);
            }
            return new Playing(false, chatSays.Comment);
        }

        internal static Playing ConvertToAudio(PlaySound sound)
        {
            bool isLoop = sound.PlayingType == PlayingType.PlaySE ? false : true;
            return new Playing(sound.Audio, isLoop);
        }

        internal static Playing ConvertToChanceEffect(PlayEffect effect)
        {
            string effectTransition = "";
            switch (effect.BorderEffectType)
            {
                case BorderEffectType.EaseIn:
                    if (effect.BorderEffect == ChanceEffectType.Ide)
                        effectTransition = "Ide_in";
                    else effectTransition = "in";
                    break;
                case BorderEffectType.EaseOut:
                    if (effect.BorderEffect == ChanceEffectType.Ide)
                        effectTransition = "ide_invoke";
                    else effectTransition = "out";
                    break;
                case BorderEffectType.EaseBeforePlay:
                    if (effect.BorderEffect == ChanceEffectType.Ide)
                        effectTransition = "Ide2";
                    else effectTransition = "win_stop";
                    break;
                case BorderEffectType.Play:
                    if (effect.BorderEffect == ChanceEffectType.Ide)
                        effectTransition = "Ide2_loop";
                    else effectTransition = "win";
                    break;
            }
            return new Playing(effect.BorderEffect, effectTransition);
        }

        internal static Playing ConvertToOtherPlayingObj(ChatGeneral general)
        {
            switch (general.PlayingType)
            {
                case PlayingType.ChatFirst:
                    return new Playing("first");
                case PlayingType.ChatMiddle:
                    return new Playing("middle");
                case PlayingType.ChatLast:
                    return new Playing("last");
                case PlayingType.ChatDelete:
                    return new Playing("delete");
                case PlayingType.ChatDeleteAll:
                    return new Playing("deleteAll");
                case PlayingType.ReadSuperChats:
                    return new Playing(true);
                default:
                    return new Playing("rainbow");
            }
        }
    }
}
