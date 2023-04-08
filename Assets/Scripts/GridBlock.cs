public class GridBlock
{
    public string symbol;
    public int x;
    public int y;
    public bool discovered = false;

    public GridBlock(){}
    public GridBlock(string symbol, int x, int y)
    {
        this.symbol = symbol;
        this.x = x;
        this.y = y;
        this.discovered = false;
    }
}
