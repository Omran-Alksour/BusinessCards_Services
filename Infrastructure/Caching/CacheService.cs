using Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Infrastructure.Caching;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private static readonly ConcurrentDictionary<string, bool> CacheKeys = new();


    public CacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }


    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        string? serializedCachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);

            if (serializedCachedValue is null)
            {
                return null;
            }

            T? cachedValue = JsonConvert.DeserializeObject<T>(serializedCachedValue);

            return cachedValue;
        
    }


    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) where T : class
    {
       string serializedValue =JsonConvert.SerializeObject(value);
        await  _distributedCache.SetStringAsync(key, serializedValue, cancellationToken);

        CacheKeys.TryAdd(key,true);

    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default) 
    {
     await  _distributedCache.RemoveAsync(key, cancellationToken);
        CacheKeys.TryRemove(key,out bool _);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default) 
    {

     IEnumerable<Task> removeTasks = CacheKeys.Keys.Where(key=>key.StartsWith(prefix))
            .Select( k=>  RemoveAsync(k, cancellationToken));

        await Task.WhenAll(removeTasks);
    }

    public async Task<T?> GetAsync<T>(string key, Func<Task<T>> func, CancellationToken cancellationToken =default) where T : class
    {
        if (!CacheKeys.Keys.Contains(key))
        {
            return await func();
        }

        T? cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue is not null)
        {
            return cachedValue;
        }
        cachedValue = await func();

      await  SetAsync(key,cachedValue,cancellationToken);

      return cachedValue; 
    }
}