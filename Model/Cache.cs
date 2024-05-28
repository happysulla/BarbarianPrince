using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BarbarianPrince
{
   [Serializable]
   public class Cache : ICache
   {
      public ITerritory TargetTerritory { get; set; } = null;
      public int Coin { get; set; } = 0;
      public Cache(ITerritory territory, int coin)
      {
         TargetTerritory = territory;
         Coin = coin;
      }
   }
   //---------------------------------------------------------
   [Serializable]
   public class Caches : IEnumerable, ICaches
   {
      private readonly ArrayList myList;
      public Caches() { myList = new ArrayList(); }
      public int Count { get { return myList.Count; } }
      public void Add(ICache c) { myList.Add(c); }
      public void Add(ITerritory territory, int coin)
      {
         ICache c = new Cache(territory, coin);
         myList.Add(c);
      }
      public ICache RemoveAt(int index)
      {
         ICache c = (ICache)myList[index];
         myList.RemoveAt(index);
         return c;
      }
      public void Insert(int index, ICache c) { myList.Insert(index, c); }
      public void Reverse() { myList.Reverse(); }
      public void Clear() { myList.Clear(); }
      public bool Contains(ICache c) { return myList.Contains(c); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(ICache c) { return myList.IndexOf(c); }
      public void Remove(ICache c) { myList.Remove(c); }
      public ICache this[int index]
      {
         get { return (ICache)(myList[index]); }
         set { myList[index] = value; }
      }
      public ICaches Sort()
      {
         ICaches sortedCaches = new Caches();
         foreach (Object o in myList)
         {
            ICache c1 = (ICache)o;
            bool isInserted = false;
            int index = 0;
            foreach (ICache c2 in sortedCaches)
            {
               if (c2.Coin <= c1.Coin)
               {
                  sortedCaches.Insert(index, c1);
                  isInserted = true;
                  break;
               }
               ++index;
            }
            if (false == isInserted) // If not inserted, add to end
               sortedCaches.Add(c1);
         }
         return sortedCaches;
      }
      public ICache Find(string tName)
      {
         foreach (Object o in myList)
         {
            ICache c = (ICache)o;
            if (tName == c.TargetTerritory.Name)
               return c;
         }
         return null;
      }
   }
}
