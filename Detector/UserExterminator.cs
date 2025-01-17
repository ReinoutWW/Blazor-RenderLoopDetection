using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Detector
{
    public static class UserExterminator
    {
        // In-memory of met een distributed cache voor multi-instance scenario’s
        private static ConcurrentDictionary<string, DateTime> _bannedUsers
            = new ConcurrentDictionary<string, DateTime>();

        public static void Exterminate(string userId, string reason)
        {
            // Stap 1: Log het probleem 
            Console.WriteLine($"Exterminating user {userId} vanwege {reason}");

            // Stap 2: Markeer de gebruiker als 'verbannen' of 'afgebroken'
            _bannedUsers[userId] = DateTime.UtcNow.AddHours(1); // Bijvoorbeeld 1 uur verbannen.

            // Stap 3: Sluit de Blazor-circuit (indien mogelijk)
            // In .NET 6/7 Blazor kun je circuits benaderen via Circuit handlers,
            // of via een service die circuits bijhoudt.
            CircuitManager.TerminateCircuit(userId);

            // Stap 4: Forceer uitlog 
            // Hangt af van je authenticatiemechanisme (bijv. sign-out via cookie oid.)
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
