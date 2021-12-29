// -----------------------------------------------------------------------
// <copyright file="IEMEventClass.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using InventorySystem.Items.Firearms.Ammo;
using Mistaken.API;
using Mistaken.EventManager.EventArgs;
using UnityEngine;

namespace Mistaken.EventManager
{
    /// <summary>
    /// Class for handling Events.
    /// </summary>
    public abstract class IEMEventClass
    {
        /// <summary>
        /// Gets a value indicating whether Event is active.
        /// </summary>
        public bool Active => EventManager.ActiveEvent?.Id == this.Id;

        /// <summary>
        /// Gets Event name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets Event id.
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// Gets Event description.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets Event translations.
        /// </summary>
        public abstract Dictionary<string, string> Translations { get; }

        /// <summary>
        /// Ends Event.
        /// </summary>
        /// <param name="player">Winner of the Event.</param>
        public void OnEnd(Player player = null)
        {
            Map.ClearBroadcasts();
            if (!(player is null))
                Map.Broadcast(10, $"{EventManager.EMLB} Nikt nie wygrał");
            else
            {
                Map.Broadcast(10, $"{EventManager.EMLB} <color=#6B9ADF>{player.Nickname}</color> wygrał!");
                EMEvents.OnPlayerWinningEvent(new PlayerWinningEventEventArgs(player, this));
                if (!player.RemoteAdminAccess)
                {
                    var lines = File.ReadAllLines(EventManager.BasePath + @"\winners.txt");
                    File.AppendAllLines(EventManager.BasePath + @"\winners.txt", new string[] { $"{player.Nickname};{player.UserId};{(lines.Any(x => x.Contains(player.Nickname)) ? int.Parse(lines.First(x => x.Contains(player.Nickname)).Split(';')[2]) + 1 : 1)}" });
                }
            }

            this.DeInitiate();
            Round.IsLocked = false;
        }

        /// <summary>
        /// Ends Event.
        /// </summary>
        /// <param name="customWinText">Custom text displayed at the end of the Event.</param>
        public void OnEnd(string customWinText = null)
        {
            Map.ClearBroadcasts();
            if (!string.IsNullOrEmpty(customWinText))
                Map.Broadcast(10, $"{EventManager.EMLB} {customWinText}");
            else
                Map.Broadcast(10, $"{EventManager.EMLB} Nikt nie wygrał");

            this.DeInitiate();
            Round.IsLocked = false;
        }

        /// <summary>
        /// Ends Event.
        /// </summary>
        /// <param name="customWinText">Custom text displayed at the end of the Event.</param>
        /// <param name="player">Winner of the event.</param>
        public void OnEnd(string customWinText, Player player)
        {
            Map.ClearBroadcasts();
            if (!string.IsNullOrEmpty(customWinText))
                Map.Broadcast(10, $"{EventManager.EMLB} {customWinText}");
            else
                Map.Broadcast(10, $"{EventManager.EMLB} Nikt nie wygrał");
            if (!(player is null))
            {
                EMEvents.OnPlayerWinningEvent(new PlayerWinningEventEventArgs(player, this));
                if (!player.RemoteAdminAccess)
                {
                    var lines = File.ReadAllLines(EventManager.BasePath + @"\winners.txt");
                    File.AppendAllLines(EventManager.BasePath + @"\winners.txt", new string[] { $"{player.Nickname};{player.UserId};{(lines.Any(x => x.Contains(player.Nickname)) ? int.Parse(lines.First(x => x.Contains(player.Nickname)).Split(';')[2]) + 1 : 1)}" });
                }
            }

            this.DeInitiate();
            Round.IsLocked = false;
        }

        /// <summary>
        /// Gets a random role from the specified team (team must be any of <see cref="Team.MTF"/> or <see cref="Team.CHI"/>).
        /// </summary>
        /// <param name="team">Team.</param>
        /// <returns>Random role from the specified team.</returns>
        public RoleType RandomTeamRole(Team team)
        {
            var rand = UnityEngine.Random.Range(0, 3);
            switch (team)
            {
                case Team.CHI:
                    {
                        switch (rand)
                        {
                            case 0:
                                return RoleType.ChaosRepressor;
                            case 1:
                                return RoleType.ChaosMarauder;
                            default:
                                return RoleType.ChaosRifleman;
                        }
                    }

                case Team.MTF:
                    {
                        switch (rand)
                        {
                            case 0:
                                return RoleType.NtfSergeant;
                            case 1:
                                return RoleType.NtfCaptain;
                            default:
                                return RoleType.NtfPrivate;
                        }
                    }

                default:
                    return RoleType.ClassD;
            }
        }

        /// <summary>
        /// Initiates Event.
        /// </summary>
        public void Initiate()
        {
            Log.Info("Event Initiated: " + this.Name);
            Log.Debug("Deinitiating modules", PluginHandler.Instance.Config.VerbouseOutput);
            Mistaken.API.Diagnostics.Module.DisableAllExcept(PluginHandler.Instance);
            Log.Debug("Deinitiated modules", PluginHandler.Instance.Config.VerbouseOutput);
            EventManager.ActiveEvent = this;
            Map.Broadcast(10, EventManager.EMLB + $"Uruchomiono event: <color=#6B9ADF>{this.Name}</color>");
            CharacterClassManager.LaterJoinEnabled = false;
            this.OnIni();
            if (this is ISpawnRandomItems)
            {
                foreach (var item in Map.Rooms)
                {
                    switch (UnityEngine.Random.Range(0, 14))
                    {
                        case 0:
                            {
                                new Firearm(ItemType.GunCOM15).Spawn(item.Position + Vector3.up);
                                new Armor(ItemType.ArmorLight).Spawn(item.Position + Vector3.up);
                                var ammo = (AmmoPickup)new Ammo(ItemType.Ammo9x19).Spawn(item.Position + Vector3.up).Base;
                                ammo.SavedAmmo = 24;
                                ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                                break;
                            }

                        case 1:
                            {
                                new Firearm(ItemType.GunCOM18).Spawn(item.Position + Vector3.up);
                                new Armor(ItemType.ArmorLight).Spawn(item.Position + Vector3.up);
                                var ammo = (AmmoPickup)new Ammo(ItemType.Ammo9x19).Spawn(item.Position + Vector3.up).Base;
                                ammo.SavedAmmo = 36;
                                ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                                break;
                            }

                        case 2:
                            {
                                new Firearm(ItemType.GunRevolver).Spawn(item.Position + Vector3.up);
                                new Armor(ItemType.ArmorLight).Spawn(item.Position + Vector3.up);
                                var ammo = (AmmoPickup)new Ammo(ItemType.Ammo44cal).Spawn(item.Position + Vector3.up).Base;
                                ammo.SavedAmmo = 16;
                                ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                                break;
                            }

                        case 3:
                            {
                                new Firearm(ItemType.GunFSP9).Spawn(item.Position + Vector3.up);
                                new Armor(ItemType.ArmorLight).Spawn(item.Position + Vector3.up);
                                var ammo = (AmmoPickup)new Ammo(ItemType.Ammo9x19).Spawn(item.Position + Vector3.up).Base;
                                ammo.SavedAmmo = 60;
                                ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                                break;
                            }

                        case 4:
                            {
                                new Firearm(ItemType.GunCrossvec).Spawn(item.Position + Vector3.up);
                                new Armor(ItemType.ArmorCombat).Spawn(item.Position + Vector3.up);
                                var ammo = (AmmoPickup)new Ammo(ItemType.Ammo9x19).Spawn(item.Position + Vector3.up).Base;
                                ammo.SavedAmmo = 100;
                                ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                                break;
                            }

                        case 5:
                            {
                                new Firearm(ItemType.GunE11SR).Spawn(item.Position + Vector3.up);
                                new Armor(ItemType.ArmorCombat).Spawn(item.Position + Vector3.up);
                                var ammo = (AmmoPickup)new Ammo(ItemType.Ammo556x45).Spawn(item.Position + Vector3.up).Base;
                                ammo.SavedAmmo = 80;
                                ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                                break;
                            }

                        case 6:
                            {
                                new Firearm(ItemType.GunAK).Spawn(item.Position + Vector3.up);
                                new Armor(ItemType.ArmorCombat).Spawn(item.Position + Vector3.up);
                                var ammo = (AmmoPickup)new Ammo(ItemType.Ammo762x39).Spawn(item.Position + Vector3.up).Base;
                                ammo.SavedAmmo = 70;
                                ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                                break;
                            }

                        case 7:
                            {
                                new Firearm(ItemType.GunShotgun).Spawn(item.Position + Vector3.up);
                                new Armor(ItemType.ArmorCombat).Spawn(item.Position + Vector3.up);
                                var ammo = (AmmoPickup)new Ammo(ItemType.Ammo12gauge).Spawn(item.Position + Vector3.up).Base;
                                ammo.SavedAmmo = 28;
                                ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                                break;
                            }

                        case 8:
                            {
                                new Firearm(ItemType.GunLogicer).Spawn(item.Position + Vector3.up);
                                new Armor(ItemType.ArmorHeavy).Spawn(item.Position + Vector3.up);
                                var ammo = (AmmoPickup)new Ammo(ItemType.Ammo762x39).Spawn(item.Position + Vector3.up).Base;
                                ammo.SavedAmmo = 200;
                                ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                                break;
                            }

                        case 9:
                            {
                                new Item(ItemType.Medkit).Spawn(item.Position + Vector3.up);
                                new Item(ItemType.Painkillers).Spawn(item.Position + Vector3.up);
                                break;
                            }

                        case 10:
                            {
                                new Item(ItemType.SCP500).Spawn(item.Position + Vector3.up);
                                break;
                            }

                        case 11:
                            {
                                new Item(ItemType.SCP207).Spawn(item.Position + Vector3.up);
                                break;
                            }

                        case 12:
                            {
                                new Throwable(ItemType.GrenadeFlash).Spawn(item.Position + Vector3.up);
                                new Throwable(ItemType.GrenadeHE).Spawn(item.Position + Vector3.up);
                                break;
                            }

                        case 13:
                            {
                                new Throwable(ItemType.SCP018).Spawn(item.Position + Vector3.up);
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Deinitiates Event.
        /// </summary>
        public void DeInitiate()
        {
            this.OnDeIni();
            Log.Info("Event Deactivated: " + this.Name);
            Log.Debug("Reinitiating modules", PluginHandler.Instance.Config.VerbouseOutput);
            Mistaken.API.Diagnostics.Module.EnableAllExcept(PluginHandler.Instance);
            Log.Debug("Modules reinitiated", PluginHandler.Instance.Config.VerbouseOutput);
            EventManager.ActiveEvent = null;
        }

        /// <summary>
        /// Called on Ini.
        /// </summary>
        public abstract void OnIni();

        /// <summary>
        /// Called on DeIni.
        /// </summary>
        public abstract void OnDeIni();
    }
}
