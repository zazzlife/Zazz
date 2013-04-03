﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Zazz.Infrastructure
{
    public class CircularBufferCache<TKey, TVal>
    {
        private readonly int _maximumSize;
        internal readonly ConcurrentDictionary<TKey, TVal> Items;
        internal readonly ConcurrentDictionary<TKey, int> RequestsCounter;

        public CircularBufferCache(int maximumSize)
        {
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
            if (RequestsCounter.Count < 1)
                return;

            var key = RequestsCounter
                .OrderBy(r => r.Value)
                .First()
                .Key;

            Remove(key);
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

            RequestsCounter.TryUpdate(key, (currentCount + 1), currentCount);
        }
    }
}