// -----------------------------------------------------------------------
// <copyright file="IAnnouncePlayersAlive.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Mistaken.EventManager
{
    /// <summary>
    /// Announcec when someone dies.
    /// </summary>
    public interface IAnnouncePlayersAlive
    {
        /// <summary>
        /// Gets a value indicating whether broadcasts should be cleared before sending.
        /// </summary>
        bool ClearPrevious { get; }
    }
}
