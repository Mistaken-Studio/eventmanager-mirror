﻿// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Exiled.API.Enums;
using Exiled.API.Features;

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
        public override PluginPriority Priority => PluginPriority.Highest;

        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new Version(3, 0, 0, 84);

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Instance = this;
            API.Diagnostics.Module.OnEnable(this);

            new EventManager(this);

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            API.Diagnostics.Module.OnDisable(this);

            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }
    }
}
