using NotLiteCode.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ClownClubServer.Classes;
using Newtonsoft.Json;

namespace ClownClubServer.NLC {
    class SharedClass : IDisposable {
        private string AuthKey { get; set; }

        [NLCCall("Auth")]
        public bool Auth(string authKey, string hwid) {
            var user = DatabaseManager.Users.FindOne(potentialUser => potentialUser.AuthKey.Equals(authKey));

            if (user is null) return false;

            //if (string.IsNullOrEmpty(user.Hwid)) {
            //    user.Hwid = hwid;
            //    DatabaseManager.Users.Update(user);
            //}
            //else if (!user.Hwid.Equals(hwid)) {
            //    return false;
            //}

            AuthKey = authKey;
            return true;
        }

        private static bool LicenseValidFor(List<License> licenses, LicenseType licenseType) {
            return licenses.Exists(x => !x.IsExpired() && (x.Type.Equals(LicenseType.All) || x.Type.Equals(LicenseType.Admin) || x.Type.Equals(licenseType)));
        }

        [NLCCall("GetCommon")]
        public string GetCommon() {
            if (string.IsNullOrEmpty(AuthKey)) return null;

            var user = DatabaseManager.Users.FindOne(potentialUser => potentialUser.AuthKey.Equals(AuthKey));

            var assemblyQueue = new Queue<byte[]>();
            assemblyQueue.Enqueue(File.ReadAllBytes("Common.dll"));
            if (LicenseValidFor(user.Licenses, LicenseType.Supreme))
                assemblyQueue.Enqueue(File.ReadAllBytes("Supreme.dll"));

            return JsonConvert.SerializeObject(assemblyQueue );
        }

        public void Dispose() { }
    }
}
