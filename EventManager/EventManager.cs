// -----------------------------------------------------------------------
// <copyright file="EventManager.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using MEC;
using Mistaken.API;
using Mistaken.EventManager.Interfaces;

namespace Mistaken.EventManager
{
    internal class EventManager : API.Diagnostics.Module
    {
        public static readonly Dictionary<string, EventBase> Events = new ();

        public static readonly Queue<EventBase> EventQueue = new ();

        public static readonly string Color = "#6B9ADF";

        public static readonly string EMLB = $"[<color={Color}><b>Event Manager</b></color> {(PluginHandler.Instance.Config.DoNotCountPlayers ? $"<color={Color}>Test Build</color>" : string.Empty)}] ";

        public static readonly string Path = System.IO.Path.Combine(Paths.Plugins, "EventManager");

        public static EventBase CurrentEvent { get; set; }

        public static ushort RoundsWithoutEvent { get; set; } = 0;

        public static string WinnersFilePath { get; set; }

        public static EventManager Instance { get; private set; }

        public static bool IsEventActive => CurrentEvent != null;

        public EventManager(IPlugin<IConfig> plugin)
            : base(plugin)
        {
            Instance = this;

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            this.UpdateWinnersFile();
            this.LoadEvents();
        }

        public override string Name => nameof(EventManager);

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Server_WaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound += this.Server_RestartingRound;
            Exiled.Events.Handlers.Player.Verified += this.Player_Verified;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            Exiled.Events.Handlers.Player.Escaping += this.Player_Escaping;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Server_WaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound -= this.Server_RestartingRound;
            Exiled.Events.Handlers.Player.Verified -= this.Player_Verified;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Player.Escaping -= this.Player_Escaping;
        }

        private void LoadEvents()
        {
            this.Log.Info("Loading Events Started");
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (!type.IsClass || type.IsAbstract || !type.IsSubclassOf(typeof(EventBase)))
                        continue;

                    var ev = Activator.CreateInstance(type) as EventBase;
                    Events[ev.Id] = ev;
                    this.Log.Info($"Loaded Event: {ev.Name}");
                }
            }

            this.Log.Info("Loading Events Completed");
        }

        private void UpdateWinnersFile()
        {
            string filePath = string.Empty;
            foreach (string file in Directory.GetFiles(Path))
            {
                var dateToParse = System.IO.Path.GetFileName(file).Split('-')[2].Replace(".txt", string.Empty);
                this.Log.Debug(dateToParse, true); // TODO: Po sprawdzeniu usunąć debug.
                if (DateTime.ParseExact(dateToParse, "yyyy.MM.dd", null) >= DateTime.UtcNow)
                    filePath = file;
            }

            if (filePath == string.Empty)
            {
                filePath = System.IO.Path.Combine(Path, $"event_winners-{DateTime.UtcNow:yyyy.MM.dd}-{DateTime.UtcNow.AddDays(PluginHandler.Instance.Config.NewWinnersFileDays):yyyy.MM.dd}.txt");
                File.Create(filePath);
            }

            WinnersFilePath = filePath;
        }

        private void Server_WaitingForPlayers()
        {
            if (IsEventActive)
                return;

            if (EventQueue.TryDequeue(out EventBase ev))
            {
                this.Log.Info("[Event Queue] Initiating: " + ev.Name);
                try
                {
                    ev.Initiate();
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex);
                }
            }
            else
            {
                if (!PluginHandler.Instance.Config.AutoEventsEnabled)
                    return;

                if (RoundsWithoutEvent < PluginHandler.Instance.Config.AutoEventsRounds)
                    return;

                Timing.CallDelayed(10f, () =>
                {
                    if (IsEventActive)
                        return;

                    var evId = PluginHandler.Instance.Config.AutoEventsList[UnityEngine.Random.Range(0, PluginHandler.Instance.Config.AutoEventsList.Count)];
                    if (Events.TryGetValue(evId, out ev) && ev is IRequiredPlayers requiredPlayers && RealPlayers.List.Count() >= requiredPlayers.PlayerCount)
                    {
                        this.Log.Info("[AutoEvent] Initiating automatic event");
                        ev.Initiate();
                        return;
                    }

                    this.Log.Error($"Failed to find event with id: {evId}");
                });
            }
        }

        private void Server_RoundStarted()
        {
            if (!IsEventActive)
                RoundsWithoutEvent += 1;
        }

        private void Server_RestartingRound()
        {
            if (IsEventActive)
                CurrentEvent.Deinitiate();
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (IsEventActive)
                ev.Player.Broadcast(8, EMLB + $"Na serwerze obecnie trwa: <color={Color}>{CurrentEvent.Name}</color>");
        }

        private void Player_Escaping(Exiled.Events.EventArgs.EscapingEventArgs ev)
        {
            if (!IsEventActive)
                return;

            if (!ev.IsAllowed)
                return;

            if (CurrentEvent is IWinOnEscape)
                CurrentEvent.OnEnd(ev.Player);
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (!IsEventActive)
                return;

            var players = RealPlayers.List.Where(p => p.IsAlive && p.IsHuman).ToArray();
            if (players.Length == 1 && CurrentEvent is IWinOnLastAlive)
                CurrentEvent.OnEnd(players[0]);
            else if (players.Length == 0 && CurrentEvent is IEndOnNoAlive)
                CurrentEvent.OnEnd("Event zakończył się");
            if (CurrentEvent is IAnnouncePlayersAlive announcePlayersAlive && players.Length > 1)
            {
                if (announcePlayersAlive.ClearPrevious)
                    Map.ClearBroadcasts();

                Map.Broadcast(5, $"{EMLB} Zostało {players.Length} <color={Color}>żywych</color>");
            }
        }
    }
}
