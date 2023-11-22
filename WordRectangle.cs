using iTextSharp.text;

namespace Greensoft.SignPdf;
public class WordRectangle {
  public required Rectangle Rectangle { get; set; }
  public required string Word { get; set; }
  public  int Index { get; set; }
}