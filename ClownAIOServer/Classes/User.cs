using System;
using System.Collections.Generic;

namespace ClownClubServer.Classes {
    public class User {
        public ulong Id { get; set; } // Discord ID
        public string AuthKey { get; set; }
        public Invite InviteCode { get; set; }
        public List<License> Licenses { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime LastInviteDate { get; set; }
        public string Hwid { get; set; }

        public User() { }

        public User(ulong id, string key, Invite invite) {
            Id = id;
            AuthKey = key;
            Licenses = new List<License>();
            RegistrationDate = DateTime.Now;
            LastInviteDate = DateTime.Now;
            InviteCode = invite;
        }
    }
}
