#if !UNITY_EDITOR
using BepInEx;
using HeadVoiceSelector.Patches;
using System;
using System.IO;

namespace HeadVoiceSelector
{
    [BepInPlugin("com.HeadVoiceSelector.Core", "HeadVoiceSelector Core", "1.0.0")]

    internal class HeadVoiceSelector : BaseUnityPlugin
    {

        public static HeadVoiceSelector instance;

        public static string modPath = Path.Combine(Environment.CurrentDirectory, "user", "mods", "WTT-HeadVoiceSelector");


        internal void Awake()
        {
            instance = this;

            new OverallScreenPatch().Enable();

        }
    }
}
#endif