// <copyright file="EventManager.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using MEC;
using Mistaken.API;
using Mistaken.API.Diagnostics;

namespace Mistaken.EventManager
{
    /// <summary>
    /// Event Manager class.
    /// </summary>
    internal class EventManager : Module
    {
        public const bool DNPN = true;

        /// <summary>
        /// Dictionary of loaded Events.
        /// </summary>
        public static readonly Dictionary<string, IEMEventClass> Events = new Dictionary<string, IEMEventClass>();

        public static readonly string Color = "#6B9ADF";

        public static readonly string EMLB = $"[<color={Color}><b>Event Manager</b></color> {(DNPN ? $"<color={Color}>Test Build</color>" : string.Empty)}] ";

        public static IEMEventClass ActiveEvent { get; set; }

        public static Queue<IEMEventClass> EventQueue { get; set; } = new Queue<IEMEventClass>();

        public static ushort RWE { get; set; } = 0;

        public static string CurrentWinnersFile { get; set; }

        public static EventManager Instance { get; private set; }

        /// <summary>
        /// Gets a value indicating whether any Event is active.
        /// </summary>
        public static bool EventActive() => ActiveEvent != null;

        /// <inheritdoc cref="Module.Module(IPlugin{IConfig})"/>
        public EventManager(PluginHandler p)
            : base(p)
        {
            Instance = this;
            this.SetBasePath(PluginHandler.Instance.Config.FolderPath);
            this.LoadEvents();
        }

        /// <inheritdoc/>
        public override string Name => nameof(EventManager);

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Server_WaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound += this.Server_RestartingRound;
            Exiled.Events.Handlers.Player.Verified += this.Player_Verified;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            Exiled.Events.Handlers.Player.Escaping += this.Player_Escaping;
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Server_WaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound -= this.Server_RestartingRound;
            Exiled.Events.Handlers.Player.Verified -= this.Player_Verified;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Player.Escaping -= this.Player_Escaping;
        }

        private static string BasePath { get; set; }

        private void LoadEvents()
        {
            this.Log.Info("Loading Events Started");
            foreach (Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(IEMEventClass))))
            {
                var ev = Activator.CreateInstance(t) as IEMEventClass;
                Events[ev.Id] = ev;
                this.Log.Debug("Event Loaded: " + ev.Name, PluginHandler.Instance.Config.VerbouseOutput);
            }

            this.Log.Info("Loading Events Completed");
        }

        private void SetBasePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                BasePath = Path.Combine(Paths.Plugins, "EventManager");
            else
                BasePath = Path.Combine(path, "EventManager");
            this.Log.Debug("Set base path to: " + BasePath, PluginHandler.Instance.Config.VerbouseOutput);
            if (!Directory.Exists(BasePath))
            {
                try
                {
                    Directory.CreateDirectory(BasePath);
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex);
                }
            }
        }

        private void UpdateWinnersFile()
        {
            var file = Directory.GetFiles(BasePath).FirstOrDefault(x => File.GetCreationTimeUtc(x).AddDays(PluginHandler.Instance.Config.NewWinnersFileDays) <= DateTime.UtcNow);
            if (file == default)
            {
                CurrentWinnersFile = Path.Combine(BasePath, $"winners - {DateTime.UtcNow.ToString("dd.MM.yyyy")} to {DateTime.UtcNow.AddDays(PluginHandler.Instance.Config.NewWinnersFileDays).ToString("dd.MM.yyyy")}.txt");
                File.Create(CurrentWinnersFile);
            }
            else
                CurrentWinnersFile = file;
        }

        private void Server_WaitingForPlayers()
        {
            this.UpdateWinnersFile();
            if (EventActive())
                return;
            if (EventQueue.TryDequeue(out var ev))
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
                if (RWE < PluginHandler.Instance.Config.AutoEventsRounds)
                    return;
                Timing.CallDelayed(10f, () =>
                {
                    if (!EventActive() && RealPlayers.List.Count() > 5)
                    {
                        var evid = PluginHandler.Instance.Config.AutoEventsList[UnityEngine.Random.Range(0, PluginHandler.Instance.Config.AutoEventsList.Count)];
                        if (Events.ContainsKey(evid))
                        {
                            this.Log.Info("[AutoEvent] Initiating: " + Events[evid].Name);
                            Events[evid].Initiate();
                            RWE = 0;
                        }
                        else
                            this.Log.Error("Failed to find event with id: " + evid);
                    }
                });
            }
        }

        private void Server_RoundStarted()
        {
            if (!EventActive())
                RWE += 1;
        }

        private void Server_RestartingRound()
        {
            if (ActiveEvent?.Active ?? false)
                ActiveEvent.DeInitiate();
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (EventActive())
                ev.Player.Broadcast(8, EMLB + $"Na serwerze obecnie trwa: <color={Color}>{ActiveEvent.Name}</color>");
        }

        private void Player_Escaping(Exiled.Events.EventArgs.EscapingEventArgs ev)
        {
            if (!EventActive())
                return;
            if (ActiveEvent is IWinOnEscape)
                ActiveEvent.OnEnd(ev.Player);
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (!EventActive())
                return;
            var players = RealPlayers.List.Where(p => p.IsAlive && p.Id != ev.Target.Id && p.IsHuman).ToList();
            if (players.Count == 1 && ActiveEvent is IWinOnLastAlive)
                ActiveEvent.OnEnd(players[0]);
            else if (players.Count == 0 && ActiveEvent is IEndOnNoAlive)
                ActiveEvent.OnEnd(player: null);
            if (ActiveEvent is IAnnouncePlayersAlive yes && players.Count > 1)
            {
                if (yes.ClearPrevious)
                    Map.ClearBroadcasts();
                Map.Broadcast(5, EMLB + $"Zostało {players.Count} <color={Color}>żywych</color>");
            }
        }
    }
}
