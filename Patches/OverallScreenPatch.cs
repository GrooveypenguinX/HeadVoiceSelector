﻿#if !UNITY_EDITOR
using Aki.Reflection.Patching;
using EFT;
using HeadVoiceSelector.Core.UI;
using System.Reflection;

namespace HeadVoiceSelector.Patches
{
    internal class OverallScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(EFT.UI.OverallScreen).GetMethod(nameof(EFT.UI.OverallScreen.Show));

        [PatchPostfix]
        public static void PatchPostfix()
        {
            NewVoiceHeadDrawers.AddCustomizationDrawers();
        }
    }
}

#endif