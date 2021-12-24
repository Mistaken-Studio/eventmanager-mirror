// <copyright file="EventManager.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Mistaken.API.Diagnostics;
using Mistaken.EventManager.EventCreator;

namespace Mistaken.EventManager
{
    /// <summary>
    /// Event Manager.
    /// </summary>
    internal class EventManager : Module
    {
        public const bool DNPN = true;

        /// <summary>
        /// Dictionary of loaded Events.
        /// </summary>
        public static readonly Dictionary<string, IEMEventClass> Events = new Dictionary<string, IEMEventClass>();

        public static IEMEventClass ActiveEvent { get; set; }

        /// <summary>
        /// Gets a value indicating whether any Event is active.
        /// </summary>
        public static bool EventActive() => ActiveEvent != null;

        /// <inheritdoc cref="Module.Module(IPlugin{IConfig})"/>
        public EventManager(PluginHandler p)
            : base(p)
        {
            this.LoadEvents();
            BasePath = this.SetBasePath(PluginHandler.Instance.Config.EMFolderPath);
            if (!Directory.GetFiles(BasePath).Contains("winners.txt"))
                File.Create(BasePath + @"\winners.txt");
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

        /// <summary>
        /// Loads Events.
        /// </summary>
        public void LoadEvents()
        {
            this.Log.Info("Loading Events Started");
            foreach (Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(IEMEventClass))))
            {
                this.PrepareEvent(Activator.CreateInstance(t) as IEMEventClass);
            }

            this.Log.Info("Loading Events Completed");
        }

        internal static readonly string EMLB = $"[<color=#6B9ADF><b>Event Manager</b></color> {(DNPN ? "<color=#6B9ADF>Test Build</color>" : string.Empty)}] ";

        /// <summary>
        /// Gets or sets queue of Events.
        /// </summary>
        internal static Queue<IEMEventClass> EventQueue { get; set; } = new Queue<IEMEventClass>();

        internal static string BasePath { get; private set; }

        private string SetBasePath(string path)
        {
            string @string;
            if (string.IsNullOrEmpty(path))
                @string = Paths.Plugins + @"\EventManager";
            else
                @string = path + @"\EventManager";
            this.Log.Debug("Set base path to: " + @string, PluginHandler.Instance.Config.VerbouseOutput);
            if (!Directory.Exists(@string))
            {
                try
                {
                    Directory.CreateDirectory(@string);
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex);
                }
            }

            return @string;
        }

        private void PrepareEvent(IEMEventClass ev)
        {
            Events[ev.Id] = ev;
            this.Log.Info("Event Loaded: " + ev.Id);
        }

        private void Server_WaitingForPlayers()
        {
            if (!EventActive() && EventQueue.TryDequeue(out var eventClass))
            {
                this.Log.Debug(eventClass.Id, PluginHandler.Instance.Config.VerbouseOutput);
                try
                {
                    eventClass.Initiate();
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex);
                }
            }
            else
            {
                this.Log.Debug(EventQueue.Count, PluginHandler.Instance.Config.VerbouseOutput);
                this.Log.Debug(EventActive(), PluginHandler.Instance.Config.VerbouseOutput);
            }
        }

        private void Server_RoundStarted()
        {
            if (EventActive())
                Map.Broadcast(5, EMLB + $"<color=#6B9ADF>{ActiveEvent.Name}</color>");
        }

        private void Server_RestartingRound()
        {
            if (ActiveEvent?.Active ?? false)
                ActiveEvent.DeInitiate();
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (EventActive())
                ev.Player.Broadcast(10, $"{EMLB} Na serwerze obecnie trwa: <color=#6B9ADF>{ActiveEvent.Name}</color>");
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
            var players = Mistaken.API.RealPlayers.List.Where(p => p.IsAlive && p.Id != ev.Target.Id && p.IsHuman).ToList();
            if (players.Count == 1 && ActiveEvent is IWinOnLastAlive)
                ActiveEvent.OnEnd(players[0]);
            else if (players.Count == 0 && ActiveEvent is IEndOnNoAlive)
                ActiveEvent.OnEnd(player: null);
            if (ActiveEvent is IAnnouncePlayersAlive && players.Count > 1)
            {
                Map.ClearBroadcasts();
                Map.Broadcast(5, EMLB + $"Zostało {players.Count} <color=#6B9ADF>żywych</color>");
            }
        }
    }
}
