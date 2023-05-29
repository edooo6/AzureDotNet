using System;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Newtonsoft.Json;
using Opc.UaFx.Client;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;
using System;
using Microsoft.Azure.Devices.Provisioning.Service;
using Opc.UaFx;

namespace OPCUADataFetcher
{
    public static class Program
    {
        public static async Task Main()
        {
            try{
            const string deviceConnectionString = "";
            var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);

            const string endpointUrl = "";
            var client = new OpcClient(endpointUrl);
            client.Connect();
            while(true)
            {
                Dictionary<string,object> data = OPCUA(client);
                var messageString = JsonConvert.SerializeObject(data);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                await deviceClient.SendEventAsync(message);
                System.Console.WriteLine("Sent");
                Thread.Sleep(1000);
            }
            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        public static Dictionary<string,object> OPCUA(OpcClient opc)
        {
            Dictionary<string, object> finalData = new();
            List<string> nodeList = new();
            List<string> displayName = new();
            string text = File.ReadAllText(@"./opcnode.json");
            var nodeData = JsonConvert.DeserializeObject<Dictionary<string,string>>(text);
            foreach(var i in nodeData)
            {
                nodeList.Add(i.Value);
                displayName.Add(i.Key);
            }
            OpcReadNode[] opcNodeReader = new OpcReadNode[nodeList.Count];
            for(int i = 0; i < nodeList.Count; i++)
            {
                opcNodeReader[i] = new OpcReadNode(nodeList[i]);
            }
                IEnumerable<OpcValue> job = opc.ReadNodes(opcNodeReader);
            int j = 0;
            foreach(var i in job)
            {
                finalData.Add(opcNodeReader[j].NodeId.ToString(), i);
                j++;
            }
            return finalData;
        }
    }
}