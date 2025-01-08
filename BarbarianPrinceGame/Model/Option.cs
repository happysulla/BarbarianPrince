using System;
using System.Collections;
using System.Text;
using System.Xml.Serialization;
using static System.Windows.Forms.Design.AxImporter;

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
      public static void LogGameType(String caller, Options options)
      {
         if (true == Logger.theLogLevel[(int)LogEnum.LE_GAME_INIT])
         {
            StringBuilder sb = new StringBuilder();
            sb.Append(caller + "(): >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            //-------------------------
            string name = "OriginalGame";
            sb.Append(" " + name + "=");
            Option option = options.Find(name);
            if (null == option)
               option = new Option(name, false);
            sb.Append(option.IsEnabled.ToString());
            //-------------------------
            name = "RandomPartyGame";
            sb.Append(" " + name + "=");
            option = options.Find(name);
            if (null == option)
               option = new Option(name, false);
            sb.Append(option.IsEnabled.ToString());
            //-------------------------
            name = "RandomHexGame";
            sb.Append(" " + name + "=");
            option = options.Find(name);
            if (null == option)
               option = new Option(name, false);
            sb.Append(option.IsEnabled.ToString());
            //-------------------------
            name = "RandomGame";
            sb.Append(" " + name + "=");
            option = options.Find(name);
            if (null == option)
               option = new Option(name, false);
            sb.Append(option.IsEnabled.ToString());
            //-------------------------
            name = "MaxFunGame";
            sb.Append(" " + name + "=");
            option = options.Find(name);
            if (null == option)
               option = new Option(name, false);
            sb.Append(option.IsEnabled.ToString());
            //-------------------------
            name = "CustomGame";
            sb.Append(" " + name + "=");
            option = options.Find(name);
            if (null == option)
               option = new Option(name, false);
            sb.Append(option.IsEnabled.ToString());
            //-------------------------
            Logger.Log(LogEnum.LE_GAME_INIT, sb.ToString());
         }
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
         Option option = Find("OriginalGame");
         option.IsEnabled = true;
      }
      public void SelectFunGameOptions()
      {
         Option option = null;
         option = this.Find("MaxFunGame");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=MaxFunGame");
         else
            option.IsEnabled = true;
         option = this.Find("OriginalGame");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=OriginalGame");
         else
            option.IsEnabled = false;
         option = this.Find("RandomPartyGame");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=RandomPartyGame");
         else
            option.IsEnabled = false;
         option = this.Find("RandomHexGame");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=RandomHexGame");
         else
            option.IsEnabled = false;
         option = this.Find("RandomGame");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=RandomGame");
         else
            option.IsEnabled = false;
         option = this.Find("CustomGame");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): this.Find() for option=CustomGame");
         else
            option.IsEnabled = false;
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
      public int GetGameIndex()
      {
         string name = "CustomGame";
         Option option = this.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return 5;
         name = "MaxFunGame";
         option = this.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return 4;
         name = "RandomGame";
         option = this.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return 3;
         name = "RandomHexGame";
         option = this.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return 2;
         name = "RandomPartyGame";
         option = this.Find(name);
         if (null == option)
            option = new Option(name, false);
         if (true == option.IsEnabled)
            return 1;
         return 0;
      }
      public string GetGameName(int index)
      {
         string gameType = "";
         switch (index)
         {
            case 0:
               gameType = "Original Game";
               break;
            case 1:
               gameType = "Random Party Game";
               break;
            case 2:
               gameType = "Random Hex Game";
               break;
            case 3:
               gameType = "All Random Game";
               break;
            case 4:
               gameType = "Fun Game";
               break;
            case 5:
               gameType = "Custom Game";
               break;
            default:
               gameType = "Total";
               break;
         }
         return gameType;
      }
   }
}
