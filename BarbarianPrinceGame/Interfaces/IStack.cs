using System;

namespace BarbarianPrince
{
    public interface IStack
    {
        ITerritory Territory { get; set; }
        IMapItems MapItems { get; set; }
        void Rotate();
        void Shuffle();
    }
    public interface IStacks : System.Collections.IEnumerable
    {
        int Count { get; }
        void Add(IStack stack);
        IStack RemoveAt(int index);
        void Insert(int index, IStack stack);
        void Clear();
        bool Contains(IStack stack);
        int IndexOf(IStack stack);
        void Remove(IStack stack);
        IStack Remove(IMapItem mi);
        IStack Find(ITerritory t);
        IStack Find(IMapItem mi);
        IStack Find(String name);
        IStack this[int index] { get; set; }
        IStacks Shuffle();
    }
}
