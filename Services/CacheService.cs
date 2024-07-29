using Newtonsoft.Json;
using StackExchange.Redis;

namespace NFC.Services
{
	public interface ICacheService
	{
		T GetData<T>(string key) where T : class;
		void SetData<T>(string key, T value, TimeSpan expirationTime) where T : class;
		bool RemoveData(string key);
	}
	public class CacheService : ICacheService
	{
		private readonly ConnectionMultiplexer _redis;
		private readonly IDatabase _database;

		public CacheService(string connectionString)
		{
			_redis = ConnectionMultiplexer.Connect(connectionString);
			_database = _redis.GetDatabase();
		}

		public T GetData<T>(string key) where T : class
		{
			var value = _database.StringGet(key);
			if (value.HasValue)
			{
				return JsonConvert.DeserializeObject<T>(value);
			}
			return null;
		}

		public void SetData<T>(string key, T value, TimeSpan expirationTime) where T : class
		{
			var stringValue = JsonConvert.SerializeObject(value);
			_database.StringSet(key, stringValue, expirationTime);
		}

		public bool RemoveData(string key)
		{
			return _database.KeyDelete(key);
		}
	}
}
