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
using MEC;
using Mistaken.API;

namespace Mistaken.EventManager.Events
{
    internal class WarheadRun : IEMEventClass, IEndOnNoAlive, IWinOnEscape
    {
        public override string Id => "whr";

        public override string Description => "WarheadRun";

        public override string Name => "WarheadRun";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D_Info", "Musisz uciec z placówki. Kto pierwszy ten lepszy!" },
        };

        public override void OnIni()
        {
            Mistaken.API.Utilities.Map.TeslaMode = Mistaken.API.Utilities.TeslaMode.DISABLED_FOR_ALL;
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Map.Lifts.First(e => e.Type() == ElevatorType.Nuke).Network_locked = true;
            foreach (var door in Map.Doors)
            {
                door.IsOpen = true;
                if (door.Type == DoorType.CheckpointEntrance || door.Type == DoorType.CheckpointLczA || door.Type == DoorType.CheckpointLczB)
                    door.ChangeLock(DoorLockType.DecontEvacuate);
                else if (door.Nametag != string.Empty)
                    door.ChangeLock(DoorLockType.AdminCommand);
            }
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
        }

        private void Server_RoundStarted()
        {
            Map.Broadcast(8, EventManager.EMLB + this.Translations["D_Info"], shouldClearPrevious: true);
            Cassie.Message("NATO_A WARHEAD WILL BE INITIATED IN T MINUS 1 MINUTE", false, true);
            Timing.CallDelayed(68f, () =>
            {
                if (!this.Active)
                    return;
                Warhead.Start();
                Warhead.IsLocked = true;
                foreach (var player in RealPlayers.Get(RoleType.ClassD))
                {
                    player.AddItem(ItemType.SCP207);
                    player.AddItem(ItemType.SCP500);
                }
            });
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole != RoleType.Spectator)
                ev.Player.SlowChangeRole(RoleType.ClassD);
        }
    }
}
