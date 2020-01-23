using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
//using System.Math;

public static class Debug
{
  public static void Log(params Object[] args)
  {
    args.ToList().ForEach(o =>
    {
      Console.WriteLine(o);
    });
  }
}

public struct HexPos
{
  public int x;
  public int y;

  public static HexPos From(int x, int y)
  {
    return new HexPos() { x = x, y = y };
  }
  public override bool Equals(Object o)
  {
    if (!(o is HexPos))
    {
      return false;
    }
    var hex = (HexPos)o;

    return this.x == hex.x && this.y == hex.y;
  }

  public static HexPos operator * (HexPos h,int x){
    return HexPos.From(h.x*x,h.y*x);
  }

  public static int Distance(HexPos A,HexPos B){
    //まずA,Bをそれぞれ2倍にする
    HexPos a =A*2;
    HexPos b =B*2;
    //Yが奇数だった場合にそれぞれのXに+1する
    a.x+= (A.y % 2 == 1 )?1:0;
    b.x+= (B.y % 2 == 1 )?1:0;

    int ydiff = Math.Abs( a.y - b.y );
    //int ydiff = 0;

    //Xの距離は diff Y *    

    int range = (
            Math.Abs(a.x - b.x)
          + Math.Abs(a.y - b.y)
          - (ydiff / 2)
          )/2;
   // Console.WriteLine($"[ {a.x},{a.y} ] [ {b.x},{b.y} ] =RANGE {range} ");
    return range;
  }
}

public struct Size
{
  public int x;
  public int y;

  public static Size From(int x, int y)
  {
    return new Size()
    {
      x = x,
      y = y
    };
  }
}

public struct Cursor
{
  public int x;
  public int y;
  public static Cursor From(int x, int y)
  {
    return new Cursor()
    {
      x = x,
      y = y
    };
  }

  public static implicit operator HexPos(Cursor c)
  {
    return HexPos.From(c.x, c.y);
  }
}

public struct HexIcon
{
  public string icon;

}

public class HexMap : Dictionary<HexPos, int>
{
  public Size size;
  public HexMap(Size size)
  {
    this.size = size;
  }

  //障害物関係なく単純に面を取得する
  public static List<HexPos> GetRangeAll(HexPos from, int range, List<HexPos> result = null)
  {
    //YはRangeにしたがう
    //XはRange/2 が加算される
    //
    return
    Enumerable.Range(-range, range * 2 + 1).Select(y =>
    Enumerable.Range(-range, range * 2 + 1).Select(x =>
        HexPos.From(from.x + x, from.y + y)
      ).Where(
        pos=> HexPos.Distance(pos,from) <= range
      )
      .ToList()
    ).Aggregate(
      new List<HexPos>(),
      (carry, item) =>
    {
      carry.AddRange(item);
      return carry;
    }
    ).ToList();

  }

  public List<HexPos> getRange(HexPos from, int range = 2, List<HexPos> result = null)
  {
    result = GetRangeAll(from, range);
    result.Remove(from);
    return result;
  }

  public List<HexPos> getRangeAster(HexPos from, int range = 1, List<HexPos> result = null)
  {
    if (result == null) result = new List<HexPos>();
    HexPos
      L = HexPos.From(from.x - 1, from.y),
      R = HexPos.From(from.x + 1, from.y),
      UL = HexPos.From(from.y % 2 == 1 ? from.x : from.x - 1, from.y - 1),
      UR = HexPos.From(from.y % 2 == 1 ? from.x + 1 : from.x, from.y - 1),
      DL = HexPos.From(from.y % 2 == 1 ? from.x : from.x - 1, from.y + 1),
      DR = HexPos.From(from.y % 2 == 1 ? from.x + 1 : from.x, from.y + 1)
    ;

    List<HexPos> way =
    (new List<HexPos>(){
      L,//左
      R,//右
      UL,//上
      UR,//上
      //下二つ
      DL,
      DR,
    }).Where(
          p =>
            !result.Exists(a => a.x == p.x && a.y == p.y) //既存でないか判定
            && (0 <= p.y && p.y < size.y && 0 <= p.x && p.x <= size.x)  //範囲内判定
    ).ToList();

    result.AddRange(way);
    if (0 < --range)
    {
      //6方向
      //result.AddRange(getRange(L, range, result.Distinct().ToList()).Where(p => result.Exists(r => r.Equals(p))));
      result.AddRange(getRange(L, range, result).Where(p => !result.Exists(r => r.Equals(p))));
      result.AddRange(getRange(R, range, result).Where(p => !result.Exists(r => r.Equals(p))));
      result.AddRange(getRange(UL, range, result).Where(p => !result.Exists(r => r.Equals(p))));
      result.AddRange(getRange(UR, range, result).Where(p => !result.Exists(r => r.Equals(p))));
      result.AddRange(getRange(DL, range, result).Where(p => !result.Exists(r => r.Equals(p))));
      result.AddRange(getRange(DR, range, result).Where(p => !result.Exists(r => r.Equals(p))));
    }
    //重複は削除

    //var before = result.Count;
    result = result.Distinct().ToList();
    //var after = result.Count;

    //Debug.Log($"FROM {before} -> {after} ");
    //自分は削除
    result.RemoveAll(a => a.Equals(from));

    return result;
  }


  //特に意味は無い。表示用
  public string this[HexPos x]
  {
    get
    {
      return this.ContainsKey(x) ? "__" : "..";
    }
  }
}




//実質テスト用クラス
public class HexRenderer
{

  public HexMap source;
  public Cursor cursor = Cursor.From(3, 3);

  public string LargeMap(){
    var sb = new StringBuilder();
    for (int y = 0; y < source.size.y; y++)
    {
      //Yが奇数のときは2ずらす
      if (y % 2 == 1)
      {
        sb.Append("|_");
      }
      for (int x = 0; x < source.size.x; x++)
      {
        sb.Append($"|_{source[HexPos.From(x, y)]}");
      }
      sb.Append("|\n");
    }
    return sb.ToString();
  }


  public string MiniMap(){
    var sb = new StringBuilder();
    for (int y = 0; y < source.size.y; y++)
    {
      //Yが奇数のときは1ずらす
      if (y % 2 == 1)
      {
        sb.Append("_");
      }
      for (int x = 0; x < source.size.x; x++)
      {
        sb.Append($"|_");
      }
      sb.Append("|\n");
    }
    return sb.ToString();
  }

  public void MinimapRender(){
    Console.Write(MiniMap());
    //自身を書き込む
    Console.SetCursorPosition((cursor.x * 2) + 1 + (cursor.y % 2 == 1 ? 1 : 0), cursor.y);
    Console.BackgroundColor = ConsoleColor.Blue;
    Console.Write("*");
    Console.ResetColor();
    //範囲を書き込み
    foreach (HexPos p in 
        this.source.getRange(cursor, 3)
        .Where(hp=>
          0 <= hp.x && 0 <= hp.y &&
          hp.x < Console.BufferWidth && hp.y < Console.BufferWidth 
          )
        )
    {
      Console.SetCursorPosition(
          (p.x * 2) + 1 + (p.y % 2 == 1 ? 1 : 0),
          p.y
      );
      Console.BackgroundColor = ConsoleColor.Red;
      Console.Write("_");
      Console.ResetColor();
    }
  }

  public void Start()
  {
    //カーソルを描画する
    while (true)
    {
      Console.Clear();
      this.MinimapRender();

      Console.SetCursorPosition(0, 30);
      //キー入力　ここから
      var k = Console.ReadKey(true);
      switch (k.Key)
      {
        case ConsoleKey.UpArrow: cursor.y--; break;
        case ConsoleKey.LeftArrow: cursor.x--; break;
        case ConsoleKey.RightArrow: cursor.x++; break;
        case ConsoleKey.DownArrow: cursor.y++; break;
        default: break;
      }
      //キー入力　ここまで
    }
  }
}
