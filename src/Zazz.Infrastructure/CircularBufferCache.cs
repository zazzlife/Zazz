using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Zazz.Infrastructure
{
    public class CircularBufferCache<TKey, TVal>
    {
        private readonly int _maximumSize;
        internal ConcurrentDictionary<TKey, TVal> Items;
        internal ConcurrentDictionary<TKey, int> RequestsCounter;

        public CircularBufferCache(int maximumSize)
        {
            _maximumSize = maximumSize;
            Items = new ConcurrentDictionary<TKey, TVal>();
            RequestsCounter = new ConcurrentDictionary<TKey, int>();
        }

        public void Add(TKey key, TVal val)
        {
            Items.TryAdd(key, val);
        }
    }
}