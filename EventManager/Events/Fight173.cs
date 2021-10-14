// -----------------------------------------------------------------------
// <copyright file="Fight173.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Mistaken.API;
using Mistaken.EventManager.EventCreator;

namespace Mistaken.EventManager.Events
{
    internal class Fight173 : IEMEventClass,
        IAnnouncePlayersAlive,
        IWinOnLastAlive
    {
        public override string Id => "f173";

        public override string Description => "Fight between SCP 173 and Class D";

        public override string Name => "Fight173";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            // { "", "" }
        };

        public bool ClearPrevious => true;

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
        }

        private void Server_RoundStarted()
        {
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            foreach (var player in RealPlayers.RandomList)
            {
                if (player.Team != Team.SCP) player.Role = RoleType.ClassD;
                else player.SlowChangeRole(RoleType.Scp173, RoleType.Scp106.GetRandomSpawnProperties().Item1);
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            MEC.Timing.CallDelayed(1f, () =>
            {
                if (ev.Player.Role != RoleType.Scp173)
                    ev.Player.Position = RoleType.Scp106.GetRandomSpawnProperties().Item1;
            });
        }
    }
}