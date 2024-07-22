using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BarbarianPrince
{
   [Serializable]
   public enum DirectionEnum
   {
      DE_START_HEX,
      DE_LEFT_TOP,
      DE_RIGHT_TOP,
      DE_LEFT_BOTTOM,
      DE_RIGHT_BOTTOM,
      DE_SAME_HEX
   };
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
      CAE_ENCOUNTER,
      CAE_ESCAPE,
      CAE_FOLLOW,
      CAE_SEARCH,
      CAE_STRUCTURE
   };
   [Serializable]
   public class EnteredHex
   {
      public String HexName { get; set; } = "";
      public string EventName { get; set; } = "";
      public List<String> Party = new List<String>();  
      public List<String> Kills = new List<String>();
      public bool IsEncounter { get; set; } = false;
      public int Position { get; set; } = 0;
      public String PreviousHex { get; set; } = "";
      public ColorActionEnum ColorAction { get; set; } = ColorActionEnum.CAE_LOST;
      public DirectionEnum Direction { get; set; } = DirectionEnum.DE_START_HEX;
      //------------------------------------------------------------------------------------------------
      public EnteredHex(IGameInstance gi, ColorActionEnum colorAction, bool isEncounter=false)
      {
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
         Direction = DirectionEnum.DE_START_HEX;
         if ( 0 < gi.EnteredHexes.Count )
         {
            String previousHex = gi.EnteredHexes.Last().HexName; // set the direction of the incoming line into the hex
            if (null != previousHex)
            {
               if (previousHex == HexName)
               {
                  Direction = DirectionEnum.DE_SAME_HEX;
               }
               else
               {
                  ITerritory prevT = Territory.theTerritories.Find(previousHex);
                  if (null == prevT)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "EnteredHex(): prevT=null for n=" + previousHex);
                     return;
                  }
                  if (prevT.CenterPoint.X < gi.NewHex.CenterPoint.X)
                  {
                     if (prevT.CenterPoint.Y < gi.NewHex.CenterPoint.Y)
                        Direction = DirectionEnum.DE_LEFT_TOP;
                     else
                        Direction = DirectionEnum.DE_LEFT_BOTTOM;
                  }
                  else
                  {
                     if (prevT.CenterPoint.Y < gi.NewHex.CenterPoint.Y)
                        Direction = DirectionEnum.DE_RIGHT_TOP;
                     else
                        Direction = DirectionEnum.DE_RIGHT_BOTTOM;
                  }
               }
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
