using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TweetStampv2.DTOs
{
    public class TimeStampResponseDTO
    {
        [DataMember(Name = "comment", EmitDefaultValue = false)]
        public string Comment
        {
            get;
            set;
        }

        [DataMember(Name = "created", EmitDefaultValue = false)]
        public bool? Created
        {
            get;
            set;
        }

        [DataMember(Name = "date_created", EmitDefaultValue = false)]
        public long? DateCreated
        {
            get;
            set;
        }

        [DataMember(Name = "hash_string", EmitDefaultValue = false)]
        public string HashString
        {
            get;
            set;
        }

        [DataMember(Name = "timestamps", EmitDefaultValue = false)]
        public List<TimeStampData> Timestamps
        {
            get;
            set;
        }

        public TimeStampResponseDTO(string comment = null, bool? created = null, long? dateCreated = null, string hashString = null, List<TimeStampData> timestamps = null)
        {
            Comment = comment;
            Created = created;
            DateCreated = dateCreated;
            HashString = hashString;
            Timestamps = timestamps;
        }
    }
}
