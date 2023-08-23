using Cysharp.Threading.Tasks;
using DG.Tweening;
using HarmonyLib;
using NGO;
using ngov3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;

namespace CustomStreamLoader
{

    [HarmonyPatch]
    internal class ReversePatches
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(LiveScenario), nameof(LiveScenario.Awake))]
        internal static void Awake_Stub(LiveScenario instance)
        {
            throw new NotImplementedException("This has not been successfully patched.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(LiveScenario), nameof(LiveScenario.StartScenario))]
        internal static async UniTask StartScenario_Stub(LiveScenario instance)
        {
            throw new NotImplementedException("This has not been successfully patched.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NgoEvent), nameof(NgoEvent.startEvent), new Type[] { typeof(CancellationToken) })]
        internal static async UniTask startEvent_Stub(NgoEvent instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("This has not been successfully patched.");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NgoEvent), nameof(NgoEvent.endEvent))]
        internal static void endEvent_Stub(NgoEvent instance)
        {
            throw new NotImplementedException("This has not been successfully patched.");
        }
    }

    [HarmonyPatch]
    internal class EventPatcher
    {
        internal static Scenario_loop1_day0_night loginInstance;

        // this one has to be patched manually, patching Live.SetScenario()
        internal static bool AwaitCustomStream(Live __instance, ref LiveScenario __result)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber == 5)
            {
                __result = __instance.SetScenario<CustomStream>();
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EventManager), nameof(EventManager.FetchDayEvent))]
        internal static bool OverrideDayEvent()
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber == 5)
                return false;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Event_CheckBGM), nameof(Event_CheckBGM.startEvent), new Type[] { typeof(CancellationToken) })]
        internal static bool OverrideSetMusic(Event_CheckBGM __instance, CancellationToken cancellationToken = default)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber == 5)
            {
                ReversePatches.startEvent_Stub(__instance, cancellationToken);
                ReversePatches.endEvent_Stub(__instance);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Scenario_loop1_day0_night), nameof(Scenario_loop1_day0_night.startEvent), new Type[] { typeof(CancellationToken) })]
        internal static bool OverrideLogin(Scenario_loop1_day0_night __instance, CancellationToken cancellationToken = default)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber != 5)
                return true;
            PlayCustomStream(__instance, cancellationToken);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EventManager), nameof(EventManager.Save))]
        internal static bool DisableSaveForCustomStream()
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber == 5)
                return false;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EventManager), nameof(EventManager.ObiActive))]
        internal static void DisableDisablingBordersIfChance(ref bool onoff)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber == 5 && !StreamLoader.customStreamSettings.isBordersOff)
                onoff = true;
        }

        internal static async UniTask PlayCustomStream(Scenario_loop1_day0_night __instance, CancellationToken cancellationToken = default)
        {
            ReversePatches.startEvent_Stub(__instance, cancellationToken);
            loginInstance = __instance;
            if (StreamLoader.customStreamSettings.HasCustomDay)
                SingletonMonoBehaviour<StatusManager>.Instance.UpdateStatusToNumber(StatusType.DayIndex, StreamLoader.customStreamSettings.CustomDay);
            else SingletonMonoBehaviour<StatusManager>.Instance.UpdateStatusToNumber(StatusType.DayIndex, 15);
            SingletonMonoBehaviour<StatusManager>.Instance.UpdateStatusToNumber(StatusType.Stress, 50);
            SingletonMonoBehaviour<StatusManager>.Instance.UpdateStatusToNumber(StatusType.Love, 50);
            SingletonMonoBehaviour<StatusManager>.Instance.UpdateStatusToNumber(StatusType.Yami, 50);
            if (!StreamLoader.hasStreamPlayed)
            {
                if (StreamLoader.customStreamSettings.HasCustomFollowerCount)
                    SingletonMonoBehaviour<StatusManager>.Instance.UpdateStatusToNumber(StatusType.Follower, StreamLoader.customStreamSettings.CustomFollowerCount);
                else SingletonMonoBehaviour<StatusManager>.Instance.UpdateStatusToNumber(StatusType.Follower, 10000);
                await UniTask.Delay(2700);
            }
            SingletonMonoBehaviour<EventManager>.Instance.SetShortcutState(false, 0.2f);
            SingletonMonoBehaviour<TaskbarManager>.Instance.SetTaskbarInteractive(false);
            if (StreamLoader.customStreamSettings.IsIntroPlaying)
            {
                SingletonMonoBehaviour<EventManager>.Instance.AddEvent<Action_HaishinStart>();
            }
            else
            {
                HaishinFirstAnimation.LoadHaishinFirstAnimation().Forget();
                await UniTask.Delay(300);
                SingletonMonoBehaviour<WindowManager>.Instance.CleanAll();
                SingletonMonoBehaviour<WindowManager>.Instance.NewWindow(AppType.Broadcast);
                SingletonMonoBehaviour<WindowManager>.Instance.Uncloseable(AppType.Broadcast);
                SingletonMonoBehaviour<WindowManager>.Instance.UnMovable(AppType.Broadcast);
            }
            ReversePatches.endEvent_Stub(__instance);
        }
    }

    [HarmonyPatch]
    internal class StreamPatcher
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RebootDialog), nameof(RebootDialog.Close))]
        internal static void PlayStreamAgain()
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber == 5)
                EventPatcher.PlayCustomStream(EventPatcher.loginInstance);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Boot), nameof(Boot.waitAccept))]
        internal static void CloseCustomStream()
        {
            StreamLoader.hasStreamPlayed = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HaishinFirstAnimation), "GetEndingHaishinFirstAnimationKey")]
        [HarmonyPatch(typeof(HaishinFirstAnimation), "GetNormalHaishinFirstAnimationKey")]
        static void GetEndingFirstAnim(ref string __result)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber == 5)
                __result = StreamLoader.customStreamSettings.StartingAnimation;

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Live), "getReadAnimationFromHaishin")]
        static bool SetReadAnim(ref string __result)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber == 5)
            {
                __result = StreamLoader.customStreamSettings.ReactionAnimation;
                return false;
            }
            return true;

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TenchanView), "Awake")]
        static void ChangeChairVisibility(TenchanView __instance)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber != 5)
                return;
            var isValidBG = (int)StreamLoader.customStreamSettings.StartingBackground < 8;
            if (isValidBG && !StreamLoader.customStreamSettings.HasChair)
                __instance._noChair.SetActive(true);
            else __instance._noChair.SetActive(false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Live), "Awake")]
        static void SetCustomWatching(Live __instance, ref int ___watcher)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber != 5)
                return;
            int watchingNum = StreamLoader.customStreamSettings.CustomFollowerCount;
            if (!StreamLoader.customStreamSettings.HasCustomFollowerCount)
                return;
            if (watchingNum < 192)
            {
                return;
            }
            if (watchingNum > 10000000)
            {
                Debug.LogError("Watching number can't be higher than 10 million. Setting number to ten million.");
                ___watcher = 10000000;
                __instance.UpdateDetail();
                return;
            }
            ___watcher = (int)watchingNum;
            __instance.UpdateDetail();
        }

        static Sprite ChangeStreamBackground(StreamBackground bg)
        {
            TenchanView view = SingletonMonoBehaviour<TenchanView>.Instance;
            Dictionary<StreamBackground, Sprite> spriteBg = new Dictionary<StreamBackground, Sprite>() {
                { StreamBackground.Default, view.background_no_shield },
                { StreamBackground.Silver, view.background_silver_shield },
                {StreamBackground.Gold, view.background_gold_shield },
                {StreamBackground.MileOne, view.background_kinen1 },
                {StreamBackground.MileTwo, view.background_kinen2 },
                {StreamBackground.MileThree, view.background_kinen3 },
                {StreamBackground.MileFour, view.background_kinen4 },
                {StreamBackground.MileFive, view.background_kinen5 },
                {StreamBackground.Guru, view._background_kyouso },
                {StreamBackground.Horror, view._background_horror },
                {StreamBackground.BigHouse, view._background_happy },
                {StreamBackground.Roof, view._background_sayonara1 }

            };
            if (StreamLoader.customStreamSettings.CustomBackground != null)
            {
                return MediaExporter.LoadImageFromFile(StreamLoader.customStreamSettings.CustomBackground.filePath);
            }
            if (bg == StreamBackground.Black || bg == StreamBackground.Void || bg == StreamBackground.None)
            {
                return null;
            }
            return spriteBg[bg];
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Live), "Awake")]
        static void SetCustomBG(Live __instance)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber != 5)
                return;
            var streamBG = StreamLoader.customStreamSettings.StartingBackground;
            if (streamBG == StreamBackground.Black)
            {
                __instance.Tenchan._backGround.color = new Color(0f, 0f, 0f, 1f);
                return;
            }
            else if (streamBG == StreamBackground.Void)
            {
                __instance.Tenchan._backGround.color = new Color(0f, 0f, 0f, 0f);
                return;
            }
            else
            {
                __instance.Tenchan._backGround.sprite = ChangeStreamBackground(streamBG);
                return;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Live), "UpdateDetail")]
        static bool NoWatching(Live __instance, ref TMP_Text ___haisinDetail, LanguageType ____lang)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber != 5)
                return true;
            int watchingNum = StreamLoader.customStreamSettings.CustomFollowerCount;
            if (!StreamLoader.customStreamSettings.HasCustomFollowerCount)
                return true;
            if (watchingNum < 192)
            {
                ___haisinDetail.text = string.Concat(new string[]
                         { watchingNum.ToString(), " ", NgoEx.SystemTextFromType(SystemTextType.Haisin_Watching_Number, ____lang), " ・ ", NgoEx.SystemTextFromType(SystemTextType.Haisin_Started_Day, ____lang), " ",NgoEx.DayText(SingletonMonoBehaviour<StatusManager>.Instance.GetStatus(StatusType.DayIndex), ____lang) });
                __instance.slider.value = 1f - __instance.NowPlaying.playing.Count / __instance.scenarioLength;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(App_BankView), "Awake")]
        static bool DarkBank(Animator ____anim)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber != 5)
                return true;
            if (!StreamLoader.customStreamSettings.IsDarkAngelPlaying)
            {
                return true;
            }
            switch (SingletonMonoBehaviour<Settings>.Instance.CurrentLanguage.Value)
            {
                case LanguageType.KO:
                    ____anim.Play("BankAnimation_Kor_B");
                    break;
                case LanguageType.CN:
                case LanguageType.TW:
                    ____anim.Play("BankAnimation_Chn_B");
                    break;
                case LanguageType.JP:
                    ____anim.Play("BankAnimation_B");
                    break;
                default:
                    ____anim.Play("BankAnimation_Eng_B");
                    break;
            }
            return false;

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Live), "AddMob")]
        static bool AutoDeleteComments(string haisinPoint, List<LiveComment> ____selectableComments)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber != 5)
                return true;
            if (haisinPoint == "deleteAll")
            {

                for (int i = 0; i < ____selectableComments.Count; i++)
                {
                    if (____selectableComments[i].playing.color != SuperchatType.White || ____selectableComments[i].isHiroizumi) { continue; }
                    ____selectableComments[i].isHiroizumi = true;
                    AudioManager.Instance.PlaySeByType(SoundType.SE_pien, false);
                    ____selectableComments[i].honbunView.DOColor(new Color(0f, 0f, 0f, 0f), 0.4f).Play();
                    ____selectableComments[i].isDeleted = true;
                };

                return false;
            }
            if (haisinPoint == "delete")
            {

                if (____selectableComments[____selectableComments.Count - 1].playing.color != SuperchatType.White || ____selectableComments[____selectableComments.Count - 1].isHiroizumi) { return false; }
                ____selectableComments[____selectableComments.Count - 1].isHiroizumi = true;
                AudioManager.Instance.PlaySeByType(SoundType.SE_pien, false);
                ____selectableComments[____selectableComments.Count - 1].honbunView.DOColor(new Color(0f, 0f, 0f, 0f), 0.4f).Play();
                ____selectableComments[____selectableComments.Count - 1].isDeleted = true;


                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TenchanView), "OnEndStream")]
        static bool IsEndSplash()
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber != 5)
                return true;
            if (!StreamLoader.customStreamSettings.hasEndScreen)
            {
                return false;
            }
            return true;

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TenchanView), "OnEndStream")]
        static void CustomEndSplash(TenchanView __instance)
        {
            if (SingletonMonoBehaviour<Settings>.Instance.saveNumber != 5)
                return;
            if (!StreamLoader.customStreamSettings.HasCustomEndScreen)
                return;
            if (!File.Exists(StreamLoader.customStreamSettings.CustomEndScreenPath))
                return;
            try
            {
                __instance._view.sprite = MediaExporter.LoadImageFromFile(StreamLoader.customStreamSettings.CustomEndScreenPath);
            }
            catch { __instance._view.sprite = __instance._endView; }
        }
    }
}
