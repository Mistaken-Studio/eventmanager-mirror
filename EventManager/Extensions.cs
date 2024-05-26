// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using MEC;
using UnityEngine;

namespace Mistaken.EventManager
{
    internal static class Extensions
    {
        public static void SlowChangeRole(this Player player, RoleType role, Vector3 pos = default)
            => EventManager.Instance.RunCoroutine(SlowFC(player, role, pos), "EventManager_SlowFC");

        public static void UpdateInWinnersFile(this Player player)
        {
            var lines = File.ReadAllLines(EventManager.WinnersFilePath);
            var playerInFile = lines.FirstOrDefault(x => x.Split(';')[1] == player.UserId);
            if (playerInFile is null)
            {
                File.AppendAllLines(EventManager.WinnersFilePath, new string[] { $"{player.Nickname};{player.UserId};1" });
                return;
            }

            File.AppendAllLines(EventManager.WinnersFilePath, new string[] { $"{player.Nickname};{player.UserId};{int.Parse(playerInFile.Split(';')[2]) + 1}" });
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
