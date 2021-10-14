// -----------------------------------------------------------------------
// <copyright file="CommandHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Mistaken.API.Commands;
using Mistaken.EventManager.EventArgs;

namespace Mistaken.EventManager
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class CommandHandler : IBetterCommand, IPermissionLocked
    {
        /*public static (bool, string[]) SetRWEventCommand(Player admin, string[] args)
        {
            if (!admin.CheckPermission(EventManager.singleton.Name + ".setrwevent")) return (false, new string[] { "You can't use this command. No permission!" });
            if (args.Length == 1 || args[1] == "") return (false, new string[] { "Wrong args", "EventManager setrwevent [amount]" });
            if (int.Parse(args[1]) < 0) return (false, new string[] { "Number has to be non-negative int" });
            EventManager.rounds_without_event = int.Parse(args[1]);
            return (true, new string[] { "Done" });
        }*/

        public static (bool, string[]) ForceEndCommand(string[] args, Player sender)
        {
            if (EventManager.ActiveEvent == null) return (false, new string[] { "No event is on going" });
            EventManager.ActiveEvent.OnEnd($"Anulowano event: <color=#6B9ADF>{EventManager.ActiveEvent.Name}</color>");
            EventManager.ActiveEvent = null;
            return (true, new string[] { "Done" });
        }

        public static (bool, string[]) ForceCommand(string[] args, Player sender)
        {
            if (Mistaken.API.RealPlayers.List.Count() < 4 && !EventManager.DNPN) return (false, new string[] { "You can't use this command. Not enough players!" });
            else if (EventManager.ActiveEvent != null) return (false, new string[] { "You can't forcestack events" });
            var name = string.Join(" ", args).ToLower();

            foreach (var item in EventManager.Events.ToArray())
            {
                if (item.Value.Name.ToLower() == name || item.Value.Id.ToLower() == name)
                {
                    item.Value.Initiate();
                    new AdminInvokingEventEventArgs(sender, item.Value);

                    return (true, new string[] { $"<color=green>Activated</color> {item.Value.Name}", item.Value.Description });
                }
            }

            return (false, new string[] { "Event not found" });
        }

        public static (bool, string[]) ListCommand(string[] args, Player sender)
        {
            List<string> tor = new List<string>
            {
                "Events:",
            };

            foreach (var item in EventManager.Events.ToArray())
                tor.Add($"<color=green>{item.Value.Id}</color>: <color=yellow>{item.Value.Name}</color> <color=red>|</color> {item.Value.Description}");

            return (true, tor.ToArray());
        }

        public static (bool, string[]) QueueCommand(string[] args, Player sender)
        {
            if (args.Length == 0)
            {
                if (EventManager.EventQueue.Count == 0) return (true, new string[] { "Queue is empty" });
                var t = new List<string>();
                foreach (var ev in EventManager.EventQueue)
                {
                    t.Add(ev.Name);
                }

                return (true, t.ToArray());
            }
            else
            {
                var name = string.Join(" ", args).ToLower();

                foreach (var item in EventManager.Events.ToArray())
                {
                    if (item.Value.Name.ToLower() == name || item.Value.Id.ToLower() == name)
                    {
                        EventManager.EventQueue.Enqueue(item.Value);
                        return (true, new string[] { $"<color=green>Enqueued</color> {item.Value.Name}", item.Value.Description });
                    }
                }

                return (false, new string[] { "Event not found" });
            }
        }

        public static (bool, string[]) ClearWinnersFile(string[] args, Player sender)
        {
            File.WriteAllText(EventManager.BasePath + @"\winners.txt", string.Empty);

            return (true, new string[] { "File cleared" });
        }

        /// <inheritdoc/>
        public override string Command => "EventManager";

        /// <inheritdoc/>
        public override string[] Aliases => new string[] { "em" };

        /// <inheritdoc/>
        public override string Description => "Event Manager :)";

        /// <inheritdoc/>
        public string Permission => "*";

        /// <inheritdoc/>
        public string PluginName => nameof(EventManager);

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0)
            {
                return new string[] { this.GetUsage() };
            }

            var admin = Player.Get(sender);
            string cmd = args[0].ToLower();
            if (this.subcommands.TryGetValue(cmd, out var commandHandler))
            {
                var retuned = commandHandler.Invoke((args.Skip(1).ToArray(), admin));
                success = retuned.isSuccess;
                return retuned.message;
            }
            else
            {
                return new string[] { "Unknown Command", this.GetUsage() };
            }
        }

        private readonly Dictionary<string, Func<(string[], Player), (bool isSuccess, string[] message)>> subcommands = new Dictionary<string, Func<(string[], Player), (bool, string[])>>()
        {
            { "q", (args) => QueueCommand(args.Item1, args.Item2) },
            { "queue", (args) => QueueCommand(args.Item1, args.Item2) },
            { "g", (args) => { return (true, new string[] { "Current event: " + EventManager.ActiveEvent?.Name ?? "None" }); } },
            { "get", (args) => { return (true, new string[] { "Current event: " + EventManager.ActiveEvent?.Name ?? "None" }); } },
            { "l", (args) => ListCommand(args.Item1, args.Item2) },
            { "list", (args) => ListCommand(args.Item1, args.Item2) },
            { "f", (args) => ForceCommand(args.Item1, args.Item2) },
            { "force", (args) => ForceCommand(args.Item1, args.Item2) },
            { "forceend", (args) => ForceEndCommand(args.Item1, args.Item2) },
            { "clearwinners", (args) => ClearWinnersFile(args.Item1, args.Item2) },
            { "clw", (args) => ClearWinnersFile(args.Item1, args.Item2) },

            // {"rwe", (ply,args) => { return (true, new string[] { "Rounds Without Event:" + EventManager.rounds_without_event }); } },
            // {"setrwevent", (ply, args) => SetRWEventCommand(ply,args) },
        };

        private string GetUsage()
        {
            return
                "\n () = optional argument" +
                "\n [] = required argument" +
                "\n EventManager list - list of available events" +
                "\n EventManager force [event name] - initialized specified event" +
                "\n EventManager forceend - deinitializes current event" +
                "\n EventManager queue (event name) - displays the current queue of events or adds an event to the queue (if specified)" +
                "\n EventManager get - gets the name of current event" +

                // "\n EventManager rwe - rounds without event" +
                "\n EventManager setrwevent [number] - sets rounds without event"
                ;
        }
    }
}
