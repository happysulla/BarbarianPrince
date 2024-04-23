namespace BarbarianPrince
{
    public interface IOption
    {
        string Name { get; set; }
        bool IsEnabled { get; set; }
    }
    public interface IOptions : System.Collections.IEnumerable
    {
        int Count { get; }
        void Add(IOption o);
        IOption RemoveAt(int index);
        void Insert(int index, IOption o);
        void Clear();
        bool Contains(IOption o);
        int IndexOf(IOption o);
        IOption Find(string name);
        IOption this[int index] { get; set; }
    }
}
