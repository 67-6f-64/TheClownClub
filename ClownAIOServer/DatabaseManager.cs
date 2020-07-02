using ClownClubServer.Classes;
using LiteDB;

namespace ClownClubServer {
    class DatabaseManager {
        public static LiteDatabase Database { get; private set; }
        public static LiteCollection<User> Users { get; private set; }
        public static LiteCollection<Invite> Invites { get; private set; }
        public static LiteCollection<License> Licenses { get; private set; }

        public static void Init() {
            Database = new LiteDatabase("clown.db");
            Users = Database.GetCollection<User>("users");
            Invites = Database.GetCollection<Invite>("invites");
            Licenses = Database.GetCollection<License>("licenses");
        }
    }
}
