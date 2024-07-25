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
      CAE_ESCAPE,
      CAE_FOLLOW,
      CAE_SEARCH,
      CAE_STRUCTURE
   };
   [Serializable]
   public class EnteredHex
   {
      private static int theId = 0;
      public string Identifer { get; set; } = "";
      public int Day { get; set; } = 0;
      public String HexName { get; set; } = "";
      public string EventName { get; set; } = "";
      public List<String> Party = new List<String>();  
      public List<String> Kills = new List<String>();
      public bool IsEncounter { get; set; } = false;
      public int Position { get; set; } = 0;
      public String PreviousHex { get; set; } = "";
      public ColorActionEnum ColorAction { get; set; } = ColorActionEnum.CAE_LOST;
      //------------------------------------------------------------------------------------------------
      public EnteredHex(IGameInstance gi, ColorActionEnum colorAction, bool isEncounter=false)
      {
         ++theId;
         Identifer = "Hex#" + theId.ToString();
         Day = gi.Days + 1;
         HexName = gi.NewHex.Name;
         EventName = gi.EventActive;
         ColorAction = colorAction;
         IsEncounter = isEncounter;
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
