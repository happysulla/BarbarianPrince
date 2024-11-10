using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbarianPrince
{

   [Serializable]
   public class GameFeat
   {
      public bool myIs500GoldWin;              // Nominal 
      public bool myIsNobleAllyWin;            // E152NobleAlly
      public bool myIsBlessedWin;              // E044HighAltarBlessed
      public bool myIsStaffOfCommandWin;       // E212Temple - Roll 12+1 on e212
      public bool myIsRoyalHelmWin;            // Treasure Table -  Row C:6 - Row Ca:6
      public bool myIsHuldraDefeatedInBattle;  // e144j
      public bool myIsHuldraDesposedWin;       // e211g
      public bool myIsLostOnTime;              // lose game on time
      //-------------------------------------
      public bool myIsAirTravel;
      public bool myIsRaftTravel;
      public bool myIsArchTravel;
      //-------------------------------------
      public bool myIsMinstelAdded;
      public bool myIsEagleAdded;
      public bool myIsFalconAdded;
      public bool myIsMerchantAdded;
      //-------------------------------------
      public bool myIsHydraTeethUsed;          // use hydra teeth
      public bool myIsRescueHier;              // Rescue Huldra Heir from Hill Tribe
      public bool myIsSneakAttack;             // Perform sneak attack on Huldra
      public bool myIsStealGems;               // Steal Gems from Dragot using Foulbane
      //-------------------------------------
      public bool myIsDragonKiller;            // kill a dragon
      public bool myIsBanditKiller;            // kill 20 bandits
      public int myNumBanditKill;
      public bool myIsOrcKiller;               // kill 30 orcs
      public int myNumOrcKill;
      public bool myIsGoblinKiller;            // kill 40 goblins
      public int myNumGoblinKill;
      public bool myIsWolfKiller;              // kill 50 wolves
      public int myNumWolfKill;
      //-------------------------------------
      public List<String> myVisitedTowns = new List<String>();
      public List<String> myVisitedTemples = new List<String>();
      public List<String> myVisitedCastles = new List<String>();
      public List<String> myVisitedRuins = new List<String>();
      public List<String> myVisitedOasises = new List<String>();
      public bool myIsVisitAllTowns;
      public bool myIsVisitAllCastles;
      public bool myIsVisitAllTemples;
      public bool myIsVisitAllRuins;
      public bool myIsVisitAllOasis;
      //-------------------------------------
      public bool myIsPurchaseFoulbane;        // Purchase foulbane
      public bool myIsPurchaseChaga;           // Purchase Chaga Drug from temple
      //-------------------------------------
      public static string IsFeatChange()
      {
         GameFeat starting = GameEngine.theFeatsInGameStarting;
         GameFeat current = GameEngine.theFeatsInGame;
         if ( starting.myIs500GoldWin != current.myIs500GoldWin )
         {
            current.myIs500GoldWin = starting.myIs500GoldWin;
            return "Win with 500 gp";
         }
         if (starting.myIsNobleAllyWin != current.myIsNobleAllyWin)
         {
            current.myIsNobleAllyWin = starting.myIsNobleAllyWin;
            return "Win with noble ally";
         }
         if (starting.myIsBlessedWin != current.myIsBlessedWin)
         {
            current.myIsBlessedWin = starting.myIsBlessedWin;
            return "Win being blessed by the gods";
         }
         if (starting.myIsStaffOfCommandWin != current.myIsStaffOfCommandWin)
         {
            current.myIsStaffOfCommandWin = starting.myIsStaffOfCommandWin;
            return "Win with the staff of command";
         }
         if (starting.myIsRoyalHelmWin != current.myIsRoyalHelmWin)
         {
            current.myIsStaffOfCommandWin = starting.myIsStaffOfCommandWin;
            return "Win with the Royal Helm ";
         }
         if (starting.myIsHuldraDefeatedInBattle != current.myIsHuldraDefeatedInBattle)
         {
            current.myIsHuldraDefeatedInBattle = starting.myIsHuldraDefeatedInBattle;
            return "Win defeating Huldra in battle";
         }
         if (starting.myIsLostOnTime != current.myIsLostOnTime)
         {
            current.myIsLostOnTime = starting.myIsLostOnTime;
            return "Lost due to time expiring";
         }
         //--------------------------------------
         if (starting.myIsAirTravel != current.myIsAirTravel)
         {
            current.myIsAirTravel = starting.myIsAirTravel;
            return "Traveled by air";
         }
         if (starting.myIsRaftTravel != current.myIsRaftTravel)
         {
            current.myIsRaftTravel = starting.myIsRaftTravel;
            return "Traveled by raft";
         }
         if (starting.myIsArchTravel != current.myIsArchTravel)
         {
            current.myIsArchTravel = starting.myIsArchTravel;
            return "Traveled by arch";
         }
         //--------------------------------------
         if (starting.myIsMinstelAdded != current.myIsMinstelAdded)
         {
            current.myIsMinstelAdded = starting.myIsMinstelAdded;
            return "Minstrel joins your party";
         }
         if (starting.myIsEagleAdded != current.myIsEagleAdded)
         {
            current.myIsEagleAdded = starting.myIsEagleAdded;
            return "Eagle joins your party";
         }
         if (starting.myIsFalconAdded != current.myIsFalconAdded)
         {
            current.myIsFalconAdded = starting.myIsFalconAdded;
            return "Falcon joins your party";
         }
         if (starting.myIsMerchantAdded != current.myIsMerchantAdded)
         {
            current.myIsMerchantAdded = starting.myIsMerchantAdded;
            return "Merchant joins your party";
         }
         //--------------------------------------
         if (starting.myIsDragonKiller != current.myIsDragonKiller)
         {
            current.myIsDragonKiller = starting.myIsDragonKiller;
            return "Killed a dragon";
         }
         if (starting.myIsBanditKiller != current.myIsBanditKiller)
         {
            current.myIsBanditKiller = starting.myIsBanditKiller;
            return "Killed 20 bandits";
         }
         if (starting.myIsOrcKiller != current.myIsOrcKiller)
         {
            current.myIsOrcKiller = starting.myIsOrcKiller;
            return "Killed 30 orcs";
         }
         if (starting.myIsGoblinKiller != current.myIsGoblinKiller)
         {
            current.myIsGoblinKiller = starting.myIsGoblinKiller;
            return "Killed 40 goblins";
         }
         if (starting.myIsWolfKiller != current.myIsWolfKiller)
         {
            current.myIsWolfKiller = starting.myIsWolfKiller;
            return "Killed 50 wolves";
         }
         //--------------------------------------
         if (starting.myIsVisitAllTowns != current.myIsVisitAllTowns)
         {
            current.myIsVisitAllTowns = starting.myIsVisitAllTowns;
            return "Visited all towns";
         }
         if (starting.myIsVisitAllCastles != current.myIsVisitAllCastles)
         {
            current.myIsVisitAllCastles = starting.myIsVisitAllCastles;
            return "Visited all castles";
         }
         if (starting.myIsVisitAllTemples != current.myIsVisitAllTemples)
         {
            current.myIsVisitAllTemples = starting.myIsVisitAllTemples;
            return "Visited all temples";
         }
         if (starting.myIsVisitAllRuins != current.myIsVisitAllRuins)
         {
            current.myIsVisitAllRuins = starting.myIsVisitAllRuins;
            return "Visited all ruins";
         }
         if (starting.myIsVisitAllOasis != current.myIsVisitAllOasis)
         {
            current.myIsVisitAllOasis = starting.myIsVisitAllOasis;
            return "Visited all oasis";
         }
         //--------------------------------------
         if (starting.myIsHydraTeethUsed != current.myIsHydraTeethUsed)
         {
            current.myIsHydraTeethUsed = starting.myIsHydraTeethUsed;
            return "Used Hydra teeth in battle";
         }
         if (starting.myIsRescueHier != current.myIsRescueHier)
         {
            current.myIsRescueHier = starting.myIsRescueHier;
            return "Rescued true hier from hill tribe";
         }
         if (starting.myIsSneakAttack != current.myIsSneakAttack)
         {
            current.myIsSneakAttack = starting.myIsSneakAttack;
            return "Sneak attack on Count Dragot";
         }
         if (starting.myIsStealGems != current.myIsStealGems)
         {
            current.myIsStealGems = starting.myIsStealGems;
            return "Steal Count Dragot's jewels";
         }
         if (starting.myIsPurchaseFoulbane != current.myIsPurchaseFoulbane)
         {
            current.myIsPurchaseFoulbane = starting.myIsPurchaseFoulbane;
            return "Purchased Foulbane from ";
         }
         if (starting.myIsPurchaseChaga != current.myIsPurchaseChaga)
         {
            current.myIsPurchaseChaga = starting.myIsPurchaseChaga;
            return "Purchased chaga drug from temple";
         }
         return "";
      }
   };

}
