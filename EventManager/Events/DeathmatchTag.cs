// -----------------------------------------------------------------------
// <copyright file="DeathmatchTag.cs" company="Mistaken">
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
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class DeathmatchTag : EventBase
    {
        public override string Id => "dmt";

        public override string Description => "Deathmatch";

        public override string Name => "DeathmatchTag";

        public Dictionary<string, string> Translations => new ()
        {
            { "CI", "Twoim zadanie jest zabicie członków <color=blue>MFO</color>, znajdziesz ich w <color=yellow>Heavy Containment Zone</color>" },
            { "MTF", "Twoim zadanie jest zabicie członków <color=green>CI</color>, znajdziesz ich w <color=yellow>Entrance Zone</color>" },
        };

        public override void Initialize()
        {
            Map.Pickups.ToList().ForEach(x => x.Destroy());
            API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            this.tickets["MTF"] = 0;
            this.tickets["CI"] = 0;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            foreach (var door in Door.List)
            {
                if (door.Type == DoorType.GateA || door.Type == DoorType.GateB)
                    door.ChangeLock(DoorLockType.AdminCommand);
                else if (door.Type == DoorType.CheckpointEntrance)
                {
                    door.ChangeLock(DoorLockType.DecontEvacuate);
                    door.IsOpen = true;
                }
            }

            foreach (var e in Exiled.API.Features.Lift.List)
            {
                if (e.Type == ElevatorType.LczA || e.Type == ElevatorType.LczB)
                    e.IsLocked = true;
            }
        }

        public override void Deinitialize()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
        }

        private readonly Dictionary<string, int> tickets = new ()
        {
            { "CI", 0 },
            { "MTF", 0 },
        };

        private void Server_RoundStarted()
        {
            int i = 0;
            foreach (var player in RealPlayers.RandomList)
            {
                if (i % 2 != 0)
                {
                    switch (Random.Range(0, 3))
                    {
                        case 0:
                            player.SlowChangeRole(this.RandomTeamRole(Team.MTF), RoleType.Scp93953.GetRandomSpawnProperties().Item1);
                            break;
                        case 1:
                            player.SlowChangeRole(this.RandomTeamRole(Team.MTF), RoleType.Scp096.GetRandomSpawnProperties().Item1);
                            break;
                        case 2:
                            player.SlowChangeRole(this.RandomTeamRole(Team.MTF), Door.List.First(d => d.Type == DoorType.HczArmory).Position + (Vector3.up * 2));
                            break;
                    }

                    player.Broadcast(8, EventManager.EMLB + this.Translations["MTF"]);
                }
                else
                {
                    player.SlowChangeRole(this.RandomTeamRole(Team.CHI), RoleType.FacilityGuard.GetRandomSpawnProperties().Item1);
                    player.Broadcast(8, EventManager.EMLB + this.Translations["CI"]);
                }

                i++;
            }
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (this.tickets["MTF"] >= 25 || RealPlayers.Get(Team.CHI).Count() == 0)
                this.OnEnd("<color=blue>MFO</color> wygrywa!");
            else if (this.tickets["CI"] >= 25 || RealPlayers.Get(Team.MTF).Count() == 0)
                this.OnEnd("<color=green>CI</color> wygrywa!");
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (ev.IsAllowed)
            {
                var team = ev.Target.Role.Team;
                if (team == Team.CHI)
                    this.tickets["MTF"] += 1;
                else
                    this.tickets["CI"] += 1;

                ev.Target.Broadcast(5, EventManager.EMLB + "Za chwilę się odrodzisz...");
                Timing.CallDelayed(5f, () =>
                {
                    Vector3 respPoint;
                    if (team == Team.MTF)
                        respPoint = RoleType.FacilityGuard.GetRandomSpawnProperties().Item1;
                    else
                    {
                        respPoint = Random.Range(0, 3) switch
                        {
                            0 => RoleType.Scp93953.GetRandomSpawnProperties().Item1,
                            1 => RoleType.Scp096.GetRandomSpawnProperties().Item1,
                            _ => Door.List.First(d => d.Type == DoorType.HczArmory).Position + (Vector3.up * 2),
                        };
                    }

                    ev.Target.SlowChangeRole(team == Team.CHI ? this.RandomTeamRole(Team.MTF) : this.RandomTeamRole(Team.CHI), respPoint);
                });
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            Timing.CallDelayed(1f, () =>
            {
                if (ev.Player.Role.Team == Team.MTF)
                    ev.Player.RemoveItem(ev.Player.Items.FirstOrDefault(x => x.Type == ItemType.GrenadeHE));
            });
        }
    }
}
