// -----------------------------------------------------------------------
// <copyright file="TSL.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using HarmonyLib;
using InventorySystem.Items.Firearms.Ammo;
using InventorySystem.Items.ThrowableProjectiles;
using MEC;
using Mistaken.API;
using Mistaken.API.Commands;
using Mistaken.API.Extensions;
using NorthwoodLib.Pools;
using Scp914;
using UnityEngine;

#pragma warning disable SA1402 // File may only contain a single type

namespace Mistaken.EventManager.Events
{
    internal enum TSLClassType
    {
        None = -1,
        Innocent = 0,
        Traitor = 1,
        Detective = 2,
    }

    internal enum TSLItemType
    {
        None = -1,
        Armor = 0,
        Heavy_Armor = 1,
        ATTD = 2,
    }

    internal class TSL : EventBase
    {
        public override string Id => "tsl";

        public override string Description => "Trouble in Secret Laboratory";

        public override string Name => "TSL";

        public Dictionary<string, string> Translations => new ()
        {
            { "I_Info", "Jesteś <color=lime>Niewinnym</color>. Wspólnie z <color=#00B7EB>Detektywem/ami</color> musicie odgadnąć kto jest <color=red>Zdrajcą</color>. Statystyki pod komendą \".tsl round\"" },
            { "T_Info", "Jesteś <color=red>Zdrajcą</color>. Twoim zadaniem jest zabicie wszystkich <color=lime>Niewinnych</color> i <color=#00B7EB>Detektywów</color>. Dostęp do sklepu pod komendą \".tsl shop\"" },
            { "D_Info", "Jesteś <color=#00B7EB>Detektywem</color>. Musisz odgadnąć kto jest <color=red>Zdrajcą</color> pośród <color=lime>Niewinnych</color>. Dostęp do sklepu pod komendą \".tsl shop\"" },
        };

        public Dictionary<string, DateTime> Cooldowns { get; set; } = new Dictionary<string, DateTime>();

        public Dictionary<Player, (TSLClassType, int, List<TSLItemType>)> Players { get; private set; } = new Dictionary<Player, (TSLClassType, int, List<TSLItemType>)>();

        public Dictionary<string, (string, TSLClassType[], int, ItemType, TSLItemType)> ShopItems { get; } = new Dictionary<string, (string, TSLClassType[], int, ItemType, TSLItemType)>()
        {
            {
                "armor", ("Armor", new TSLClassType[] { TSLClassType.Detective, TSLClassType.Traitor }, 1, ItemType.None, TSLItemType.Armor)
            },
            {
                "heavyarmor", ("Heavy Armor", new TSLClassType[] { TSLClassType.Detective, TSLClassType.Traitor }, 3, ItemType.None, TSLItemType.Heavy_Armor)
            },
            {
                "attd", ("ATTD", new TSLClassType[] { TSLClassType.Traitor }, 2, ItemType.None, TSLItemType.ATTD)
            },
            {
                "scp018", ("SCP-018", new TSLClassType[] { TSLClassType.Traitor }, 2, ItemType.SCP018, TSLItemType.None)
            },
            {
                "usp", ("USP", new TSLClassType[] { TSLClassType.Traitor }, 2, ItemType.GunCOM18, TSLItemType.None)
            },
            {
                "frag", ("Frag", new TSLClassType[] { TSLClassType.Traitor }, 2, ItemType.GrenadeHE, TSLItemType.None)
            },
            {
                "deagle", ("Deagle", new TSLClassType[] { TSLClassType.Detective }, 2, ItemType.GunCOM18, TSLItemType.None)
            },
            {
                "adrenaline", ("Adrenaline", new TSLClassType[] { TSLClassType.Detective }, 1, ItemType.Adrenaline, TSLItemType.None)
            },
            {
                "scp500", ("SCP-500", new TSLClassType[] { TSLClassType.Detective }, 1, ItemType.SCP500, TSLItemType.None)
            },
        };

        public Dictionary<string, (string, int)> TraitorEvents { get; } = new Dictionary<string, (string, int)>()
        {
            {
                "blackout", ("zgasza światło w całej strefie na 15 sekund", 1)
            },
            {
                "grenade914", ("respi granat w wyjściu i wejściu SCP-914", 1)
            },
            {
                "lock914", ("zamyka drzwi SCP-914 na 15 sekund", 1)
            },
        };

        public override void Initialize()
        {
            this.Players.Clear();
            this.staticPlayers.Clear();
            this.traitorTesterUses = 0;
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            Map.Pickups.ToList().ForEach(x => x.Destroy());
            API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            PluginHandler.Harmony.Patch(typeof(Scp914Upgrader).GetMethod("Upgrade", new Type[] { typeof(Collider[]), typeof(Vector3), typeof(Scp914Mode), typeof(Scp914KnobSetting) }), new HarmonyMethod(typeof(Scp914UpgradePatch).GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static)));
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;

            // Exiled.Events.Handlers.Player.Shot += this.Player_Shot;
            Exiled.Events.Handlers.Player.Left += this.Player_Left;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
        }

        public override void Deinitialize()
        {
            PluginHandler.Harmony.Unpatch(typeof(Scp914Upgrader).GetMethod("Upgrade", new Type[] { typeof(Collider[]), typeof(Vector3), typeof(Scp914Mode), typeof(Scp914KnobSetting) }), HarmonyPatchType.Prefix);
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;

            // Exiled.Events.Handlers.Player.Shot -= this.Player_Shot;
            Exiled.Events.Handlers.Player.Left -= this.Player_Left;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
        }

        public string TSL_RoundInfo()
        {
            List<string> round = new ()
            {
                "<color=grey>Przydatne statystyki:</color>",
                $"<color=grey>Pozostało {this.Players.Count(x => x.Value.Item1 == TSLClassType.Traitor)} <color=red>Zdrajców</color>, {this.Players.Count(x => x.Value.Item1 == TSLClassType.Innocent)} <color=lime>Niewinnych</color>, {this.Players.Count(x => x.Value.Item1 == TSLClassType.Detective)} <color=#00B7EB>Detektywów</color></color>",
                "<color=grey>Lista Graczy:</color>",
            };

            foreach (var item in this.staticPlayers)
            {
                if (this.Players.ContainsKey(item.Key))
                {
                    switch (item.Value.Item2)
                    {
                        case TSLClassType.Innocent:
                            round.Add($"<color=grey>Gracz {item.Key.Nickname} - <color=lime>Niewinny</color></color>");
                            break;
                        case TSLClassType.Traitor:
                            round.Add($"<color=grey>Gracz {item.Key.Nickname} - <color=red>Zdrajca</color></color>");
                            break;
                        case TSLClassType.Detective:
                            round.Add($"<color=grey>Gracz {item.Key.Nickname} - <color=#00B7EB>Detektyw</color></color>");
                            break;
                        default:
                            round.Add($"<color=grey>Gracz {item.Key.Nickname} - Nieznany</color>");
                            break;
                    }
                }
                else
                {
                    switch (item.Value.Item1)
                    {
                        case TSLClassType.Innocent:
                            round.Add($"<color=grey>Gracz {item.Key.Nickname} był <color=lime>Niewinnym</color></color>");
                            break;
                        case TSLClassType.Traitor:
                            round.Add($"<color=grey>Gracz {item.Key.Nickname} był <color=red>Zdrajcą</color></color>");
                            break;
                        case TSLClassType.Detective:
                            round.Add($"<color=grey>Gracz {item.Key.Nickname} był <color=#00B7EB>Detektywem</color></color>");
                            break;
                        default:
                            round.Add($"<color=grey>Gracz {item.Key.Nickname} był Nieznanym</color>");
                            break;
                    }
                }
            }

            return string.Join("\n", round);
        }

        public void Scp914_UpgradingPlayers(HashSet<Player> players, Scp914KnobSetting setting)
        {
            if (this.traitorTesterUses >= (this.staticPlayers.Count / 1.6))
                return;
            if (setting == Scp914KnobSetting.OneToOne && players.Count == 1)
            {
                var player = players.First();
                if (this.Players.ContainsKey(player))
                {
                    switch (this.Players[player].Item1)
                    {
                        case TSLClassType.Traitor:
                            if (this.Players[player].Item3.Contains(TSLItemType.ATTD))
                            {
                                this.staticPlayers[player] = (this.Players[player].Item1, TSLClassType.Innocent);
                                this.Players[player].Item3.Remove(TSLItemType.ATTD);
                            }
                            else
                            {
                                this.staticPlayers[player] = (this.Players[player].Item1, TSLClassType.Traitor);
                                Room.List.First(x => x.Type == RoomType.Lcz914).TurnOffLights(5f);
                            }

                            break;
                        case TSLClassType.Innocent:
                            this.staticPlayers[player] = (this.Players[player].Item1, TSLClassType.Innocent);
                            break;
                        default:
                            return;
                    }
                }
            }

            this.traitorTesterUses++;
        }

        private readonly Dictionary<Player, (TSLClassType, TSLClassType)> staticPlayers = new ();

        private int traitorTesterUses = 0;

        private void Server_RoundStarted()
        {
            foreach (var door in Door.List)
            {
                if (door.Type == DoorType.CheckpointLczA || door.Type == DoorType.CheckpointLczB)
                    door.ChangeLock(DoorLockType.DecontLockdown);
                else if (door.Type == DoorType.Scp330 || door.Type == DoorType.Scp914Gate || door.Type == DoorType.Scp330Chamber || door.Type == DoorType.LczArmory)
                {
                    door.IsOpen = true;
                    door.ChangeLock(DoorLockType.AdminCommand);
                }
            }

            foreach (var e in Exiled.API.Features.Lift.List)
            {
                if (e.Name.StartsWith("El"))
                    e.IsLocked = true;
            }

            var rooms = Room.List.Where(x => x.Zone == ZoneType.LightContainment && x.Type != RoomType.Lcz173 && x.Type != RoomType.Lcz330).ToList();
            foreach (var item in rooms)
            {
                switch (UnityEngine.Random.Range(0, 11))
                {
                    case 0:
                        {
                            Item.Create(ItemType.GunCOM15).Spawn(item.Position + Vector3.up);
                            Item.Create(ItemType.ArmorLight).Spawn(item.Position + Vector3.up);
                            var ammo = (AmmoPickup)Item.Create(ItemType.Ammo9x19).Spawn(item.Position + Vector3.up).Base;
                            ammo.SavedAmmo = 24;
                            ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                            break;
                        }

                    case 1:
                        {
                            Item.Create(ItemType.GunCOM18).Spawn(item.Position + Vector3.up);
                            Item.Create(ItemType.ArmorLight).Spawn(item.Position + Vector3.up);
                            var ammo = (AmmoPickup)Item.Create(ItemType.Ammo9x19).Spawn(item.Position + Vector3.up).Base;
                            ammo.SavedAmmo = 36;
                            ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                            break;
                        }

                    case 2:
                        {
                            Item.Create(ItemType.GunRevolver).Spawn(item.Position + Vector3.up);
                            Item.Create(ItemType.ArmorLight).Spawn(item.Position + Vector3.up);
                            var ammo = (AmmoPickup)Item.Create(ItemType.Ammo44cal).Spawn(item.Position + Vector3.up).Base;
                            ammo.SavedAmmo = 16;
                            ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                            break;
                        }

                    case 8:
                    case 3:
                        {
                            Item.Create(ItemType.GunFSP9).Spawn(item.Position + Vector3.up);
                            Item.Create(ItemType.ArmorLight).Spawn(item.Position + Vector3.up);
                            var ammo = (AmmoPickup)Item.Create(ItemType.Ammo9x19).Spawn(item.Position + Vector3.up).Base;
                            ammo.SavedAmmo = 60;
                            ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                            break;
                        }

                    case 4:
                        {
                            Item.Create(ItemType.GunCrossvec).Spawn(item.Position + Vector3.up);
                            Item.Create(ItemType.ArmorCombat).Spawn(item.Position + Vector3.up);
                            var ammo = (AmmoPickup)Item.Create(ItemType.Ammo9x19).Spawn(item.Position + Vector3.up).Base;
                            ammo.SavedAmmo = 100;
                            ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                            break;
                        }

                    case 5:
                        {
                            Item.Create(ItemType.GunE11SR).Spawn(item.Position + Vector3.up);
                            Item.Create(ItemType.ArmorCombat).Spawn(item.Position + Vector3.up);
                            var ammo = (AmmoPickup)Item.Create(ItemType.Ammo556x45).Spawn(item.Position + Vector3.up).Base;
                            ammo.SavedAmmo = 80;
                            ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                            break;
                        }

                    case 6:
                        {
                            Item.Create(ItemType.GunAK).Spawn(item.Position + Vector3.up);
                            Item.Create(ItemType.ArmorCombat).Spawn(item.Position + Vector3.up);
                            var ammo = (AmmoPickup)Item.Create(ItemType.Ammo762x39).Spawn(item.Position + Vector3.up).Base;
                            ammo.SavedAmmo = 70;
                            ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                            break;
                        }

                    case 7:
                        {
                            Item.Create(ItemType.GunShotgun).Spawn(item.Position + Vector3.up);
                            Item.Create(ItemType.ArmorCombat).Spawn(item.Position + Vector3.up);
                            var ammo = (AmmoPickup)Item.Create(ItemType.Ammo12gauge).Spawn(item.Position + Vector3.up).Base;
                            ammo.SavedAmmo = 28;
                            ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                            break;
                        }

                    /*case 8:
                        {
                            Item.Create(ItemType.GunLogicer).Spawn(item.Position + Vector3.up);
                            Item.Create(ItemType.ArmorHeavy).Spawn(item.Position + Vector3.up);
                            var ammo = (AmmoPickup)Item.Create(ItemType.Ammo762x39).Spawn(item.Position + Vector3.up).Base;
                            ammo.SavedAmmo = 200;
                            ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                            break;
                        }*/
                    case 9:
                        {
                            Item.Create(ItemType.Medkit).Spawn(item.Position + Vector3.up);
                            Item.Create(ItemType.Painkillers).Spawn(item.Position + Vector3.up);
                            break;
                        }

                    case 10:
                        {
                            Item.Create(ItemType.Medkit).Spawn(item.Position + Vector3.up);
                            Item.Create(ItemType.Painkillers).Spawn(item.Position + Vector3.up);
                            break;
                        }
                }
            }

            double converter = 0.25;
            foreach (Player player in RealPlayers.RandomList)
            {
                if (converter % 1 == 0)
                {
                    this.Players.Add(player, (TSLClassType.Traitor, 1, new List<TSLItemType>()));
                    this.staticPlayers.Add(player, (TSLClassType.Traitor, TSLClassType.None));
                }
                else if (converter % 1.75 == 0)
                {
                    this.Players.Add(player, (TSLClassType.Detective, 2, new List<TSLItemType>()));
                    this.staticPlayers.Add(player, (TSLClassType.Detective, TSLClassType.Detective));
                }
                else
                {
                    this.Players.Add(player, (TSLClassType.Innocent, 0, new List<TSLItemType>()));
                    this.staticPlayers.Add(player, (TSLClassType.Innocent, TSLClassType.None));
                }

                player.SlowChangeRole(RoleType.ClassD, rooms[UnityEngine.Random.Range(0, rooms.Count)].Position + (Vector3.up * 2));
                converter += 0.25;
            }

            Timing.CallDelayed(10f, () => Timing.RunCoroutine(this.UpdateHealthRanks()));
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (this.Players.TryGetValue(ev.Player, out (TSLClassType, int, List<TSLItemType>) items))
            {
                switch (items.Item1)
                {
                    case TSLClassType.Innocent:
                        ev.Player.Broadcast(8, EventManager.EMLB + this.Translations["I_Info"]);
                        break;
                    case TSLClassType.Traitor:
                        ev.Player.Broadcast(8, EventManager.EMLB + this.Translations["T_Info"]);
                        Timing.CallDelayed(2f, () =>
                        {
                            foreach (var item in this.Players.Where(x => x.Value.Item1 == TSLClassType.Traitor))
                            {
                                CustomInfoHandler.SetTarget(ev.Player, "tsl_traitors", "<color=red>Zdrajca</color>", item.Key);
                                ev.Player.ChangeAppearance(item.Key, RoleType.Tutorial);
                                ev.Player.AddItem(ItemType.Radio);
                            }
                        });
                        break;
                    case TSLClassType.Detective:
                        ev.Player.Broadcast(8, EventManager.EMLB + this.Translations["D_Info"]);
                        CustomInfoHandler.Set(ev.Player, "tsl_detective", "<color=#00B7EB>Detektyw</color>");
                        Timing.CallDelayed(2f, () => ev.Player.ChangeAppearance(RoleType.FacilityGuard));
                        break;
                }
            }

            CustomInfoHandler.Set(ev.Player, "tsl_health", "<color=#22de18>Wyleczony</color>");
            ev.Player.InfoArea &= ~PlayerInfoArea.Role;
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (this.Players.TryGetValue(ev.Target, out (TSLClassType, int, List<TSLItemType>) items))
            {
                if (items.Item3.Contains(TSLItemType.Heavy_Armor))
                    ev.Amount *= 0.7f;
                else if (items.Item3.Contains(TSLItemType.Armor))
                    ev.Amount *= 0.9f;
            }
        }

        private IEnumerator<float> UpdateHealthRanks()
        {
            while (this.Active)
            {
                yield return Timing.WaitForSeconds(5f);
                foreach (var player in this.Players.Keys)
                {
                    if (this.Players[player].Item1 == TSLClassType.Detective)
                        continue;
                    if (!player.IsConnected)
                        continue;
                    switch (player.Health)
                    {
                        case float n when n < 100 && n >= 80:
                            CustomInfoHandler.Set(player, "tsl_health", "<color=#dce858>Lekko Ranny</color>");
                            break;

                        case float n when n < 80 && n >= 55:
                            CustomInfoHandler.Set(player, "tsl_health", "<color=#d69a33>Ranny</color>");
                            break;

                        case float n when n < 55 && n >= 20:
                            CustomInfoHandler.Set(player, "tsl_health", "<color=#92522>Poważnie Ranny</color>");
                            break;

                        case float n when n < 20:
                            CustomInfoHandler.Set(player, "tsl_health", "<color=#525252>Bliski śmierci</color>");
                            break;
                    }
                }
            }
        }

        private void Player_Shot(Exiled.Events.EventArgs.ShotEventArgs ev)
        {
            /*if (ev.Shooter.CurrentItem.Type == ItemType.GunCOM18)
            {
                if (ev.Shooter.CurrentItem.modBarrel == 1)
                    ev.Damage *= 1.7f;
                else if (ev.Shooter.CurrentItem.modBarrel == 2)
                {
                    ev.Damage *= 10f;
                    ev.Shooter.RemoveItem(ev.Shooter.CurrentItem);
                }
            }*/
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (!ev.Target.IsConnected)
                return;
            CustomInfoHandler.Set(ev.Target, "tsl_health", string.Empty);
            if (this.Players.TryGetValue(ev.Target, out (TSLClassType, int, List<TSLItemType>) targetItems))
            {
                switch (targetItems.Item1)
                {
                    case TSLClassType.Innocent:
                        {
                            this.Players.Remove(ev.Target);
                            if (this.Players.TryGetValue(ev.Killer, out (TSLClassType, int, List<TSLItemType>) killerItems) && killerItems.Item1 == TSLClassType.Traitor)
                            {
                                this.Players[ev.Killer] = (killerItems.Item1, killerItems.Item2 + 1, killerItems.Item3);
                                ev.Killer.Broadcast(4, "Otrzymałeś 1 kredyt");
                            }
                        }

                        break;
                    case TSLClassType.Traitor:
                        {
                            this.Players.Remove(ev.Target);
                            if (this.Players.TryGetValue(ev.Killer, out (TSLClassType, int, List<TSLItemType>) killerItems) && killerItems.Item1 == TSLClassType.Detective)
                            {
                                this.Players[ev.Killer] = (killerItems.Item1, killerItems.Item2 + 1, killerItems.Item3);
                                ev.Killer.Broadcast(4, "Otrzymałeś 1 kredyt");
                            }

                            foreach (var item in this.staticPlayers.Where(x => x.Value.Item1 == TSLClassType.Traitor))
                            {
                                if (item.Key.IsConnected)
                                    CustomInfoHandler.SetTarget(ev.Target, "tsl_traitors", string.Empty, item.Key);
                            }
                        }

                        break;
                    case TSLClassType.Detective:
                        {
                            CustomInfoHandler.Set(ev.Target, "tsl_detective", string.Empty);
                            this.Players.Remove(ev.Target);
                            if (this.Players.TryGetValue(ev.Killer, out (TSLClassType, int, List<TSLItemType>) killerItems) && killerItems.Item1 == TSLClassType.Traitor)
                            {
                                this.Players[ev.Killer] = (killerItems.Item1, killerItems.Item2 + 1, killerItems.Item3);
                                ev.Killer.Broadcast(4, "Otrzymałeś 2 kredyty");
                            }
                        }

                        break;
                }
            }

            if (this.Players.Count(x => x.Value.Item1 == TSLClassType.Innocent || x.Value.Item1 == TSLClassType.Detective) == 0 && this.Players.Count(x => x.Value.Item1 == TSLClassType.Traitor) != 0)
            {
                this.TSL_RoundSummary("<color=red>Zdrajcy</color>");
                this.OnEnd("<color=red>Zdrajcy</color> wygrywają!");
            }
            else if (this.Players.Count(x => x.Value.Item1 == TSLClassType.Innocent || x.Value.Item1 == TSLClassType.Detective) != 0 && this.Players.Count(x => x.Value.Item1 == TSLClassType.Traitor) == 0)
            {
                this.TSL_RoundSummary("<color=lime>Niewinni</color>");
                this.OnEnd("<color=lime>Niewinni</color> wygrywają!");
            }
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (ev.IsAllowed && this.Players.ContainsKey(ev.Target))
            {
                if (this.Players[ev.Target].Item1 == TSLClassType.Traitor)
                    ev.Target.RemoveItem(ev.Target.Items.FirstOrDefault(x => x.Type == ItemType.Radio));
            }
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if (this.Players.ContainsKey(ev.Player))
            {
                this.Players.Remove(ev.Player);
                this.staticPlayers.Remove(ev.Player);
            }
        }

        private void TSL_RoundSummary(string winner)
        {
            List<string> roundSummary = new ()
            {
                $"Rundę wygrywają {winner}!",
                "Klasy graczy:",
            };

            foreach (var item in this.staticPlayers)
            {
                switch (item.Value.Item1)
                {
                    case TSLClassType.Innocent:
                        roundSummary.Add($"Gracz {item.Key.Nickname} był <color=lime>Niewinnym</color>");
                        break;
                    case TSLClassType.Traitor:
                        roundSummary.Add($"Gracz {item.Key.Nickname} był <color=red>Zdrajcą</color>");
                        break;
                    case TSLClassType.Detective:
                        roundSummary.Add($"Gracz {item.Key.Nickname} był <color=#00B7EB>Detektywem</color>");
                        break;
                }
            }

            foreach (Player player in RealPlayers.List.ToArray())
                player.SendConsoleMessage(string.Join("\n", roundSummary), "green");
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    internal class ShopCommandHandler : IBetterCommand
    {
        public override string Command => "tsl";

        public override string Description => "TSL command";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = Player.Get(sender);
            if (EventManager.CurrentEvent is not TSL tsl)
                return new string[] { "Ten event aktualnie nie trwa" };

            this.tsl = tsl;
            player.SendConsoleMessage("TSL | " + this.Execute(player, args), "green");
            success = true;
            return new string[] { "Done" };
        }

        private TSL tsl;

        private string Execute(Player player, string[] args)
        {
            if (args.Length == 0)
                return "Musisz sprecyzować czy chcesz użyć \"Round\", \"Event\" bądź \"Shop\"";

            return args[0].ToLower() switch
            {
                "round" => this.tsl.TSL_RoundInfo(),
                "event" => this.RequestEvent(player, args.Skip(1).ToArray()),
                "shop" => this.RequestItem(player, args.Skip(1).ToArray()),
                _ => "Nieznany argument. Użyj \"Round\", \"Event\" bądź \"Shop\"",
            };
        }

        private string RequestItem(Player player, string[] args)
        {
            if ((this.tsl.Players.TryGetValue(player, out (TSLClassType, int, List<TSLItemType>) playerItems) && playerItems.Item1 == TSLClassType.Traitor) || playerItems.Item1 == TSLClassType.Detective)
            {
                if (args.Length != 0)
                {
                    if (this.tsl.ShopItems.TryGetValue(string.Join(string.Empty, args).ToLower(), out (string, TSLClassType[], int, ItemType, TSLItemType) items))
                    {
                        if (playerItems.Item2 >= items.Item3)
                        {
                            if (items.Item2.Contains(playerItems.Item1))
                            {
                                if (!playerItems.Item3.Contains(items.Item5))
                                {
                                    if (items.Item5 != TSLItemType.None)
                                    {
                                        List<TSLItemType> tslItems = new ()
                                        {
                                            items.Item5,
                                        };

                                        foreach (var element in playerItems.Item3)
                                            tslItems.Add(element);
                                        this.tsl.Players[player] = (playerItems.Item1, playerItems.Item2 - items.Item3, tslItems);
                                        return $"Kupiłeś {items.Item1}";
                                    }
                                    else
                                    {
                                        if (player.Items.Count < 8)
                                        {
                                            if (items.Item4 == ItemType.GunCOM18)
                                            {
                                                // TODO: ZMIENIĆ TO JAK NAJSZYBCIEJ
                                                if (playerItems.Item1 == TSLClassType.Traitor)
                                                {
                                                    player.AddItem(items.Item4);
                                                    player.Ammo[ItemType.Ammo9x19] += 36;
                                                }
                                                else
                                                {
                                                    player.AddItem(items.Item4);
                                                    player.Ammo[ItemType.Ammo9x19] += 36;
                                                }
                                            }
                                            else
                                                player.AddItem(items.Item4);

                                            return $"Kupiłeś {items.Item1}";
                                        }
                                        else
                                            return "Nie masz miejsca w ekwipunku by zakupić ten przedmiot";
                                    }
                                }
                                else
                                    return "Już posiadasz ten przedmiot";
                            }
                            else
                                return "Ten przedmiot nie jest dostępny dla Ciebie";
                        }
                        else
                            return "Nie masz wystarczającej ilości kredytów";
                    }
                    else
                        return this.GetShopUsage(playerItems.Item1, playerItems.Item2);
                }
                else
                    return this.GetShopUsage(playerItems.Item1, playerItems.Item2);
            }
            else
                return "Nie jesteś <color=red>Zdrajcą</color> lub <color=#00B7EB>Detektywem</color> by użyć tej komendy";
        }

        private string RequestEvent(Player player, string[] args)
        {
            if (this.tsl.Players.TryGetValue(player, out (TSLClassType, int, List<TSLItemType>) playerItems) && playerItems.Item1 == TSLClassType.Traitor)
            {
                if (args.Length != 0)
                {
                    var arg = string.Join(string.Empty, args).ToLower();
                    if (this.tsl.TraitorEvents.TryGetValue(arg, out (string, int) items))
                    {
                        if (playerItems.Item2 >= items.Item2)
                        {
                            if (!this.tsl.Cooldowns.TryGetValue(arg, out DateTime time))
                                this.tsl.Cooldowns.Add(arg, DateTime.Now);
                            if (DateTime.Now < time)
                                return $"Musisz odczekać jeszcze {(time - DateTime.Now).Seconds} sekund";
                            else
                            {
                                this.tsl.Cooldowns[arg] = DateTime.Now.AddSeconds(90);
                                this.tsl.Players[player] = (playerItems.Item1, playerItems.Item2 - items.Item2, playerItems.Item3);
                                switch (arg)
                                {
                                    case "blackout":
                                        {
                                            Map.TurnOffAllLights(15f);

                                            return "Pomyślnie zgasiłeś światła na 15 sekund";
                                        }

                                    case "grenade914":
                                        {
                                            List<Vector3> positions = new () { new Vector3(-7f, 1f, 6.3f), new Vector3(-7.4f, 1f, 6.3f) };
                                            foreach (var pos in positions)
                                            {
                                                var room = Room.List.First(x => x.Type == RoomType.Lcz914);
                                                var basePos = room.Position;
                                                var offset = pos;
                                                offset = (player.CurrentRoom.transform.forward * -offset.x) + (player.CurrentRoom.transform.right * -offset.z) + (Vector3.up * offset.y);
                                                basePos += offset;
                                                var grenade = (ExplosionGrenade)Item.Create(ItemType.GrenadeHE).Spawn(basePos).Base;
                                                grenade.PreviousOwner = new Footprinting.Footprint(player.ReferenceHub);
                                                grenade.GetComponent<TimeGrenade>()._fuseTime = 0.1f;
                                                grenade.ServerActivate();
                                            }

                                            return "Pomyślnie wysadziłeś wnętrza komór SCP-914";
                                        }

                                    case "lock914":
                                        {
                                            var door = Door.List.First(x => x.Type == DoorType.Scp914Gate);
                                            door.IsOpen = false;
                                            Timing.CallDelayed(15f, () => door.IsOpen = true);

                                            return "Pomyślnie zablokowałeś bramę SCP-914 na 15 sekund";
                                        }

                                    default:
                                        return "Coś poszło nie tak";
                                }
                            }
                        }
                        else
                            return "Nie masz wystarczającej ilości kredytów";
                    }
                    else
                        return this.GetEventUsage(playerItems.Item2);
                }
                else
                    return this.GetEventUsage(playerItems.Item2);
            }
            else
                return "Nie jesteś <color=red>Zdrajcą</color> by użyć tej komendy";
        }

        private string GetShopUsage(TSLClassType classType, int credits)
        {
            List<string> tor = new ()
            {
                "Przedmioty dostępne w sklepie:",
            };

            foreach (var item in this.tsl.ShopItems.Where(x => x.Value.Item2.Contains(classType)))
                tor.Add($"{item.Value.Item1} za {item.Value.Item3} kretydów");
            tor.Add($"Posiadasz {credits} kredytów");
            return string.Join("\n", tor);
        }

        private string GetEventUsage(int credits)
        {
            List<string> tor = new ()
            {
                "Dostępne eventy:",
            };
            foreach (var item in this.tsl.TraitorEvents)
                tor.Add($"{item.Key} - {item.Value.Item1} za {item.Value.Item2} kredytów");
            tor.Add($"Posiadasz {credits} kredytów");
            return string.Join("\n", tor);
        }
    }

    [HarmonyPatch(typeof(Scp914Upgrader), "Upgrade", new Type[] { typeof(Collider[]), typeof(Vector3), typeof(Scp914Mode), typeof(Scp914KnobSetting) })]
    internal class Scp914UpgradePatch
    {
        public static bool Prefix(Collider[] intake, Vector3 moveVector, Scp914Mode mode, Scp914KnobSetting setting)
        {
            HashSet<GameObject> hashSet = HashSetPool<GameObject>.Shared.Rent();
            HashSet<Player> upgradedPlayers = new ();
            for (int i = 0; i < intake.Length; i++)
            {
                GameObject gameObject = intake[i].transform.root.gameObject;
                if (hashSet.Add(gameObject) && ReferenceHub.TryGetHub(gameObject, out ReferenceHub ply))
                {
                    upgradedPlayers.Add(Player.Get(ply));
                    Scp914Upgrader.ProcessPlayer(ply, false, false, moveVector, setting);
                }
            }

            (EventManager.CurrentEvent as TSL).Scp914_UpgradingPlayers(upgradedPlayers, setting);
            HashSetPool<GameObject>.Shared.Return(hashSet);

            return false;
        }
    }
}