using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IOP.CosmosDb.Models;
using IOP.CosmosDb.GenerateToken;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using System.IO;

namespace IOP.CosmosDb.FunctionRestore
{
    class RestoreCosmosDb
    {
        public async Task<object> RestoreCosmosAccountAsync()
        {
            string validateString = "";
            object restoreAccountData;
            //Retrieve parameters from FetchRestorableTimeStamp class for restoration.
            FetchRestorableTimeStamp fetchRestorableTimeStamp = new FetchRestorableTimeStamp();
            try
            {
                restoreAccountData = await fetchRestorableTimeStamp.RestoreParameterAsync();
                if(restoreAccountData.GetType() == validateString.GetType() )
                {
                    return restoreAccountData.ToString();
                }
            }
            catch (Exception ex)
            {
                return  ex.Message;
            }
            //CosmosDbModel cosmosDbModel = new CosmosDbModel();
            Token generateToken = new Token();
            var token = await generateToken.TokenAsync();
            var validation = ValidateCosmosList(restoreAccountData, token);
            return validation;
        }
        public object ValidateCosmosList(object restoreParameter, string token)
        {
            List<object> availableData = (List<object>)restoreParameter;
            CosmosDbModel cosmosDbModel = new CosmosDbModel();
            try
            {
                List<object> validateListError = new List<object>();
                ReadListOfCosmosDb readListOfCosmos = new ReadListOfCosmosDb();
                var result = readListOfCosmos.ListOfCosmos();
                if (result.GetType() == validateListError.GetType())
                {
                    List<object> listOfCosmos = (List<object>)readListOfCosmos.ListOfCosmos();
                    foreach (var jsonData in listOfCosmos)
                    {
                        var json = JsonConvert.SerializeObject(jsonData);
                        var jsonCosmosDbData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                        var rawListOfCosmos = jsonCosmosDbData["CosmosDb"];
                        JArray myListOfCosmos = JArray.Parse(rawListOfCosmos.ToString());
                        cosmosDbModel.SubscriptionId = (string)jsonCosmosDbData["Subscription"];
                        cosmosDbModel.ResourceGroup = (string)jsonCosmosDbData["ResourceGroup"];
                        object finalResult = "";
                        foreach (var listOfCosmosDbName in myListOfCosmos)
                        {
                            string rawListOfCosmosDb = listOfCosmosDbName.ToString();
                            string listOfCosmosDb = rawListOfCosmosDb.Replace(" ", "");
                            foreach (var dictOfCosmos in availableData)
                            {
                                var values = JObject.FromObject(dictOfCosmos).ToObject<Dictionary<string, string>>();
                                var cosmosName = values["Name"].ToString();
                                if ((string)cosmosName == (string)listOfCosmosDb)
                                {
                                    cosmosDbModel.ApplyRestoreName = cosmosName;
                                    var restoreUrl = $"https://management.azure.com/subscriptions/{cosmosDbModel.SubscriptionId}/resourceGroups/{cosmosDbModel.ResourceGroup}/providers/Microsoft.DocumentDB/databaseAccounts/{cosmosDbModel.ApplyRestoreName}/?api-version=2022-05-15-preview";
                                    cosmosDbModel.cosmosName = values["Name"];
                                    var rawTime = values["Time"];
                                    cosmosDbModel.cosmosTimeStamp = rawTime;
                                    cosmosDbModel.cosmosLocation = values["Location"];
                                    cosmosDbModel.cosmosId = values["Id"];
                                    result = (string)RestoreCosmos(restoreUrl, token, cosmosDbModel.cosmosName, cosmosDbModel.cosmosLocation, cosmosDbModel.cosmosTimeStamp, cosmosDbModel.cosmosId);
                                }
                            }
                        }
                    }
                    return result;
                }
                else
                {
                    Exception ex = new Exception();
                    Console.WriteLine("Check Json File! or Check ReadListOfCosmos File!");
                    return "Check Json File! or Check ReadListOfCosmos File!";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured reading list of Cosmos! | Message ==> " + ex.Message);
                return "Error occured reading list of Cosmos! | Message ==> " + ex.Message;
            }
        }
        public object RestoreCosmos(string restoreUrl, string tokenid, string cosmosName, string cosmosLocation, string cosmosTime, string cosmosId)
        {
            var removeSpaceLocation = cosmosLocation.Replace(" ", "");
            var lowerCaseLocation = removeSpaceLocation.ToLower();
            Dictionary<string, object> restoreParameter = new Dictionary<string, object>();
            restoreParameter.Add("restoreMode", "PointInTime");
            restoreParameter.Add("restoreSource", cosmosId);
            restoreParameter.Add("restoreTimestampInUtc", cosmosTime);
            Dictionary<string, object> locationParameter = new Dictionary<string, object>();
            locationParameter.Add("locationName", cosmosLocation);
            locationParameter.Add("failoverPriority", 0);
            List<object> locationData = new List<object>();
            locationData.Add(locationParameter);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("locations", locationData);
            properties.Add("databaseAccountOfferType", "Standard");
            properties.Add("createMode", "Restore");
            properties.Add("restoreParameters", restoreParameter);
            Dictionary<string, object> requestBody = new Dictionary<string, object>();
            requestBody.Add("location", lowerCaseLocation);
            requestBody.Add("properties", properties);
            var url = restoreUrl;
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "PUT";
                httpRequest.Headers.Add("Authorization", $"Bearer {tokenid}");
                httpRequest.ContentType = "application/json";
                string data1 = "{\"location\":" + "\"" + lowerCaseLocation + "\"" + ",\"properties\":{\"locations\":[{\"locationName\":" + "\"" + cosmosLocation + "\"" + ",\"failoverPriority\":0}],\"databaseAccountOfferType\":\"Standard\",\"createMode\": \"Restore\",\"restoreParameters\":{\"restoreMode\":\"PointInTime\",\"restoreSource\":" + "\"" + cosmosId + "\"" + ",\"restoreTimestampInUtc\":" + "\"" + cosmosTime + "\"" + "}}}";
                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data1);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
                Console.WriteLine($"Restored {cosmosName} \n");
                return "Successfully restored the CosmosDbList! :)";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error possibility: CosmosDb Account {cosmosName} already restored! / Error in Validating the CosmosDb Account in ValidateCosmosList Class. | Message ==> " + ex.Message);
                return "Error possibility: CosmosDb Account already restored! / Error in Validating the CosmosDb Account in ValidateCosmosList Class. | Message ==> " + ex.Message;
            }
        }
    }
}
