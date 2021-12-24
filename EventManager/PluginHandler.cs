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
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config>
    {
        /// <inheritdoc/>
        public override string Author => "Mistaken Devs";

        /// <inheritdoc/>
        public override string Name => "EventManager";

        /// <inheritdoc/>
        public override string Prefix => "EM";

        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Higher - 1;

        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new Version(4, 1, 7);

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Instance = this;

            new EventManager(this);

            Harmony = new Harmony("com.eventmanager.patch");
            API.Diagnostics.Module.OnEnable(this);
            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            Harmony.UnpatchAll();
            API.Diagnostics.Module.OnDisable(this);
            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }

        internal static Harmony Harmony { get; private set; }
    }
}
