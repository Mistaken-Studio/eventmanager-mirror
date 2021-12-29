// -----------------------------------------------------------------------
// <copyright file="Blank.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Exiled.API.Features;

namespace Mistaken.EventManager.Events
{
    internal class Blank : IEMEventClass
    {
        public override string Id => "blank";

        public override string Description => "Blank event";

        public override string Name => "Blank";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            // { "", "" }
        };

        public override void OnIni()
        {
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            MapGeneration.InitiallySpawnedItems.Singleton.ClearAll();
        }

        public override void OnDeIni()
        {
        }
    }
}
