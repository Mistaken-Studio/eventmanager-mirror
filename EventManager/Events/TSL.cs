// -----------------------------------------------------------------------
// <copyright file="TSL.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Mirror;
using Mistaken.EventManager;
using Mistaken.EventManager.EventCreator;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    /*internal class TSL : IEMEventClass
    {
        public override string Id => "tsl";

        public override string Description => "Trouble in Secret Laboratory";

        public override string Name => "TSL";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>
        {
            { "I_Info", "Jesteś <color=lime>Niewinnym</color>. Wspólnie z <color=#00B7EB>Detektywem/ami</color> musicie odgadnąć kto jest <color=red>Zdrajcą</color>. Statystyki pod komendą \".tsl round\"" },
            { "T_Info", "Jesteś <color=red>Zdrajcą</color>. Twoim zadaniem jest zabicie wszystkich <color=lime>Niewinnych</color> i <color=#00B7EB>Detektywów</color>. Dostęp do sklepu pod komendą \".tsl shop\"" },
            { "D_Info", "Jesteś <color=#00B7EB>Detektywem</color>. Musisz odgadnąć kto jest <color=red>Zdrajcą</color> pośród <color=lime>Niewinnych</color>. Dostęp do sklepu pod komendą \".tsl shop\"" },
        };

        public readonly Dictionary<string, DateTime> Cooldowns = new Dictionary<string, DateTime>();

        public readonly Dictionary<Player, (TSLClassType, int, List<TSLItemType>)> players = new Dictionary<Player, (TSLClassType, int, List<TSLItemType>)>();

        public readonly Dictionary<Player, (TSLClassType, TSLClassType)> staticPlayers = new Dictionary<Player, (TSLClassType, TSLClassType)>();

        public readonly Dictionary<string, (string, TSLClassType[], int, ItemType, TSLItemType)> shopItems = new Dictionary<string, (string, TSLClassType[], int, ItemType, TSLItemType)>()
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
                "usp", ("USP", new TSLClassType[] { TSLClassType.Traitor }, 2, ItemType.GunUSP, TSLItemType.None)
            },
            {
                "frag", ("Frag", new TSLClassType[] { TSLClassType.Traitor }, 2, ItemType.GrenadeFrag, TSLItemType.None)
            },
            {
                "deagle", ("Deagle", new TSLClassType[] { TSLClassType.Detective }, 2, ItemType.GunUSP, TSLItemType.None)
            },
            {
                "adrenaline", ("Adrenaline", new TSLClassType[] { TSLClassType.Detective }, 1, ItemType.Adrenaline, TSLItemType.None)
            },
            {
                "scp500", ("SCP-500", new TSLClassType[] { TSLClassType.Detective }, 1, ItemType.SCP500, TSLItemType.None)
            },
        };

        public readonly Dictionary<string, (string, int)> traitorEvents = new Dictionary<string, (string, int)>()
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


        public override void OnIni()
        {
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            MapGeneration.ConditionalItemSpawner.DoorTriggers.Clear();
            Pickup.Instances.ForEach(x => x.Delete());
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            Exiled.Events.Handlers.Player.MedicalItemUsed += Player_MedicalItemUsed;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;
            Exiled.Events.Handlers.Player.Shot += this.Player_Shot;
            Exiled.Events.Handlers.Scp914.UpgradingItems += Scp914_UpgradingItems;
            Exiled.Events.Handlers.Player.Left += this.Player_Left;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Player.MedicalItemUsed -= Player_MedicalItemUsed;
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;
            Exiled.Events.Handlers.Player.Shot -= this.Player_Shot;
            Exiled.Events.Handlers.Scp914.UpgradingItems -= Scp914_UpgradingItems;
            Exiled.Events.Handlers.Player.Left -= this.Player_Left;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
        }

        private void Server_RoundStarted()
        {
            this.players.Clear();
            this.staticPlayers.Clear();
            this.traitorTesterUses = 0;
            Gamer.Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            foreach (var locker in LockerManager.singleton.lockers)
            {
                locker._itemsToSpawn = new List<Locker.ItemToSpawn>();
                locker._assignedPickups = new List<Pickup>();
            }

            foreach (var door in Map.Doors)
            {
                var doorType = door.Type();
                if (doorType == DoorType.CheckpointLczA || doorType == DoorType.CheckpointLczB)
                    door.ServerChangeLock(DoorLockType.DecontLockdown, true);
                else if (doorType == DoorType.Scp012 || doorType == DoorType.Scp914 || doorType == DoorType.Scp012Bottom || doorType == DoorType.LczArmory)
                {
                    door.IsOpen = true;
                    door.ServerChangeLock(DoorLockType.AdminCommand, true);
                }
            }

            foreach (var e in Map.Lifts)
            {
                if (e.elevatorName.StartsWith("El"))
                    e.Network_locked = true;
            }

            var rooms = MapPlus.Rooms.Where(x => x.Zone == ZoneType.LightContainment && x.Type != RoomType.Lcz173).ToList();
            foreach (var item in rooms)
            {
                switch (UnityEngine.Random.Range(0, 9))
                {
                    case 0:
                        ItemType.GunCOM15.Spawn(UnityEngine.Random.Range(0, 12), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Ammo9mm.Spawn(UnityEngine.Random.Range(4, 25), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Ammo9mm.Spawn(UnityEngine.Random.Range(4, 25), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        break;
                    case 1:
                        ItemType.GunUSP.Spawn(UnityEngine.Random.Range(0, 18), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Ammo9mm.Spawn(UnityEngine.Random.Range(4, 27), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Ammo9mm.Spawn(UnityEngine.Random.Range(4, 27), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        break;
                    case 2:
                        ItemType.GunMP7.Spawn(UnityEngine.Random.Range(0, 35), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Ammo762.Spawn(UnityEngine.Random.Range(5, 60), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Ammo762.Spawn(UnityEngine.Random.Range(5, 60), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        break;
                    case 3:
                        ItemType.GunProject90.Spawn(UnityEngine.Random.Range(0, 50), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Ammo9mm.Spawn(UnityEngine.Random.Range(5, 70), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Ammo9mm.Spawn(UnityEngine.Random.Range(5, 70), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        break;
                    case 4:
                        ItemType.GunE11SR.Spawn(UnityEngine.Random.Range(0, 40), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Ammo556.Spawn(UnityEngine.Random.Range(5, 60), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Ammo556.Spawn(UnityEngine.Random.Range(5, 60), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        break;
                    case 5:
                        ItemType.GunLogicer.Spawn(UnityEngine.Random.Range(0, 75), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Ammo762.Spawn(UnityEngine.Random.Range(10, 80), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Ammo762.Spawn(UnityEngine.Random.Range(10, 80), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        break;
                    case 6:
                        ItemType.Medkit.Spawn(1, item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Medkit.Spawn(1, item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        break;
                    case 7:
                        ItemType.Adrenaline.Spawn(1, item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        break;
                    case 8:
                        ItemType.Painkillers.Spawn(1, item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        ItemType.Painkillers.Spawn(1, item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                        break;
                }
            }

            double converter = 0.25;
            foreach (Player player in RealPlayers.RandomList)
            {
                if (converter % 1 == 0)
                {
                    this.players.Add(player, (TSLClassType.Traitor, 1, new List<TSLItemType>()));
                    this.staticPlayers.Add(player, (TSLClassType.Traitor, TSLClassType.None));
                }
                else if (converter % 1.75 == 0)
                {
                    this.players.Add(player, (TSLClassType.Detective, 2, new List<TSLItemType>()));
                    this.staticPlayers.Add(player, (TSLClassType.Detective, TSLClassType.Detective));
                }
                else
                {
                    this.players.Add(player, (TSLClassType.Innocent, 0, new List<TSLItemType>()));
                    this.staticPlayers.Add(player, (TSLClassType.Innocent, TSLClassType.None));
                }
                player.SlowChangeRole(RoleType.ClassD, rooms[UnityEngine.Random.Range(0, rooms.Count)].Position + Vector3.up * 2);
                converter += 0.25;
            }
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (this.players.TryGetValue(ev.Player, out (TSLClassType, int, List<TSLItemType>) items))
            {
                switch (items.Item1)
                {
                    case TSLClassType.Innocent:
                        ev.Player.Broadcast(8, EventManager.EMLB + this.Translations["I_Info"]);
                        break;
                    case TSLClassType.Traitor:
                        ev.Player.Broadcast(8, EventManager.EMLB + this.Translations["T_Info"]);
                        MEC.Timing.CallDelayed(2f, () =>
                        {
                            foreach (var item in this.players.Where(x => x.Value.Item1 == TSLClassType.Traitor))
                            {
                                CustomInfoHandler.SetTarget(ev.Player, "tsl_traitors", "<color=red>Zdrajca</color>", item.Key);
                                ev.Player.ChangeAppearance(item.Key, RoleType.Tutorial);
                                ev.Player.Items.Add(new Inventory.SyncItemInfo { durability = 100, id = ItemType.Radio });
                            }
                        });
                        break;
                    case TSLClassType.Detective:
                        ev.Player.Broadcast(8, EventManager.EMLB + this.Translations["D_Info"]);
                        CustomInfoHandler.Set(ev.Player, "tsl_detective", "<color=#00B7EB>Detektyw</color>");
                        MEC.Timing.CallDelayed(2f, () => ev.Player.ChangeAppearance(RoleType.FacilityGuard));
                        break;
                }
            }
            CustomInfoHandler.Set(ev.Player, "tsl_health", "<color=#22de18>Wyleczony</color>");
            ev.Player.InfoArea &= ~PlayerInfoArea.Role;
        }

        #region health_ranks
        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (this.players.TryGetValue(ev.Target, out (TSLClassType, int, List<TSLItemType>) items))
            {
                if (items.Item3.Contains(TSLItemType.Heavy_Armor))
                {
                    ev.Amount *= 0.7f;
                }
                else if (items.Item3.Contains(TSLItemType.Armor))
                {
                    ev.Amount *= 0.9f;
                }
            }

            if (items.Item1 != TSLClassType.Detective && ev.Target.Role != RoleType.Spectator)
            {
                switch (ev.Target.Health)
                {
                    case float n when n < 100 && n >= 80:
                        CustomInfoHandler.Set(ev.Target, "tsl_health", "<color=#dce858>Lekko Ranny</color>");
                        break;

                    case float n when n < 80 && n >= 55:
                        CustomInfoHandler.Set(ev.Target, "tsl_health", "<color=#d69a33>Ranny</color>");
                        break;

                    case float n when n < 55 && n >= 20:
                        CustomInfoHandler.Set(ev.Target, "tsl_health", "<color=#92522>Poważnie Ranny</color>");
                        break;

                    case float n when n < 20:
                        CustomInfoHandler.Set(ev.Target, "tsl_health", "<color=#525252>Bliski śmierci</color>");
                        break;
                }
            }
        }

        private void Player_MedicalItemUsed(Exiled.Events.EventArgs.UsedMedicalItemEventArgs ev)
        {
            if (this.players.TryGetValue(ev.Player, out (TSLClassType, int, List<TSLItemType>) items) && items.Item1 != TSLClassType.Detective)
            {
                switch (ev.Player.Health)
                {
                    case float n when n < 100 && n >= 80:
                        CustomInfoHandler.Set(ev.Player, "tsl_health", "<color=#dce858>Lekko Ranny</color>");
                        break;

                    case float n when n < 80 && n >= 55:
                        CustomInfoHandler.Set(ev.Player, "tsl_health", "<color=#d69a33>Ranny</color>");
                        break;

                    case float n when n < 55 && n >= 20:
                        CustomInfoHandler.Set(ev.Player, "tsl_health", "<color=#92522>Poważnie Ranny</color>");
                        break;

                    case float n when n < 20:
                        CustomInfoHandler.Set(ev.Player, "tsl_health", "<color=#525252>Bliski śmierci</color>");
                        break;
                }
            }
        }
        #endregion

        private void Player_Shot(Exiled.Events.EventArgs.ShotEventArgs ev)
        {
            if (ev.Shooter.CurrentItem.id == ItemType.GunUSP)
            {
                if (ev.Shooter.CurrentItem.modBarrel == 1)
                    ev.Damage *= 1.7f;
                else if (ev.Shooter.CurrentItem.modBarrel == 2)
                {
                    ev.Damage *= 10f;
                    ev.Shooter.Items.Remove(ev.Shooter.CurrentItem);
                }
            }
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (!ev.Target.IsConnected) return;
            CustomInfoHandler.Set(ev.Target, "tsl_health", "");
            if (this.players.TryGetValue(ev.Target, out (TSLClassType, int, List<TSLItemType>) targetItems))
            {
                switch (targetItems.Item1)
                {
                    case TSLClassType.Innocent:
                        {
                            this.players.Remove(ev.Target);
                            if (this.players.TryGetValue(ev.Killer, out (TSLClassType, int, List<TSLItemType>) KillerItems) && KillerItems.Item1 == TSLClassType.Traitor)
                            {
                                this.players[ev.Killer] = (KillerItems.Item1, KillerItems.Item2 + 1, KillerItems.Item3);
                                ev.Killer.Broadcast(4, "Otrzymałeś 1 kredyt");
                            }
                        }
                        break;
                    case TSLClassType.Traitor:
                        {
                            this.players.Remove(ev.Target);
                            if (this.players.TryGetValue(ev.Killer, out (TSLClassType, int, List<TSLItemType>) KillerItems) && KillerItems.Item1 == TSLClassType.Detective)
                            {
                                this.players[ev.Killer] = (KillerItems.Item1, KillerItems.Item2 + 1, KillerItems.Item3);
                                ev.Killer.Broadcast(4, "Otrzymałeś 1 kredyt");
                            }
                            foreach (var item in this.staticPlayers.Where(x => x.Value.Item1 == TSLClassType.Traitor))
                            {
                                if (item.Key.IsConnected)
                                    CustomInfoHandler.SetTarget(ev.Target, "tsl_traitors", "", item.Key);
                            }
                        }
                        break;
                    case TSLClassType.Detective:
                        {
                            CustomInfoHandler.Set(ev.Target, "tsl_detective", "");
                            this.players.Remove(ev.Target);
                            if (this.players.TryGetValue(ev.Killer, out (TSLClassType, int, List<TSLItemType>) KillerItems) && KillerItems.Item1 == TSLClassType.Traitor)
                            {
                                this.players[ev.Killer] = (KillerItems.Item1, KillerItems.Item2 + 1, KillerItems.Item3);
                                ev.Killer.Broadcast(4, "Otrzymałeś 2 kredyty");
                            }
                        }
                        break;
                }
            }
            if (this.players.Count(x => x.Value.Item1 == TSLClassType.Innocent || x.Value.Item1 == TSLClassType.Detective) == 0 && this.players.Count(x => x.Value.Item1 == TSLClassType.Traitor) != 0)
            {
                this.TSL_RoundSummary("<color=red>Zdrajcy</color>");
                this.OnEnd("<color=red>Zdrajcy</color> wygrywają!", true);
            }
            else if (this.players.Count(x => x.Value.Item1 == TSLClassType.Innocent || x.Value.Item1 == TSLClassType.Detective) != 0 && this.players.Count(x => x.Value.Item1 == TSLClassType.Traitor) == 0)
            {
                this.TSL_RoundSummary("<color=lime>Niewinni</color>");
                this.OnEnd("<color=lime>Niewinni</color> wygrywają!", true);
            }
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (ev.IsAllowed && this.players.ContainsKey(ev.Target))
            {
                if (this.players[ev.Target].Item1 == TSLClassType.Traitor)
                    ev.Target.Items.Remove(ev.Target.Items.First(x => x.id == ItemType.Radio));
            }
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if (this.players.ContainsKey(ev.Player))
            {
                this.players.Remove(ev.Player);
                this.staticPlayers.Remove(ev.Player);
            }
        }

        private void TSL_RoundSummary(string winner)
        {
            List<string> roundSummary = new List<string>();
            roundSummary.Add($"Rundę wygrywają {winner}!");
            roundSummary.Add("Klasy graczy:");
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
            foreach (Player player in RealPlayers.List)
            {
                player.SendConsoleMessage(string.Join("\n", roundSummary), "green");
            }
        }

        public string TSL_RoundInfo()
        {
            List<string> round = new List<string>();
            round.Add("<color=grey>Przydatne statystyki:</color>");
            round.Add($"<color=grey>Pozostało {this.players.Count(x => x.Value.Item1 == TSLClassType.Traitor)} <color=red>Zdrajców</color>, {this.players.Count(x => x.Value.Item1 == TSLClassType.Innocent)} <color=lime>Niewinnych</color>, {this.players.Count(x => x.Value.Item1 == TSLClassType.Detective)} <color=#00B7EB>Detektywów</color></color>");
            round.Add("<color=grey>Lista Graczy:</color>");
            foreach (var item in this.staticPlayers)
            {
                if (this.players.ContainsKey(item.Key))
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

        private int traitorTesterUses = 0;
        private void Scp914_UpgradingItems(Exiled.Events.EventArgs.UpgradingItemsEventArgs ev)
        {
            ev.IsAllowed = false;
            if (this.traitorTesterUses >= (this.staticPlayers.Count / 1.6)) return;
            if (ev.KnobSetting == Scp914.Scp914Knob.OneToOne && ev.Players.Count == 1)
            {
                var player = ev.Players.First();
                if (this.players.ContainsKey(player))
                {
                    switch (this.players[player].Item1)
                    {
                        case TSLClassType.Traitor:
                            if (this.players[player].Item3.Contains(TSLItemType.ATTD))
                            {
                                this.staticPlayers[player] = (this.players[player].Item1, TSLClassType.Innocent);
                                this.players[player].Item3.Remove(TSLItemType.ATTD);
                            }
                            else
                            {
                                this.staticPlayers[player] = (this.players[player].Item1, TSLClassType.Traitor);
                                MapPlus.Rooms.First(x => x.Type == RoomType.Lcz914).TurnOffLights(5f);
                            }
                            break;
                        case TSLClassType.Innocent:
                            this.staticPlayers[player] = (this.players[player].Item1, TSLClassType.Innocent);
                            break;
                        default:
                            return;
                    }
                }
            }
            this.traitorTesterUses++;
        }
    }

    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class ShopCommandHandler : IBetterCommand
    {
        public override string Command => "TSL";

        public override string[] Aliases => new string[] { "tsl" };

        public override string Description => "TSL command";

        private string Execute(Player player, string[] args)
        {
            if (args.Length != 0)
            {
                switch (args[0].ToLower())
                {
                    case "round":
                        return (EventManager.ActiveEvent as TSL).TSL_RoundInfo();
                    case "event":
                        return this.RequestEvent(player, args.Skip(1).ToArray());
                    case "shop":
                        return this.RequestItem(player, args.Skip(1).ToArray());
                    default:
                        return "Nieznany argument. Użyj \"Round\", \"Event\" bądź \"Shop\"";
                }
            }
            else return "Musisz sprecyzować czy chcesz użyć \"Round\", \"Event\" bądź \"Shop\"";
        }

        private string RequestItem(Player player, string[] args)
        {
            if ((EventManager.ActiveEvent as TSL).players.TryGetValue(player, out (TSLClassType, int, List<TSLItemType>) playerItems) && playerItems.Item1 == TSLClassType.Traitor || playerItems.Item1 == TSLClassType.Detective)
            {
                if (args.Length != 0)
                {
                    if ((EventManager.ActiveEvent as TSL).shopItems.TryGetValue(string.Join("", args).ToLower(), out (string, TSLClassType[], int, ItemType, TSLItemType) items))
                    {
                        if (playerItems.Item2 >= items.Item3)
                        {
                            if (items.Item2.Contains(playerItems.Item1))
                            {
                                if (!playerItems.Item3.Contains(items.Item5))
                                {
                                    if (items.Item5 != TSLItemType.None)
                                    {
                                        List<TSLItemType> tslItems = new List<TSLItemType>();
                                        tslItems.Add(items.Item5);
                                        foreach (var element in playerItems.Item3)
                                        {
                                            tslItems.Add(element);
                                        }
                                        (EventManager.ActiveEvent as TSL).players[player] = (playerItems.Item1, playerItems.Item2 - items.Item3, tslItems);
                                        return $"Kupiłeś {items.Item1}";
                                    }
                                    else
                                    {
                                        if (player.Items.Count < 8)
                                        {
                                            if (items.Item4 == ItemType.GunUSP)
                                            {
                                                if (playerItems.Item1 == TSLClassType.Traitor)
                                                {
                                                    player.Items.Add(new Inventory.SyncItemInfo
                                                    {
                                                        durability = 18,
                                                        id = items.Item4,
                                                        modBarrel = 1,
                                                        modSight = 1
                                                    });
                                                    player.Ammo[(int)AmmoType.Nato9] += 36;
                                                }
                                                else
                                                {
                                                    player.Items.Add(new Inventory.SyncItemInfo
                                                    {
                                                        durability = 1,
                                                        id = items.Item4,
                                                        modBarrel = 2,
                                                        modSight = 1
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                player.Items.Add(new Inventory.SyncItemInfo
                                                {
                                                    durability = 0,
                                                    id = items.Item4
                                                });
                                            }
                                            return $"Kupiłeś {items.Item1}";
                                        }
                                        else return "Nie masz miejsca w ekwipunku by zakupić ten przedmiot";
                                    }
                                }
                                else return "Już posiadasz ten przedmiot";
                            }
                            else return "Ten przedmiot nie jest dostępny dla Ciebie";
                        }
                        else return "Nie masz wystarczającej ilości kredytów";
                    }
                    else return this.GetShopUsage(playerItems.Item1, playerItems.Item2);
                }
                else return this.GetShopUsage(playerItems.Item1, playerItems.Item2);
            }
            else return "Nie jesteś <color=red>Zdrajcą</color> lub <color=#00B7EB>Detektywem</color> by użyć tej komendy";
        }

        private string RequestEvent(Player player, string[] args)
        {
            if ((EventManager.ActiveEvent as TSL).players.TryGetValue(player, out (TSLClassType, int, List<TSLItemType>) playerItems) && playerItems.Item1 == TSLClassType.Traitor)
            {
                if (args.Length != 0)
                {
                    var arg = string.Join("", args).ToLower();
                    if ((EventManager.ActiveEvent as TSL).traitorEvents.TryGetValue(arg, out (string, int) items))
                    {
                        if (playerItems.Item2 >= items.Item2)
                        {
                            if (!(EventManager.ActiveEvent as TSL).Cooldowns.TryGetValue(arg, out DateTime time))
                                (EventManager.ActiveEvent as TSL).Cooldowns.Add(arg, DateTime.Now);
                            if (DateTime.Now < time)
                                return $"Musisz odczekać jeszcze {(time - DateTime.Now).Seconds} sekund";
                            else
                            {
                                (EventManager.ActiveEvent as TSL).Cooldowns[arg] = DateTime.Now.AddSeconds(90);
                                (EventManager.ActiveEvent as TSL).players[player] = (playerItems.Item1, playerItems.Item2 - items.Item2, playerItems.Item3);
                                switch (arg)
                                {
                                    case "blackout":
                                        {
                                            Map.TurnOffAllLights(15f);
                                        }
                                        return "Pomyślnie zgasiłeś światła na 15 sekund";
                                    case "grenade914":
                                        {
                                            List<Vector3> positions = new List<Vector3>() { new Vector3(-7f, 1f, 6.3f), new Vector3(-7.4f, 1f, 6.3f) };
                                            foreach (var pos in positions)
                                            {
                                                var room = MapPlus.Rooms.First(x => x.Type == RoomType.Lcz914);
                                                var basePos = room.Position;
                                                var offset = pos;
                                                offset = player.CurrentRoom.transform.forward * -offset.x + player.CurrentRoom.transform.right * -offset.z + Vector3.up * offset.y;
                                                basePos += offset;
                                                Grenade grenade = UnityEngine.Object.Instantiate(player.GrenadeManager.availableGrenades[0].grenadeInstance).GetComponent<Grenade>();
                                                grenade.gameObject.transform.position = basePos;
                                                grenade.gameObject.transform.rotation = Quaternion.identity;
                                                grenade.fuseDuration = 0f;
                                                grenade.InitData(player.GrenadeManager, Vector3.zero, Vector3.zero, 0f);
                                                NetworkServer.Spawn(grenade.gameObject);
                                            }
                                        }
                                        return "Pomyślnie wysadziłeś wnętrza komór SCP-914";
                                    case "lock914":
                                        {
                                            var door = Map.Doors.First(x => x.Type() == DoorType.Scp914);
                                            door.IsOpen = false;
                                            Timing.CallDelayed(15f, () => door.IsOpen = true);
                                        }
                                        return "Pomyślnie zablokowałeś bramę SCP-914 na 15 sekund";
                                    default:
                                        return "Coś poszło nie tak";
                                }
                            }
                        }
                        else return "Nie masz wystarczającej ilości kredytów";
                    }
                    else return this.GetEventUsage(playerItems.Item2);
                }
                else return this.GetEventUsage(playerItems.Item2);
            }
            else return "Nie jesteś <color=red>Zdrajcą</color> by użyć tej komendy";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (!(EventManager.ActiveEvent is TSL)) return new string[] { "Ten event aktualnie nie trwa" };
            player.SendConsoleMessage("TSL| " + Execute(player, args), "green");
            success = true;
            return new string[] { "Done" };
        }

        private string GetShopUsage(TSLClassType classType, int credits)
        {
            List<string> tor = new List<string>();
            tor.Add("Przedmioty dostępne w sklepie:");
            foreach (var item in (EventManager.ActiveEvent as TSL).shopItems.Where(x => x.Value.Item2.Contains(classType)))
            {
                tor.Add($"{item.Value.Item1} za {item.Value.Item3} kretydów");
            }
            tor.Add($"Posiadasz {credits} kredytów");
            return string.Join("\n", tor);
        }

        private string GetEventUsage(int credits)
        {
            List<string> tor = new List<string>();
            tor.Add("Dostępne eventy:");
            foreach (var item in (EventManager.ActiveEvent as TSL).traitorEvents)
            {
                tor.Add($"{item.Key} - {item.Value.Item1} za {item.Value.Item2} kredytów");
            }
            tor.Add($"Posiadasz {credits} kredytów");
            return string.Join("\n", tor);
        }
    }

    internal enum TSLClassType
    {
        None = -1,
        Innocent = 0,
        Traitor = 1,
        Detective = 2
    }

    internal enum TSLItemType
    {
        None = -1,
        Armor = 0,
        Heavy_Armor = 1,
        ATTD = 2
    }*/
}