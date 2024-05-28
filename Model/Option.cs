using System;
using System.Collections;

namespace BarbarianPrince
{
   [Serializable]
   public class Option : IOption
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public Option(string name, bool isEnabled)
        {
            Name = name;
            IsEnabled = isEnabled;
        }
    }
   [Serializable]
   public class Options : IEnumerable, IOptions
    {
        private readonly ArrayList myList;
        public Options() { myList = new ArrayList(); }
        public int Count { get => myList.Count; }
        public void Add(IOption o) { myList.Add(o); }
        public IOption RemoveAt(int index)
        {
            IOption option = (IOption)myList[index];
            myList.RemoveAt(index);
            return option;
        }
        public void Insert(int index, IOption o) { myList.Insert(index, o); }
        public void Clear() { myList.Clear(); }
        public bool Contains(IOption o) { return myList.Contains(o); }
        public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
        public int IndexOf(IOption o) { return myList.IndexOf(o); }
        public IOption Find(string name)
        {
            int i = 0;
            foreach (Object o in myList)
            {
                IOption option = (IOption)o;
                if (name == option.Name)
                    return option;
                ++i;
            }
            return null;
        }
        public IOption this[int index]
        {
            get { return (IOption)(myList[index]); }
            set { myList[index] = value; }
        }
    }
}
