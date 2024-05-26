// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;

namespace Mistaken.EventManager
{
    internal class PluginHandler : Plugin<Config>
    {
        public static PluginHandler Instance { get; private set; }

        public static Harmony Harmony { get; private set; }

        public override string Author => "Mistaken Devs";

        public override string Name => "EventManager";

        public override string Prefix => "EM";

        public override PluginPriority Priority => PluginPriority.Higher - 1;

        public override Version RequiredExiledVersion => new (5, 2, 2);

        public override void OnEnabled()
        {
            Instance = this;

            new EventManager(this);

            Harmony = new Harmony("com.eventmanager.patch");
            API.Diagnostics.Module.OnEnable(this);
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Harmony.UnpatchAll();
            API.Diagnostics.Module.OnDisable(this);
            base.OnDisabled();
        }
    }
}
