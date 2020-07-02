using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using ClownClubServer.NLC;
using NotLiteCode.Network;
using NotLiteCode.Serialization;
using NotLiteCode.Server;

namespace ClownClubServer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private static readonly NLCSocket Socket = new NLCSocket(new GroBufSerializationProvider(), true, ServerCertificate: GenerateSelfSignedCert("TheClownClub", "localhost"));

        private static readonly Server<SharedClass> Server = new Server<SharedClass>(Socket);

        public MainWindow() {
            InitializeComponent();
            Server.Start(1338);
        }

        public static X509Certificate2 GenerateSelfSignedCert(string certificateName, string hostName) {
            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddDnsName(hostName);

            var distinguishedName = new X500DistinguishedName($"CN={certificateName}");

            using (var rsa = RSA.Create(2048)) {
                var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(
                        X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment |
                        X509KeyUsageFlags.DigitalSignature, false));

                request.CertificateExtensions.Add(sanBuilder.Build());

                var certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
                    new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));

                return new X509Certificate2(certificate.Export(X509ContentType.Pfx));
            }
        }
    }
}
