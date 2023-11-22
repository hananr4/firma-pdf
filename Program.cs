using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf.security;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

using System.Security.Cryptography.X509Certificates;

class Program
{
  static void Main(string[] args)
  {
    SignPdfService.Sign(
        pathToPdf: @".\docs\sample2.pdf"
      , pathToPfx: Environment.GetEnvironmentVariable("P12_FILE") ?? ""
      , pfxPassword: Environment.GetEnvironmentVariable("P12_PASSWORD") ?? ""
      , searchText: "[FIRMA_________ASESOR]"
      , imagePath: @".\docs\firma.png"
      , reason: "Aprobación de crédito"
      , contact: "user@domain.com"
      , location: "Quito"
    );
  }
}

