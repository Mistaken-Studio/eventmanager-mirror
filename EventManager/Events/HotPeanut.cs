// -----------------------------------------------------------------------
// <copyright file="HotPeanut.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using Mistaken.API;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class HotPeanut : EventBase, IAnnouncePlayersAlive, IWinOnLastAlive
    {
        public override string Id => "hp";

        public override string Description => "Fight between SCP 173 and Class D";

        public override string Name => "Hot Peanut";

        public bool ClearPrevious => true;

        public override void Initialize()
        {
            this.spawn = RoleType.Scp106.GetRandomSpawnProperties().Item1;
            API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
        }

        public override void Deinitialize()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
        }

        private Vector3 spawn;

        private void Server_RoundStarted()
        {
            var isscp = false;
            foreach (var player in RealPlayers.RandomList)
            {
                if (!isscp)
                {
                    player.SlowChangeRole(RoleType.Scp173);
                    isscp = true;
                }
                else
                    player.SlowChangeRole(RoleType.ClassD, this.spawn);
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.Spectator)
                return;
            if (ev.NewRole == RoleType.Scp173)
                Timing.CallDelayed(10f, () => ev.Player.Position = this.spawn);
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            ev.Target.SlowChangeRole(RoleType.Scp173, this.spawn);
        }
    }
}
