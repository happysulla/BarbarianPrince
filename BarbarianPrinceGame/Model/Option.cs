using System;
using System.Collections;
using System.Xml.Serialization;

namespace BarbarianPrince
{
   [Serializable]
   public class Option
   {
      public string Name { get; set; }
      public bool IsEnabled { get; set; }
      public Option()
      {
      }
      public Option(string name, bool isEnabled)
      {
         Name = name;
         IsEnabled = isEnabled;
      }
   }
   [XmlInclude(typeof(Option))]
   [Serializable]
   public class Options : IEnumerable
   {
      [NonSerialized] public const int MEMBER_COUNT = 16;
      [NonSerialized]
      public static string[] theDefaults = new string[81] // first 16 entries must be persons
      {
         "Dwarf",
         "Eagle",
         "Elf",
         "ElfWarrior",
         "Falcon",
         "Griffon",
         "Harpy",
         "Magician",
         "Mercenary",
         "Merchant",
         "Minstrel",
         "Monk",
         "PorterSlave",
         "Priest",
         "TrueLove",
         "Wizard",
         "AutoSetup",
         "AutoWealthRollForUnderFive",
         "PrinceHorse",
         "PrincePegasus",
         "PrinceCoin",
         "PrinceFood",
         "StartWithNerveGame",
         "StartWithNecklass",
         "RandomHex",
         "RandomParty10",
         "RandomParty08",
         "RandomParty05",
         "RandomParty03",
         "RandomParty01",
         "PartyCustom",
         "PartyMounted",
         "PartyAirborne",
         "RandomHex",
         "RandomTown",
         "RandomLeft",
         "RandomRight",
         "RandomBottom",
         "0109",
         "0206",
         "0708",
         "0711",
         "1212",
         "0323",
         "1923",
         "0418",
         "0722",
         "0409",
         "0406",
         "1611",
         "0411",
         "1507",
         "1905",
         "1723",
         "EasiestMonsters",
         "EasyMonsters",
         "LessHardMonsters",
         "AutoLostDecrease",
         "ExtendEndTime",
         "ReduceLodgingCosts",
         "SteadyIncome",
         "EasyRoute",
         "NoLostRoll",
         "ForceNoLostEvent",
         "ForceLostEvent",
         "ForceNoEvent",
         "ForceEvent",
         "ForceNoRoadEvent",
         "ForceNoAirEvent",
         "ForceAirEvent",
         "ForceNoCrossEvent",
         "ForceCrossEvent",
         "ForceLostAfterCrossEvent",
         "ForceNoRaftEvent",
         "ForceRaftEvent",
         "OriginalGame",
         "RandomPartyGame",
         "RandomHexGame",
         "RandomGame",
         "MaxFunGame",
         "CustomGame"
      };
      private readonly ArrayList myList;
      public Options() { myList = new ArrayList(); }
      public int Count { get => myList.Count; }
      public void Add(Option o) { myList.Add(o); }
      public void Add(System.Object o) { myList.Add(o); }
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
      public Options Clone()
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
      public void SetOriginalGameOptions()
      {
         Clear();
         foreach (string s in theDefaults)
            this.Add(new Option(s, false));
      }
      public void SelectFunGameOptions()
      {
         Option option = null;
         option = this.Find("MaxFunGame");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=MaxFunGame");
         else
            option.IsEnabled = true;
         option = this.Find("AutoWealthRollForUnderFive");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=AutoWealthRollForUnderFive");
         else
            option.IsEnabled = true;
         option = this.Find("AutoLostDecrease");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=AutoLostDecrease");
         else
            option.IsEnabled = true;
         option = this.Find("ExtendEndTime");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=ExtendEndTime");
         else
            option.IsEnabled = true;
         option = this.Find("ReduceLodgingCosts");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=ReduceLodgingCosts");
         else
            option.IsEnabled = true;
         option = this.Find("SteadyIncome");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=SteadyIncome");
         else
            option.IsEnabled = true;
         option = this.Find("EasyRoute");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=EasyRoute");
         else
            option.IsEnabled = true;
         option = this.Find("EasyMonsters");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=EasyMonsters");
         else
            option.IsEnabled = true;
         option = this.Find("PrinceFood");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=PrinceFood");
         else
            option.IsEnabled = true;
         option = this.Find("PrinceCoin");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=PrinceCoin");
         else
            option.IsEnabled = true;
         option = this.Find("StartWithNerveGame");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=StartWithNerveGame");
         else
            option.IsEnabled = true;
         option = this.Find("StartWithNecklass");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=StartWithNecklass");
         else
            option.IsEnabled = true;
         option = this.Find("RandomParty05");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=RandomParty05");
         else
            option.IsEnabled = true;
         option = this.Find("RandomHex");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=RandomHex");
         else
            option.IsEnabled = true;
      }
   }
}
