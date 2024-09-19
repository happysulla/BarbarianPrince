using System;
using System.Collections.Generic;

namespace BarbarianPrince
{
   public interface ITerritory
   {
      string Name { get; set; }
      string Type { get; set; }
      IMapPoint CenterPoint { get; set; }
      bool IsTown { get; set; }
      bool IsCastle { get; set; }
      bool IsRuin { get; set; }
      bool IsTemple { get; set; }
      bool IsOasis { get; set; }
      string DownRiver { get; set; } // E126 - downriver territory if raft in current
      List<String> Roads { get; set; }
      List<String> Rivers { get; set; }
      List<String> Adjacents { get; }
      List<String> Rafts { get; set; }
      List<IMapPoint> Points { set; get; }
   }
   //--------------------------------------------------------
   public interface ITerritories : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(ITerritory t);
      ITerritory RemoveAt(int index);
      void Insert(int index, ITerritory t);
      void Clear();
      bool Contains(ITerritory t);
      int IndexOf(ITerritory t);
      void Remove(ITerritory tName);
      ITerritory Remove(string tName);
      ITerritory Find(string tName);
      ITerritory this[int index] { get; set; }
   }
}
