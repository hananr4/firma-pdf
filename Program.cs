using System.IO;
using Org.BouncyCastle.Pkcs;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using iTextSharp.text.pdf.security;
using System.Text;
using iTextSharp.text.pdf.parser;

string pathToPdf = @"C:\temp\dotnet\firmaPdf\sample3.pdf";
string pathToNewPdf = @"C:\temp\dotnet\firmaPdf\sample3_firmado.pdf";
string pathToPfx = @"C:\temp\fact-elect\greensoft\certificado\GERENTE GENERAL HANAN LISANDRO RODRIGUEZ MERINO 010523100928.p12";
string pfxPassword = "Mioysolomio1995";

// Cargar el certificado PFX
Pkcs12Store store = new Pkcs12Store(new FileStream(pathToPfx, FileMode.Open, FileAccess.Read), pfxPassword.ToCharArray());
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

X509Certificate2 cert = new X509Certificate2(pathToPfx, pfxPassword, X509KeyStorageFlags.Exportable);
Org.BouncyCastle.X509.X509Certificate bcCert = DotNetUtilities.FromX509Certificate(cert);
AsymmetricKeyParameter privKey = store.GetKey(alias).Key;

// Abrir el PDF y preparar el objeto de firma
PdfReader reader = new PdfReader(pathToPdf);
string searchText = "OBLIGADO A LLEVAR CONTABILIDAD";

float left= 0;
float top = 0;
Rectangle rect2 = new Rectangle(100,100,100,100);   
        for (int i = 1; i <= reader.NumberOfPages; i++)
        {
            var strategy = new MyLocationTextExtractionStrategy(searchText);
            string currentText = PdfTextExtractor.GetTextFromPage(reader, i, strategy);

            if (strategy.MatchedLocations.Count > 0)
            {
                foreach (var rect in strategy.MatchedLocations)
                {
                    Console.WriteLine($"Encontrado en la página {i} en la ubicación {rect.Left},{rect.Bottom}");
                    left = rect.Left;
                    top = rect.Top;
                    rect2 = new Rectangle(rect.Left, rect.Bottom, rect.Right, rect.Top);
                }
            }
        }




FileStream os = new FileStream(pathToNewPdf, FileMode.Create);
PdfStamper stamper = PdfStamper.CreateSignature(reader, os, '\0');  
PdfSignatureAppearance appearance = stamper.SignatureAppearance;
appearance.Reason = "Aprobación de crédito";
appearance.Contact = "hanan.rodriguez@greensoft.com.ec";
appearance.Location = "Quito, Ecuador";
appearance.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION;
appearance.SignatureGraphic = Image.GetInstance(@"C:\temp\cedia.png");  
  
appearance.SetVisibleSignature(new Rectangle(left, top, left + 100, top + 100), 1, "Firma");

// appearance.SetVisibleSignature( rect2!, 1, "Firma");
 //appearance.SetVisibleSignature( new Rectangle(50, 50,50, 50, 1), 1, "Firma");

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






public class MyLocationTextExtractionStrategy : LocationTextExtractionStrategy
{
    public List<iTextSharp.text.Rectangle> MatchedLocations = new List<iTextSharp.text.Rectangle>();
    private string searchText;

    public MyLocationTextExtractionStrategy(string searchText)
    {
        this.searchText = searchText;
    }

    public override void RenderText(TextRenderInfo renderInfo)
    {
        base.RenderText(renderInfo);

        string currentText = renderInfo.GetText();
        if (currentText.Contains(searchText))
        {
            var bottomLeft = renderInfo.GetDescentLine().GetStartPoint();
            var topRight = renderInfo.GetAscentLine().GetEndPoint();
            var rect = new iTextSharp.text.Rectangle(
                bottomLeft[Vector.I1], bottomLeft[Vector.I2],
                topRight[Vector.I1], topRight[Vector.I2]
            );
            MatchedLocations.Add(rect);
        }
    }
}
