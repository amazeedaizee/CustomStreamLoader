using Cysharp.Threading.Tasks;
using ngov3;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.Rendering;

namespace CustomStreamLoader
{
    internal class CustomStream : LiveScenario
    {
        public override void Awake()
        {
            base.Awake();
            title = StreamLoader.customStreamSettings.StringTitle;
            playing.AddRange(StreamLoader.customPlayingList);
        }

        public override async UniTask StartScenario()
        {

            var music = StreamLoader.customStreamSettings.StartingMusic;
            var effect = StreamLoader.customStreamSettings.StartingEffect;
            var effectIntensity = StreamLoader.customStreamSettings.EffectIntensity;
            if (StreamLoader.customStreamSettings.ChatSettings == StreamChatSettings.Celebration)
                _Live.isOiwai = true;
            else if (StreamLoader.customStreamSettings.ChatSettings == StreamChatSettings.Uncontrollable)
                _Live.isUncontrollable = true;
            AudioManager.Instance.PlayBgmByType(music, true);
            if (effect != EffectType.Kenjo)
            {
                PostEffectManager.Instance.SetShader(effect);
                PostEffectManager.Instance.SetShaderWeight(effectIntensity);
            }
            if (StreamLoader.customStreamSettings.IsInvertedColors)
                GameObject.Find("InvertVolume").GetComponent<Volume>().enabled = true;
            if (StreamLoader.customStreamSettings.isBordersOff)
                SingletonMonoBehaviour<EventManager>.Instance.ObiActive(false);
            await base.StartScenario();
            StreamLoader.hasStreamPlayed = true;
            _Live.HaishinClean();
            if (StreamLoader.customStreamSettings.hasDarkInterface)
                SingletonMonoBehaviour<WindowManager>.Instance.CloseApp(AppType.LiveDark);
            SingletonMonoBehaviour<WindowManager>.Instance.CloseApp(AppType.TaskManager);
            GameObject.Find("InvertVolume").GetComponent<Volume>().enabled = false;
            SingletonMonoBehaviour<EventManager>.Instance.ObiActive(true);
            CrashReportHandler.enableCaptureExceptions = true;
            var window = SingletonMonoBehaviour<WindowManager>.Instance.NewWindow(AppType.RebootDialog);
            window.Uncloseable();

        }
    }
}
