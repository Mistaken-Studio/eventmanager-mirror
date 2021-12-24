﻿// -----------------------------------------------------------------------
// <copyright file="CoalWar.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using MEC;
using Mirror;
using Mistaken.API;
using Mistaken.EventManager.EventCreator;
using PlayerStatsSystem;
using UnityEngine;

namespace Mistaken.EventManager.Events
{
    internal class CoalWar : IEMEventClass, IWinOnLastAlive, IAnnouncePlayersAlive
    {
        public override string Id => "CoWar";

        public override string Description => "U are all naughty.. or are you?";

        public override string Name => "Coal War";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "D_Spawn", "Walka <color=black>węglem</color>.. Takie czarne <color=yellow>złoto</color> w tych czasach.. <color=#6B9ADF>Ostatni żywy wygrywa</color>" },
        };

        public bool ClearPrevious => true;

        public override void OnIni()
        {
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            MapGeneration.InitiallySpawnedItems.Singleton.ClearAll();
            Mistaken.API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            PluginHandler.Harmony.Patch(typeof(ThrowableItem).GetMethod("ServerThrow", new Type[] { typeof(float), typeof(float), typeof(Vector3), typeof(Vector3) }), new HarmonyMethod(typeof(Patch2).GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static)));
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Escaping += this.Player_Escaping;
            Exiled.Events.Handlers.Player.Verified += this.Player_Verified;
        }

        public override void OnDeIni()
        {
            PluginHandler.Harmony.Unpatch(typeof(ThrowableItem).GetMethod("ServerThrow", new Type[] { typeof(float), typeof(float), typeof(Vector3), typeof(Vector3) }), HarmonyPatchType.Prefix);
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Escaping -= this.Player_Escaping;
            Exiled.Events.Handlers.Player.Verified -= this.Player_Verified;
            this.roundStarted = false;
        }

        private bool roundStarted = false;

        private void Server_RoundStarted()
        {
            this.roundStarted = true;
            Timing.RunCoroutine(this.UpdateCoal());
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole != RoleType.Spectator)
            {
                ev.Player.SlowChangeRole(RoleType.ClassD, new Vector3(28f, 990f, -59f));
                ev.Player.Broadcast(8, EventManager.EMLB + " " + this.Translations["D_Spawn"], shouldClearPrevious: true);
            }
        }

        private void Player_Escaping(Exiled.Events.EventArgs.EscapingEventArgs ev)
        {
            ev.IsAllowed = false;
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (this.roundStarted && Round.ElapsedTime.TotalSeconds < 30)
            {
                Timing.CallDelayed(1f, () =>
                {
                    ev.Player.SlowChangeRole(RoleType.ClassD, new Vector3(28f, 990f, -59f));
                    ev.Player.Broadcast(8, EventManager.EMLB + " " + this.Translations["D_Spawn"], shouldClearPrevious: true);
                });
            }
        }

        private IEnumerator<float> UpdateCoal()
        {
            yield return Timing.WaitForSeconds(10f);
            while (this.Active)
            {
                yield return Timing.WaitForSeconds(1.5f);
                foreach (var player in RealPlayers.List)
                    player.AddItem(ItemType.Coal);
            }
        }
    }

#pragma warning disable SA1313
#pragma warning disable SA1402

    [HarmonyPatch(typeof(ThrowableItem), "ServerThrow", new Type[] { typeof(float), typeof(float), typeof(Vector3), typeof(Vector3) })]
    internal class Patch2
    {
        public static bool Prefix(ThrowableItem __instance, float forceAmount, float upwardFactor, Vector3 torque, Vector3 startVel)
        {
            __instance._destroyTime = Time.timeSinceLevelLoad + __instance._postThrownAnimationTime;
            __instance._alreadyFired = true;
            ThrownProjectile thrownProjectile = UnityEngine.Object.Instantiate<ThrownProjectile>(__instance.Projectile, __instance.Owner.PlayerCameraReference.position, __instance.Owner.PlayerCameraReference.rotation);
            PickupSyncInfo pickupSyncInfo = new PickupSyncInfo
            {
                ItemId = __instance.ItemTypeId,
                Locked = !__instance._repickupable,
                Serial = __instance.ItemSerial,
                Weight = __instance.Weight,
                Position = thrownProjectile.transform.position,
                Rotation = new LowPrecisionQuaternion(thrownProjectile.transform.rotation),
            };
            thrownProjectile.NetworkInfo = pickupSyncInfo;
            thrownProjectile.PreviousOwner = new Footprinting.Footprint(__instance.Owner);
            NetworkServer.Spawn(thrownProjectile.gameObject);
            thrownProjectile.InfoReceived(default(PickupSyncInfo), pickupSyncInfo);
            if (thrownProjectile.TryGetComponent<Rigidbody>(out var rb))
                __instance.PropelBody(rb, torque, startVel, forceAmount, upwardFactor);
            thrownProjectile.gameObject.AddComponent<CoalComponent>();
            thrownProjectile.ServerActivate();

            return false;
        }
    }

    internal class CoalComponent : MonoBehaviour
    {
        private EffectGrenade coal;
        private bool used;
        private Vector3 lastPosition;
        private float distance;

        private void Awake()
        {
            this.coal = this.GetComponent<EffectGrenade>();
            this.lastPosition = this.coal.transform.position;
        }

        private void FixedUpdate()
        {
            this.distance += Vector3.Distance(this.coal.transform.position, this.lastPosition);
            this.lastPosition = this.coal.transform.position;

            foreach (var obj in Physics.OverlapSphere(this.coal.transform.position, 0.15f))
            {
                if (obj.TryGetComponent<IDestructible>(out var p))
                {
                    if (p.NetworkId == this.coal.PreviousOwner.NetId)
                        return;

                    if (!this.used)
                    {
                        float damage = 10f;
                        this.used = true;
                        if (this.distance > 8f && this.distance < 60f)
                            damage = this.distance * 3.2f;

                        p.Damage(0f, new CustomReasonDamageHandler("Skill issue", damage), this.transform.position);
                        this.coal.ServerFuseEnd();
                        ((CollisionDetectionPickup)this.coal).MakeCollisionSound(5.5f);
                    }
                }
            }
        }
    }
}
