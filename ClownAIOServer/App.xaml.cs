using NotLiteCode.Network;
using NotLiteCode.Server;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using NotLiteCode.Serialization;

namespace ClownClubServer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private static X509Certificate2 GenerateSelfSignedCert(string CertificateName, string HostName) {
            SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddDnsName(HostName);

            X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN={CertificateName}");

            using (RSA rsa = RSA.Create(2048)) {
                var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

                request.CertificateExtensions.Add(sanBuilder.Build());

                var certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
                certificate.FriendlyName = CertificateName;

                return new X509Certificate2(certificate.Export(X509ContentType.Pfx));
            }
        }

        private void Initialize(object sender, StartupEventArgs e) {
            DatabaseManager.Init();
            _ = BotStartup.RunAsync();
            var serverSocket = new NLCSocket(new GroBufSerializationProvider(), UseSSL: true, ServerCertificate: GenerateSelfSignedCert("TheClownClub", "theclown.club"));
            var server = new Server<NLC.SharedClass>(serverSocket);
            server.Start();

            var window = new MainWindow();
            window.Show();
        }
    }
}
