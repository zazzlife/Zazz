namespace Zazz.Core.Interfaces
{
    /// <summary>
    /// A caching handler for one item only. It should be a singleton.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TVal"></typeparam>
    public interface ICacheSystem<TKey, TVal>
    {
        void Add(TKey key, TVal val);

        void Remove(TKey key);

        TVal TryGet(TKey key);
    }
}