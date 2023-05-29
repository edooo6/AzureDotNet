using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using IOP.CosmosDb.GenerateToken;
using IOP.CosmosDb.FunctionDelete;
using System.Linq;
using IOP.CosmosDb.Models;

namespace IOP.CosmosDb.FunctionDelete
{
    class CheckForResource
    {
        private readonly ILogger<CheckForResource> _logger;
        public async Task<object> CheckResourceAsync()
        {
            string validateString = "";
            IDictionary<string, List<string>> myDictionary = new Dictionary<string, List<string>>
                {
                    { "name", new List<string>()},
                    { "id", new List<string>()}
                };
            JArray jsonObject = new JArray();
            CosmosDbModel cosmosDbModel = new CosmosDbModel();
            Token generateToken = new Token();
            var token = await generateToken.TokenAsync();
            var listOfCosmosDbAccount = FetchCosmos();
            if(listOfCosmosDbAccount.GetType() == validateString.GetType())
            {
                return listOfCosmosDbAccount;
            }
            List<object> listOfCosmosAcc = (List<object>)listOfCosmosDbAccount;
            List<object> listOfComosAcrossResourceGroup = new List<object>();
            foreach (JObject dataList in listOfCosmosAcc)
            {
                cosmosDbModel.SubscriptionId = (string)dataList["Subscription"];
                cosmosDbModel.ResourceGroup = (string)dataList["ResourceGroup"];
                JArray listOfResource = (JArray)await ListAllResourceAsync(cosmosDbModel.SubscriptionId, cosmosDbModel.ResourceGroup, token);
                listOfComosAcrossResourceGroup.Add(listOfResource);
            }
            foreach (JArray jArrayData in listOfComosAcrossResourceGroup)
            {
                foreach (JObject dictData in jArrayData)
                {
                    string check = (string)dictData["type"];
                    if (check == "Microsoft.DocumentDb/databaseAccounts" || check == "Microsoft.DocumentDB/databaseAccounts")
                    {
                        var accountId = dictData["id"];
                        var accountName = dictData["name"];
                        myDictionary["name"].Add(accountName.ToString());
                        myDictionary["id"].Add(accountId.ToString());
                    }
                }
            }
            var listOfOnlineResourceName = myDictionary["name"];
            var listOfOnlineResourceId = myDictionary["id"];
            List<string> listOfAcceptedCosmosDb = new List<string>();
            foreach (JObject dataList in listOfCosmosAcc)
            {
                cosmosDbModel.SubscriptionId = (string)dataList["Subscription"];
                cosmosDbModel.ResourceGroup = (string)dataList["ResourceGroup"];
                var cosmosList = dataList["CosmosDb"];
                foreach (var lists in cosmosList)
                {
                    var datas = lists.ToObject<string>();
                    var resourceId = $"/subscriptions/{cosmosDbModel.SubscriptionId}/resourceGroups/{cosmosDbModel.ResourceGroup}/providers/Microsoft.DocumentDb/databaseAccounts/{lists}";
                    var resourceIds = $"/subscriptions/{cosmosDbModel.SubscriptionId}/resourceGroups/{cosmosDbModel.ResourceGroup}/providers/Microsoft.DocumentDB/databaseAccounts/{lists}";

                    foreach (var onLineList in listOfOnlineResourceName)
                    {
                        foreach (var onlineListOfId in listOfOnlineResourceId)
                        {
                            if (datas == onLineList && ((string)resourceId == (string)onlineListOfId || (string)resourceIds == (string)onlineListOfId))
                            {
                                if (listOfAcceptedCosmosDb.Contains(lists.ToString()) != true)
                                {
                                    listOfAcceptedCosmosDb.Add(lists.ToString());
                                }
                                else
                                {
                                    return "Error occured! Possibility : Given CosmosAccount not present in ResourceGroup / Duplicate data found in List!";

                                }
                            }
                        }
                    }
                }
            }
            if (listOfAcceptedCosmosDb.Count == 0)
            {
                Console.WriteLine("CosmosDb not Present in the Resource Group! Please Check!");
                return "CosmosDb not Present in the Resource Group! Please Check!";
            }
            else
            {
                return listOfAcceptedCosmosDb;
            }
        }
        public async Task<object> ListAllResourceAsync(string subscriptionId, string resourceGroup, string token)
        {
            CosmosDbModel cosmosDbModel = new CosmosDbModel();
            try
            {
                HttpClient client = new HttpClient();
                cosmosDbModel.SubscriptionId = subscriptionId;
                cosmosDbModel.ResourceGroup = resourceGroup;
                var listAllResourcesUrl = $"https://management.azure.com/subscriptions/{cosmosDbModel.SubscriptionId}/resources?$filter=substringof('{cosmosDbModel.ResourceGroup}',resourceGroup)&api-version=2021-04-01";
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var rawListOfResources = await client.GetStringAsync(listAllResourcesUrl);
                var listOfAllResources = JsonConvert.DeserializeObject<object>(rawListOfResources);
                var resourceDictionary = JObject.FromObject(listOfAllResources).ToObject<Dictionary<string, object>>();
                var resourceData = resourceDictionary["value"];
                return resourceData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured in Class - CheckForResource & Method - ListAllResourceAsync. | Message ==> " + ex.Message);
                return "Error occured in Class - CheckForResource & Method - ListAllResourceAsync. | Possibility ==> Check for Cosmos Data provided. | Message ==> " + ex.Message;
            }
        }
        public static object FetchCosmos()
        {
            List<object> validateListError = new List<object>();
            try
            {
                ReadListOfCosmosDbAccount readListOfCosmosDbAccount = new ReadListOfCosmosDbAccount();
                var result = readListOfCosmosDbAccount.ListOfCosmos();
                if (result.GetType() == validateListError.GetType())
                {
                    List<object> listOfCosmos = (List<object>)readListOfCosmosDbAccount.ListOfCosmos();
                    return listOfCosmos;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured reading list of Cosmos! | Message ==> " + ex.Message);
                return "Error occured reading list of Cosmos! | Message ==> " + ex.Message;
            }
        }
    }
}
