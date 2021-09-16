// -----------------------------------------------------------------------
// <copyright file="Titan.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Mistaken.API;
using Mistaken.EventManager.EventCreator;

namespace Mistaken.EventManager.Events
{
    internal class Titan : IEMEventClass
    {
        public override string Id => "titan";

        public override string Description => "Kill the powerfull titan";

        public override string Name => "Titan";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "T", "Jesteś <color=green>Tytanem</color>. Twoim zadaniem jest rozprawienie się z <color=blue>MFO</color> atakujących Ciebie." },
            { "M", "Waszym zadaniem (<color=blue>MFO</color>) jest zabicie <color=green>Tytana</color>, który znajduje się na spawnie CI. Uważajcie!" },
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
        }

        private void Server_RoundStarted()
        {
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            foreach (var e in Map.Lifts)
            {
                if (e.elevatorName.ToUpper().StartsWith("GATE"))
                    e.Network_locked = true;
            }

            var players = RealPlayers.List.ToList();
            var titan = players[UnityEngine.Random.Range(0, players.Count())];
            players.Remove(titan);
            titan.SlowChangeRole(RoleType.ChaosRifleman);
            foreach (var player in players)
            {
                switch (UnityEngine.Random.Range(0, 3))
                {
                    case 0:
                        player.SlowChangeRole(RoleType.NtfPrivate);
                        break;
                    case 1:
                        player.SlowChangeRole(RoleType.NtfSergeant);
                        break;
                    case 2:
                        player.SlowChangeRole(RoleType.NtfCaptain);
                        break;
                }

                player.Broadcast(10, EventManager.EMLB + this.Translations["M"]);
            }

            MEC.Timing.CallDelayed(2, () =>
            {
                titan.Broadcast(8, EventManager.EMLB + this.Translations["T"]);
                titan.Health *= players.Count() + 1;
                titan.ArtificialHealth = (players.Count() + 1) * 30;
            });
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            var players = RealPlayers.List.Where(x => x != ev.Target);
            if (players.Count(x => x.Team == Team.MTF) == 0) this.OnEnd($"<color=green>Tytan {ev.Killer.Nickname}</color> wygrał!", true);
            else if (players.Count(x => x.Role == RoleType.ChaosRifleman) == 0) this.OnEnd("<color=blue>MFO</color> wygrywa!", true);
            else if (ev.Target.Team == Team.MTF) players.First(x => x.Role == RoleType.ChaosRifleman).ArtificialHealth += 8 * players.Count();
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.DamageType == DamageTypes.Logicer && ev.Amount > 30) ev.Amount = 150;
            else if (ev.DamageType == DamageTypes.Logicer) ev.Amount = 51;
        }
    }
}
