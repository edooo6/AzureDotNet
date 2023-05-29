using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IOP.CosmosDb.FunctionDelete
{
    public class ReadListOfCosmosDbAccount
    {
        public object ListOfCosmos()
        {
            try
            {
                var cosmosData = RetrieveFromAzureStorageAsync().Result;
                return cosmosData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured! ListOfCosmos Error! | Message ==> " + ex.Message);
                return "Error occured! ListOfCosmos Error! | Message ==> " + ex.Message;
            }
        }
        public static async System.Threading.Tasks.Task<object> RetrieveFromAzureStorageAsync()
        {
            try
            {
                List<object> resourceData = new List<object>();
                int i = 1;
                List<object> listOfCosmosData = new List<object>();
                string connectionString = Environment.GetEnvironmentVariable("TableStorageCS").ToString();
                CloudStorageAccount accountConnection = CloudStorageAccount.Parse(connectionString);
                CloudTableClient tableClient = accountConnection.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference("ListOfCosmosAccountForDeletionAndRestoration");
                while (true)
                {
                    TableOperation retrieveTable = TableOperation.Retrieve<TableDatas>(i.ToString(), i.ToString());
                    i++;
                    TableResult rawTableData = await table.ExecuteAsync(retrieveTable);
                    var serializedTableData = JsonConvert.SerializeObject(rawTableData);
                    var jsonTableData = JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedTableData);
                    var resourceCosmosData = jsonTableData["Result"];
                    if (resourceCosmosData == null)
                    {
                        break;
                    }
                    var cosmosDetail = JsonConvert.DeserializeObject<Dictionary<string, object>>(resourceCosmosData.ToString());
                    var subscriptionId = cosmosDetail["Subscription"];
                    var resourceGroup = cosmosDetail["ResourceGroup"];
                    var cosmosName = cosmosDetail["CosmosName"];
                    if (cosmosName == null && resourceGroup == null && subscriptionId == null)
                    {
                        return "CosmosName, ResourceGroupName and Subscription is empty on Azure Table Storage!";
                    }
                    if ((string)cosmosName == "" && (string)resourceGroup == "" && (string)subscriptionId == "")
                    {
                        return "CosmosName, ResourceGroupName and Subscription is empty on Azure Table Storage!";
                    }
                    if (cosmosName == null)
                    {
                        return "CosmosName for the ResourceGroup is Empty! Kindly check the Azure Table Storage!";
                    }
                    if ((string)cosmosName == "")
                    {
                        return "CosmosName for the ResourceGroup is Empty! Kindly check the Azure Table Storage!";
                    }
                    if (resourceGroup == null)
                    {
                        return "ResourceGroup is Empty! Kindly check the Azure Table Storage!";
                    }
                    if ((string)resourceGroup == "")
                    {
                        return "ResourceGroup is Empty! Kindly check the Azure Table Storage!";
                    }
                    if (subscriptionId == null)
                    {
                        return "SubscriptionId is Empty! Kindly check the Azure Table Storage!";
                    }
                    if ((string)subscriptionId == "")
                    {
                        return "SubscriptionId is Empty! Kindly check the Azure Table Storage!";
                    }
                    string rawCosmosNameSpace = cosmosName.ToString();
                    var rawCosmosName = rawCosmosNameSpace.Replace(" ","");
                    List<string> listOfCosmos = rawCosmosName.Split(',').ToList();
                    Dictionary<string, object> resourceDetails = new Dictionary<string, object>();
                    resourceDetails.Add("Subscription", subscriptionId);
                    resourceDetails.Add("ResourceGroup", resourceGroup);
                    resourceDetails.Add("CosmosDb", listOfCosmos);
                    listOfCosmosData.Add(resourceDetails);
                    var serializedData = JsonConvert.SerializeObject(listOfCosmosData);
                    resourceData = JsonConvert.DeserializeObject<List<object>>(serializedData);
                }
                if (resourceData.Count == 0)
                {
                    return "Data provided on Azure Table Storage is Empty!";
                }
                return resourceData;
            }
            catch (Exception ex)
            {
                return "Error occured at Azure Table Storage! Kindly check the RetrieveFromAzureStorageAsync Class and Azure Portal | Message ==> " + ex.Message;
            }
        }
    }
    class TableDatas : TableEntity
    {
        public string Subscription { set; get; }
        public string ResourceGroup { set; get; }
        public string CosmosName { set; get; }
    }
}


