// -----------------------------------------------------------------------
// <copyright file="TryNotToBlink.cs" company="Mistaken">
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
using Mistaken.EventManager.EventCreator;

namespace Mistaken.EventManager.Events
{
    internal class TryNotToBlink : IEMEventClass,
        IWinOnLastAlive
    {
        public override string Id => "trynottoblink";

        public override string Description => "The name explains it all :)";

        public override string Name => "TryNotToBlink";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            // { "", "" }
        };

        public override void OnIni()
        {
            MapGeneration.InitiallySpawnedItems.Singleton.ClearAll();
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
        }

        public override void OnDeIni()
        {
            Mistaken.API.Utilities.Map.Blackout.Enabled = false;
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
        }

        private void Server_RoundStarted()
        {
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            foreach (var door in Map.Doors)
            {
                var doortype = door.Type;
                if (doortype == DoorType.CheckpointLczA || doortype == DoorType.CheckpointLczB)
                    door.ChangeLock(DoorLockType.DecontLockdown);
                else
                {
                    door.IsOpen = true;
                    door.ChangeLock(DoorLockType.AdminCommand);
                }
            }

            foreach (var player in RealPlayers.RandomList)
            {
                if (player.Side != Side.Scp) player.Role = RoleType.ClassD;
                else player.Role = RoleType.Scp173;
            }

            Cassie.Message("LIGHT SYSTEM ERRROR . LIGHTS OUT", false, true);
            Mistaken.API.Utilities.Map.Blackout.Delay = 99999;
            Mistaken.API.Utilities.Map.Blackout.Length = 99999;
            Mistaken.API.Utilities.Map.Blackout.Enabled = true;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            MEC.Timing.CallDelayed(1f, () =>
            {
                if (ev.Player.Role == RoleType.ClassD) ev.Player.AddItem(ItemType.Flashlight);
            });
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            var role = ev.Target.Role;
            MEC.Timing.CallDelayed(1f, () =>
            {
                if (role == RoleType.ClassD) ev.Target.Role = RoleType.Scp173;
            });
        }
    }
}
