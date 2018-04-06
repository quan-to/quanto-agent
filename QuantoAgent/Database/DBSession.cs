using System;
using SQLite;

namespace QuantoAgent.Database {
    public class DBSession {
        [PrimaryKey, MaxLength(50)]
        public string SessionId { get; set; }
        [MaxLength(255)]
        public string UserId { get; set; }
    }
}
