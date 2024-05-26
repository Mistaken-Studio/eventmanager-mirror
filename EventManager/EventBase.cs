// -----------------------------------------------------------------------
// <copyright file="EventBase.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using InventorySystem.Items.Firearms.Ammo;
using MEC;
using Mistaken.EventManager.EventArgs;
using UnityEngine;

namespace Mistaken.EventManager
{
    /// <summary>
    /// Base class for Events.
    /// </summary>
    public abstract class EventBase
    {
        /// <summary>
        /// Gets a value indicating whether Event is active.
        /// </summary>
        public bool Active => EventManager.CurrentEvent == this;

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
        /// Called on Ini.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Called on DeIni.
        /// </summary>
        public abstract void Deinitialize();

        /// <summary>
        /// Initiates Event.
        /// </summary>
        public void Initiate()
        {
            EventManager.RoundsWithoutEvent = 0;
            Log.Info("Event Initiated: " + this.Name);
            Log.Debug("Deinitiating modules", PluginHandler.Instance.Config.VerbouseOutput);
            API.Diagnostics.Module.DisableAllExcept(PluginHandler.Instance);
            Log.Debug("Deinitiated modules", PluginHandler.Instance.Config.VerbouseOutput);
            EventManager.CurrentEvent = this;
            Map.Broadcast(10, EventManager.EMLB + $"Uruchomiono event: <color={EventManager.Color}>{this.Name}</color>");
            CharacterClassManager.LaterJoinEnabled = false;
            try
            {
                this.Initialize();
            }
            catch (Exception ex)
            {
                this.Deinitiate();
                Log.Error(ex);
            }

            if (this is ISpawnRandomItems)
            {
                foreach (var item in Room.List.ToArray())
                {
                    switch (UnityEngine.Random.Range(0, 14))
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

                        case 8:
                            {
                                Item.Create(ItemType.GunLogicer).Spawn(item.Position + Vector3.up);
                                Item.Create(ItemType.ArmorHeavy).Spawn(item.Position + Vector3.up);
                                var ammo = (AmmoPickup)Item.Create(ItemType.Ammo762x39).Spawn(item.Position + Vector3.up).Base;
                                ammo.SavedAmmo = 200;
                                ammo.NetworkSavedAmmo = ammo.SavedAmmo;
                                break;
                            }

                        case 9:
                            {
                                Item.Create(ItemType.Medkit).Spawn(item.Position + Vector3.up);
                                Item.Create(ItemType.Painkillers).Spawn(item.Position + Vector3.up);
                                break;
                            }

                        case 10:
                            {
                                Item.Create(ItemType.SCP500).Spawn(item.Position + Vector3.up);
                                break;
                            }

                        case 11:
                            {
                                Item.Create(ItemType.SCP207).Spawn(item.Position + Vector3.up);
                                break;
                            }

                        case 12:
                            {
                                Item.Create(ItemType.GrenadeFlash).Spawn(item.Position + Vector3.up);
                                Item.Create(ItemType.GrenadeHE).Spawn(item.Position + Vector3.up);
                                break;
                            }

                        case 13:
                            {
                                Item.Create(ItemType.SCP018).Spawn(item.Position + Vector3.up);
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Deinitiates Event.
        /// </summary>
        public void Deinitiate()
        {
            Round.IsLocked = false;
            this.Deinitialize();
            Log.Info("Event Deactivated: " + this.Name);
            Log.Debug("Reinitiating modules", PluginHandler.Instance.Config.VerbouseOutput);
            API.Diagnostics.Module.EnableAllExcept(PluginHandler.Instance);
            Log.Debug("Modules reinitiated", PluginHandler.Instance.Config.VerbouseOutput);
            EventManager.CurrentEvent = null; // Dodać tu gui
            Timing.CallDelayed(10f, () => RoundRestarting.RoundRestart.InitiateRoundRestart());
        }

        /// <summary>
        /// Ends Event.
        /// </summary>
        /// <param name="player">Winner of the Event.</param>
        public void OnEnd(Player player = null)
            => this.OnEnd(null, player);

        /// <summary>
        /// Ends Event.
        /// </summary>
        /// <param name="customWinText">Custom text displayed at the end of the Event.</param>
        public void OnEnd(string customWinText = null)
            => this.OnEnd(customWinText, null);

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
            else if (player != null)
            {
                Map.Broadcast(10, $"{EventManager.EMLB} <color={EventManager.Color}>{player.Nickname}</color> wygrał!");
                EventHandler.OnPlayerWinningEvent(new PlayerWinningEventEventArgs(player, this));
                player.UpdateInWinnersFile();
            }
            else
                Map.Broadcast(10, $"{EventManager.EMLB} Nikt nie wygrał");

            this.Deinitiate();
        }

        /// <summary>
        /// Gets a random role from the specified team.
        /// </summary>
        /// <param name="team">Team.</param>
        /// <returns>Random role from the specified team.</returns>
        public RoleType RandomTeamRole(Team team)
            => TeamRoles[team][UnityEngine.Random.Range(0, TeamRoles[team].Length)];

        private static readonly Dictionary<Team, RoleType[]> TeamRoles = new ()
        {
            { Team.SCP, new RoleType[] { RoleType.Scp049, RoleType.Scp0492, RoleType.Scp173, RoleType.Scp106, RoleType.Scp096, RoleType.Scp93953, RoleType.Scp93989, RoleType.Scp079 } },
            { Team.CHI, new RoleType[] { RoleType.ChaosRifleman, RoleType.ChaosConscript, RoleType.ChaosRepressor, RoleType.ChaosMarauder } },
            { Team.MTF, new RoleType[] { RoleType.NtfPrivate, RoleType.NtfSergeant, RoleType.NtfSpecialist, RoleType.NtfCaptain } },
            { Team.RSC, new RoleType[] { RoleType.Scientist } },
            { Team.RIP, new RoleType[] { RoleType.Spectator } },
            { Team.TUT, new RoleType[] { RoleType.Tutorial } },
            { Team.CDP, new RoleType[] { RoleType.ClassD } },
        };
    }
}
