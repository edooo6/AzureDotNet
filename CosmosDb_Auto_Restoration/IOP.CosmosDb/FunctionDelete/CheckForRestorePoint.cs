using IOP.CosmosDb.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IOP.CosmosDb.GenerateToken;

namespace IOP.CosmosDb.FunctionDelete
{
    class CheckForRestorePoint
    {
        public static string FetchSubscription()
        {
            List<object> validateListError = new List<object>();
            CosmosDbModel cosmosDbModel = new CosmosDbModel();
            try
            {
                ReadListOfCosmosDbAccount readListOfCosmosDbAccount = new ReadListOfCosmosDbAccount();
                var result = readListOfCosmosDbAccount.ListOfCosmos();
                if (result.GetType() == validateListError.GetType())
                {
                    List<object> listOfCosmos = (List<object>)readListOfCosmosDbAccount.ListOfCosmos();
                    var validateSubscription = "";
                    foreach (JObject dataList in listOfCosmos)
                    {
                        cosmosDbModel.SubscriptionId = (string)dataList["Subscription"];
                        if (validateSubscription.Contains(cosmosDbModel.SubscriptionId) != true)
                        {
                            validateSubscription = cosmosDbModel.SubscriptionId;
                            break;
                        }
                        else
                        {
                            return "Error";
                        }

                    }
                    return validateSubscription;
                }
                else
                {
                    return (string)result.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured reading list of Cosmos! | Message ==> " + ex.Message);
                return "Error occured reading list of Cosmos! | Message ==> " + ex.Message;
            }
        }
        public static async Task<object> ListOfRestorableAccountsAsync(string token, string subscriptionId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var fetchRestorableData = $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.DocumentDB/restorableDatabaseAccounts?api-version=2021-04-01-preview";
                var rawData = await client.GetStringAsync(fetchRestorableData);
                var listRestorableData = JsonConvert.DeserializeObject<object>(rawData);
                var restorableData = JObject.FromObject(listRestorableData).ToObject<Dictionary<string, object>>();
                var restorableAccount = restorableData["value"];
                JArray jArrayOfRestorableAccount = JArray.Parse(restorableAccount.ToString());
                return jArrayOfRestorableAccount;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured at CheckForRestorePoint Class and ListOfRestorableAccountsAsync method" + ex.Message);
                return "Error occured at CheckForRestorePoint Class and ListOfRestorableAccountsAsync method" + ex.Message;
            }
        }
        public async Task<object> RestorableTimeStampAsync()
        {
            string validateString = "";
            List<string> backUpAvailable = new List<string>();
            List<string> validatType = new List<string>();
            Token generateToken = new Token();
            var token = await generateToken.TokenAsync();
            CosmosDbModel cosmosDbModel = new CosmosDbModel();
            CheckForResource checkForResource = new CheckForResource();
            var validatedCosmosDb = await checkForResource.CheckResourceAsync();
            if (validatedCosmosDb.GetType() == validateString.GetType())
            {
                return validatedCosmosDb;
            }
            if (validatedCosmosDb.GetType() != validatType.GetType())
            {
                return validatedCosmosDb;
            }
            List<string> listOfAvailableResourceValidated = (List<string>)validatedCosmosDb;
            var subscriptionId = FetchSubscription();
            if(subscriptionId == "Error")
            {
                return subscriptionId;
            }
            JArray listOfAllRestorableAccount = (JArray)await ListOfRestorableAccountsAsync(token, subscriptionId);
            foreach (JObject listOfOnlineBackup in listOfAllRestorableAccount)
            {
                var jsonListOfOnlineBackup = listOfOnlineBackup.ToObject<Dictionary<string, object>>();
                var properties = jsonListOfOnlineBackup["properties"];
                var jsonDataProperty = JsonConvert.SerializeObject(properties);
                var jsonAccountName = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonDataProperty);
                var backUpName = jsonAccountName["accountName"];
                foreach (var cosmosDbName in listOfAvailableResourceValidated)
                {
                    if ((string)cosmosDbName == (string)backUpName)
                    {
                        if (backUpAvailable.Contains(cosmosDbName) != true)
                        {
                            backUpAvailable.Add(cosmosDbName);
                        }
                    }
                }
            }
            return backUpAvailable;
        }
    }
}

