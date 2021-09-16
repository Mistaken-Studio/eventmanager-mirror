// -----------------------------------------------------------------------
// <copyright file="HotPeanut.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Mistaken.API;
using Mistaken.API.Extensions;
using Mistaken.EventManager.EventCreator;

namespace Mistaken.EventManager.Events
{
    internal class HotPeanut :
        IEMEventClass,
        IAnnouncePlayersAlive,
        IWinOnLastAlive
    {
        public override string Id => "hp";

        public override string Description => "Fight between SCP 173 and Class D";

        public override string Name => "Hot Peanut";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            // { "", "" }
        };

        public bool ClearPrevious => true;

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
        }

        private void Server_RoundStarted()
        {
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            var isscp = false;
            var plist = RealPlayers.List.ToList();
            plist.Shuffle();
            foreach (var player in plist)
            {
                if (!isscp)
                {
                    player.Role = RoleType.Scp173;
                    MEC.Timing.CallDelayed(10f, () =>
                    {
                        player.Position = RoleType.Scp106.GetRandomSpawnProperties().Item1;
                        player.Health = 10;
                    });
                    isscp = true;
                }
                else
                    player.Role = RoleType.ClassD;
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

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (RealPlayers.List.Where(x => x.Role == RoleType.ClassD && x.Id != ev.Target.Id).ToArray().Length > 1)
            {
                MEC.Timing.CallDelayed(0.1f, () =>
                {
                    ev.Target.Role = RoleType.Scp173;
                    ev.Target.Position = RoleType.Scp106.GetRandomSpawnProperties().Item1;
                    ev.Target.Health = 10;
                });
            }
        }
    }
}
