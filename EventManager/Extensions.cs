// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using MEC;
using UnityEngine;

namespace Mistaken.EventManager
{
    /// <summary>
    /// Extensions.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Waits 0.1s, changes role, waits 1s, sets position if not default.
        /// </summary>
        /// <param name="player">player.</param>
        /// <param name="role">role.</param>
        /// <param name="pos">position.</param>
        public static void SlowChangeRole(this Player player, RoleType role, Vector3 pos = default) => Timing.RunCoroutine(SlowFC(player, role, pos));

        internal static bool UpdateInWinnersFile(this Player player)
        {
            if (!player.RemoteAdminAccess)
            {
                var lines = File.ReadAllLines(EventManager.CurrentWinnersFile);
                File.AppendAllLines(EventManager.CurrentWinnersFile, new string[] { $"{player.Nickname};{player.UserId};{(lines.Any(x => x.Contains(player.Nickname)) ? int.Parse(lines.First(x => x.Contains(player.Nickname)).Split(';')[2]) + 1 : 1)}" });
                return true;
            }
            else
                return false;
        }

        private static IEnumerator<float> SlowFC(Player player, RoleType role, Vector3 pos = default)
        {
            yield return Timing.WaitForSeconds(0.1f);
            player.Role.Type = role;
            if (pos != default)
            {
                yield return Timing.WaitForSeconds(1f);
                player.Position = pos;
            }
        }
    }
}
