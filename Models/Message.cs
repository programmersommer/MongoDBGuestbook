using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Guestbook.Models
{

    public class Message
    {
        [BsonId]
        public ObjectId ID { get; set; }
        public string SenderName { get; set; }
        public string Email { get; set; }
        public string MessageText { get; set; }
        public DateTime MessageDate { get; set; }
    }

}

