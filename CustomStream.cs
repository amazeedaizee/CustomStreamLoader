using Cysharp.Threading.Tasks;
using NGO;
using ngov3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace CustomStreamLoader
{
    internal class CustomStream : LiveScenario
    {
        StreamSettings set = StreamLoader.customStreamSettings;
        public override void Awake()
        {
            base.Awake();
            title = set.StringTitle;
            playing.AddRange(StreamLoader.customPlayingList);

        }

        public override async UniTask StartScenario()
        {

            var music = set.StartingMusic;
            var effect = set.StartingEffect;
            var effectIntensity = set.EffectIntensity;
            if (set.ChatSettings == StreamChatSettings.Celebration)
                _Live.isOiwai = true;
            else if (set.ChatSettings == StreamChatSettings.Uncontrollable)
                _Live.isUncontrollable = true;
            AudioManager.Instance.PlayBgmByType(music, true);
            if (effect != EffectType.Kenjo)
            {
                PostEffectManager.Instance.SetShader(effect);
                PostEffectManager.Instance.SetShaderWeight(effectIntensity);
            }
            if (set.IsInvertedColors)
                GameObject.Find("InvertVolume").GetComponent<Volume>().enabled = true;
            if (set.isBordersOff)
                SingletonMonoBehaviour<EventManager>.Instance.ObiActive(false);
            await base.StartScenario();
            StreamLoader.hasStreamPlayed = true;
            _Live.HaishinClean();
            if (set.hasDarkInterface)
                SingletonMonoBehaviour<WindowManager>.Instance.CloseApp(AppType.LiveDark);
            SingletonMonoBehaviour<WindowManager>.Instance.CloseApp(AppType.TaskManager);
            GameObject.Find("InvertVolume").GetComponent<Volume>().enabled = false;
            SingletonMonoBehaviour<EventManager>.Instance.ObiActive(true);
            CrashReportHandler.enableCaptureExceptions = true;
            if (set.hasATweet || set.hasKTweet && !(string.IsNullOrWhiteSpace(set.kTweet) && string.IsNullOrWhiteSpace(set.aTweet)))
            {


                List<string> list = set.tweetReps.Trim().Split('\n').ToList();
                list.Reverse();
                SingletonMonoBehaviour<PoketterManager>.Instance.KUSOREPPROBABILITY = 0;
                if ((!string.IsNullOrWhiteSpace(set.kPic) && File.Exists(set.kPic)) || (!string.IsNullOrWhiteSpace(set.aPic) && File.Exists(set.aPic)))
                {
                 
                    SingletonMonoBehaviour<PoketterManager>.Instance.AddQueueWithKusoreps( (TweetType)10000, null, list);
                }
                else
                {
                    if (!(string.IsNullOrWhiteSpace(set.kTweet)))
                        SingletonMonoBehaviour<PoketterManager>.Instance.AddQueueWithKusoreps(set.kTweet, true, null, list);
                    if (!(string.IsNullOrWhiteSpace(set.aTweet)))
                        SingletonMonoBehaviour<PoketterManager>.Instance.AddQueueWithKusoreps(set.aTweet, false);
                }
                SingletonMonoBehaviour<WindowManager>.Instance.NewWindow(AppType.Result);
                SingletonMonoBehaviour<PoketterManager>.Instance.KUSOREPPROBABILITY = 50;
                await NgoEvent.DelaySkippable(25000);
            }
            
            var window = SingletonMonoBehaviour<WindowManager>.Instance.NewWindow(AppType.RebootDialog);
            window.Uncloseable();

        }
    }
}
