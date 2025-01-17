using System.Collections.Concurrent;

namespace Blazor_RenderLoopDetection.Detectors
{
    public class RenderLoopDetector
    {
        private readonly IUserExterminator _userExterminator;
        private readonly int _maxRendersPerInterval;
        private readonly TimeSpan _checkInterval;
        private readonly ConcurrentDictionary<string, SlidingCounter> _rendersPerUser
            = new ConcurrentDictionary<string, SlidingCounter>();

        public RenderLoopDetector(int maxRendersPerInterval, TimeSpan checkInterval, IUserExterminator userExterminator)
        {
            _maxRendersPerInterval = maxRendersPerInterval;
            _checkInterval = checkInterval;
            _userExterminator = userExterminator;
        }

        public void RegisterRender(string userId)
        {
            var slidingCounter = _rendersPerUser.GetOrAdd(userId, _ => new SlidingCounter(_checkInterval));
            slidingCounter.Increment();

            if (slidingCounter.Count > _maxRendersPerInterval)
            {
                _userExterminator.Exterminate(userId, reason: "RenderLoopDetected");
            }
        }
    }

    // Simple sliding window counter
    // https://medium.com/@avocadi/rate-limiter-sliding-window-counter-7ec08dbe21d6
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

            RemovePreviousMeasurements(now);
        }

        private void RemovePreviousMeasurements(DateTime now)
        {
            while (_timestamps.Count > 0 && (now - _timestamps.Peek()) > _windowSize)
            {
                _timestamps.Dequeue();
            }
        }
    }

}
