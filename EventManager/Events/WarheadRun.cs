// -----------------------------------------------------------------------
// <copyright file="WarheadRun.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Mistaken.API;

namespace Mistaken.EventManager.Events
{
    internal class WarheadRun : EventBase, IEndOnNoAlive, IWinOnEscape
    {
        public override string Id => "whr";

        public override string Description => "WarheadRun";

        public override string Name => "WarheadRun";

        public Dictionary<string, string> Translations => new ()
        {
            { "D_Info", "Musisz uciec z placówki. Kto pierwszy ten lepszy!" },
        };

        public override void Initialize()
        {
            API.Utilities.Map.TeslaMode = API.Utilities.TeslaMode.DISABLED_FOR_ALL;
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.API.Features.Lift.List.First(x => x.Type == ElevatorType.Nuke).IsLocked = true;
            foreach (var door in Door.List.ToArray())
            {
                door.IsOpen = true;
                var type = door.Type;
                if (type == DoorType.CheckpointEntrance || type == DoorType.CheckpointLczA || type == DoorType.CheckpointLczB)
                    door.ChangeLock(DoorLockType.DecontEvacuate);
                else if (door.Nametag != string.Empty)
                    door.ChangeLock(DoorLockType.AdminCommand);
            }
        }

        public override void Deinitialize()
        {
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = false;
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
        }

        private void Server_RoundStarted()
        {
            Map.Broadcast(8, EventManager.EMLB + this.Translations["D_Info"], shouldClearPrevious: true);
            Cassie.Message("NATO_A WARHEAD WILL BE INITIATED IN T MINUS 1 MINUTE", false, true);
            static void StartWarhead()
            {
                Warhead.Start();
                Warhead.IsLocked = true;
                foreach (var player in RealPlayers.Get(RoleType.ClassD).ToArray())
                {
                    player.AddItem(ItemType.SCP207);
                    player.AddItem(ItemType.SCP500);
                }
            }

            EventManager.Instance.CallDelayed(68f, StartWarhead, "EventManager_WarheadRun_StartWarhead", true);
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole != RoleType.Spectator)
                ev.Player.SlowChangeRole(RoleType.ClassD);
        }
    }
}
