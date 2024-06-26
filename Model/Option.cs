using System;
using System.Collections;

namespace BarbarianPrince
{
   [Serializable]
   public class Option
   {
      [NonSerialized] public const int MEMBER_COUNT = 14;
      [NonSerialized] public static string[] theStartingMembers = new string[MEMBER_COUNT] { "Dwarf", "Eagle", "Elf", "Falcon", "Griffon", "Harpy", "Magician", "Mercenary", "Merchant", "Minstrel", "Monk", "PorterSlave", "TrueLove", "Wizard" };
      public string Name { get; set; }
      public bool IsEnabled { get; set; }
      public Option(string name, bool isEnabled)
      {
         Name = name;
         IsEnabled = isEnabled;
      }
   }
   [Serializable]
   public class Options : IEnumerable
   {
      private readonly ArrayList myList;
      public Options() { myList = new ArrayList(); }
      public int Count { get => myList.Count; }
      public void Add(Option o) { myList.Add(o); }
      public Option RemoveAt(int index)
      {
         Option option = (Option)myList[index];
         myList.RemoveAt(index);
         return option;
      }
      public void Insert(int index, Option o) { myList.Insert(index, o); }
      public void Clear() { myList.Clear(); }
      public bool Contains(Option o) { return myList.Contains(o); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(Option o) { return myList.IndexOf(o); }
      public Option Find(string name)
      {
         int i = 0;
         foreach (Object o in myList)
         {
            Option option = (Option)o;
            if (name == option.Name)
               return option;
            ++i;
         }
         return null;
      }
      public Option this[int index]
      {
         get { return (Option)(myList[index]); }
         set { myList[index] = value; }
      }
      public Options GetDeepCopy()
      {
         Options copy = new Options();
         foreach (Object o in myList)
         {
            Option option = (Option)o;
            Option copyO = new Option(option.Name, option.IsEnabled);
            copy.Add(copyO);
         }
         return copy;
      }
   }
}
