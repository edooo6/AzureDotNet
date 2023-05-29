using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IOP.CosmosDb.GenerateToken;
using IOP.CosmosDb.Models;

namespace IOP.CosmosDb.FunctionDelete
{
    public class DeleteCosmosDbAccount
    {
        public static object FetchAllCosmosAccount()
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
                    return (string)result.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured reading list of Cosmos! | Message ==> " + ex.Message);
                return "Error occured reading list of Cosmos! | Message ==> " + ex.Message;
            }
        }
        public async Task<object> DeleteCosmosAccountAsync()
        {
            string validateString = "";
            string validateStrings = "";
            try
            {
                List<string> finallyDelete = new List<string>();
                List<string> validateList = new List<string>();
                Token generateToken = new Token();
                var token = await generateToken.TokenAsync();
                CosmosDbModel cosmosDbModel = new CosmosDbModel();
                var validateListOfGivenCosmos = FetchAllCosmosAccount();
                if(validateListOfGivenCosmos.GetType() == validateString.GetType() )
                {
                    return validateListOfGivenCosmos;
                }
                List<object> listOfGivenCosmos = (List<object>)validateListOfGivenCosmos;
                var listOfGivenCosmosDb = listOfGivenCosmos[0];
                var jsonData = JObject.FromObject(listOfGivenCosmosDb).ToObject<Dictionary<string, object>>();
                JArray listOfCosmosData = (JArray)jsonData["CosmosDb"];
                CheckForRestorePoint checkForRestorePoint = new CheckForRestorePoint();
                var listOfDatas = await checkForRestorePoint.RestorableTimeStampAsync();
                if (listOfDatas == "Error")
                {
                    return "Error occured at CheckRestorePoint Class and FetchSubscriptionAsync Method. Possibility: Multiple Subscription Found!";
                }
                List<string> listOfCosmosDb = (List<string>)listOfDatas;
                cosmosDbModel.SubscriptionId = (string)jsonData["Subscription"];
                cosmosDbModel.ResourceGroup = (string)jsonData["ResourceGroup"];
                foreach (var givenList in listOfCosmosData)
                {
                    foreach (var onlineList in listOfCosmosDb)
                    {
                        if ((string)givenList == (string)onlineList)
                        {
                            if (finallyDelete.Contains((string)givenList) != true)
                            {
                                finallyDelete.Add(givenList.ToString());
                            }
                        }
                    }
                }
                if (finallyDelete.Count == 0)
                {
                    return "Backup not available for given Account. Kindly enable the PointInTimeRestore feature";
                }
                List<string> successfullyDelete = new List<string>();
                List<string> unableToDelete = new List<string>();
                foreach (var cosmosDb in finallyDelete)
                {
                    cosmosDbModel.CosmosDbName = cosmosDb;
                    var deleteUrl = $"https://management.azure.com/subscriptions/{cosmosDbModel.SubscriptionId}/resourceGroups/{cosmosDbModel.ResourceGroup}/providers/Microsoft.DocumentDB/databaseAccounts/{cosmosDbModel.CosmosDbName}?api-version=2015-04-08";
                    string response = InvokeDelete(token, deleteUrl);
                    if (response == "200")
                    {
                        successfullyDelete.Add(cosmosDb);
                        Console.WriteLine($"Deleted {cosmosDb}!");
                    }
                    else
                    {
                        unableToDelete.Add(cosmosDb);
                        Console.WriteLine($"Not Deleted {cosmosDb}!");
                    }
                }
                return "Deleted the possible Cosmos Account!";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured at DeleteCosmosDbAccount Class and DeleteCosmosAccountAsync Method! | Message ==> " + ex.Message);
                return "Error occured at DeleteCosmosDbAccount Class and DeleteCosmosAccountAsync Method! | Message ==> " + ex.Message;
            }
        }
        public static string InvokeDelete(string token1, string url)
        {
            try
            {
                string token2 = token1;
                Uri uri = new Uri(url);

                var response = DeleteResource(uri, token2);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured at Invoke Delete Method! | Message ==> " + ex.Message);
                return "Error occured at Invoke Delete Method! | Message ==> " + ex.Message;
            }
        }
        public static string DeleteResource(Uri uri, string token3)
        {
            try
            {
                string tokenpass = token3;
                var response = string.Empty;
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenpass);
                    HttpResponseMessage result = client.DeleteAsync(uri).Result;
                    response = result.StatusCode.ToString();
                }
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured at DeleteResource Method. | Message ==> " + ex.Message);
                return "Error occured at DeleteResource Method. | Message ==> " + ex.Message;
            }
        }
    }
}