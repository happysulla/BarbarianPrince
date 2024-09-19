namespace BarbarianPrince
{
    public interface IMapItemMove
    {
        IMapItem MapItem { get; set; }
        ITerritory OldTerritory { get; set; }
        ITerritory NewTerritory { get; set; }
        IMapPath BestPath { get; set; }
        RiverCrossEnum RiverCross { get; set; } 
    }
    public interface IMapItemMoves : System.Collections.IEnumerable
    {
        int Count { get; }
        void Add(IMapItemMove mim);
        IMapItemMove RemoveAt(int index);
        void Insert(int index, IMapItemMove mim);
        void Clear();
        bool Contains(IMapItemMove mim);
        int IndexOf(IMapItemMove mim);
        void Remove(IMapItemMove mim);
        IMapItemMove Remove(IMapItem mi);
        IMapItemMove Find(IMapItem mi);
        IMapItemMoves Shuffle();
        IMapItemMove this[int index] { get; set; }
    }
}
