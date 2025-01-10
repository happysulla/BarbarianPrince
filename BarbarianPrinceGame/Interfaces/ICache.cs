using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BarbarianPrince
{
   public interface ICache
   {
      ITerritory CacheTerritory { get; set; }
      int Coin { get; set; }
   }
   public interface ICaches : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(ICache c);
      void Add(ITerritory territory, int coin);
      ICache RemoveAt(int index);
      void Insert(int index, ICache c);
      void Reverse();
      void Clear();
      bool Contains(ICache c);
      int IndexOf(ICache c);
      void Remove(ICache c);
      ICache this[int index] { get; set; }
      ICaches Sort();
      ICache Find(string tName);
   }
}
