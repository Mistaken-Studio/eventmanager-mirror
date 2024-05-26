// -----------------------------------------------------------------------
// <copyright file="Titan.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using Mistaken.API;
using Mistaken.API.Shield;

#pragma warning disable SA1402 // File may only contain a single type

namespace Mistaken.EventManager.Events
{
    internal class Titan : EventBase
    {
        public override string Id => "titan";

        public override string Description => "Kill the powerful titan";

        public override string Name => "Titan";

        public Dictionary<string, string> Translations => new ()
        {
            { "T_Info", "Jesteś <color=green>Tytanem</color>. Twoim zadaniem jest rozprawienie się z <color=blue>MFO</color> atakujących Ciebie." },
            { "MTF_Info", "Waszym zadaniem (<color=blue>MFO</color>) jest zabicie <color=green>Tytana</color>, który znajduje się na spawnie CI. Uważajcie!" },
        };

        public override void Initialize()
        {
            API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            foreach (var e in Exiled.API.Features.Lift.List)
            {
                if (e.Type == ElevatorType.GateA || e.Type == ElevatorType.GateB)
                    e.IsLocked = true;
            }
        }

        public override void Deinitialize()
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
                player.SlowChangeRole(this.RandomTeamRole(Team.MTF));
                player.Broadcast(8, EventManager.EMLB + this.Translations["MTF_Info"]);
            }

            Timing.CallDelayed(0.2f, () =>
            {
                Shield.Ini<TitanShield>(titan);
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
            if (RealPlayers.Get(Team.MTF).Count() == 0)
                this.OnEnd($"<color=green>Tytan {ev.Killer.Nickname}</color> wygrał!");
            else if (RealPlayers.Get(RoleType.ChaosMarauder).Count() == 0)
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
