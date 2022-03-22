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
using MEC;
using Mistaken.API;
using Mistaken.API.Shield;

namespace Mistaken.EventManager.Events
{
#pragma warning disable SA1402
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
            foreach (var e in Exiled.API.Features.Lift.List)
            {
                if (e.Type == ElevatorType.GateA || e.Type == ElevatorType.GateB)
                    e.IsLocked = true;
            }
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
        }

        private void Server_RoundStarted()
        {
            var players = RealPlayers.List.ToList();
            var titan = players[UnityEngine.Random.Range(0, players.Count)];
            players.Remove(titan);
            titan.SlowChangeRole(RoleType.ChaosMarauder);
            titan.Broadcast(8, EventManager.EMLB + this.Translations["T_Info"]);
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

            Timing.CallDelayed(0.2f, () =>
            {
                TitanShield.Ini<TitanShield>(titan);
                titan.RemoveItem(titan.Items.First(x => x.Type == ItemType.KeycardChaosInsurgency));
                titan.AddItem(ItemType.GunE11SR);
                titan.AddItem(ItemType.GunShotgun);
                titan.AddItem(ItemType.GunRevolver);
                titan.AddItem(ItemType.GrenadeHE);
                titan.SetAmmo(AmmoType.Ammo12Gauge, 74);
                titan.SetAmmo(AmmoType.Nato556, 200);
                titan.SetAmmo(AmmoType.Ammo44Cal, 68);
            });
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (RealPlayers.List.Count(x => x.Role.Team == Team.MTF) == 0)
                this.OnEnd($"<color=green>Tytan {ev.Killer.Nickname}</color> wygrał!");
            else if (RealPlayers.List.FirstOrDefault(x => x.Role == RoleType.ChaosMarauder) == default)
                this.OnEnd("<color=blue>MFO</color> wygrywa!");
        }
    }

    internal class TitanShield : Shield
    {
        protected override float MaxShield => 1000;

        protected override float ShieldRechargeRate => 50;

        protected override float ShieldEffectivnes => 1;

        protected override float TimeUntilShieldRecharge => 5;
    }
}
