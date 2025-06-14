﻿using RestSharp;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SIMS
{
    public static class Helper
    {
        //api_url
        static string APIURL = Environment.GetEnvironmentVariable("api_url");

        //?? "http://localhost:8888";

        public static string URL_getToken = $"{APIURL}/AuthService?username=%1&password=%2";
        public static string URL_checkToken = $"{APIURL}/AuthService/check?username=%1&token=%2";
        public static string URL_stopInstance = $"{APIURL}/AuthService/STOP?resourceID=%1";

        public static string getToken(string username, string password)
        {
            RestClient client = new RestClient(Helper.URL_getToken.Replace("%1", username).Replace("%2", password));
            RestRequest request = new RestRequest("", Method.Post);
            RestResponse response = client.Execute(request);
            string token = response.Content ?? "";
            token = token.Replace("\"", "");
            return token;
        }

        public static bool checkToken(string username, string token)
        {
            RestClient client = new RestClient(Helper.URL_checkToken.Replace("%1", username).Replace("%2", token));
            RestRequest request = new RestRequest("", Method.Get);
            RestResponse response = client.Execute(request);
            return response.StatusCode == System.Net.HttpStatusCode.OK ? true : false;
        }

        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) { builder.Append(bytes[i].ToString("x2")); }
                return builder.ToString();
            }
        }

        public static string stopInstance(string resourceID)
        {

                //KOMMENTAR:
                System.Console.WriteLine("HELPER stopInstance start.");

                RestClient client = new RestClient(Helper.URL_stopInstance.Replace("%1", resourceID));
                RestRequest request = new RestRequest("", Method.Post);
                RestResponse response = client.Execute(request);
                System.Console.WriteLine("RAW JSON RESPONSE= " + response.Content);
            try
            {
                string contentbody = JsonSerializer.Deserialize<string>(response.Content);
                using JsonDocument doc = JsonDocument.Parse(contentbody);
                JsonElement root = doc.RootElement;
                string body = root.GetProperty("body").GetString();

                using JsonDocument innerDoc = JsonDocument.Parse(body);
                JsonElement innerRoot = innerDoc.RootElement;
                string message = innerRoot.GetProperty("message").GetString();

                Console.WriteLine("stopInstance: stopMessage = " + message);

                return message;
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Fehler beim Parsen der Antwort als JSON:" + ex.Message);
                return "Fehler: Antwort ist kein gültiges JSON siehe exception Message.";
            }
        }
    }

}
