using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BarbarianPrince
{
   [Serializable]
   public enum ColorActionEnum
   {
      CAE_START,
      CAE_LOST,
      CAE_REST,
      CAE_JAIL,
      CAE_TRAVEL,
      CAE_TRAVEL_AIR,
      CAE_TRAVEL_RAFT,
      CAE_TRAVEL_DOWNRIVER,
      CAE_ESCAPE,
      CAE_FOLLOW,
      CAE_SEARCH,
      CAE_SEARCH_RUINS,
      CAE_SEEK_NEWS,
      CAE_HIRE,
      CAE_AUDIENCE,
      CAE_OFFERING
   };
   [Serializable]
   public class EnteredHex
   {
      public static int theId = 0;
      public bool CtorError { get; } = false;
      public string Identifer { get; set; } = "";
      public int Day { get; set; } = 0;
      public String HexName { get; set; } = "";
      public bool IsEncounter { get; set; } = false;
      public int Position { get; set; } = 0;         // postion in the hex - if 1+ elispes exist in same hex, they are offset by position
      public ColorActionEnum ColorAction { get; set; } = ColorActionEnum.CAE_LOST;
      public List<String> EventNames { get; set; } = new List<String>();
      public List<String> Party = new List<String>();
      //------------------------------------------------------------------------------------------------
      public EnteredHex(string identifier, int day, string hexName, bool isEncounter, int position, ColorActionEnum action, List<String> eventNames, List<String> partyNames)
      {
         Identifer = identifier;
         Day = day;
         HexName = hexName;
         IsEncounter = isEncounter;
         Position = position;
         ColorAction = action;
         EventNames = eventNames;
         Party = partyNames;
      }
      public EnteredHex(IGameInstance gi, ColorActionEnum colorAction)
      {
         if( null == gi.NewHex )
         {
            Logger.Log(LogEnum.LE_ERROR, "EnteredHex(): gi.NewHex=null");
            CtorError = true;
            return;
         }
         ++theId;
         Identifer = "Hex#" + theId.ToString();
         Day = gi.Days + 1;
         HexName = gi.NewHex.Name;
         EventNames.Add(gi.EventActive);
         ColorAction = colorAction;
         //-----------------------------------------------
         Position = 0;
         foreach (EnteredHex hex in gi.EnteredHexes.AsEnumerable().Reverse())
         {
            if (hex.HexName == gi.NewHex.Name)
            {
               Position = hex.Position + 1;
               break;
            }
         }
         //-----------------------------------------------
         const string pattern = @"\d+$"; // use regex to remove last numbers from mapitem name
         foreach (IMapItem mi in gi.PartyMembers)
         {
            if (true == mi.Name.Contains("Prince"))
               continue;
            string miName = Regex.Replace(mi.Name, pattern, "");
            Party.Add(miName);
         }
      }
   };
}
