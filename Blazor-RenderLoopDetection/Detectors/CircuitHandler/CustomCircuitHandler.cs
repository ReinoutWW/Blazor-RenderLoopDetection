using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;

namespace Blazor_RenderLoopDetection.Detectors.CircuitHandler
{
    public class CustomCircuitHandler : Microsoft.AspNetCore.Components.Server.Circuits.CircuitHandler
    {
        private static readonly ConcurrentDictionary<string, string> _users = new();
        private static readonly ConcurrentDictionary<string, Circuit> _circuits = new();

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            // Example: Hardcoding 'user1', but you’d use actual logic to figure out the user
            _users.TryAdd(circuit.Id, "user1");

            // Store the actual circuit for later access
            _circuits[circuit.Id] = circuit;

            return Task.CompletedTask;
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            // Remove from both dictionaries to free references
            _users.TryRemove(circuit.Id, out _);
            _circuits.TryRemove(circuit.Id, out _);

            return Task.CompletedTask;
        }

        public static FrozenDictionary<string, string> GetCircuitIdsForUserId(string userId)
        {
            return _users
                .Where(kvp => kvp.Value == userId)
                .ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Forcibly close (dispose) a circuit by ID.
        /// </summary>
        public static async Task TerminateCircuitAsync(string circuitId)
        {
            // Do we have a live circuit object?
            if (_circuits.TryGetValue(circuitId, out var circuit))
            {
                // Reflect to get IAsyncDisposable and forcibly dispose
                var iDisposable = GetCircuitHostAsDisposable(circuit);
                if (iDisposable != null)
                {
                    await iDisposable.DisposeAsync();
                }

                // Optionally remove it from our dictionaries
                _circuits.TryRemove(circuitId, out _);
                _users.TryRemove(circuitId, out _);
            }
        }

        /// <summary>
        /// Reflection: obtains the private _circuitHost field and casts it to IAsyncDisposable.
        /// </summary>
        private static IAsyncDisposable? GetCircuitHostAsDisposable(Circuit circuit)
        {
            var fieldInfo = typeof(Circuit).GetField("_circuitHost",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo == null)
            {
                return null;
            }

            // _circuitHost is an internal type that implements IAsyncDisposable
            var hostObject = fieldInfo.GetValue(circuit);
            return hostObject as IAsyncDisposable;
        }
    }
}
