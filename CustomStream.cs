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
                SingletonMonoBehaviour<PoketterManager>.Instance.history.Clear();
                SingletonMonoBehaviour<PoketterManager>.Instance._tweetQueue.Clear();
                Debug.Log(SingletonMonoBehaviour<PoketterView2D>.Instance._tweetCells.Count);
                foreach (var cell in SingletonMonoBehaviour<PoketterView2D>.Instance._tweetCells)
                {
                    Destroy(cell.gameObject);
                  
                }
                SingletonMonoBehaviour<PoketterView2D>.Instance._tweetCells.Clear();
                SingletonMonoBehaviour<PoketterManager>.Instance.AddHistory(new TweetData(TweetType.AfterTweet_Zatudan1, true, 1000, 1, CmdType.Zatudan_1));
                SingletonMonoBehaviour<PoketterManager>.Instance.AddHistory(new TweetData(TweetType.AfterTweet_Zatudan1, false, 1000, 1, CmdType.Zatudan_1));
                //var day = SingletonMonoBehaviour<StatusManager>.Instance.GetStatus(StatusType.DayIndex);
                //for (var i = 0; i < day; i++)
                //{
                //    const int COMMAND_LEN = 53;
                //    HashSet<int> hashSet = new HashSet<int>();
                //    int num;
                //    doSingletonMonoBehaviour<PoketterView2D>.Instance._tweetCells
                //    {
                //        num = UnityEngine.Random.Range(0, COMMAND_LEN);
                //        if (!hashSet.Contains(num))
                //        {
                //            hashSet.Add(num);
                //            var twt = TweetFetcher.CommandTweet((CommandType)num, CommandResult.success);
                //            SingletonMonoBehaviour<PoketterManager>.Instance.AddQueueWithKusoreps(twt);
                //        }
                //    } while (hashSet.Contains(num));
                //}

                List<string> list = set.tweetReps.Trim().Split('\n').ToList();
               
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
                await NgoEvent.DelaySkippable(25000);
            }
            var window = SingletonMonoBehaviour<WindowManager>.Instance.NewWindow(AppType.RebootDialog);
            window.Uncloseable();

        }
    }
}
