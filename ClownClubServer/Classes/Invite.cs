namespace ClownClubServer.Classes {
    public class Invite {
        public string Id { get; set; }
        public ulong Inviter { get; set; }

        public Invite() { }

        public Invite(string code, ulong generatorId) {
            Id = code;
            Inviter = generatorId;
        }
    }
}
