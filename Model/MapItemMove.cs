using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   [Serializable]
   public class MapItemMove : IMapItemMove
   {
      public bool CtorError { get; } = false;
      public IMapItem MapItem { set; get; } = null;        // Represents the map item that is being moved
      public ITerritory OldTerritory { set; get; } = null; // Represents the old territory that the MapItem is being moved from.
      public ITerritory NewTerritory { set; get; } = null; // Represents the new territory that the MapItem is being moved to.
      public IMapPath BestPath { set; get; } = null;
      public RiverCrossEnum RiverCross { get; set; } = RiverCrossEnum.TC_NO_RIVER;
      //------------------------------------------------------------------------------
      public MapItemMove() // default constructor
      {
      }
      public MapItemMove(ITerritory oldT, ITerritory newT)
      {
         OldTerritory = oldT;
         NewTerritory = newT;
      }
      public MapItemMove(List<ITerritory> territories, IMapItem movingMapItem, ITerritory newTerritory) // Do not move into overstacked region
      {
         MapItem = movingMapItem;
         OldTerritory = movingMapItem.Territory;
         BestPath = GetBestPath(territories, OldTerritory, newTerritory, 100);
         if (null == BestPath)
         {
            String msg = "MapItemMove():BestPath=null for";
            msg += MapItem.ToString();
            msg += " from ";
            msg += OldTerritory.Name;
            BestPath = new MapPath(OldTerritory.ToString());
            Logger.Log(LogEnum.LE_ERROR, "MapItemMove(): Not able to find best path");
            CtorError = true;
            return;
         }
         // Remove last territory if exceeds stacking limit.
         int countOfTerritories = BestPath.Territories.Count;
         NewTerritory = BestPath.Territories[countOfTerritories - 1];
      }
      public MapItemMove(IMapItemMove mim)
      {
         MapItem = mim.MapItem;
         OldTerritory = mim.OldTerritory;
         NewTerritory = mim.NewTerritory;
         BestPath = new MapPath(mim.BestPath);
         RiverCross = mim.RiverCross;
      }
      public MapItemMove(IMapItem movingMapItem, ITerritory oldTerritory, ITerritory newTerritory, IMapPath bestPath) // Copy Contructor
      {
         MapItem = movingMapItem;
         OldTerritory = oldTerritory;
         NewTerritory = newTerritory;
         BestPath = new MapPath(bestPath);
      }
      //------------------------------------------------------------------------------
      static public double GetDistance(ITerritory startT, ITerritory endT)
      {
         System.Windows.Point startPoint = new System.Windows.Point(startT.CenterPoint.X, startT.CenterPoint.Y);
         System.Windows.Point endPoint = new System.Windows.Point(endT.CenterPoint.X, endT.CenterPoint.Y);
         double xDelta = endPoint.X - startPoint.X;
         double yDelta = endPoint.Y - startPoint.Y;
         double distance = Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
         return distance;
      }
      public IMapPath GetBestPath(List<ITerritory> territories, ITerritory startT, ITerritory endT, int moveFactor)
      {
         IMapPaths paths = new MapPaths();
         if (moveFactor < 1)
            return null;
         IMapPaths adjPaths = new MapPaths();
         if (startT.Name == endT.Name)
         {
            IMapPath path = new MapPath(endT.Name);
            path.Territories.Add(endT);
            paths.Add(path);
            return path;
         }
         else
         {
            // Setup a path map for each adjacent territory
            foreach (String adjTerritory in startT.Adjacents)
            {
               IMapPath path = new MapPath(adjTerritory);
               ITerritory adj = TerritoryExtensions.Find(territories, adjTerritory);
               path.Territories.Add(adj);
               path.Metric = MapItemMove.GetDistance(adj, endT);
               paths.Add(path);
               adjPaths.Add(path);
               // If the adjacent territory is the end territory, no need to continue.  It is the best path.
               if (adjTerritory == endT.ToString())
               {
                  Logger.Log(LogEnum.LE_VIEW_MIM, "GetBestPath(): Adjacent Move of " + MapItem.Name + " moving from " + startT.Name + " to " + endT.Name);
                  return path;
               }
            }
            // For each IMapPath object, determine the next Territory that  moves the object closer to the end goal.
            bool isEndTerritoryReached = false;
            for (int i = 1; i < moveFactor; ++i)
            {
               //Console.WriteLine("---------------->>MF={0}<<-------------------------", i.ToString());
               // Perform no more movement if end territory is reached by one of the paths.
               if (true == isEndTerritoryReached)
                  break;
               // Iterate through the IMapPath objects trying to find the lowest metric score for each adjacent territory.
               foreach (IMapPath path in paths)
               {
                  //Console.WriteLine("==> Adding to {0} ", path.ToString());
                  if (path.Metric == Double.MaxValue)
                     continue;
                  // Set a threshold for the lowest metric score.
                  // Set it to a very high number because the first interation of 
                  // the following loop determines what metric score to bcontinue.
                  // If a metric score is less than this number, it is set as
                  // the new threshold, i.e. trying to find the minimum metric score.
                  double lowestMetricScore = Double.MaxValue; // Set to high number
                  ITerritory lowestTerritory = null;
                  // A Territory is better if the distance between the center
                  // point of the territory and all other alternatives is 
                  // the smallest.
                  ITerritory adj1 = path.Territories[path.Territories.Count - 1];
                  foreach (String alternative in adj1.Adjacents)
                  {
                     //Console.WriteLine("     ==> Trying {0}", alternative);
                     ITerritory adj2 = TerritoryExtensions.Find(territories, alternative);
                     // If the end territory is reached, no need to continue
                     // looking at alternates.
                     if (adj2.Name == endT.Name)
                     {
                        //Console.WriteLine("     ==> ==>Reached End Territory {0} for PATH={1}", adj2.ToString(), path.ToString());
                        // Calculate the metric between this adjacent territory and
                        // the end territory.  If it results in a lower path metric,
                        // set it at the low water mark.
                        double altDistanceMetric = GetDistance(adj2, endT);
                        altDistanceMetric += path.Metric;
                        if (altDistanceMetric <= lowestMetricScore)
                        {
                           lowestMetricScore = altDistanceMetric;
                           lowestTerritory = adj2;
                        }
                        isEndTerritoryReached = true;
                        break; // end reached so break out of loop
                     }
                     // Exclude alternative paths that fold back to start territory
                     if (adj2.Name == startT.Name)
                     {
                        //Console.WriteLine("     ==> ==>{0} is start territory", adj2.ToString());
                        continue;
                     }
                     // Exclude alternative paths that fold back to other adjacent territories
                     bool isMatchFound = false;
                     foreach (IMapPath aPath in adjPaths)
                     {
                        if (alternative == aPath.Name)
                        {
                           isMatchFound = true;
                           break;
                        }
                     }
                     if (true == isMatchFound)
                     {
                        //Console.WriteLine("     ==> ==> {0} is already adjacent {1}", adj2.ToString(), path.ToString());
                        continue;
                     }
                     // Exclude alternative paths that fold back on themselves, i.e.
                     // do not choose a Territory that is already on this MapPath.
                     IEnumerable<ITerritory> results1 = from territory in path.Territories
                                                        where territory.Name == adj2.Name
                                                        select territory;
                     if (0 < results1.Count())
                     {
                        //Console.WriteLine("     ==> ==> {0} is already in {1}", adj2.ToString(), path.ToString());
                        continue;
                     }
                     // Calculate the metric between this adjacent territory and
                     // the end territory.  If it results in a lower path metric,
                     // set it at the low water mark.
                     double altDistanceMetric2 = GetDistance(adj2, endT);
                     altDistanceMetric2 += path.Metric;
                     if (altDistanceMetric2 <= lowestMetricScore)
                     {
                        lowestMetricScore = altDistanceMetric2;
                        lowestTerritory = adj2;
                     }
                  } // end foreach (String alternative in adj1.Adjacents)
                    // Check if a territory was added to Map Path for this instance.
                    // If not, then this map path needs to be deleted.
                  if (Double.MaxValue == lowestMetricScore)
                  {
                     //Console.WriteLine("     ==> Skipping {0} at Max Value", path.ToString());
                     path.Metric = Double.MaxValue;
                     continue;
                  }
                  else // Add the Territory with the lowest Metric to the path
                  {
                     path.Territories.Add(lowestTerritory);
                     path.Metric = lowestMetricScore;
                     //Console.WriteLine("     ==> Appending to {0}", path.ToString());
                  }
               } // end foreach (IMapPath path in paths)
            } // end for (int i = 0; i < moveFactor; ++i)
         } // end else startT is not equal to endT
           // Determine from all paths which is the lowest metric
         int i1 = 1;
         int count = paths.Count;
         if (count < 1)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetBestPath(): did not reach " + startT.Name + " from " + endT.Name);
            return null;
         }
         IMapPath bestPath = paths[0];
         foreach (IMapPath path in paths)
         {
            //Console.WriteLine("{0}.) {1}", i1.ToString(), path.ToString());
            if (path.Metric < bestPath.Metric)
               bestPath = path;
            ++i1;
         }
         Logger.Log(LogEnum.LE_VIEW_MIM, "GetBestPath(): " + MapItem.Name + " moving from " + startT.Name + " to " + endT.Name + " using " + bestPath.ToString());
         return bestPath;
      }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder("");
         sb.Append("mi=");
         sb.Append(MapItem.Name);
         sb.Append(",oT=");
         sb.Append(OldTerritory.Name);
         sb.Append(",nT=");
         sb.Append(NewTerritory.Name);
         sb.Append(",rc?=");
         sb.Append(RiverCross.ToString());
         return sb.ToString();
      }
   }
   //-------------------------------------------------------
   [Serializable]
   public class MapItemMoves : IMapItemMoves
   {
      private readonly ArrayList myList;
      public MapItemMoves() { myList = new ArrayList(); }
      public void Add(IMapItemMove mim) { myList.Add(mim); }
      public IMapItemMove RemoveAt(int index)
      {
         IMapItemMove mim = (IMapItemMove)myList[index];
         myList.RemoveAt(index);
         return mim;
      }
      public void Insert(int index, IMapItemMove mim) { myList.Insert(index, mim); }
      public int Count { get { return myList.Count; } }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMapItemMove mim) { return myList.Contains(mim); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMapItemMove mim) { return myList.IndexOf(mim); }
      public void Remove(IMapItemMove mim) { myList.Remove(mim); }
      public IMapItemMove Find(IMapItem mi)
      {
         foreach (Object o in myList)
         {
            IMapItemMove mim = (IMapItemMove)o;
            if (mi.Name == mim.MapItem.Name)
               return mim;
         }
         return null;
      }
      public IMapItemMove Remove(IMapItem mi)
      {
         foreach (Object o in myList)
         {
            IMapItemMove mim = (IMapItemMove)o;
            if (mi.Name == mim.MapItem.Name)
            {
               myList.Remove(mim);
               return mim;
            }
         }
         return null;
      }
      public IMapItemMoves Shuffle()
      {
         IMapItemMoves newOrder = new MapItemMoves();

         // Random select card in myCards list and
         // remove it.  Then add it to new list. 

         int count = myList.Count;
         for (int i = 0; i < count; i++)
         {
            int index = Utilities.RandomGenerator.Next(myList.Count);
            if (index < myList.Count)
            {
               IMapItemMove randomIndex = (IMapItemMove)myList[index];
               myList.RemoveAt(index);
               newOrder.Add(randomIndex);
            }
         }

         return newOrder;
      }
      public IMapItemMove this[int index]
      {
         get { return (IMapItemMove)(myList[index]); }
         set { myList[index] = value; }
      }
   }
}
