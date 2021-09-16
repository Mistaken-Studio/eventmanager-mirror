// -----------------------------------------------------------------------
// <copyright file="Achtung.cs" company="Mistaken">
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
    /*internal class Achtung : IEMEventClass,
        IWinOnLastAlive,
        IAnnouncePlayersAlive
    {
        public override string Id => "achtung";

        public override string Description => "Achtung :)";

        public override string Name => "Achtung";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D", "Granaty pojawią się pod Tobą. Ostatni żywy wygrywa!" },
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
            foreach (var player in RealPlayers.List)
                player.Role = RoleType.ClassD;
            this.SpawnGrenades(25);
            Map.Broadcast(10, $"{EventManager.EMLB} {this.Translations["D"]}");
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            MEC.Timing.CallDelayed(1f, () => ev.Player.Position = RoleType.Scp106.GetRandomSpawnProperties().Item1);
        }

        private void SpawnGrenades(float time)
        {
            if (!this.Active)
                return;
            MEC.Timing.CallDelayed(time, () =>
            {
                time--;
                if (time < 2)
                    time = 2;
                this.SpawnGrenades(time);
                if (!this.Active)
                    return;
                foreach (var player in RealPlayers.List)
                {
                    player.DropGrenadeUnder(0, 1);
                    if (time < 7)
                    {
                        MEC.Timing.CallDelayed(1, () =>
                        {
                            player.DropGrenadeUnder(0, UnityEngine.Random.Range(0, 3));
                        });
                    }
                }
            });
        }
    }*/
}
