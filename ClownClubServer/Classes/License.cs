using System;

namespace ClownClubServer.Classes {
    public enum LicenseTypes {
        Admin,
        ActivityGen,
        Supreme
    }
    public class License {
        public int Id { get; set; }
        public bool Redeemed { get; set; }
        public DateTime RedeemedTime { get; set; } // Start of license
        public TimeSpan ExpirationTime { get; set; }
        public LicenseTypes Type { get; set; }

        public License() {
            Redeemed = false;
            Type = LicenseTypes.Admin;
        }

        public License(LicenseTypes type, TimeSpan expir) {
            Redeemed = false;
            Type = type;
            ExpirationTime = expir;
        }
    }
}
