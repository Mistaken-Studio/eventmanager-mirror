// -----------------------------------------------------------------------
// <copyright file="WDB.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Mistaken.API;
using Mistaken.EventManager.EventCreator;

namespace Mistaken.EventManager.Events
{
    internal class WDB : IEMEventClass
    {
        public override string Id => "wdb";

        public override string Description => "When Day Breaks";

        public override string Name => "WDB";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            // { "", "" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RespawningTeam += this.Server_RespawningTeam;
            Exiled.Events.Handlers.Player.ItemUsed += this.Player_ItemUsed;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RespawningTeam -= this.Server_RespawningTeam;
            Exiled.Events.Handlers.Player.ItemUsed -= this.Player_ItemUsed;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
        }

        private readonly Dictionary<Player, bool> infected = new Dictionary<Player, bool>();

        private void Server_RoundStarted()
        {
            this.infected.Clear();

            foreach (var door in Map.Doors.Where(x => x.Type == DoorType.GateA || x.Type == DoorType.GateB))
            {
                door.IsOpen = true;
                door.ChangeLock(DoorLockType.NoPower);
            }

            foreach (var player in RealPlayers.RandomList)
            {
                this.infected.Add(player, false);
            }
        }

        private void Server_RespawningTeam(Exiled.Events.EventArgs.RespawningTeamEventArgs ev)
        {
            throw new System.NotImplementedException();
        }

        private void Player_ItemUsed(Exiled.Events.EventArgs.UsedItemEventArgs ev)
        {
            if (ev.Item.Type == ItemType.SCP500)
                this.infected[ev.Player] = false;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            throw new System.NotImplementedException();
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (this.infected[ev.Target])
            {
            }
        }
    }
}
