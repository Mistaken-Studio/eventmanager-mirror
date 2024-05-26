// -----------------------------------------------------------------------
// <copyright file="Morbus.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Mistaken.API;
using PlayerStatsSystem;

namespace Mistaken.EventManager.Events
{
    internal class Morbus : EventBase, ISpawnRandomItems
    {
        public override string Id => "morbus";

        public override string Description => "Morbus event";

        public override string Name => "Morbus";

        public Dictionary<string, string> Translations => new ()
        {
            { "Mother", string.Empty },
            { "D", string.Empty },
        };

        public override void Initialize()
        {
            LightContainmentZoneDecontamination.DecontaminationController.Singleton.disableDecontamination = true;
            API.Utilities.Map.RespawnLock = true;
            Round.IsLocked = true;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            Exiled.Events.Handlers.Map.GeneratorActivated += this.Map_GeneratorActivated;
            Exiled.Events.Handlers.Player.ActivatingGenerator += this.Player_ActivatingGenerator;
        }

        public override void Deinitialize()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Map.GeneratorActivated -= this.Map_GeneratorActivated;
            Exiled.Events.Handlers.Player.ActivatingGenerator -= this.Player_ActivatingGenerator;
        }

        private readonly List<int> morbusesFirst = new ();

        private readonly List<int> morbusesSecond = new ();

        private Player mother;

        private void Server_RoundStarted()
        {
            Map.TurnOffAllLights(float.MaxValue);
            var players = RealPlayers.List.ToList();
            this.mother = players[UnityEngine.Random.Range(0, players.Count)];
            this.mother.SlowChangeRole(RoleType.ClassD);
            this.mother.AddItem(ItemType.Coin);
            players.Remove(this.mother);
            this.mother.Broadcast(10, EventManager.EMLB + this.Translations["Mother"]);
            foreach (var item in players)
            {
                item.Role.Type = RoleType.ClassD;
                item.Broadcast(10, EventManager.EMLB + this.Translations["D"]);
            }
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (ev.Target.Id == this.mother.Id)
                this.OnEndMorbus();
        }

        private void Map_GeneratorActivated(Exiled.Events.EventArgs.GeneratorActivatedEventArgs ev)
        {
            if (Generator.List.Count(x => x.IsEngaged) > 4)
                this.OnEndMorbus();
        }

        private void Player_ActivatingGenerator(Exiled.Events.EventArgs.ActivatingGeneratorEventArgs ev)
        {
            ev.Generator.ActivationTime = 180;
        }

        private void OnEndMorbus()
        {
            MEC.Timing.CallDelayed(60, () =>
            {
                foreach (var player in RealPlayers.List)
                {
                    if (this.morbusesFirst.Contains(player.Id))
                        player.Hurt(new CustomReasonDamageHandler("Decontaminated"));
                    if (this.morbusesSecond.Contains(player.Id))
                        player.Hurt(new CustomReasonDamageHandler("Decontaminated"));
                    if (player.Id == this.mother.Id)
                        player.Hurt(new CustomReasonDamageHandler("Decontaminated"));
                }
            });
        }
    }
}
