using System.Runtime.Serialization;

namespace TweetStampv2.DTOs
{
    public class TimeStampData
    {
        [DataMember(Name = "currency_id", EmitDefaultValue = false)]
        public int? CurrencyId
        {
            get;
            set;
        }

        [DataMember(Name = "private_key", EmitDefaultValue = false)]
        public string PrivateKey
        {
            get;
            set;
        }

        [DataMember(Name = "seed_id", EmitDefaultValue = false)]
        public string SeedId
        {
            get;
            set;
        }

        [DataMember(Name = "submit_status", EmitDefaultValue = false)]
        public long? SubmitStatus
        {
            get;
            set;
        }

        [DataMember(Name = "timestamp", EmitDefaultValue = false)]
        public long? Timestamp
        {
            get;
            set;
        }

        [DataMember(Name = "transaction", EmitDefaultValue = false)]
        public string Transaction
        {
            get;
            set;
        }

        public TimeStampData(int? currencyId = null, string privateKey = null, string seedId = null, long? submitStatus = null, long? timestamp = null, string transaction = null)
        {
            CurrencyId = currencyId;
            PrivateKey = privateKey;
            SeedId = seedId;
            SubmitStatus = submitStatus;
            Timestamp = timestamp;
            Transaction = transaction;
        }
    }
}
