// -----------------------------------------------------------------------
// <copyright file="TryNotToBlink.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;

namespace Mistaken.EventManager.Events
{
    internal class TryNotToBlink : IEMEventClass, IWinOnLastAlive
    {
        public override string Id => "tntb";

        public override string Description => "The name explains it all :)";

        public override string Name => "TryNotToBlink";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            // { "", "" }
        };

        public override void OnIni()
        {
            Map.Pickups.ToList().ForEach(x => x.Destroy());
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            foreach (var door in Door.List)
            {
                if (door.Type == DoorType.CheckpointLczA || door.Type == DoorType.CheckpointLczB)
                {
                    door.ChangeLock(DoorLockType.DecontLockdown);
                    door.IsOpen = true;
                }
                else
                {
                    door.IsOpen = true;
                    door.ChangeLock(DoorLockType.AdminCommand);
                }
            }

            foreach (var e in Exiled.API.Features.Lift.List)
                e.IsLocked = true;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
        }

        private void Server_RoundStarted()
        {
            Cassie.Message("LIGHT SYSTEM ERROR . LIGHTS OUT", false, true);
            Map.TurnOffAllLights(float.MaxValue);
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.Spectator)
                return;
            Timing.CallDelayed(1f, () =>
            {
                if (ev.Player.Role.Team == Team.SCP)
                    ev.Player.SlowChangeRole(RoleType.Scp173);
                else
                {
                    ev.Player.SlowChangeRole(RoleType.ClassD);
                    Timing.CallDelayed(0.5f, () => ev.Player.AddItem(ItemType.Flashlight));
                }
            });
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            ev.Target.SlowChangeRole(RoleType.Scp173);
        }
    }
}
