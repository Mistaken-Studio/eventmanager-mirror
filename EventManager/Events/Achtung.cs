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
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class Achtung : IEMEventClass, IWinOnLastAlive, IAnnouncePlayersAlive
    {
        public override string Id => "achtung";

        public override string Description => "Achtung :)";

        public override string Name => "Achtung";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D_Info", "Granaty pojawią się pod Tobą. Ostatni żywy wygrywa" },
        };

        public bool ClearPrevious => true;

        public override void OnIni()
        {
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
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
            EventManager.Instance.RunCoroutine(this.SpawnGrenades(20), "achtung_spawngrenades");
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleType.Spectator)
                return;
            ev.Player.SlowChangeRole(RoleType.ClassD, RoleType.Scp106.GetRandomSpawnProperties().Item1);
            ev.Player.Broadcast(8, EventManager.EMLB + this.Translations["D_Info"], shouldClearPrevious: true);
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
                    if (player.Role == RoleType.Spectator)
                        continue;

                    this.DropGrenadeUnder(player);
                    if (time < 7)
                        MEC.Timing.CallDelayed(UnityEngine.Random.Range(0f, 2f), () => this.DropGrenadeUnder(player, (ushort)UnityEngine.Random.Range(1, 3)));
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
