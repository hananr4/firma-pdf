
using System.Security.Cryptography.X509Certificates;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace Greensoft.SignPdf;
public class SignPdfService
{
  public static void Sign(
    string pathToPdf,
    string pathToPfx,
    string pfxPassword,
    string searchText ,
    string? imagePath = null,
    string? reason = null,
    string? contact = null,
    string? location = null)
  {
    if (!File.Exists(pathToPfx))
    {
      Console.WriteLine("No se encontró el archivo P12");
      return;
    }

    if (!File.Exists(pathToPdf))
    {
      Console.WriteLine("No se encontró el archivo PDF");
      return;
    }

    var fi = new FileInfo(pathToPdf);
    var dirOutput = System.IO.Path.Combine(fi.DirectoryName!, "output");
    if (!Directory.Exists(dirOutput))
    {
      Directory.CreateDirectory(dirOutput);
    }
    string pathToNewPdf = System.IO.Path.Combine(dirOutput, $"{fi.Name}_firmado.pdf");

    // Cargar el certificado PFX
    var store = new Pkcs12Store(new FileStream(pathToPfx, FileMode.Open, FileAccess.Read), pfxPassword.ToCharArray());
    string alias = string.Empty;

    // Buscar el alias del certificado
    foreach (string al in store.Aliases)
    {
      if (store.IsKeyEntry(al) && store.GetKey(al).Key.IsPrivate)
      {
        alias = al;
        break;
      }
    }

    var cert = new X509Certificate2(pathToPfx, pfxPassword, X509KeyStorageFlags.Exportable);
    Org.BouncyCastle.X509.X509Certificate bcCert = DotNetUtilities.FromX509Certificate(cert);
    AsymmetricKeyParameter privKey = store.GetKey(alias).Key;

    // Abrir el PDF y preparar el objeto de firma
    var reader = new PdfReader(pathToPdf);

    float left = 0;
    float top = 0;
    float right = 0;
    float bottom = 0;
    for (int i = 1; i <= reader.NumberOfPages; i++)
    {
      var strategy = new MyLocationTextExtractionStrategy(searchText);

      string currentText = PdfTextExtractor.GetTextFromPage(reader, i, strategy);
      strategy.MatchWord();
      if (strategy.MatchedLocations.Count > 0)
      {
        foreach (var rect in strategy.MatchedLocations)
        {
          Console.WriteLine($"Encontrado en la página {i} en la ubicación {rect.Left},{rect.Bottom}");
          left = rect.Left;
          top = rect.Top;
          right = rect.Right;
          bottom = rect.Bottom;

        }
      }
    }


    FileStream os = new FileStream(pathToNewPdf, FileMode.Create);
    PdfStamper stamper = PdfStamper.CreateSignature(reader, os, '\0');
    PdfSignatureAppearance appearance = stamper.SignatureAppearance;
    appearance.Reason = reason;
    appearance.Contact = contact;
    appearance.Location = location;
    appearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION;

    if (imagePath != null && File.Exists(imagePath)){
      appearance.SignatureGraphic = Image.GetInstance(imagePath);
    }


    // Localizar rectangulo donde se va a firmar
    var rectangle = new Rectangle(left, bottom, right, top)
    {
      Left = left,
      Right = right,
      Top = top - (top - bottom),
      Bottom = bottom - 50
    };
    appearance.SetVisibleSignature(rectangle, 1, "Firma");


    // Crear la firma
    IExternalSignature signature = new PrivateKeySignature(privKey, "SHA-256");
    MakeSignature.SignDetached(appearance, signature, new Org.BouncyCastle.X509.X509Certificate[] { bcCert }, null, null, null, 0, CryptoStandard.CMS);


    // Leer los campos

    AcroFields formFields = reader.AcroFields;
    foreach (KeyValuePair<string, AcroFields.Item> entry in formFields.Fields)
    {
      Console.WriteLine(entry.Key);
    }


    // Cerrar el stamper y el lector
    stamper.Close();
    reader.Close();
  }
}
