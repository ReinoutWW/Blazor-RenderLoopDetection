using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Detector
{
    public class RenderLoopDetector
    {
        private readonly int _maxRendersPerInterval;
        private readonly TimeSpan _checkInterval;
        private readonly ConcurrentDictionary<string, SlidingCounter> _rendersPerUser
            = new ConcurrentDictionary<string, SlidingCounter>();

        public RenderLoopDetector(int maxRendersPerInterval, TimeSpan checkInterval)
        {
            _maxRendersPerInterval = maxRendersPerInterval;
            _checkInterval = checkInterval;
        }

        public void RegisterRender(string userId)
        {
            var slidingCounter = _rendersPerUser.GetOrAdd(userId, _ => new SlidingCounter(_checkInterval));
            slidingCounter.Increment();

            if (slidingCounter.Count > _maxRendersPerInterval)
            {
                // Signaal afgeven aan UserExterminator
                UserExterminator.Exterminate(userId, reason: "RenderLoopDetected");
            }
        }
    }

    // Eenvoudige sliding window counter
    public class SlidingCounter
    {
        private readonly TimeSpan _windowSize;
        private readonly Queue<DateTime> _timestamps = new Queue<DateTime>();

        public SlidingCounter(TimeSpan windowSize)
        {
            _windowSize = windowSize;
        }

        public int Count => _timestamps.Count;

        public void Increment()
        {
            var now = DateTime.UtcNow;
            _timestamps.Enqueue(now);

            // Oude metingen weggooien
            while (_timestamps.Count > 0 && (now - _timestamps.Peek()) > _windowSize)
            {
                _timestamps.Dequeue();
            }
        }
    }

}
