using NGO;
using ngov3;
using System;
using System.Collections.Generic;

namespace CustomStreamLoader
{
    public enum CustomAssetType
    {
        Background, Sprite
    }

    public enum CustomAssetFileType
    {
        ImageFile, AssetBundle, AddressableBundle
    }

    public class CustomAsset
    {
        public CustomAssetType customAssetType;
        public CustomAssetFileType customAssetFileType;
        public string fileName;
        public string filePath;
        public string catalogPath;
        public int picWidth;
        public int picHeight;

        public CustomAsset() { }
    }
    public enum StreamBackground
    {
        Default, Silver, Gold, MileOne, MileTwo, MileThree, MileFour, MileFive, Guru, Horror, BigHouse, Roof, Black, Void, None = 1000
    }
    public enum StreamChatSettings
    {
        Normal,
        Celebration,
        Uncontrollable
    }
    public enum ChatCommentType
    {
        Normal, Stressful, Super
    }
    public class StreamSettings
    {
        public string StringTitle;
        public StreamChatSettings ChatSettings;
        public string StartingAnimation;
        public CustomAsset CustomStartingAnimation;
        public StreamBackground StartingBackground;
        public CustomAsset CustomBackground;
        public SoundType StartingMusic;
        public EffectType StartingEffect;
        public float EffectIntensity;

        public string ReactionAnimation;

        public bool IsIntroPlaying = true;
        public bool IsDarkAngelPlaying;
        public bool HasCustomFollowerCount;
        public int CustomFollowerCount = 0;
        public bool HasCustomDay;
        public int CustomDay = 15;

        public bool HasCustomEndScreen;
        public string CustomEndScreenPath;

        public bool HasChair = true;

        public bool IsInvertedColors;
        public bool isBordersOff;

        public bool hasEndScreen = true;

        public List<PlayingObject> PlayingList = new();

        public bool hasDarkInterface = false;

        public StreamSettings() { }
    }
    public enum PlayingType
    {
        KAngelSays,
        KAngelCallout,
        ChatSays,
        ChatSuper,
        ChatBad,
        PlaySE,
        PlayBGM,
        PlayEffect,
        ChatFirst,
        ChatMiddle,
        ChatLast,
        ChatRainbow,
        ChatDelete,
        ChatDeleteAll,
        ReadSuperChats
    }

    public enum BorderEffectType
    {
        EaseIn,
        EaseBeforePlay,
        Play,
        EaseOut
    }

    [Serializable]
    public class PlayingObject
    {
        public virtual PlayingType PlayingType { get; set; }

    }

    [Serializable]
    public class KAngelSays : PlayingObject
    {
        public override PlayingType PlayingType { get => PlayingType.KAngelSays; }

        public bool IsCustomAnim;
        public CustomAsset customAnim;

        public string AnimName;
        public string Dialogue;

        public KAngelSays() { }

        public KAngelSays(string animName, string dialogue)
        {
            AnimName = animName;
            Dialogue = dialogue;
        }
    }

    [Serializable]
    public class KAngelCallout : KAngelSays
    {
        public override PlayingType PlayingType { get => PlayingType.KAngelCallout; }

        public string HaterComment;

        public KAngelCallout() { }
        public KAngelCallout(string hateComment)
        {
            HaterComment = hateComment;
        }

        public void AddHateCallout(string hateComment)
        {
            HaterComment = hateComment;
        }
    }

    [Serializable]
    public class ChatSays : PlayingObject
    {
        public string Comment;

        public List<KAngelSays> Replies;

        public ChatSays() { }

        public ChatSays(string comment)
        {
            Comment = comment;
            SetNormalComment();
        }

        public ChatSays(string comment, bool isBad = true)
        {
            Comment = comment;
            SetBadComment();
        }
        public ChatSays(string comment, List<KAngelSays> replies)
        {
            Comment = comment;
            SetSuperChat(replies);
        }
        public void SetBadComment()
        {
            PlayingType = PlayingType.ChatBad;
            Replies = null;
        }

        public void SetSuperChat(List<KAngelSays> replies)
        {
            PlayingType = PlayingType.ChatSuper;
            Replies = replies;
        }

        public void SetNormalComment()
        {
            PlayingType = PlayingType.ChatSays;
            Replies = null;
        }
    }

    [Serializable]
    public class PlaySound : PlayingObject
    {
        public SoundType Audio;

        public PlaySound() { }
        public PlaySound(SoundType sound)
        {
            ChangeSound(sound);
        }

        public void ChangeSound(SoundType sound)
        {
            Audio = sound;
            PlayingType = sound.ToString().Contains("BGM_") ? PlayingType.PlayBGM : PlayingType.PlaySE;
        }
    }

    [Serializable]
    public class PlayEffect : PlayingObject
    {
        public override PlayingType PlayingType { get => PlayingType.PlayEffect; }
        public ChanceEffectType BorderEffect;
        public BorderEffectType BorderEffectType;

        public PlayEffect() { }
        public PlayEffect(ChanceEffectType borderEffect, BorderEffectType borderEffectType)
        {
            ChangeEffectType(borderEffect, borderEffectType);
        }

        public void ChangeEffectType(ChanceEffectType borderEffect, BorderEffectType borderEffectType)
        {
            BorderEffect = borderEffect;
            BorderEffectType = borderEffectType;
        }
    }

    [Serializable]
    public class ChatGeneral : PlayingObject
    {

        public ChatGeneral() { }
        public ChatGeneral(PlayingType playingType)
        {
            ChangePlayingType(playingType);
        }
        public void ChangePlayingType(PlayingType playingType)
        {
            if ((int)playingType < 8)
                throw new ArgumentOutOfRangeException(nameof(playingType) + " - This PlayingType is not supported for this class.");
            PlayingType = playingType;
        }
    }
}
