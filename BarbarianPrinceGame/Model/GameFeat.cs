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
      public GameFeat()
      {
         myIs500GoldWin = false;
         myIsNobleAllyWin = false;
         myIsBlessedWin = false;
         myIsStaffOfCommandWin = false;
         myIsRoyalHelmWin = false;
         myIsHuldraDefeatedInBattle = false;
         myIsHuldraDesposedWin = false;
         myIsLostOnTime = false;
         //-------------------------------------
         myIsAirTravel = false;
         myIsRaftTravel = false;
         myIsArchTravel = false;
         //-------------------------------------
         myIsMinstelAdded = false;
         myIsEagleAdded = false;
         myIsFalconAdded = false;
         myIsMerchantAdded = false;
         //-------------------------------------
         myIsHydraTeethUsed = false;
         myIsRescueHier = false;
         myIsSneakAttack = false;
         myIsStealGems = false;
         //-------------------------------------
         myIsDragonKiller = false;
         myIsBanditKiller = false;
         myNumBanditKill = 0;
         myIsOrcKiller = false;
         myNumOrcKill = 0;
         myIsGoblinKiller = false;
         myNumGoblinKill = 0;
         myIsWolfKiller = false;
         myNumWolfKill = 0;
         //-------------------------------------
         myVisitedTowns = new List<String>();
         myVisitedTemples = new List<String>();
         myVisitedCastles = new List<String>();
         myVisitedRuins = new List<String>();
         myVisitedOasises = new List<String>();
         myIsVisitAllTowns = false;
         myIsVisitAllCastles = false;
         myIsVisitAllTemples = false;
         myIsVisitAllRuins = false;
         myIsVisitAllOasis = false;
         //-------------------------------------
         myIsPurchaseFoulbane = false;
         myIsPurchaseChaga = false;
      }
      public GameFeat Clone()
      {
         GameFeat starting = new GameFeat();
         starting.myIs500GoldWin = this.myIs500GoldWin;
         starting.myIsNobleAllyWin = this.myIsNobleAllyWin;
         starting.myIsBlessedWin = this.myIsBlessedWin;
         starting.myIsStaffOfCommandWin = this.myIsStaffOfCommandWin;
         starting.myIsStaffOfCommandWin = this.myIsStaffOfCommandWin;
         starting.myIsHuldraDefeatedInBattle = this.myIsHuldraDefeatedInBattle;
         starting.myIsLostOnTime = this.myIsLostOnTime;
         starting.myIsAirTravel = this.myIsAirTravel;
         starting.myIsRaftTravel = this.myIsRaftTravel;
         starting.myIsArchTravel = this.myIsArchTravel;
         starting.myIsMinstelAdded = this.myIsMinstelAdded;
         starting.myIsEagleAdded = this.myIsEagleAdded;
         starting.myIsFalconAdded = this.myIsFalconAdded;
         starting.myIsMerchantAdded = this.myIsMerchantAdded;
         starting.myIsDragonKiller = this.myIsDragonKiller;
         starting.myIsBanditKiller = this.myIsBanditKiller;
         starting.myIsOrcKiller = this.myIsOrcKiller;
         starting.myIsGoblinKiller = this.myIsGoblinKiller;
         starting.myIsWolfKiller = this.myIsWolfKiller;
         starting.myIsVisitAllTowns = this.myIsVisitAllTowns;
         starting.myIsVisitAllCastles = this.myIsVisitAllCastles;
         starting.myIsVisitAllTemples = this.myIsVisitAllTemples;
         starting.myIsVisitAllRuins = this.myIsVisitAllRuins;
         starting.myIsVisitAllOasis = this.myIsVisitAllOasis;
         starting.myIsHydraTeethUsed = this.myIsHydraTeethUsed;
         starting.myIsRescueHier = this.myIsRescueHier;
         starting.myIsSneakAttack = this.myIsSneakAttack;
         starting.myIsStealGems = this.myIsStealGems;
         starting.myIsPurchaseFoulbane = this.myIsPurchaseFoulbane;
         starting.myIsPurchaseChaga = this.myIsPurchaseChaga;
         foreach(string s in this.myVisitedTowns)
            starting.myVisitedTowns.Add(s);
         foreach (string s in this.myVisitedTemples)
            starting.myVisitedTemples.Add(s);
         foreach (string s in this.myVisitedCastles)
            starting.myVisitedCastles.Add(s);
         foreach (string s in this.myVisitedRuins)
            starting.myVisitedRuins.Add(s);
         foreach (string s in this.myVisitedOasises)
            starting.myVisitedOasises.Add(s);
         return starting;
      }
      public bool IsEqual(GameFeat starting)
      {
         if (this.myIs500GoldWin != starting.myIs500GoldWin)
            return false;
         if (this.myIsNobleAllyWin != starting.myIsNobleAllyWin)
            return false;
         if (this.myIsBlessedWin != starting.myIsBlessedWin)
            return false;
         if (this.myIsStaffOfCommandWin != starting.myIsStaffOfCommandWin)
            return false;
         if (this.myIsRoyalHelmWin != starting.myIsRoyalHelmWin)
            return false;
         if (this.myIsHuldraDefeatedInBattle != starting.myIsHuldraDefeatedInBattle)
            return false;
         if (this.myIsLostOnTime != starting.myIsLostOnTime)
            return false;
         //--------------------------------------
         if (this.myIsAirTravel != starting.myIsAirTravel)
            return false;
         if (this.myIsRaftTravel != starting.myIsRaftTravel)
            return false;
         if (this.myIsArchTravel != starting.myIsArchTravel)
            return false;
         //--------------------------------------
         if (this.myIsMinstelAdded != starting.myIsMinstelAdded)
            return false;
         if (this.myIsEagleAdded != starting.myIsEagleAdded)
            return false;
         if (this.myIsFalconAdded != starting.myIsFalconAdded)
            return false;
         if (this.myIsMerchantAdded != starting.myIsMerchantAdded)
            return false;
         //--------------------------------------
         if (this.myIsDragonKiller != starting.myIsDragonKiller)
            return false;
         if (this.myIsBanditKiller != starting.myIsBanditKiller)
            return false;
         if (this.myIsOrcKiller != starting.myIsOrcKiller)
            return false;
         if (this.myIsGoblinKiller != starting.myIsGoblinKiller)
            return false;
         if (this.myIsWolfKiller != starting.myIsWolfKiller)
            return false;
         //--------------------------------------
         if (this.myIsVisitAllTowns != starting.myIsVisitAllTowns)
            return false;
         if (this.myIsVisitAllCastles != starting.myIsVisitAllCastles)
            return false;
         if (this.myIsVisitAllTemples != starting.myIsVisitAllTemples)
            return false;
         if (this.myIsVisitAllRuins != starting.myIsVisitAllRuins)
            return false;
         if (this.myIsVisitAllOasis != starting.myIsVisitAllOasis)
            return false;
         //--------------------------------------
         if (this.myIsHydraTeethUsed != starting.myIsHydraTeethUsed)
            return false;
         if (this.myIsRescueHier != starting.myIsRescueHier)
            return false;
         if (this.myIsSneakAttack != starting.myIsSneakAttack)
            return false;
         if (this.myIsStealGems != starting.myIsStealGems)
            return false;
         if (this.myIsPurchaseFoulbane != starting.myIsPurchaseFoulbane)
            return false;
         if (this.myIsPurchaseChaga != starting.myIsPurchaseChaga)
            return false;
         return true;
      }
      public string GetFeatChange(GameFeat starting)
      {
         if ( starting.myIs500GoldWin != this.myIs500GoldWin )
         {
            starting.myIs500GoldWin = this.myIs500GoldWin;
            return "Win with 500 gp";
         }
         if (starting.myIsNobleAllyWin != this.myIsNobleAllyWin)
         {
            starting.myIsNobleAllyWin = this.myIsNobleAllyWin;
            return "Win with noble ally";
         }
         if (starting.myIsBlessedWin != this.myIsBlessedWin)
         {
            starting.myIsBlessedWin = this.myIsBlessedWin;
            return "Win being blessed by the gods";
         }
         if (starting.myIsStaffOfCommandWin != this.myIsStaffOfCommandWin)
         {
            starting.myIsStaffOfCommandWin = this.myIsStaffOfCommandWin;
            return "Win with the staff of command";
         }
         if (starting.myIsRoyalHelmWin != this.myIsRoyalHelmWin)
         {
            starting.myIsStaffOfCommandWin = this.myIsStaffOfCommandWin;
            return "Win with the Royal Helm ";
         }
         if (starting.myIsHuldraDefeatedInBattle != this.myIsHuldraDefeatedInBattle)
         {
            starting.myIsHuldraDefeatedInBattle = this.myIsHuldraDefeatedInBattle;
            return "Win defeating Huldra in battle";
         }
         if (starting.myIsLostOnTime != this.myIsLostOnTime)
         {
            starting.myIsLostOnTime = this.myIsLostOnTime;
            return "Lost due to time expiring";
         }
         //--------------------------------------
         if (starting.myIsAirTravel != this.myIsAirTravel)
         {
            starting.myIsAirTravel = this.myIsAirTravel;
            return "Traveled by air";
         }
         if (starting.myIsRaftTravel != this.myIsRaftTravel)
         {
            starting.myIsRaftTravel = this.myIsRaftTravel;
            return "Traveled by raft";
         }
         if (starting.myIsArchTravel != this.myIsArchTravel)
         {
            starting.myIsArchTravel = this.myIsArchTravel;
            return "Traveled by arch";
         }
         //--------------------------------------
         if (starting.myIsMinstelAdded != this.myIsMinstelAdded)
         {
            starting.myIsMinstelAdded = this.myIsMinstelAdded;
            return "Minstrel joins your party";
         }
         if (starting.myIsEagleAdded != this.myIsEagleAdded)
         {
            starting.myIsEagleAdded = this.myIsEagleAdded;
            return "Eagle joins your party";
         }
         if (starting.myIsFalconAdded != this.myIsFalconAdded)
         {
            starting.myIsFalconAdded = this.myIsFalconAdded;
            return "Falcon joins your party";
         }
         if (starting.myIsMerchantAdded != this.myIsMerchantAdded)
         {
            starting.myIsMerchantAdded = this.myIsMerchantAdded;
            return "Merchant joins your party";
         }
         //--------------------------------------
         if (starting.myIsDragonKiller != this.myIsDragonKiller)
         {
            starting.myIsDragonKiller = this.myIsDragonKiller;
            return "Killed a dragon";
         }
         if (starting.myIsBanditKiller != this.myIsBanditKiller)
         {
            starting.myIsBanditKiller = this.myIsBanditKiller;
            return "Killed 20 bandits";
         }
         if (starting.myIsOrcKiller != this.myIsOrcKiller)
         {
            starting.myIsOrcKiller = this.myIsOrcKiller;
            return "Killed 30 orcs";
         }
         if (starting.myIsGoblinKiller != this.myIsGoblinKiller)
         {
            starting.myIsGoblinKiller = this.myIsGoblinKiller;
            return "Killed 40 goblins";
         }
         if (starting.myIsWolfKiller != this.myIsWolfKiller)
         {
            starting.myIsWolfKiller = this.myIsWolfKiller;
            return "Killed 50 wolves";
         }
         //--------------------------------------
         if (starting.myIsVisitAllTowns != this.myIsVisitAllTowns)
         {
            starting.myIsVisitAllTowns = this.myIsVisitAllTowns;
            return "Visited all towns";
         }
         if (starting.myIsVisitAllCastles != this.myIsVisitAllCastles)
         {
            starting.myIsVisitAllCastles = this.myIsVisitAllCastles;
            return "Visited all castles";
         }
         if (starting.myIsVisitAllTemples != this.myIsVisitAllTemples)
         {
            starting.myIsVisitAllTemples = this.myIsVisitAllTemples;
            return "Visited all temples";
         }
         if (starting.myIsVisitAllRuins != this.myIsVisitAllRuins)
         {
            starting.myIsVisitAllRuins = this.myIsVisitAllRuins;
            return "Visited all ruins";
         }
         if (starting.myIsVisitAllOasis != this.myIsVisitAllOasis)
         {
            starting.myIsVisitAllOasis = this.myIsVisitAllOasis;
            return "Visited all oasis";
         }
         //--------------------------------------
         if (starting.myIsHydraTeethUsed != this.myIsHydraTeethUsed)
         {
            starting.myIsHydraTeethUsed = this.myIsHydraTeethUsed;
            return "Used Hydra teeth in battle";
         }
         if (starting.myIsRescueHier != this.myIsRescueHier)
         {
            starting.myIsRescueHier = this.myIsRescueHier;
            return "Rescued true hier from hill tribe";
         }
         if (starting.myIsSneakAttack != this.myIsSneakAttack)
         {
            starting.myIsSneakAttack = this.myIsSneakAttack;
            return "Sneak attack on Count Dragot";
         }
         if (starting.myIsStealGems != this.myIsStealGems)
         {
            starting.myIsStealGems = this.myIsStealGems;
            return "Steal Count Dragot's jewels";
         }
         if (starting.myIsPurchaseFoulbane != this.myIsPurchaseFoulbane)
         {
            starting.myIsPurchaseFoulbane = this.myIsPurchaseFoulbane;
            return "Purchased Foulbane";
         }
         if (starting.myIsPurchaseChaga != this.myIsPurchaseChaga)
         {
            starting.myIsPurchaseChaga = this.myIsPurchaseChaga;
            return "Purchased chaga drug from temple";
         }
         return "";
      }
   };

}
