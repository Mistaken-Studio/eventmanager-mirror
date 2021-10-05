// -----------------------------------------------------------------------
// <copyright file="DeathmatchTag.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Mistaken.API;
using Mistaken.EventManager.EventCreator;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class DeathmatchTag : IEMEventClass
    {
        public override string Id => "dmt";

        public override string Description => "Blank event";

        public override string Name => "DeathmatchTag";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "CI", "Twoim zadanie jest zabicie członków <color=blue>MFO</color>, znajdziesz ich w <color=yellow>Heavy Containment Zone</color>" },
            { "MTF", "Twoim zadanie jest zabicie członków <color=green>CI</color>, znajdziesz ich w <color=yellow>Entrance Zone</color>" },
        };

        public override void OnIni()
        {
            MapGeneration.InitiallySpawnedItems.Singleton.ClearAll();
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
        }

        private readonly Dictionary<string, int> tickets = new Dictionary<string, int>()
        {
            { "CI", 0 },
            { "MTF", 0 },
        };

        private void Server_RoundStarted()
        {
            this.tickets["MTF"] = 0;
            this.tickets["CI"] = 0;
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            foreach (var door in Map.Doors)
            {
                var doorType = door.Type;
                if (doorType == DoorType.GateA || doorType == DoorType.GateB)
                    door.ChangeLock(DoorLockType.AdminCommand);
                else if (doorType == DoorType.CheckpointEntrance)
                {
                    door.ChangeLock(DoorLockType.DecontEvacuate);
                    door.IsOpen = true;
                }
            }

            foreach (var e in Map.Lifts)
            {
                var elevatorType = e.Type();
                if (elevatorType == ElevatorType.LczA || elevatorType == ElevatorType.LczB)
                    e.Network_locked = true;
            }

            int i = 0;
            foreach (var player in RealPlayers.RandomList)
            {
                if (i % 2 != 0)
                {
                    switch (UnityEngine.Random.Range(0, 3))
                    {
                        case 0:
                            player.SlowChangeRole(RoleType.NtfSergeant, RoleType.Scp93953.GetRandomSpawnProperties().Item1);
                            break;
                        case 1:
                            player.SlowChangeRole(RoleType.NtfSergeant, RoleType.Scp096.GetRandomSpawnProperties().Item1);
                            break;
                        case 2:
                            player.SlowChangeRole(RoleType.NtfSergeant, Map.Doors.First(d => d.Type == DoorType.HczArmory).Position + (Vector3.up * 2));
                            break;
                    }

                    player.Broadcast(10, EventManager.EMLB + this.Translations["MTF"]);
                }
                else
                {
                    player.SlowChangeRole(RoleType.ChaosRifleman, RoleType.FacilityGuard.GetRandomSpawnProperties().Item1);
                    player.Broadcast(10, EventManager.EMLB + this.Translations["CI"]);
                }

                i++;
            }
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (ev.IsAllowed)
            {
                var role = ev.Target.Role;
                if (role == RoleType.ChaosRifleman)
                    this.tickets["MTF"] += 1;
                else
                    this.tickets["CI"] += 1;
                if (this.tickets["MTF"] >= 35 || RealPlayers.List.Count(x => x != ev.Target && x.Role == RoleType.ChaosRifleman) == 0)
                    this.OnEnd(null, "<color=blue>MFO</color> wygrywa!");
                else if (this.tickets["CI"] >= 35 || RealPlayers.List.Count(x => x != ev.Target && x.Role == RoleType.NtfSergeant) == 0)
                    this.OnEnd(null, "<color=green>CI</color> wygrywa!");

                ev.Target.Broadcast(5, EventManager.EMLB + "Za chwilę się odrodzisz!");
                MEC.Timing.CallDelayed(5f, () =>
                {
                    Vector3 respPoint;
                    if (role == RoleType.NtfSergeant)
                        respPoint = RoleType.FacilityGuard.GetRandomSpawnProperties().Item1;
                    else
                    {
                        switch (UnityEngine.Random.Range(0, 3))
                        {
                            case 0:
                                respPoint = RoleType.Scp93953.GetRandomSpawnProperties().Item1;
                                break;
                            case 1:
                                respPoint = RoleType.Scp096.GetRandomSpawnProperties().Item1;
                                break;
                            default:
                                respPoint = Map.Doors.First(d => d.Type == DoorType.HczArmory).Position + (Vector3.up * 2);
                                break;
                        }
                    }

                    ev.Target.SlowChangeRole(role == RoleType.ChaosRifleman ? RoleType.NtfSergeant : RoleType.ChaosRifleman, respPoint);
                });
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player.Role == RoleType.NtfSergeant)
                MEC.Timing.CallDelayed(1f, () => ev.Player.RemoveItem(ev.Player.Items.First(x => x.Type == ItemType.GrenadeHE)));
        }
    }
}
