/*using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;

namespace SIMS
{
    public class DBBase
    {
        string connectionString;
        static async Task GetSecret()
        {
            string secretName = "sims/rdb";
            string region = "eu-central-1";

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT"
            };

            GetSecretValueResponse response;

            try
            {
                response = await client.GetSecretValueAsync(request);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to retrieve secret from AWS Secrets Manager", e);
            }

            string secret = response.SecretString;
            //Bis hier wars amazon code
            // Parse JSON vom secret, evtl unnedig
            var json = JsonDocument.Parse(secret).RootElement;

            string host = json.GetProperty("host").GetString();
            string port = json.GetProperty("port").GetString();
            string user = json.GetProperty("username").GetString();
            string password = json.GetProperty("password").GetString();
            string db_id = json.GetProperty("dbInstanceIdentifier").GetString();
            string db = json.GetProperty("dbname").GetString();

            //Connection string erstellen
            connectionString = $"Host={host};Port={port};Username={user};Password={password};Database={db};";
            //Environment Variable setzen
            Environment.SetEnvironmentVariable("POSTGRES_CONNECTION", connectionString);


            return connectionString;
        }


        //redis 
        GetSecret();
        public string ConnectionString { get; set; } = connectionString;
        
        //?? "Host=localhost;Username=postgresadmin;Password=1234;Database=db1";


    }



    
}
//username, password, identifier hat tobi gesetzt
//sims_rdb
*/
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;

namespace SIMS
{
    public class DBBase
    {
        public string ConnectionString { get; private set; }

        public DBBase()
        {
            // Blockiere Konstruktor nicht mit async -> ruft Init() synchronisiert auf
            ConnectionString = GetSecret().GetAwaiter().GetResult();
            Environment.SetEnvironmentVariable("POSTGRES_CONNECTION", ConnectionString);
        }

        private static async Task<string> GetSecret()
        {
            string secretName = "sims/rdb";
            string region = "eu-central-1";

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            var request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT"
            };

            GetSecretValueResponse response;

            try
            {
                response = await client.GetSecretValueAsync(request);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to retrieve secret from AWS Secrets Manager", e);
            }

            string secret = response.SecretString;

            var json = JsonDocument.Parse(secret).RootElement;
            string host = json.GetProperty("host").GetString();
            string port = json.GetProperty("port").GetString();
            string user = json.GetProperty("username").GetString();
            string password = json.GetProperty("password").GetString();
            string db = json.GetProperty("dbname").GetString();

            return $"Host={host};Port={port};Username={user};Password={password};Database={db};";
        }
    }
}
