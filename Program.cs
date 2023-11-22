
namespace Greensoft.SignPdf;

class Program
{
  static void Main(string[] args)
  {
    SignPdfService.Sign(
        pathToPdf: @".\docs\sa1mple2.pdf"
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

