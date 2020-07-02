using System;
using System.Linq;

namespace ClownClubServer.Classes {
    public enum LicenseType {
        All,
        Admin,
        Supreme,
        Shopify
    }
    public class License {
        private static readonly Random random = new Random();
        public int Id { get; set; }
        public string Code { get; set; }
        public DateTime RedeemedTime { get; set; } // Start of license
        public TimeSpan ExpirationTime { get; set; }
        public LicenseType Type { get; set; }

        private static string RandomString(int length) {
            const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public License() {
            Code = RandomString(12);
            Type = LicenseType.Admin;
        }

        public License(LicenseType type, TimeSpan expir) {
            Code = RandomString(12);
            Type = type;
            ExpirationTime = expir;
        }

        public bool IsExpired() {
            return DateTime.Now - RedeemedTime > ExpirationTime;
        }
    }
}
