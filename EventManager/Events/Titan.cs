// -----------------------------------------------------------------------
// <copyright file="Titan.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Mistaken.API;

namespace Mistaken.EventManager.Events
{
    internal class Titan : IEMEventClass
    {
        public override string Id => "titan";

        public override string Description => "Kill the powerful titan";

        public override string Name => "Titan";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "T_Info", "Jesteś <color=green>Tytanem</color>. Twoim zadaniem jest rozprawienie się z <color=blue>MFO</color> atakujących Ciebie." },
            { "MTF_Info", "Waszym zadaniem (<color=blue>MFO</color>) jest zabicie <color=green>Tytana</color>, który znajduje się na spawnie CI. Uważajcie!" },
        };

        public override void OnIni()
        {
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;

            // Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;
            foreach (var e in Map.Lifts)
            {
                var etype = e.Type();
                if (etype == ElevatorType.GateA || etype == ElevatorType.GateB)
                    e.Network_locked = true;
            }
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;

            // Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;
        }

        private void Server_RoundStarted()
        {
            var players = RealPlayers.List.ToList();
            var titan = players[UnityEngine.Random.Range(0, players.Count())];
            players.Remove(titan);
            titan.SlowChangeRole(RoleType.ChaosMarauder);
            titan.Broadcast(8, EventManager.EMLB + this.Translations["T_Info"]);
            titan.Health *= players.Count() + 1;
            titan.ArtificialHealth = (players.Count() + 1) * 30;
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

                player.Broadcast(8, EventManager.EMLB + this.Translations["MTF_Info"]);
            }
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            var players = RealPlayers.List.Where(x => x.IsAlive);
            if (players.Count(x => x.Team == Team.MTF) == 0)
                this.OnEnd($"<color=green>Tytan {ev.Killer.Nickname}</color> wygrał!");
            else if (players.FirstOrDefault(x => x.Role == RoleType.ChaosMarauder) == default)
                this.OnEnd("<color=blue>MFO</color> wygrywa!");
            else if (ev.Target.Team == Team.MTF)
                players.First(x => x.Role == RoleType.ChaosRifleman).ArtificialHealth += 8 * players.Count();
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            // if (ev.DamageType == DamageTypes.Logicer && ev.Amount > 30) ev.Amount = 150;
            // else if (ev.DamageType == DamageTypes.Logicer) ev.Amount = 51;
        }
    }
}
