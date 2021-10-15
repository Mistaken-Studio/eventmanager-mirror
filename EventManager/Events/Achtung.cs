// -----------------------------------------------------------------------
// <copyright file="Achtung.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Mistaken.API;
using Mistaken.API.Extensions;
using Mistaken.EventManager.EventCreator;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class Achtung : IEMEventClass,
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
            MEC.Timing.RunCoroutine(this.SpawnGrenades(20));
            Map.Broadcast(10, $"{EventManager.EMLB} {this.Translations["D"]}");
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            MEC.Timing.CallDelayed(1f, () => ev.Player.Position = RoleType.Scp106.GetRandomSpawnProperties().Item1);
        }

        private IEnumerator<float> SpawnGrenades(float time)
        {
            while (this.Active)
            {
                yield return MEC.Timing.WaitForSeconds(time);
                if (time > 2)
                    time--;
                foreach (var player in RealPlayers.List)
                {
                    this.DropGrenadeUnder(player);
                    if (time < 7)
                    {
                        yield return MEC.Timing.WaitForSeconds(1);
                        this.DropGrenadeUnder(player, (ushort)UnityEngine.Random.Range(0, 3));
                    }
                }
            }
        }

        private void DropGrenadeUnder(Player player, ushort count = 1)
        {
            for (; count > 0; count--)
            {
                var grenade = new Throwable(ItemType.GrenadeHE, player);
                grenade.Throw(player.Position, Vector3.zero);
            }
        }
    }
}
