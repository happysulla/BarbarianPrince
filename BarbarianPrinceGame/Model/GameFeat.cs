using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbarianPrince
{
   public class GameFeats
   {
      public bool myIsVisitAllTowns;
      public bool myIsVisitAllCastles;
      public bool myIsVisitAllTemples;
      public bool myIs500GoldWin;              // Nominal 
      public bool myIsNobleAllyWin;            // E152NobleAlly
      public bool myIsBlessedWin;             // E044HighAltarBlessed
      public bool myIsStaffOfCommandWin;      // E212Temple - Roll 12+1 on e212
      public bool myIsRoyalHelmWin;            // Treasure Table -  Row C:6 - Row Ca:6
      public bool myIsHuldraDefeatedInBattle;  // e144j
      public bool myIsHuldraDesposedWin;       // e211g
      public bool myHydraTeethVictory;
      public bool myIsArchTraveled;
      public string[] myVisitedTowns;
      public string[] myVisitedTemples;
      public string[] myVisitedCastles;
   };
}
