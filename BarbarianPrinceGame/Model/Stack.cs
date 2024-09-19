using System;
using System.Collections;
using System.Text;

namespace BarbarianPrince
{
    //-------------------------------------------------------------------
    [Serializable]
    public class Stack : IStack
    {
        public ITerritory Territory { get; set; } = null;
        public IMapItems MapItems { get; set; } = new MapItems();
        public Stack(ITerritory t)
        {
            Territory = t;
        }
        public void Rotate() { MapItems.Rotate(1); }
        public void Shuffle() { MapItems = MapItems.Shuffle(); }
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder("");
            sb.Append(Territory.ToString());
            sb.Append("==>");
            foreach (IMapItem mi in MapItems)
            {
                sb.Append(mi.Name);
                sb.Append(" ");
            }
            return sb.ToString();
        }
    }
    //-------------------------------------------------------------------
    [Serializable]
    public class Stacks : IStacks
    {
        private readonly ArrayList myList;
        public Stacks() { myList = new ArrayList(); }
        public void Add(IStack stack) { myList.Add(stack); }
        public IStack RemoveAt(int index)
        {
            IStack stack = (IStack)myList[index];
            myList.RemoveAt(index);
            return stack;
        }
        public void Insert(int index, IStack stack) { myList.Insert(index, stack); }
        public int Count { get { return myList.Count; } }
        public void Clear() { myList.Clear(); }
        public bool Contains(IStack stack) { return myList.Contains(stack); }
        public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
        public int IndexOf(IStack stack) { return myList.IndexOf(stack); }
        public void Remove(IStack stack) { myList.Remove(stack); }
        public IStack Find(ITerritory t)
        {
            string territoryName = Utilities.RemoveSpaces(t.Name);
            foreach (Object o in myList)
            {
                IStack stack = (IStack)o;
                if (territoryName == Utilities.RemoveSpaces(stack.Territory.Name))
                    return stack;
            }
            return null;
        }
        public IStack Find(IMapItem mi)
        {
            foreach (Object o in myList)
            {
                IStack stack = (IStack)o;
                foreach (MapItem mapItem in stack.MapItems)
                {
                    if (mi.Name == mapItem.Name)
                        return stack;
                }
            }
            return null;
        }
        public IStack Find(String name)
        {
            foreach (Object o in myList)
            {
                IStack stack = (IStack)o;
                foreach (MapItem mapItem in stack.MapItems)
                {
                    if (Utilities.RemoveSpaces(mapItem.Name) == name)
                        return stack;
                }
            }
            return null;
        }
        public IStack Remove(IMapItem mi)
        {
            foreach (Object o in myList)
            {
                IStack stack = (IStack)o;
                foreach (MapItem mapItem in stack.MapItems)
                {
                    if (mi.Name == mapItem.Name)
                    {
                        stack.MapItems.Remove(mapItem);
                        return stack;
                    }
                }
            }
            return null;
        }
        public IStack this[int index]
        {
            get { return (IStack)(myList[index]); }
            set { myList[index] = value; }
        }
        public IStacks Shuffle()
        {
            IStacks newStacks = new Stacks();
            int count = myList.Count + 100;
            for (int i = 0; i < count; i++) // Randomly select object. Remove it and readd at top.
            {
                int index = Utilities.RandomGenerator.Next(myList.Count);
                if (index < myList.Count)
                {
                    IStack stack = (IStack)myList[index];
                    myList.RemoveAt(index);
                    stack.Shuffle();
                    newStacks.Add(stack);
                }
            }
            return newStacks;
        }
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder("stacks[");
            sb.Append(this.Count.ToString());
            sb.Append("]=");
            foreach (IStack stack in this)
            {
                sb.Append("{");
                sb.Append(stack.ToString());
                sb.Append("}");
            }
            return sb.ToString();
        }
    }
}
