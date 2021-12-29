// -----------------------------------------------------------------------
// <copyright file="TryNotToBlink.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using Mistaken.API;

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
            MapGeneration.InitiallySpawnedItems.Singleton.ClearAll();
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            foreach (var door in Map.Doors)
            {
                if (door.Type == DoorType.CheckpointLczA || door.Type == DoorType.CheckpointLczB)
                    door.ChangeLock(DoorLockType.DecontLockdown);
                else
                {
                    door.IsOpen = true;
                    door.ChangeLock(DoorLockType.AdminCommand);
                }
            }
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
        }

        private void Server_RoundStarted()
        {
            foreach (var player in RealPlayers.RandomList)
            {
                if (player.Team != Team.SCP)
                    player.SlowChangeRole(RoleType.ClassD);
                else
                    player.SlowChangeRole(RoleType.Scp173);
            }

            Cassie.Message("LIGHT SYSTEM ERROR . LIGHTS OUT", false, true);
            Map.TurnOffAllLights(float.MaxValue);
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            Timing.CallDelayed(1f, () =>
            {
                if (ev.Player.Role == RoleType.ClassD)
                    ev.Player.AddItem(ItemType.Flashlight);
            });
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            ev.Target.SlowChangeRole(RoleType.Scp173);
        }
    }
}
