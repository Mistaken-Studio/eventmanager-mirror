// -----------------------------------------------------------------------
// <copyright file="CodeWhite.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using Mistaken.API;
using Mistaken.EventManager;
using Mistaken.EventManager.EventCreator;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class CodeWhite : IEMEventClass
    {
        public override string Id => "cw";

        public override string Description => "Code White";

        public override string Name => "CodeWhite";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>
        {
            // { "", "" }
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            GameObject.FindObjectOfType<Respawning.RespawnManager>().enabled = false;
            Round.IsLocked = true;
            var doors = Map.Doors;
            foreach (Door door in doors)
            {
                if (door.Type == DoorType.GateA || door.Type == DoorType.GateB || door.Type == DoorType.NukeSurface || door.Type == DoorType.Scp106Primary || door.Type == DoorType.Scp106Secondary || door.Type == DoorType.Scp106Bottom)
                {
                    door.ChangeLock(DoorLockType.AdminCommand);
                    door.IsOpen = false;
                }
                else if (door.Type == DoorType.Scp049Armory)
                {
                    door.IsOpen = true;
                    door.ChangeLock(DoorLockType.AdminCommand);
                }
            }
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
        }

        private bool escaped = false;

        private bool dt = false;

        private Player scientist = null;

        private void Server_RoundStarted()
        {
            var doors = Map.Doors.ToList();
            Timing.CallDelayed(120, () =>
            {
                Cassie.Message("PITCH_.45 .G2 PITCH_0.94 ATTENTION ALL MTFUNIT NU 7 . GATE A AND B LOCKDOWN DEACTIVATED .G2 DETECTED CODE WHITE PITCH_0.1 .G3", false, true);
                foreach (Door door in doors)
                {
                    if (door.Type == DoorType.GateA || door.Type == DoorType.GateB)
                        door.IsOpen = true;
                }
            });
            Timing.CallDelayed(2, () =>
            {
                int rep = 0;
                var players = Player.List.Count();
                var nofaggs = Player.List.Where(x => x.Role == RoleType.Scientist || x.Role == RoleType.FacilityGuard || x.Role == RoleType.ChaosRifleman).ToArray();
                if (players > 4)
                {
                    foreach (Player player in Player.List.Where(x => x.Role == RoleType.ClassD || x.Team == Team.SCP))
                    {
                        player.SlowChangeRole(RoleType.ChaosRifleman, doors.First(x => x.Type == DoorType.Scp173Armory).Position + (Vector3.up * 2));
                    }

                    this.scientist = nofaggs[UnityEngine.Random.Range(0, nofaggs.Count())];
                    this.scientist.SlowChangeRole(RoleType.Scientist, doors.First(x => x.Type == (UnityEngine.Random.Range(0, 2) == 0 ? DoorType.Scp049Armory : DoorType.Scp096)).Position + (Vector3.up * 2));

                    foreach (Player faggot in nofaggs)
                    {
                        if (this.scientist.Id != faggot.Id)
                        {
                            switch (UnityEngine.Random.Range(0, 3))
                            {
                                case 0:
                                    faggot.SlowChangeRole(RoleType.NtfPrivate);
                                    break;
                                case 1:
                                    faggot.SlowChangeRole(RoleType.NtfSergeant);
                                    break;
                                case 2:
                                    faggot.SlowChangeRole(RoleType.NtfCaptain);
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (Player player in RealPlayers.RandomList)
                    {
                        if (rep < 2)
                        {
                            player.SlowChangeRole(RoleType.ChaosRifleman, doors.First(x => x.Type == DoorType.Scp173Armory).Position + (Vector3.up * 2));
                            rep++;
                        }
                        else if (rep == 2)
                        {
                            player.SlowChangeRole(RoleType.NtfSergeant);
                            rep++;
                        }
                        else if (rep == 3)
                        {
                            player.SlowChangeRole(RoleType.Scientist, doors.First(x => x.Type == (UnityEngine.Random.Range(0, 2) == 0 ? DoorType.Scp049Armory : DoorType.Scp096)).Position + (Vector3.up * 2));
                        }
                    }
                }

                Timing.CallDelayed(5, () =>
                {
                    foreach (Player p in Player.List)
                    {
                        if (p.Role == RoleType.Scientist)
                        {
                            var items = p.Items.Where(x => x.Type == ItemType.KeycardScientist);
                            foreach (var item in items)
                            {
                                p.RemoveItem(item);
                            }

                            p.AddItem(ItemType.KeycardFacilityManager);
                            p.AddItem(ItemType.GunCOM18);
                            p.AddItem(ItemType.Medkit);
                            p.Ammo[ItemType.Ammo9x19] = 18;
                        }

                        if (p.Role == RoleType.NtfPrivate || p.Role == RoleType.NtfSergeant || p.Role == RoleType.NtfCaptain)
                        {
                            var items = p.Items.Where(x => x.Type == ItemType.KeycardNTFCommander || x.Type == ItemType.KeycardNTFLieutenant || x.Type == ItemType.KeycardNTFOfficer);
                            foreach (var item in items)
                            {
                                p.RemoveItem(item);
                            }

                            p.AddItem(ItemType.KeycardO5);
                        }

                        if (p.Role == RoleType.ChaosRifleman)
                        {
                            var items = p.Items.Where(x => x.Type == ItemType.KeycardChaosInsurgency);
                            foreach (var item in items)
                            {
                                p.RemoveItem(item);
                            }

                            p.AddItem(ItemType.KeycardO5);
                            p.AddItem(ItemType.Radio);
                        }
                    }
                });
            });
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            var doors = Map.Doors.ToList();
            var players = Player.List;
            if (ev.Player.Role == RoleType.ChaosRifleman)
            {
                ev.Player.Broadcast(10, "Jesteś <color=green>Rebeliantem Chaosu</color>. Twoim zadaniem jest zabicie <color=red>Dyrektora</color> i odparcie ataku/ów <color=blue>MFO</color>.");
            }

            if (ev.Player.Role == RoleType.Scientist && ev.Player.Id == this.scientist.Id)
            {
                ev.Player.Broadcast(10, "Jesteś <color=red>Dyrektorem Placówki</color>. Twoim zadaniem jest ucieczka z placówki. <color=green>Rebelia Chaosu</color> chce twojej śmierci.");
            }

            if (RealPlayers.Get(Team.RSC).Count() == 0 && ev.Player.Role != RoleType.NtfSpecialist && !this.escaped && !this.dt)
            {
                Cassie.Message("PITCH_.45.G2 PITCH_0.94 FACILITY SCAN INITIATED. . . . . .G2 SCAN COMPLETED.FACILITY MANAGER TERMINATED BELL_END", false, true);
                this.dt = true;
            }

            if (ev.Player.Role == RoleType.NtfSpecialist)
            {
                this.escaped = true;
                var specplayers = Player.List.Where(x => x.Role == RoleType.Spectator);
                ev.Player.SlowChangeRole(RoleType.Spectator);
                int i = 0;
                foreach (Player faggot in specplayers)
                {
                    if (i < 7)
                    {
                        switch (UnityEngine.Random.Range(0, 3))
                        {
                            case 0:
                                faggot.SlowChangeRole(RoleType.NtfPrivate);
                                break;
                            case 1:
                                faggot.SlowChangeRole(RoleType.NtfSergeant);
                                break;
                            case 2:
                                faggot.SlowChangeRole(RoleType.NtfCaptain);
                                break;
                        }
                    }

                    i++;
                }

                Round.IsLocked = false;
            }
        }
    }
}