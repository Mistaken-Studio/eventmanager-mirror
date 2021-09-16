// -----------------------------------------------------------------------
// <copyright file="WarheadRun.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects.DoorUtils;
using Mistaken.API;

namespace Mistaken.EventManager.Events
{
    internal class WarheadRun :
        EventCreator.IEMEventClass,
        EventCreator.IEndOnNoAlive,
        EventCreator.IWinOnEscape
    {
        public override string Id => "whr";

        public override string Description => "WarheadRun";

        public override string Name => "WarheadRun";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D_Info", "Musisz uciec z placówki.. kto pierwszy ten lepszy!" },
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
        }

        private void Server_RoundStarted()
        {
            Mistaken.API.Utilities.Map.TeslaMode = Mistaken.API.Utilities.TeslaMode.DISABLED_FOR_ALL;
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            Map.Lifts.First(e => e.Type() == ElevatorType.Nuke).Network_locked = true;
            foreach (var door in Map.Doors)
            {
                door.IsOpen = true;
                var type = door.Type;
                if (type == DoorType.CheckpointEntrance || type == DoorType.CheckpointLczA || type == DoorType.CheckpointLczB)
                    door.ChangeLock(DoorLockType.DecontEvacuate);
                else if (door.Nametag != string.Empty)
                    door.ChangeLock(DoorLockType.AdminCommand);
            }

            foreach (var player in RealPlayers.List)
                player.SlowChangeRole(RoleType.ClassD);
            Map.Broadcast(8, EventManager.EMLB + this.Translations["D_Info"]);
            Cassie.Message("nato_a warhead will be initiated in t minus 1 minute", false, true);
            MEC.Timing.CallDelayed(68, () =>
            {
                if (!this.Active)
                    return;
                Warhead.Start();
                Warhead.IsLocked = true;
                foreach (var p in Player.Get(RoleType.ClassD))
                {
                    p.AddItem(ItemType.SCP207);
                    p.AddItem(ItemType.SCP500);
                }
            });
        }
    }
}
