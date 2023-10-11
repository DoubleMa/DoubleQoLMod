﻿using DoubleQoL.Config;
using DoubleQoL.Game.Patcher.Helper;
using HarmonyLib;
using Mafi.Unity.UiFramework;
using System;
using System.Reflection;

namespace DoubleQoL.Game.Patcher {

    internal class PopulationStatusBarPatcher : APatcher {
        public static readonly PopulationStatusBarPatcher Instance = new PopulationStatusBarPatcher();
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_statusbar.Value;
        private static Type Typ;

        public PopulationStatusBarPatcher() : base("PopulationStatusBar") {
            Typ = Assembly.Load("Mafi.Unity").GetType("Mafi.Unity.InputControl.Levelling.PopulationStatusBarView");
            AddBlockedMethod(AccessTools.Method(Typ, "Mafi.Unity.UiFramework.IUnityUi.RegisterUi"), AccessTools.Method(GetType(), "MyPostfix"));
        }

        private static void MyPostfix(IUnityUi __instance) {
            Action onCLick = (Action)AccessTools.Field(Typ, "m_onClick").GetValue(__instance);
            PatcherHelper.Instance.PopulationStatusBarView.OnClick(onCLick);
            PatcherHelper.Instance.UnityStatusBarView.OnClick(onCLick);
        }
    }
}