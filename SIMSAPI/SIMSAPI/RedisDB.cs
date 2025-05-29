/*using StackExchange.Redis;

namespace SIMSAPI
{
    public class RedisDB
    {
        private string dbname = "";

        public RedisDB() 
        {
            dbname = Environment.GetEnvironmentVariable("redisdb") ;
            //?? "localhost";
            
        }

        public void StoreToken(string username, string token) 
        {
            try
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(dbname);
                IDatabase db = redis.GetDatabase();
                db.StringSet(username, token);
            }
            catch
            {
                throw;
            }
        }

        public bool CheckToken(string username, string token)
        {
            try
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(dbname);
                IDatabase db = redis.GetDatabase();
                return db.StringGet(username).ToString() == token ? true : false;
            }
            catch
            {
                throw;
            }
        }

    }
}
*/

/*using StackExchange.Redis;

namespace SIMSAPI
{
    public class RedisDB
    {
        private readonly string redisHost;
        private readonly string redisPort;
        private readonly ConnectionMultiplexer redis;

        public RedisDB()
        {
            redisHost = Environment.GetEnvironmentVariable("redis_host");
            redisPort = Environment.GetEnvironmentVariable("redis_port");

            var config = $"{redisHost}:{redisPort}";

            try
            {
                redis = ConnectionMultiplexer.Connect(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Redis connection failed: {ex.Message}");
                throw;
            }
        }

        public void StoreToken(string username, string token)
        {
            try
            {
                IDatabase db = redis.GetDatabase();
                db.StringSet(username, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StoreToken failed: {ex.Message}");
                throw;
            }
        }

        public bool CheckToken(string username, string token)
        {
            try
            {
                IDatabase db = redis.GetDatabase();
                var storedToken = db.StringGet(username);
                return storedToken == token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CheckToken failed: {ex.Message}");
                throw;
            }
        }
    }
}
*/
using StackExchange.Redis;
using System;

namespace SIMSAPI
{
    public class RedisDB
    {
        private readonly string redisHost;
        private readonly string redisPort;
        private readonly ConnectionMultiplexer redis;

        public RedisDB()
        {
            redisHost = Environment.GetEnvironmentVariable("redis_host");
            redisPort = Environment.GetEnvironmentVariable("redis_port");

            if (string.IsNullOrWhiteSpace(redisHost) || string.IsNullOrWhiteSpace(redisPort))
            {
                throw new InvalidOperationException("Redis host or port environment variables are not set.");
            }

            Console.WriteLine($"Connecting to Redis at {redisHost}:{redisPort} with TLS...");

            var config = new ConfigurationOptions
            {
                EndPoints = { $"{redisHost}:{redisPort}" },
                Ssl = true,
                AbortOnConnectFail = false,
                AllowAdmin = false
            };

            try
            {
                redis = ConnectionMultiplexer.Connect(config);
                Console.WriteLine("Redis connection established successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Redis connection failed: {ex.Message}");
                throw;
            }
        }

        public void StoreToken(string username, string token)
        {
            try
            {
                IDatabase db = redis.GetDatabase();
                db.StringSet(username, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StoreToken failed: {ex.Message}");
                throw;
            }
        }

        public bool CheckToken(string username, string token)
        {
            try
            {
                IDatabase db = redis.GetDatabase();
                var storedToken = db.StringGet(username);
                return storedToken == token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CheckToken failed: {ex.Message}");
                throw;
            }
        }
    }
}
