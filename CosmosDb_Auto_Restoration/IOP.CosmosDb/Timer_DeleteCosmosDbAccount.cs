using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using IOP.CosmosDb.FunctionDelete;
using System.Threading.Tasks;

namespace IOP.CosmosDb
{
    public class Timer_DeleteCosmosDbAccount
    {
        [FunctionName("Timer_DeleteCosmosDbAccount")]
        public async Task RunAsync([TimerTrigger("0 */5 * * * *",RunOnStartup = true)] TimerInfo myTimer, ILogger log)

        //public async Task RunAsync([TimerTrigger("0 30 15 * * 1-5")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Deleting CosmosDb Account Initiated! | Time: {DateTime.Now}");
            DeleteCosmosDbAccount deleteCosmosDb = new DeleteCosmosDbAccount();
            var deletionResult = await deleteCosmosDb.DeleteCosmosAccountAsync();
            log.LogInformation(deletionResult.ToString());
            log.LogInformation($"Deleting CosmosDb Account Completed! | Time: {DateTime.Now}");
        }
    }
}
