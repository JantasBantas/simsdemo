using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Npgsql;
using RestSharp;
using Amazon.Lambda;
using Amazon.Lambda.Model;



namespace SIMSAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthService : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }

        [HttpGet]
        [Route("check")]
        public IActionResult Get(string username, string token)
        {
            if (new RedisDB().CheckToken(username, token) == true)
            {
                return Ok();
            }

            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public string Post(string username, string password)
        {
            if (checkUser(username, password))
            {
                //UPDATE LAST LOGIN:
                //KOMMENTAR
                System.Console.WriteLine("START update lastlogin");

                string connectionString = Environment.GetEnvironmentVariable("connectionstring");
                using (NpgsqlConnection db = new NpgsqlConnection(connectionString))
                {
                    db.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand($"UPDATE sims.simsuser SET lastlogin = NOW() WHERE username = @username", db))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        cmd.ExecuteNonQuery();
                    }
                    db.Close();
                }
                //KOMMENTAR
                System.Console.WriteLine("END update lastlogin");


                //KOMMENTAR:
                Console.WriteLine("POST-if-checkuser-OK");

                string token = generateToken(username, password);
                new RedisDB().StoreToken(username, token);

                //KOMMENTAR:
                Console.WriteLine("TOKEN-stored");

                return token;
            }
            else
            {
                //KOMMENTAR:
                System.Console.WriteLine("TOKEN-NOT-stored");
                return "";
            }
        }

/*
        [HttpPost]
        [Route("STOP")]
        public string Post(string resourceID)
        {
            string _resourceID = resourceID;
                //KOMMENTAR:
                System.Console.WriteLine("API resourceID = " + _resourceID);
                
            string LambdaURL = Environment.GetEnvironmentVariable("URL");
            RestClient client = new RestClient(LambdaURL);
                //KOMMENTAR:
                System.Console.WriteLine("API LambdaURL = " + LambdaURL);
            RestRequest request = new RestRequest("", Method.Post);
            var body = new
            {
                cluster = "SIMS_Cluster",
                task = _resourceID
            };
            request.AddJsonBody(body);
                //KOMMENTAR:
                System.Console.WriteLine("requestbody =" + body );
                System.Console.WriteLine("full request API to lambda = " + request);
            RestResponse response = client.Execute(request);
                //KOMMENTAR:
                System.Console.WriteLine("API response from Lambda = " + response.Content);
            return response.Content;
        }
        */

//NEUER VERSUCH ÜBER SDK:

        [HttpPost]
        [Route("STOP")]
        public async Task<string> Post(string resourceID)
        {
            string functionName = Environment.GetEnvironmentVariable("Lambda_ARN");

            var lambdaClient = new AmazonLambdaClient(); // verwendet standardmäßig Umgebungs- oder EC2-Rollen

            var payloadObj = new
            {
                cluster = "SIMS_Cluster",
                task = resourceID
            };

            string payloadJson = JsonSerializer.Serialize(payloadObj);

            var request = new InvokeRequest
            {
                FunctionName = functionName,
                Payload = payloadJson
            };

            InvokeResponse response = await lambdaClient.InvokeAsync(request);

            using var reader = new StreamReader(response.Payload);
            string responseBody = await reader.ReadToEndAsync();

            Console.WriteLine("API response from Lambda = " + responseBody);
            return responseBody;
        }



        private string generateToken(string username, string password)
        {
            //HACK generate cool Token ;-) -> base64 is not encryption!
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + password + DateTime.Now.ToString()));
        }

        private bool checkUser(string username, string password)
        {
            try
            {
                //KOMMENTAR:
                Console.WriteLine("checkuser-begin");

                bool result = false;

                //Diese Environment Variable ist vermutlich nicht erreichbar, weil sie SIMS_WEB erstellt. In Docker compose local geht es, aber in einzelnen Containern eher nicht.
                //string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
                string connectionString = Environment.GetEnvironmentVariable("connectionstring");

                //KOMMENTAR:
                Console.WriteLine("connectionstring-value= " + connectionString);


                using (NpgsqlConnection db = new NpgsqlConnection(connectionString))
                {
                    db.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand($"select * from sims.simsuser where username = @username and pwdhash = @pwdhash", db))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        cmd.Parameters.AddWithValue("pwdhash", computeSha256Hash(password));
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            result = reader.HasRows;
                        }
                    }
                    db.Close();
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        private string computeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) { builder.Append(bytes[i].ToString("x2")); }
                return builder.ToString();
            }

        }
    }
}