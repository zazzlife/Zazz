using System;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure
{
    public class CircularBufferCache<TKey, TVal> : ICacheSystem<TKey, TVal>
    {
        private readonly int _maximumSize;
        internal readonly ConcurrentDictionary<TKey, TVal> Items;
        internal readonly ConcurrentDictionary<TKey, int> RequestsCounter;

        public CircularBufferCache(int maximumSize)
        {
            if (maximumSize < 1)
                throw new ArgumentOutOfRangeException("maximumSize");

            _maximumSize = maximumSize;
            Items = new ConcurrentDictionary<TKey, TVal>();
            RequestsCounter = new ConcurrentDictionary<TKey, int>();
        }

        public void Add(TKey key, TVal val)
        {
            if (Items.Count >= _maximumSize)
                RemoveLowestItem();

            Items.TryAdd(key, val);
            RequestsCounter.TryAdd(key, 0);
        }

        private void RemoveLowestItem()
        {
            try
            {
                if (RequestsCounter.Count < 1)
                    return;

                var key = RequestsCounter
                    .OrderBy(r => r.Value)
                    .First()
                    .Key;

                Remove(key);
            }
            catch (Exception)
            {} // we dont really want to break the request only because it failed to remove because of race condition
        }

        public void Remove(TKey key)
        {
            TVal val;
            Items.TryRemove(key, out val);
            
            int count;
            RequestsCounter.TryRemove(key, out count);
        }

        public TVal TryGet(TKey key)
        {
            TVal val;
            Items.TryGetValue(key, out val);

            if (!val.Equals(default(TVal)))
                IncrementCounter(key);

            return val;
        }

        private void IncrementCounter(TKey key)
        {
            int currentCount;
            RequestsCounter.TryGetValue(key, out currentCount);

            if (currentCount == Int32.MaxValue)
                return;

            RequestsCounter.TryUpdate(key, (currentCount + 1), currentCount);
        }
    }
}