// -----------------------------------------------------------------------
// <copyright file="Blank.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;

namespace Mistaken.EventManager.Events
{
    internal class Blank : EventBase
    {
        public override string Id => "blank";

        public override string Description => "Blank event";

        public override string Name => "Blank";

        public override void Initialize()
        {
            API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Map.Pickups.ToList().ForEach(x => x.Destroy());
        }

        public override void Deinitialize()
        {
        }
    }
}
