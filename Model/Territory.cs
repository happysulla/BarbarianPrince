using System;
using System.Collections.Generic;
using System.Linq;

namespace BarbarianPrince
{
   [Serializable]
   public class Territory : ITerritory
   {
      [NonSerialized] static public List<ITerritory> theTerritories = new List<ITerritory>();
      public string Name { get; set; } = "";
      public string Type { get; set; } = "";
      public bool IsTown { get; set; } = false;
      public bool IsCastle { get; set; } = false;
      public bool IsRuin { get; set; } = false;
      public bool IsTemple { get; set; } = false;
      public bool IsOasis { get; set; } = false;
      public string DownRiver { get; set; } = "";
      public List<String> Roads { get; set; } = new List<String>();
      public List<String> Rivers { get; set; } = new List<String>();
      public List<String> Adjacents { get; set; } = new List<String>();
      public List<String> Rafts { get; set; } = new List<String>();
      public IMapPoint CenterPoint { get; set; } = new MapPoint();
      public List<IMapPoint> Points { get; set; } = new List<IMapPoint>();
      //---------------------------------------------------------------
      public Territory(string name) { Name = name; }
      public override String ToString()
      {
         return this.Name;
      }
      public ITerritory Find(List<ITerritory> territories, string name)
      {
         IEnumerable<ITerritory> results = from territory in territories
                                           where territory.Name == name
                                           select territory;
         if (0 < results.Count())
            return results.First();
         else
            throw (new Exception("Territory.Find(): Unknown Territory=" + name));
      }
   }
   public static class TerritoryExtensions
   {
      public static ITerritory Find(this IList<ITerritory> territories, string name)
      {
         try
         {
            //int index1 = nameAndSector.IndexOf(":");
            //string sSector = nameAndSector.Substring(0, index1);
            //string name = nameAndSector.Substring(index1 + 1);
            //int sector = Int32.Parse(sSector);
            IEnumerable<ITerritory> results = from territory in territories
                                              where territory.Name == name
                                              select territory;
            if (0 < results.Count())
               return results.First();
         }
         catch (Exception e)
         {
            Console.WriteLine("MyTerritoryExtensions.Find(list, nameAndSector): nameAndSector={0} causes e.Message={1}", name, e.Message);
         }
         return null;
      }

   }
}
