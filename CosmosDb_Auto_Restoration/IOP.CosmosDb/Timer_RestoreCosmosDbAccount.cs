using System;
using IOP.CosmosDb.FunctionRestore;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace IOP.CosmosDb
{
    public class Timer_RestoreCosmosDbAccount
    {
        [FunctionName("Timer_RestoreCosmosDbAccount")]
        public async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)

        //public async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 15 03 * * 1-5")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Restoring CosmosDb Account Initiated! | Time: {DateTime.Now}");
            RestoreCosmosDb restoreCosmosDb = new RestoreCosmosDb();
            var restoreResult = await restoreCosmosDb.RestoreCosmosAccountAsync();
            log.LogInformation(restoreResult.ToString());
            log.LogInformation($"Restoring CosmosDb Account Completed! | Time: {DateTime.Now}");
        }
    }
}

//Original