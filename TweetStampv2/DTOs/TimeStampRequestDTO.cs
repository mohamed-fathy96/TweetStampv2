using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TweetStampv2.DTOs
{
    public class TimeStampRequestDTO
    {
        [DataMember(Name = "comment", EmitDefaultValue = false)]
        public string Comment
        {
            get;
            set;
        }

        [DataMember(Name = "hash", EmitDefaultValue = false)]
        public string Hash
        {
            get;
            set;
        }

        [DataMember(Name = "notifications", EmitDefaultValue = false)]
        public List<Notification> Notifications
        {
            get;
            set;
        }

        [JsonConstructor]
        protected TimeStampRequestDTO()
        {
        }

        public TimeStampRequestDTO(string comment = null, string hash = null, List<Notification> notifications = null)
        {
            if (hash == null)
            {
                throw new Exception("hash is a required property for TimestampRequest and cannot be null");
            }

            Hash = hash;
            Comment = comment;
            Notifications = notifications;
        }
    }
}
