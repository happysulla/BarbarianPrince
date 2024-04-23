using System.Collections.Generic;

namespace BarbarianPrince
{
    public interface IMapPath
    {
        string Name { get; set; }
        double Metric { get; set; }
        List<ITerritory> Territories { get; }
    }

    public interface IMapPaths : System.Collections.IEnumerable
    {
        int Count { get; }
        void Add(IMapPath path);
        IMapPath RemoveAt(int index);
        void Insert(int index, IMapPath path);
        void Clear();
        bool Contains(IMapPath path);
        int IndexOf(IMapPath path);
        void Remove(IMapPath pathName);
        IMapPath Remove(string pathName);
        IMapPath Find(string pathName);
        IMapPath Find(IMapPath pathToMatch);
        IMapPath this[int index] { get; set; }
    }
}
