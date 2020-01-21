using System;

class Program
{
  static void Main(string[] args)
  {
    HexRenderer hex = new HexRenderer();
    hex.source = new HexMap(Size.From(20, 20));
    hex.Start();
  }
}
