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
using InventorySystem.Items.Flashlight;
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
            { "H_Info", "Nastąpił wyłom <color=gray>SCP-575</color>! Zdobądźcie latarkę lub zamontujcie ją na broni.. inaczej [REDACTED]" },
        };

        public override void OnIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += this.Server_RoundEnded;
        }

        public override void OnDeIni()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= this.Server_RoundEnded;
        }

        private bool attackPhase = false;

        private void Server_RoundStarted()
        {
            foreach (var player in RealPlayers.List.Where(x => x.Role.Team != Team.SCP))
                player.Broadcast(8, EventManager.EMLB + this.Translations["H_Info"], shouldClearPrevious: true);

            EventManager.Instance.RunCoroutine(this.Lights(), "scp575_lights");
            EventManager.Instance.RunCoroutine(this.Attack(), "scp575_attack");
        }

        private void Server_RoundEnded(Exiled.Events.EventArgs.RoundEndedEventArgs ev)
        {
            this.DeInitiate();
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
            yield return Timing.WaitForSeconds(10);
            while (this.Active)
            {
                if (!this.attackPhase)
                {
                    yield return Timing.WaitForSeconds(0.5f);
                    continue;
                }

                foreach (var player in RealPlayers.List)
                {
                    if (player.Role.Team == Team.SCP)
                        continue;
                    if (player.Zone != ZoneType.HeavyContainment)
                        continue;

                    if (player.CurrentItem?.Base is Firearm firearm && firearm.Status.Flags.HasFlag(FirearmStatusFlags.FlashlightEnabled))
                        continue;
                    else if (player.CurrentItem?.Base is FlashlightItem flashlight && flashlight.IsEmittingLight)
                        continue;
                    else
                        player.Hurt(new CustomReasonDamageHandler("SCP-575", UnityEngine.Random.Range(20f, 35f)));
                }

                yield return Timing.WaitForSeconds(15);
            }
        }
    }
}
