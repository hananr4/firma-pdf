using iTextSharp.text;
using iTextSharp.text.pdf.parser;

public class MyLocationTextExtractionStrategy : LocationTextExtractionStrategy
{
  public List<Rectangle> MatchedLocations = new List<Rectangle>();
  public List< WordRectangle> MapText = new List< WordRectangle>();
  private string searchText;

  public MyLocationTextExtractionStrategy(string searchText)
  {
    this.searchText = searchText;
  }

  public override void BeginTextBlock()
  {
    base.BeginTextBlock();
  }

  public override void EndTextBlock()
  {
    base.EndTextBlock();
  }

  public override string GetResultantText()
  {
    return base.GetResultantText();
  }
  public override void RenderText(TextRenderInfo renderInfo)
  {
    base.RenderText(renderInfo);

    string currentText = renderInfo.GetText();

    var bottomLeft = renderInfo.GetDescentLine().GetStartPoint();
    var topRight = renderInfo.GetAscentLine().GetEndPoint();

    MapText.Add(new WordRectangle() { 
      Rectangle = new Rectangle(bottomLeft[Vector.I1], bottomLeft[Vector.I2], topRight[Vector.I1], topRight[Vector.I2]), 
      Word = currentText });

  }

 
  public void MatchWord(){
    
    var text = MapText.Select(x=>x.Word).Aggregate((a,b)=>a+b);
    int pos = 0;
    MapText.ForEach(x=> {
        x.Index = pos + x.Word.Length;
        pos = x.Index;
    });
    pos = -1;
    pos = text.IndexOf(searchText);

    if(pos > -1){
      var matched = MapText.Where(x=>x.Index > pos && x.Index <= pos + searchText.Length);
      if (matched.Count() > 0){
        var left = matched.Select(x=>x.Rectangle.Left).Min();
        var right = matched.Select(x=>x.Rectangle.Right).Max();
        var bottom = matched.Select(x=>x.Rectangle.Bottom).Min();
        var top = matched.Select(x=>x.Rectangle.Top).Max();
        MatchedLocations.Add(new Rectangle(left, bottom, right, top));
      }
    }  
  }
}
