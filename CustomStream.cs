using Cysharp.Threading.Tasks;
using NGO;
using ngov3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.Rendering;

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
            if (set.hasATweet || set.hasKTweet)
            {
                var day = SingletonMonoBehaviour<StatusManager>.Instance.GetStatus(StatusType.DayIndex);
                for (var i = 0; i < day; i++)
                {
                    const int COMMAND_LEN = 53;
                    HashSet<int> hashSet = new HashSet<int>();
                    int num;
                    do
                    {
                        num = UnityEngine.Random.Range(0, COMMAND_LEN);
                        if (!hashSet.Contains(num))
                        {
                            hashSet.Add(num);
                            var twt = TweetFetcher.CommandTweet((CommandType)num, CommandResult.success);
                            SingletonMonoBehaviour<PoketterManager>.Instance.AddQueueWithKusoreps(twt);
                        }
                    } while (hashSet.Contains(num));
                }
                SingletonMonoBehaviour<PoketterManager>.Instance.AddHistoryFromQueueAll();
            }
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
            if (set.hasATweet || set.hasKTweet)
            {
                SingletonMonoBehaviour<PoketterManager>.Instance.AddQueueWithKusoreps(set.kTweet, true, null, set.tweetReps);
                SingletonMonoBehaviour<PoketterManager>.Instance.AddQueueWithKusoreps(set.aTweet, false);
                await NgoEvent.DelaySkippable(10000);
            }
            var window = SingletonMonoBehaviour<WindowManager>.Instance.NewWindow(AppType.RebootDialog);
            window.Uncloseable();

        }
    }
}
