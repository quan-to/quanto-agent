using SQLite;

namespace QuantoAgent.Database {
    public class DBUser {
        [PrimaryKey, MaxLength(50)]
        public string UserId { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(50), Unique]
        public string UserName { get; set; }

        [MaxLength(255)]
        public string Password { get; set; }
    }
}
