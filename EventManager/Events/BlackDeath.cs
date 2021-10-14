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
using Interactables.Interobjects.DoorUtils;
using Mistaken.API;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class BlackDeath :
        EventCreator.IEMEventClass
    {
        public override string Id => "bdeath";

        public override string Description => "BlackDeath event";

        public override string Name => "BlackDeath";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "scp", string.Empty },
            { "d", string.Empty },
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Map.GeneratorActivated += this.Map_GeneratorActivated;
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Map_ExplodingGrenade;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.EnteringPocketDimension += this.Player_EnteringPocketDimension;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            Exiled.Events.Handlers.Player.ActivatingGenerator += this.Player_ActivatingGenerator;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.EnteringPocketDimension -= this.Player_EnteringPocketDimension;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Map_ExplodingGrenade;
            Exiled.Events.Handlers.Map.GeneratorActivated -= this.Map_GeneratorActivated;
            Exiled.Events.Handlers.Player.ActivatingGenerator -= this.Player_ActivatingGenerator;
        }

        private void Server_RoundStarted()
        {
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            foreach (var door in Map.Doors)
            {
                var doorType = door.Type;
                if (doorType == DoorType.HczArmory)
                {
                    door.ChangeLock(DoorLockType.AdminCommand);
                    door.IsOpen = true;
                }
                else if (door.Nametag != string.Empty && !(doorType == DoorType.HIDLeft || doorType == DoorType.HIDRight || doorType == DoorType.Scp106Primary || doorType == DoorType.Scp106Secondary || doorType == DoorType.Scp106Bottom))
                    door.ChangeLock(DoorLockType.AdminCommand);
            }

            foreach (var elevator in Map.Lifts)
            {
                elevator.Network_locked = true;
            }

            var rooms = Map.Rooms.ToList();
            rooms.RemoveAll(r => r.Zone != ZoneType.HeavyContainment);
            for (int i = 0; i < 5; i++)
            {
                if (rooms.Count == 0)
                    break;
                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];
                new Item(ItemType.KeycardNTFCommander).Spawn(room.Position + Vector3.up);
                rooms.Remove(room);
            }

            for (int i = 0; i < 10; i++)
            {
                if (rooms.Count == 0)
                    break;
                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];
                new Item(ItemType.GrenadeFlash).Spawn(room.Position + Vector3.up);
                rooms.Remove(room);
            }

            for (int i = 0; i < 15; i++)
            {
                if (rooms.Count == 0)
                    break;
                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)];
                new Item(ItemType.Flashlight).Spawn(room.Position + Vector3.up);
                rooms.Remove(room);
            }

            Map.TurnOffAllLights(float.MaxValue);
            var players = RealPlayers.List.ToList();
            var scp = players[UnityEngine.Random.Range(0, players.Count())];
            scp.SlowChangeRole(RoleType.Scp106);
            scp.Broadcast(10, EventManager.EMLB + this.Translations["scp"]);
            players.Remove(scp);
            foreach (var player in players)
            {
                player.SlowChangeRole(RoleType.ClassD);
                player.Broadcast(10, EventManager.EMLB + this.Translations["d"]);
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            var players = RealPlayers.List.Where(p => p.Role != RoleType.Scp106 && p.Role != RoleType.Spectator && p.Id != ev.Player.Id).Count();
            if (ev.Player.Role == RoleType.ClassD)
                ev.Player.Position = Map.Doors.First(d => d.Type == DoorType.HczArmory).Base.transform.position + Vector3.up;
            else if (ev.Player.Role == RoleType.Spectator && players == 0)
                this.OnEnd("<color=red>SCP 106 wygrywa!</color>");
        }

        private void Player_ActivatingGenerator(Exiled.Events.EventArgs.ActivatingGeneratorEventArgs ev)
        {
            ev.Generator._totalActivationTime = 180;
        }

        private void Player_EnteringPocketDimension(Exiled.Events.EventArgs.EnteringPocketDimensionEventArgs ev)
        {
            ev.Player.Kill(DamageTypes.Scp106);
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (ev.Target.Role == RoleType.Scp106)
                this.OnEnd("<color=orange>Klasa D wygrywa!</color>");
        }

        private void Map_GeneratorActivated(Exiled.Events.EventArgs.GeneratorActivatedEventArgs ev)
        {
            if (Map.ActivatedGenerators > 4)
            {
                Cassie.Message("ALL GENERATORS HAS BEEN SUCCESSFULLY ENGAGED . SCP 1 0 6 RECONTAINMENT SEQUENCE COMMENCING IN T MINUS 1 MINUTE", false, true);
                MEC.Timing.CallDelayed(60, () =>
                {
                    Cassie.Message("SCP 1 0 6 RECONTAINMENT SEQUENCE COMMENCING IN 3 . 2 . 1 . ", false, true);
                    MEC.Timing.CallDelayed(8, () =>
                    {
                        var rh = ReferenceHub.GetHub(PlayerManager.localPlayer);
                        foreach (var player in RealPlayers.Get(RoleType.Scp106))
                            player.ReferenceHub.scp106PlayerScript.Contain(rh);
                        rh.playerInteract.RpcContain106(rh.gameObject);
                        MEC.Timing.CallDelayed(10, () =>
                        {
                            this.OnEnd("<color=orange>Klasa D wygrywa!</color>");
                        });
                    });
                });
            }
        }

        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            Log.Debug(ev.Grenade.name);
            if (ev.Grenade.name == "FLASHBANG")
            {
                foreach (var player in RealPlayers.List.Where(p => p.Role == RoleType.Scp106))
                {
                    if (Vector3.Distance(player.Position, ev.Grenade.transform.position) < 5)
                        player.Position = RoleType.Scp106.GetRandomSpawnProperties().Item1;
                }
            }
        }
    }
}
