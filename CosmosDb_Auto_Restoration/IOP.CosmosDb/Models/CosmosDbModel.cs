using System;
using System.Collections.Generic;
using System.Text;

namespace IOP.CosmosDb.Models
{
    class CosmosDbModel
    {
        public string SubscriptionId { set; get; }
        public string ResourceGroup { set; get; }
        public string Tenant { set; get; }
        public string CosmosDbName { set; get; }
        public string TokenUrl { set; get; }

        public string ApplyRestoreName { set; get; }


        public string cosmosName { set; get; }
        public string cosmosLocation { set; get; }
        public string cosmosTimeStamp { set; get; }
        public string cosmosId { set; get; }

    }
}
