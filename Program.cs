using System;

class Program
{
  static void Main(string[] args)
  {
    HexRenderer hex = new HexRenderer();

    hex.source = HexMap.Random();


    while (true)
    {
      Console.Clear();
      hex.MinimapRender();
      Console.SetCursorPosition(0, 30);
      //キー入力　ここから
      var k = Console.ReadKey(true);
      switch (k.Key)
      {
        case ConsoleKey.UpArrow: hex.cursor.y--; break;
        case ConsoleKey.LeftArrow: hex.cursor.x--; break;
        case ConsoleKey.RightArrow: hex.cursor.x++; break;
        case ConsoleKey.DownArrow: hex.cursor.y++; break;
        default: break;
      }
      //キー入力　ここまで
    }
  }
}
