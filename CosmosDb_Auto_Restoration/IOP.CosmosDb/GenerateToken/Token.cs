using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using IOP.CosmosDb.Models;

namespace IOP.CosmosDb.GenerateToken
{
    /// <summary>
    /// Generate a Service Principal Token.
    /// </summary>
    class Token 
    {
        //private readonly ILogger<GenerateToken> _logger;

        public async Task<string> TokenAsync()
        {
            Dictionary<string, string> tokenRequest = new Dictionary<string, string>();
            CosmosDbModel cosmosDbModel = new CosmosDbModel();

            try
            {
                cosmosDbModel.Tenant = Environment.GetEnvironmentVariable("TenantId");
                cosmosDbModel.SubscriptionId = Environment.GetEnvironmentVariable("Subscription");
                var clientId = Environment.GetEnvironmentVariable("ClientId").ToString();
                var clientSecret = Environment.GetEnvironmentVariable("SecretValue").ToString();
                cosmosDbModel.TokenUrl = $"https://login.microsoftonline.com/{cosmosDbModel.Tenant}/oauth2/v2.0/token";
                tokenRequest.Add("client_id", clientId );
                tokenRequest.Add("client_secret", clientSecret);
                tokenRequest.Add("scope", "https://management.azure.com/.default");
                tokenRequest.Add("grant_type", "client_credentials");
                var tokenResult = await GenerateTokenAsync(cosmosDbModel.TokenUrl, tokenRequest);
                return tokenResult;
            }   
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured at Generate Token Class. | {ex.Message}");
                return "Error occured at Generate Token. | Error Message ==> " + ex.Message;
            }

           

        }

        async Task<string> GenerateTokenAsync(string tokenUrl, Dictionary<string, string> tokenRequestBody)
        {
            try
            {
                HttpClient client = new HttpClient();
                var tokenPostMethod = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(tokenRequestBody));
                var jsonTokenData = await tokenPostMethod.Content.ReadAsStringAsync();
                //var dictionaryTokenData = JsonConvert.DeserializeObject<object>(jsonTokenData);
                JObject jsonObjTokenData = JObject.Parse(jsonTokenData);
                Dictionary<string, string> jsonDataToken = jsonObjTokenData.ToObject<Dictionary<string, string>>();
                var token = jsonDataToken["access_token"];
                var statusCodeToken = (int)tokenPostMethod.StatusCode;
                if (statusCodeToken == 200)
                {
                    return token;
                }
                else
                {
                    Console.WriteLine("Status Code ==> " + statusCodeToken);
                    return "Error! Status Code ==> " + statusCodeToken.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured at Generate Token Class HttpMethod. | {ex.Message}");
                return "Error Occured at Generate Token HttpMethod. | Error Message ==> " + ex.Message;
            }
        }

    }
}
