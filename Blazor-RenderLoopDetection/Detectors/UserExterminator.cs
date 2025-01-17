using Blazor_RenderLoopDetection.Detectors.CircuitHandler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_RenderLoopDetection.Detectors
{
    /// <summary>
    /// This is the core user exterminator logic. Multiple signals can be used to trigger a Exterminate
    /// </summary>
    public class UserExterminator : IUserExterminator
    {
        private static ConcurrentDictionary<string, DateTime> _bannedUsers
            = new ConcurrentDictionary<string, DateTime>();

        public void Exterminate(string userId, string reason)
        {
            // Stap 1: Log het probleem 
            Console.WriteLine($"Exterminating user {userId} because {reason}");

            // Stap 2: Possibly mark the user as temp banned
            _bannedUsers[userId] = DateTime.UtcNow.AddHours(1); // e.g. one hour

            var userCircuitIds = CustomCircuitHandler.GetCircuitIdsForUserId(userId);

            // Stap 3: Force logout
            // Depending on chosen auth. This will close the blazor circuit

            // OR
            userCircuitIds.ToList().ForEach(circuitkeyValuePair => CustomCircuitHandler.TerminateCircuitAsync(circuitkeyValuePair.Key));
        }

        public static bool IsBanned(string userId)
        {
            if (_bannedUsers.TryGetValue(userId, out var bannedUntil))
            {
                if (bannedUntil > DateTime.UtcNow)
                    return true;
                else
                    _bannedUsers.TryRemove(userId, out _);
            }
            return false;
        }
    }

}
