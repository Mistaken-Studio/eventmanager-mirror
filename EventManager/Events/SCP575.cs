// -----------------------------------------------------------------------
// <copyright file="SCP575.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using InventorySystem.Items.Firearms;
using MEC;
using Mistaken.API;
using PlayerStatsSystem;

namespace Mistaken.EventManager.Events
{
    internal class SCP575 : IEMEventClass
    {
        public override string Id => "575";

        public override string Description => "SCP-575 breach";

        public override string Name => "SCP-575";

        public override Dictionary<string, string> Translations => new Dictionary<string, string>()
        {
            { "HUMAN_Info", "Nastąpił wyłom <color=gray>SCP-575</color>! Zdobądźcie latarkę lub zamontujcie ją na broni.. inaczej [REDACTED]" },
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += this.Server_RoundEnded;
            Exiled.Events.Handlers.Player.TogglingFlashlight += this.Player_TogglingFlashlight;
            Exiled.Events.Handlers.Player.TogglingWeaponFlashlight += this.Player_TogglingWeaponFlashlight;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= this.Server_RoundEnded;
            Exiled.Events.Handlers.Player.TogglingFlashlight -= this.Player_TogglingFlashlight;
            Exiled.Events.Handlers.Player.TogglingWeaponFlashlight -= this.Player_TogglingWeaponFlashlight;
        }

        private readonly Dictionary<Player, (bool flashlight, bool weapon)> onFlashlights = new Dictionary<Player, (bool, bool)>();

        private bool attackPhase = false;

        private void Server_RoundStarted()
        {
            foreach (var player in RealPlayers.List.Where(x => x.Team != Team.SCP))
                player.Broadcast(8, EventManager.EMLB + this.Translations["HUMAN_Info"], shouldClearPrevious: true);

            EventManager.Instance.RunCoroutine(this.Lights(), "scp575_lights");
            EventManager.Instance.RunCoroutine(this.Attack(), "scp575_attack");
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            this.DeInitiate();
        }

        private void Player_TogglingWeaponFlashlight(Exiled.Events.EventArgs.TogglingWeaponFlashlightEventArgs ev)
        {
            if (!this.onFlashlights.ContainsKey(ev.Player))
                this.onFlashlights[ev.Player] = (false, false);
            this.onFlashlights[ev.Player] = (ev.NewState, this.onFlashlights[ev.Player].weapon);
        }

        private void Player_TogglingFlashlight(Exiled.Events.EventArgs.TogglingFlashlightEventArgs ev)
        {
            if (!this.onFlashlights.ContainsKey(ev.Player))
                this.onFlashlights[ev.Player] = (false, false);
            this.onFlashlights[ev.Player] = (this.onFlashlights[ev.Player].flashlight, ev.NewState);
        }

        private IEnumerator<float> Lights()
        {
            while (this.Active)
            {
                int time = UnityEngine.Random.Range(100, 200);
                Map.TurnOffAllLights(time, ZoneType.HeavyContainment);
                this.attackPhase = true;
                yield return Timing.WaitForSeconds(time);
                this.attackPhase = false;
                yield return Timing.WaitForSeconds(60);
            }
        }

        private IEnumerator<float> Attack()
        {
            while (this.Active)
            {
                yield return Timing.WaitForSeconds(15);
                if (!this.attackPhase)
                    continue;
                foreach (var player in RealPlayers.List)
                {
                    if (player.Team == Team.SCP)
                        continue;
                    if (player.Zone == ZoneType.HeavyContainment)
                    {
                        if (this.onFlashlights[player].flashlight || this.onFlashlights[player].weapon)
                        {
                            if (player.CurrentItem.Base is Firearm || player.CurrentItem.Type == ItemType.Flashlight)
                                continue;
                            else
                                player.Hurt(new CustomReasonDamageHandler("SCP-575", UnityEngine.Random.Range(20f, 35f)));
                        }
                        else
                            player.Hurt(new CustomReasonDamageHandler("SCP-575", UnityEngine.Random.Range(20f, 35f)));
                    }
                }
            }
        }
    }
}
