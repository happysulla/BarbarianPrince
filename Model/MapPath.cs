using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BarbarianPrince
{
    /// <summary>
    /// This class represents a connection of ITerritory objects
    /// from a start Territory to an end Terrirtory.
    /// </summary>
    class MapPath : IMapPath
    {
        private string myName;
        public string Name
        {
            get { return myName; }
            set { myName = value; }
        }
        private double myMetric = 0;
        public double Metric
        {
            get { return myMetric; }
            set { myMetric = value; }
        }
        private List<ITerritory> myTerritories = new List<ITerritory>();
        public List<ITerritory> Territories
        {
            get { return myTerritories; }
        }
        //-----------------------------------------------------------
        public MapPath()
        {
        }
        public MapPath(String pathName)
        {
            myName = pathName;
        }
        public MapPath(IMapPath path)
        {
            myName = path.Name;
            myMetric = path.Metric;
            foreach (ITerritory t in path.Territories)
                myTerritories.Add(t);
        }
        //-----------------------------------------------------------
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Name);
            sb.Append("(");
            sb.Append(this.Metric.ToString());
            sb.Append(") PATH=");
            int count = 0;
            foreach (ITerritory t in Territories)
            {
                sb.Append(t.ToString());
                if (++count < Territories.Count)
                    sb.Append("->");
            }
            return sb.ToString();
        }
    }
    //---------------------------------------------------------------------------
    /// <summary>
    /// Summary description for MapPaths. Wrapper class around the 
    /// ArrayList of IMapPath objects representing a path on the map between
    /// two ITerrirtory locations.  The wrapper class provides
    /// type casting on objects in the ArrayList.  All objects in the
    /// ArrayList are of type "IMapPath".
    /// </summary>
    [Serializable]
    public class MapPaths : IEnumerable, IMapPaths
    {
        private ArrayList myList;
        public MapPaths() { myList = new ArrayList(); }
        public void Add(IMapPath path) { myList.Add(path); }
        public IMapPath RemoveAt(int index)
        {
            IMapPath path = (IMapPath)myList[index];
            myList.RemoveAt(index);
            return path;
        }
        public void Insert(int index, IMapPath path) { myList.Insert(index, path); }
        public int Count { get { return myList.Count; } }
        public void Clear() { myList.Clear(); }
        public bool Contains(IMapPath path) { return myList.Contains(path); }
        public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
        public int IndexOf(IMapPath path) { return myList.IndexOf(path); }
        public void Remove(IMapPath path) { myList.Remove(path); }
        public IMapPath Find(IMapPath pathToMatch)
        {
            foreach (Object o in myList)
            {
                IMapPath path = (IMapPath)o;
                if (path.Name == pathToMatch.Name)
                    return path;
            }
            return null;
        }
        public IMapPath Find(string pathName)
        {
            foreach (Object o in myList)
            {
                IMapPath path = (IMapPath)o;
                if (path.Name == pathName)
                    return path;
            }
            return null;
        }
        public IMapPath Remove(string pathName)
        {
            foreach (Object o in myList)
            {
                IMapPath path = (IMapPath)o;
                if (path.Name == pathName)
                {
                    Remove(path);
                    return path;
                }
            }
            return null;
        }
        public IMapPath this[int index]
        {
            get { return (IMapPath)(myList[index]); }
            set { myList[index] = value; }
        }
    }
}
