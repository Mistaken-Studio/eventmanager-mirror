// -----------------------------------------------------------------------
// <copyright file="BlackDeath.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using Mistaken.API;
using PlayerStatsSystem;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class BlackDeath : IEMEventClass
    {
        public override string Id => "bdeath";

        public override string Description => "Black death event";

        public override string Name => "BlackDeath";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "SCP_Info", "Jesteś <color=gray>SCP-106</color>. Twoim zadaniem jest złapanie wszystkich <color=orange>Klas D</color> zanim włączą wszystkie <color=yellow>generatory</color>. Powodzenia!" },
            { "D_Info", "Jesteś <color=orange>Klasą D</color>. Twoim zadaniem jest włączenie wszystkich <color=yellow>generatorów</color>. Nie daj się złapać <color=gray>SCP-106</color> (<color=yellow>natychmiastowa śmierć</color>)." },
            { "D_Death", "Liczne oparzenia na ciele wskazują na działanie wysoko żrącej substancji" },
        };

        public override void OnIni()
        {
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            Map.Pickups.ToList().ForEach(x => x.Destroy());
            var rooms = Map.Rooms.ToList();
            rooms.RemoveAll(x => x.Zone != ZoneType.HeavyContainment);
            for (int i = 0; i < 5; i++)
            {
                if (rooms.Count == 0)
                    break;
                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];
                new Item(ItemType.KeycardNTFCommander).Spawn(room.Position + (Vector3.up * 2));
                rooms.Remove(room);
            }

            for (int i = 0; i < 10; i++)
            {
                if (rooms.Count == 0)
                    break;
                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];
                new Throwable(ItemType.GrenadeFlash).Spawn(room.Position + (Vector3.up * 2));
                rooms.Remove(room);
            }

            for (int i = 0; i < 15; i++)
            {
                if (rooms.Count == 0)
                    break;
                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];
                new Flashlight(ItemType.Flashlight).Spawn(room.Position + (Vector3.up * 2));
                rooms.Remove(room);
            }

            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Map.GeneratorActivated += this.Map_GeneratorActivated;
            Exiled.Events.Handlers.Player.ActivatingGenerator += this.Player_ActivatingGenerator;
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Map_ExplodingGrenade;
            Exiled.Events.Handlers.Player.EnteringPocketDimension += this.Player_EnteringPocketDimension;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            this.classDSpawn = Map.Doors.First(d => d.Type == DoorType.HczArmory).Base.transform.position + Vector3.up;
            foreach (var door in Map.Doors)
            {
                if (door.Type == DoorType.HczArmory)
                {
                    door.ChangeLock(DoorLockType.AdminCommand);
                    door.IsOpen = true;
                }
                else if (door.Type == DoorType.HID || door.Type == DoorType.Scp106Primary || door.Type == DoorType.Scp106Secondary || door.Type == DoorType.Scp106Bottom)
                    door.ChangeLock(DoorLockType.AdminCommand);
                else if (door.Type == DoorType.CheckpointEntrance)
                    door.ChangeLock(DoorLockType.DecontLockdown);
            }

            foreach (var e in Map.Lifts)
            {
                if (e.Type() != ElevatorType.Nuke)
                    e.Network_locked = true;
            }
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Map.GeneratorActivated -= this.Map_GeneratorActivated;
            Exiled.Events.Handlers.Player.ActivatingGenerator -= this.Player_ActivatingGenerator;
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Map_ExplodingGrenade;
            Exiled.Events.Handlers.Player.EnteringPocketDimension -= this.Player_EnteringPocketDimension;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
        }

        private Vector3 classDSpawn;

        private void Server_RoundStarted()
        {
            Map.TurnOffAllLights(float.MaxValue);
            var players = RealPlayers.List.ToList();
            var scp = players[UnityEngine.Random.Range(0, players.Count())];
            scp.SlowChangeRole(RoleType.Scp106);
            scp.Broadcast(8, EventManager.EMLB + this.Translations["SCP_Info"], shouldClearPrevious: true);
            players.Remove(scp);
            foreach (var player in players)
            {
                player.SlowChangeRole(RoleType.ClassD, this.classDSpawn);
                player.Broadcast(8, EventManager.EMLB + this.Translations["D_Info"], shouldClearPrevious: true);
            }
        }

        private void Player_ActivatingGenerator(Exiled.Events.EventArgs.ActivatingGeneratorEventArgs ev)
        {
            if (ev.Generator._totalActivationTime > 180f)
                ev.Generator._totalActivationTime = 180f;
        }

        private void Player_EnteringPocketDimension(Exiled.Events.EventArgs.EnteringPocketDimensionEventArgs ev)
        {
            ev.Player.Hurt(new CustomReasonDamageHandler(this.Translations["D_Death"]));
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (ev.Target.Role == RoleType.Scp106)
                this.OnEnd("<color=orange>Klasa D wygrywa!</color>");
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (!RealPlayers.Any(RoleType.ClassD))
                this.OnEnd("<color=gray>SCP-106 wygrywa!</color>");
        }

        private void Map_GeneratorActivated(Exiled.Events.EventArgs.GeneratorActivatedEventArgs ev)
        {
            if (Map.ActivatedGenerators > 1)
            {
                Cassie.Message("ALL GENERATORS HAVE BEEN SUCCESSFULLY ENGAGED . SCP 1 0 6 RECONTAINMENT SEQUENCE COMMENCING IN T MINUS 1 MINUTE", false, true);
                Timing.CallDelayed(60f, () =>
                {
                    Cassie.Message("SCP 1 0 6 RECONTAINMENT SEQUENCE COMMENCING IN 3 . 2 . 1 . ", false, true);
                    Timing.CallDelayed(8f, () =>
                    {
                        var rh = ReferenceHub.GetHub(PlayerManager.localPlayer);
                        foreach (var player in RealPlayers.Get(RoleType.Scp106))
                            player.ReferenceHub.scp106PlayerScript.Contain(new Footprinting.Footprint(rh));
                        rh.playerInteract.RpcContain106(rh.gameObject);
                        Timing.CallDelayed(10f, () => this.OnEnd("<color=orange>Klasa D wygrywa!</color>"));
                    });
                });
            }
        }

        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (ev.GrenadeType == GrenadeType.Flashbang)
            {
                foreach (var player in RealPlayers.List.Where(x => x.Role == RoleType.Scp106))
                {
                    if (Vector3.Distance(player.Position, ev.Grenade.transform.position) < 5)
                        player.Position = RoleType.Scp106.GetRandomSpawnProperties().Item1;
                }
            }
        }
    }
}
