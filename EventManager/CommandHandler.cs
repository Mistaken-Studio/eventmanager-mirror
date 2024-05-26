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
using Mistaken.API;
using Mistaken.API.Commands;
using Mistaken.API.Extensions;
using Mistaken.EventManager.EventArgs;
using Mistaken.EventManager.Interfaces;

#pragma warning disable SA1118 // Parameter spans multiple lines

namespace Mistaken.EventManager
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class CommandHandler : IBetterCommand, IPermissionLocked
    {
        public override string Command => "EventManager";

        public override string[] Aliases => new string[] { "em" };

        public override string Description => "Event Manager :)";

        public string Permission => "*";

        public string PluginName => nameof(EventManager);

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (args.Length == 0)
                return new string[] { Usage };

            var player = Player.Get(sender);
            string command = args[0].ToLower();
            if (SubCommands.TryGetValue(command, out var commandHandler))
            {
                var retuned = commandHandler.Invoke(new CommandArguments(player, args.Skip(1).ToArray()));
                success = retuned.Success;
                return retuned.Response;
            }

            return new string[] { "Unknown Command", Usage };
        }

        private static readonly Dictionary<string, Func<CommandArguments, CommandResult>> SubCommands = new ()
        {
            { "q", (args) => QueueCommand(args) },
            { "queue", (args) => QueueCommand(args) },
            { "g", (args) => new CommandResult(new string[] { "Current event: " + EventManager.CurrentEvent?.Name ?? "None" }, true) },
            { "get", (args) => new CommandResult(new string[] { "Current event: " + EventManager.CurrentEvent?.Name ?? "None" }, true) },
            { "l", (args) => ListCommand() },
            { "list", (args) => ListCommand() },
            { "f", (args) => ForceCommand(args) },
            { "force", (args) => ForceCommand(args) },
            { "forceend", (args) => ForceEndCommand() },
            { "rwe", (args) => new CommandResult(new string[] { "Rounds Without Event:" + EventManager.RoundsWithoutEvent }, true) },
            { "setrwe", (args) => SetRWECommand(args) },
            { "prtwin", (args) => PrintWinners() },
        };

        private static readonly string Usage = string.Join(Environment.NewLine, new string[]
        {
            "() = optional argument",
            "[] = required argument",
            "EventManager list - list of available events",
            "EventManager force [event id/name without spaces] - initialized specified event",
            "EventManager forceend - deinitializes current event",
            "EventManager queue (event id/name without spaces) - displays the current queue of events or adds an event to the queue",
            "EventManager get - gets the name of current event",
            "EventManager rwe - gets the rounds without events",
            "EventManager setrwe [number] - sets rounds without events",
            "EventManager prtwin - prints a list of this 2 weeks event winners",
        });

        private static CommandResult QueueCommand(CommandArguments cmdArgs)
        {
            if (cmdArgs.Arguments.Length == 0)
            {
                if (EventManager.EventQueue.Count == 0)
                    return new CommandResult(new string[] { "Queue is empty" }, true);

                return new CommandResult(EventManager.EventQueue.Select(x => x.Name).ToArray(), true);
            }

            var name = cmdArgs.Arguments[0].ToLower();
            var ev = EventManager.Events.FirstOrDefault(x => x.Key == name || x.Value.Name.ToLower() == name).Value;
            if (ev is null)
                return new CommandResult(new string[] { "Event not found" });

            EventManager.EventQueue.Enqueue(ev);
            return new CommandResult(new string[] { $"<color=green>Enqueued</color> {ev.Name}", ev.Description }, true);
        }

        private static CommandResult ListCommand()
        {
            List<string> tor = new ()
            {
                "Events:",
            };

            foreach (var ev in EventManager.Events)
                tor.Add($"<color=green>{ev.Value.Id}</color>: <color=yellow>{ev.Value.Name}</color> <color=red>|</color> {ev.Value.Description}");

            return new CommandResult(tor.ToArray(), true);
        }

        private static CommandResult ForceCommand(CommandArguments cmdArgs)
        {
            if (cmdArgs.Arguments.Length == 0)
                return new CommandResult(new string[] { "Wrong args", "EventManager force [event id/name without spaces]" });

            var name = cmdArgs.Arguments[0].ToLower();
            var ev = EventManager.Events.FirstOrDefault(x => x.Key == name || x.Value.Name.ToLower() == name).Value;
            if (ev is null)
                return new CommandResult(new string[] { "Event not found" });

            if (EventManager.IsEventActive)
            {
                EventManager.EventQueue.Enqueue(ev);
                return new CommandResult(new string[] { $"<color=green>Enqueued</color> {ev.Name}", ev.Description }, true);
            }

            if (ev is IRequiredPlayers requiredPlayers && requiredPlayers.PlayerCount > RealPlayers.List.Count() && !PluginHandler.Instance.Config.DoNotCountPlayers)
                return new CommandResult(new string[] { "You can't use this command. Not enough players!" });

            EventHandler.OnAdminInvokingEvent(new AdminInvokingEventEventArgs(cmdArgs.Sender, ev));
            ev.Initiate();
            return new CommandResult(new string[] { $"<color=green>Activated</color> {ev.Name}", ev.Description }, true);
        }

        private static CommandResult ForceEndCommand()
        {
            if (!EventManager.IsEventActive)
                return new CommandResult(new string[] { "No event is on going" });

            EventManager.CurrentEvent.OnEnd($"Anulowano event: <color={EventManager.Color}>{EventManager.CurrentEvent.Name}</color>");
            return new CommandResult(new string[] { "Done" }, true);
        }

        private static CommandResult SetRWECommand(CommandArguments cmdArgs)
        {
            if (!cmdArgs.Sender.IsDev())
                return new CommandResult(new string[] { "This command is only for the Devs!" });

            if (cmdArgs.Arguments.Length == 0)
                return new CommandResult(new string[] { "Wrong args", "EventManager setrwevent [amount]" });

            if (cmdArgs.Arguments[1] == string.Empty)
                return new CommandResult(new string[] { "Wrong args", "EventManager setrwevent [amount]" });

            if (ushort.TryParse(cmdArgs.Arguments[1], out var value))
            {
                EventManager.RoundsWithoutEvent = value;
                return new CommandResult(new string[] { "Done" }, true);
            }

            return new CommandResult(new string[] { "You must provide a value between 0 and 65535" });
        }

        private static CommandResult PrintWinners()
        {
            var lines = File.ReadAllLines(EventManager.WinnersFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                var content = lines[i].Split(';');
                lines[i] = content[0] + " - " + content[2] + " wygranych eventów";
            }

            return new CommandResult(lines, true);
        }

        private struct CommandResult
        {
            public readonly string[] Response;

            public readonly bool Success;

            public CommandResult(string[] response, bool success = false)
            {
                this.Response = response;
                this.Success = success;
            }
        }

        private struct CommandArguments
        {
            public readonly Player Sender;

            public readonly string[] Arguments;

            public CommandArguments(Player sender, string[] arguments)
            {
                this.Sender = sender;
                this.Arguments = arguments;
            }
        }
    }
}
