using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IOP.CosmosDb.GenerateToken;
using IOP.CosmosDb.Models;

namespace IOP.CosmosDb.FunctionRestore
{
    /// <summary>
    /// Return Available Restorable Account with Latest TimeStamp
    /// </summary>
    public class FetchRestorableTimeStamp
    {
        /// <summary>
        /// Main Function to Filter all the required paramter for Restoration
        /// </summary>
        public async Task<object> RestoreParameterAsync()
        {
            string validateString = "";
            Dictionary<string, object> validateData = new Dictionary<string, object>();
            List<object> restorableAccountParameter = new List<object>();
            List<Dictionary<string, object>> listParam = new List<Dictionary<string, object>>();
            List<List<string>> allParameter = new List<List<string>>();
            List<Dictionary<string, string>> listData = new List<Dictionary<string, string>>();
            List<Dictionary<string, object>> finalParameter = new List<Dictionary<string, object>>();
            List<object> maxDateTime = new List<object>();
            Dictionary<string, List<object>> timeAndId = new Dictionary<string, List<object>>();
            //Generate a Token
            Token generateToken = new Token();
            var token = await generateToken.TokenAsync();
            CosmosDbModel cosmosDbModel = new CosmosDbModel();
            //Fetch all available Account for Restoration
            var listOfCosmosDbAccount = FetchCosmos();
            if(listOfCosmosDbAccount.GetType() == validateString.GetType() )
            {
                return listOfCosmosDbAccount.ToString();
            }
            List<object> listOfCosmosAcc = (List<object>)listOfCosmosDbAccount;
            foreach (var list in listOfCosmosAcc)
            {
                JObject listCosmos = (JObject)list;
                var givenList = listCosmos.ToObject<Dictionary<string, object>>();
                var rawListOfCosmos = givenList["CosmosDb"];
                JArray myListOfCosmos = JArray.Parse(rawListOfCosmos.ToString()); //Contains List of All given Cosmos Account
                cosmosDbModel.SubscriptionId = (string)givenList["Subscription"];
                var  rawGetRestorableTimeStamp = await RestorableDatabaseAccountAsync(token, cosmosDbModel.SubscriptionId);
                if(rawGetRestorableTimeStamp.GetType() == validateString.GetType() )
                {
                    return rawGetRestorableTimeStamp;
                }
                Dictionary<string, object> getRestorableTimeStamp = (Dictionary<string, object>)rawGetRestorableTimeStamp;
                if (getRestorableTimeStamp.GetType() != validateData.GetType()) //edwin//
                {
                    Console.WriteLine("Possibility ==> SubscriptionId in give list is Invalid / Issue with Generating Token / SubscriptionId doesn't exist!");
                    return "Possibility ==> SubscriptionId in give list is Invalid / Issue with Generating Token / SubscriptionId doesn't exist! ";
                }
                var availableAccountForRestore = getRestorableTimeStamp["value"];
                JArray jsonDataTimeStamp = JArray.Parse(availableAccountForRestore.ToString()); // Contains List of Dictionary for available Account
                var checkDuplicate = myListOfCosmos.GroupBy(n => n).Any(c => c.Count() > 1);
                if (checkDuplicate == true)
                {
                    return "Kindly Check the List of Cosmos Account. It has duplicate values!";
                }
                foreach (JObject obj in jsonDataTimeStamp)
                {
                    var json = JsonConvert.SerializeObject(obj);
                    var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    ///Filtering the data for retrieving parameters
                    var properties = jsonData["properties"];
                    var jsonDataProperty = JsonConvert.SerializeObject(properties);
                    var restoreParameter = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonDataProperty);
                    var cosmosName = restoreParameter["accountName"];
                    var jsonProperties = JsonConvert.SerializeObject(properties);
                    Dictionary<string, object> otherData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonDataProperty);
                    JArray rawData = (JArray)otherData["restorableLocations"];
                    var otherParameters = rawData[0];
                    var name = restoreParameter["accountName"];
                    var location = otherParameters["locationName"];
                    DateTime timestamp;
                    try
                    {
                        timestamp = (DateTime)restoreParameter["deletionTime"];
                    }
                    catch
                    {
                        //return $"CosmosDb {name} has been restored with latest timestamp!.";
                        continue;
                    }
                    var id = jsonData["id"];
                    foreach (var cosmosDb in myListOfCosmos)
                    {
                        string cosmos1 = cosmosDb.ToString();
                        string cosmos = cosmos1.Replace(" ", "");
                        string onlineCosmos1 = cosmosName.ToString();
                        string onlineCosmos = onlineCosmos1.Replace(" ", "");
                        
                        if (cosmos == onlineCosmos)
                        {
                            Dictionary<string, object> param = new Dictionary<string, object>
                            {
                                {"Name",(string)name },
                                {"Time",timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                                {"Location",(string) location },
                                {"Id", (string) id }
                            };
                            if (listParam.Contains(param) != true)
                            {
                                listParam.Add(param);

                            }
                        }
                    }
                }
                foreach (var updatedList in listParam)
                {
                    var cosmosName = updatedList["Name"];
                    foreach (var checkUpdatedList in listParam)
                    {
                        var cosmosName1 = checkUpdatedList["Name"];
                        if ((string)cosmosName1 == (string)cosmosName)
                        {
                            maxDateTime.Add(checkUpdatedList["Time"]);
                        }
                    }
                    foreach (var list2 in listParam)
                    {
                        var cosmosName2 = list2["Name"];
                        var cosmosTime2 = list2["Time"];
                        if ((string)cosmosName2 == (string)cosmosName && (string)cosmosTime2 == maxDateTime.Max())
                        {
                            maxDateTime.Clear();
                            bool containValue = finalParameter.Any(x => x.ContainsValue(cosmosName2));
                            if (containValue == false)
                            {
                                finalParameter.Add(list2);
                            }
                        }
                    }
                }
                foreach (var listInFinalParameter in finalParameter)
                {
                    if (restorableAccountParameter.Contains(listInFinalParameter) != true)
                    {
                        restorableAccountParameter.Add(listInFinalParameter);
                    }

                }
            }
            if(restorableAccountParameter == null)
            {
                return "The Restorable Cosmos Db List is Empty!";
            }
            var checkDuplicates = restorableAccountParameter.GroupBy(n => n).Any(c => c.Count() > 1);
            if (checkDuplicates == true)
            {
                return "Kindly Check the list of Data provided, it may have duplicate data!";
            }
            return restorableAccountParameter;
        }
        /// <summary>
        /// Read List of CosmosAccount from given Json File
        /// </summary>
        public static object FetchCosmos()
        {
            List<object> validateListError = new List<object>();
            try
            {
                ReadListOfCosmosDb readListOfCosmos = new ReadListOfCosmosDb();
                var result = readListOfCosmos.ListOfCosmos();
                if (result.GetType() == validateListError.GetType())
                {
                    List<object> listOfCosmos = (List<object>)readListOfCosmos.ListOfCosmos();

                    return listOfCosmos;
                }
                else
                {
                    Exception ex = new Exception();
                    Console.WriteLine("Check Table Storage or Check ReadListOfCosmos Class");
                    return readListOfCosmos.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured reading list of Cosmos! | Message ==> " + ex.Message);
                return "Error occured reading list of Cosmos! | Message ==> " + ex.Message;
            }
        }
        /// <summary>
        /// Fetch all Account available for Restoration
        /// </summary>
        public static async Task<object> RestorableDatabaseAccountAsync(string token, string subscriptionId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var listOfRestorableAccount = $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.DocumentDB/restorableDatabaseAccounts?api-version=2021-04-01-preview";
                HttpResponseMessage response = await client.GetAsync(listOfRestorableAccount);
                if ((int)response.StatusCode == 200)
                {
                    var rawTimeStamp = await client.GetStringAsync(listOfRestorableAccount);
                    var listOfTimeStamps = JsonConvert.DeserializeObject<object>(rawTimeStamp);
                    var restorableData = JObject.FromObject(listOfTimeStamps).ToObject<Dictionary<string, object>>();
                    return restorableData;
                }
                else
                {
                    Console.WriteLine("Error occured at RestorableDatabaseAccountAsync HttpClient Method | Code ==>" + (int)response.StatusCode);
                    return "Error occured at RestorableDatabaseAccountAsync HttpClient Method | Code ==>" + (int)response.StatusCode;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RestorableDatabaseAccountAsync Method! | Message ==>  " + ex.Message);
                return "Error in RestorableDatabaseAccountAsync Method! | Message ==>  " + ex.Message;

            }
        }
    }
}
