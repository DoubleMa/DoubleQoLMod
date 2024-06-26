﻿using DoubleQoL.Extensions;
using DoubleQoL.QoL.UI.Statusbar;
using DoubleQoL.XML.config;
using Mafi.Unity.UiFramework;
using System;
using System.Reflection;

namespace DoubleQoL.Game.Patcher {

    internal class LogisticsStatusBarPatcher : APatcher<LogisticsStatusBarPatcher> {
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_statusbar.Value;
        private static Type Typ;

        public LogisticsStatusBarPatcher() : base("LogisticsStatusBar") {
            Typ = Assembly.Load("Mafi.Unity").GetType("Mafi.Unity.InputControl.Levelling.LogisticsStatusBarView");
            AddMethod(Typ, "RegisterIntoStatusBar", this.GetHarmonyMethod(nameof(MyPostfix)));
        }

        private static void MyPostfix(IUnityUi __instance) => GetResolvedInstance<LogisticsStatusBarView>()?.OnClick(__instance.GetField<Action>("m_onClick"));
    }
}