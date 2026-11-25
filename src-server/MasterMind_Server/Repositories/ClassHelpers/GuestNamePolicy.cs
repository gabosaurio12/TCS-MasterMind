using MasterMind_Server.Data;
using System;
using System.Linq;

namespace MasterMind_Server.Repositories.ClassHelpers
{
    internal static class GuestNamePolicy
    {
        internal static bool IsValid(string username)
        {
            return !string.IsNullOrWhiteSpace(username) && !username.Any(char.IsWhiteSpace);
        }

        internal static Player BuildGuest(string username, byte onlineValue)
        {
            return new Player
            {
                username = username,
                email = $"guest_{Guid.NewGuid():N}@guest.invalid",
                password = Guid.NewGuid().ToString("N"),
                is_online = onlineValue,
                is_guest = true
            };
        }
    }
}
