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
      [NonSerialized] public const int LOCATIONS_TEMPLE = 5;
      [NonSerialized] public const int LOCATIONS_TOWN = 12;
      [NonSerialized] public const int LOCATIONS_CASTLE= 3;
      [NonSerialized] public const int LOCATIONS_RUIN= 3;
      [NonSerialized] public const int LOCATIONS_OASIS = 4;
      public static string theGameFeatDirectory = "";
      //----------------------------------------------------
      public bool myIsOriginalGameWin;         // Win the original game
      public bool myIsRandomPartyGameWin;
      public bool myIsRandomHexGameWin;
      public bool myIsRandomGameWin;
      public bool myIsFunGameWin;
      //-------------------------------------
      public bool myIsLowWitWin;               // Win game with Wit and Wiles equal 2 
      public bool myIs500GoldWin;              // Nominal 
      public bool myIsNobleAllyWin;            // E152NobleAlly
      public bool myIsBlessedWin;              // E044HighAltarBlessed
      public bool myIsStaffOfCommandWin;       // E212Temple - Roll 12+1 on e212
      public bool myIsRoyalHelmWin;            // Treasure Table -  Row C:6 - Row Ca:6
      public bool myIsHuldraDefeatedInBattleWin;  // e144j
      public bool myIsHuldraDesposedWin;       // e211g
      public bool myIsLostOnTime;              // lose game on time
      public bool myIsLostAxeDeath;            // lose game with execution
      //-------------------------------------
      public bool myIsAirTravel;
      public bool myIsRaftTravel;
      public bool myIsArchTravel;
      //-------------------------------------
      public bool myIsMinstelAdded;
      public bool myIsEagleAdded;
      public bool myIsFalconAdded;
      public bool myIsMerchantAdded;
      public bool myIsTrueLoveAdded;
      //-------------------------------------
      public bool myIsResistenceRingUsed;    // find a resistence ring
      public bool myIsHydraTeethUsed;          // use hydra teeth
      public bool myIsRescueHeir;              // Rescue Huldra Heir from Hill Tribe
      public bool myIsSneakAttack;             // Perform sneak attack on Huldra
      public bool myIsStealGems;               // Steal Gems from Dragot using Foulbane
      public bool myIsLadyAeravirAccused;      // Use secret knowledge to accuses Lady A of promiscuity
      //-------------------------------------
      public bool myIsDragonKiller;            // kill a dragon
      public bool myIsBanditKiller;         
      public int myNumBanditKill;
      public bool myIsOrcKiller;          
      public int myNumOrcKill;
      public bool myIsGoblinKiller;          
      public int myNumGoblinKill;
      public bool myIsWolfKiller;             
      public int myNumWolfKill;
      public bool myIsNightsInJail;           
      public int myNumNightsInJail;
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
         myIsOriginalGameWin = false;
         myIsRandomPartyGameWin = false;
         myIsRandomHexGameWin = false;
         myIsRandomGameWin = false;
         myIsFunGameWin = false;
         //-------------------------------------
         myIsLowWitWin = false;
         myIs500GoldWin = false;
         myIsNobleAllyWin = false;
         myIsBlessedWin = false;
         myIsStaffOfCommandWin = false;
         myIsRoyalHelmWin = false;
         myIsHuldraDefeatedInBattleWin = false;
         myIsHuldraDesposedWin = false;
         myIsLostOnTime = false;
         myIsLostAxeDeath = false;
         //-------------------------------------
         myIsAirTravel = false;
         myIsRaftTravel = false;
         myIsArchTravel = false;
         //-------------------------------------
         myIsMinstelAdded = false;
         myIsEagleAdded = false;
         myIsFalconAdded = false;
         myIsMerchantAdded = false;
         myIsTrueLoveAdded = false;
         //-------------------------------------
         myIsResistenceRingUsed = false;
         myIsHydraTeethUsed = false;
         myIsRescueHeir = false;
         myIsSneakAttack = false;
         myIsStealGems = false;
         myIsLadyAeravirAccused = false;
         //-------------------------------------
         myIsDragonKiller = false;
         myIsNightsInJail = false;
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
         starting.myIsOriginalGameWin = this.myIsOriginalGameWin;
         starting.myIsRandomPartyGameWin = this.myIsRandomPartyGameWin;
         starting.myIsRandomHexGameWin = this.myIsRandomHexGameWin;
         starting.myIsRandomGameWin = this.myIsRandomGameWin;
         starting.myIsFunGameWin = this.myIsFunGameWin;
         //-------------------------------------
         starting.myIsLowWitWin = this.myIsLowWitWin;
         starting.myIs500GoldWin = this.myIs500GoldWin;
         starting.myIsNobleAllyWin = this.myIsNobleAllyWin;
         starting.myIsBlessedWin = this.myIsBlessedWin;
         starting.myIsStaffOfCommandWin = this.myIsStaffOfCommandWin;
         starting.myIsRoyalHelmWin = this.myIsRoyalHelmWin;
         starting.myIsHuldraDefeatedInBattleWin = this.myIsHuldraDefeatedInBattleWin;
         starting.myIsHuldraDesposedWin = this.myIsHuldraDesposedWin;
         starting.myIsLostOnTime = this.myIsLostOnTime;
         starting.myIsLostAxeDeath = this.myIsLostAxeDeath;
         //-------------------------------------
         starting.myIsAirTravel = this.myIsAirTravel;
         starting.myIsRaftTravel = this.myIsRaftTravel;
         starting.myIsArchTravel = this.myIsArchTravel;
         //-------------------------------------
         starting.myIsMinstelAdded = this.myIsMinstelAdded;
         starting.myIsEagleAdded = this.myIsEagleAdded;
         starting.myIsFalconAdded = this.myIsFalconAdded;
         starting.myIsMerchantAdded = this.myIsMerchantAdded;
         starting.myIsTrueLoveAdded = this.myIsTrueLoveAdded;
         //-------------------------------------
         starting.myIsResistenceRingUsed = this.myIsResistenceRingUsed;
         starting.myIsHydraTeethUsed = this.myIsHydraTeethUsed;
         starting.myIsRescueHeir = this.myIsRescueHeir;
         starting.myIsSneakAttack = this.myIsSneakAttack;
         starting.myIsStealGems = this.myIsStealGems;
         starting.myIsLadyAeravirAccused = this.myIsLadyAeravirAccused;
         //-------------------------------------
         starting.myIsDragonKiller = this.myIsDragonKiller;
         starting.myIsBanditKiller = this.myIsBanditKiller;
         starting.myNumBanditKill = this.myNumBanditKill;
         starting.myIsOrcKiller = this.myIsOrcKiller;
         starting.myNumOrcKill = this.myNumOrcKill;
         starting.myIsGoblinKiller = this.myIsGoblinKiller;
         starting.myNumGoblinKill = this.myNumGoblinKill;
         starting.myIsWolfKiller = this.myIsWolfKiller;
         starting.myNumWolfKill = this.myNumWolfKill;
         starting.myIsNightsInJail = this.myIsNightsInJail;
         starting.myNumNightsInJail = this.myNumNightsInJail;
         //-------------------------------------
         starting.myIsVisitAllTowns = this.myIsVisitAllTowns;
         starting.myIsVisitAllCastles = this.myIsVisitAllCastles;
         starting.myIsVisitAllTemples = this.myIsVisitAllTemples;
         starting.myIsVisitAllRuins = this.myIsVisitAllRuins;
         starting.myIsVisitAllOasis = this.myIsVisitAllOasis;
         foreach (string s in this.myVisitedTowns)
            starting.myVisitedTowns.Add(s);
         foreach (string s in this.myVisitedTemples)
            starting.myVisitedTemples.Add(s);
         foreach (string s in this.myVisitedCastles)
            starting.myVisitedCastles.Add(s);
         foreach (string s in this.myVisitedRuins)
            starting.myVisitedRuins.Add(s);
         foreach (string s in this.myVisitedOasises)
            starting.myVisitedOasises.Add(s);
         //-------------------------------------
         starting.myIsPurchaseChaga = this.myIsPurchaseChaga;
         starting.myIsPurchaseFoulbane = this.myIsPurchaseFoulbane;
         return starting;
      }
      public bool IsEqual(GameFeat starting)
      {
         if (this.myIsOriginalGameWin != starting.myIsOriginalGameWin)
            return false;
         if (this.myIsRandomPartyGameWin != starting.myIsRandomPartyGameWin)
            return false;
         if (this.myIsRandomHexGameWin != starting.myIsRandomHexGameWin)
            return false;
         if (this.myIsRandomGameWin != starting.myIsRandomGameWin)
            return false;
         if (this.myIsFunGameWin != starting.myIsFunGameWin)
            return false;
         //--------------------------------------
         if (this.myIsLowWitWin != starting.myIsLowWitWin)
            return false;
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
         if (this.myIsHuldraDefeatedInBattleWin != starting.myIsHuldraDefeatedInBattleWin)
            return false;
         if (this.myIsHuldraDesposedWin != starting.myIsHuldraDesposedWin)
            return false;
         if (this.myIsLostOnTime != starting.myIsLostOnTime)
            return false;
         if (this.myIsLostAxeDeath != starting.myIsLostAxeDeath)
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
         if (this.myIsTrueLoveAdded != starting.myIsTrueLoveAdded)
            return false;
         //--------------------------------------
         if (this.myIsResistenceRingUsed != starting.myIsResistenceRingUsed)
            return false;
         if (this.myIsHydraTeethUsed != starting.myIsHydraTeethUsed)
            return false;
         if (this.myIsRescueHeir != starting.myIsRescueHeir)
            return false;
         if (this.myIsSneakAttack != starting.myIsSneakAttack)
            return false;
         if (this.myIsStealGems != starting.myIsStealGems)
            return false;
         if (this.myIsLadyAeravirAccused != starting.myIsLadyAeravirAccused)
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
         if (this.myIsNightsInJail != starting.myIsNightsInJail)
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
         if (this.myIsPurchaseFoulbane != starting.myIsPurchaseFoulbane)
            return false;
         if (this.myIsPurchaseChaga != starting.myIsPurchaseChaga)
            return false;
         return true;
      }
      public string GetFeatChange(GameFeat starting)
      {
         if (starting.myIsOriginalGameWin != this.myIsOriginalGameWin)
         {
            starting.myIsOriginalGameWin = this.myIsOriginalGameWin;
            return "Win the original game";
         }
         if (starting.myIsRandomPartyGameWin != this.myIsRandomPartyGameWin)
         {
            starting.myIsRandomPartyGameWin = this.myIsRandomPartyGameWin;
            return "Win the random starting party game";
         }
         if (starting.myIsRandomHexGameWin != this.myIsRandomHexGameWin)
         {
            starting.myIsRandomHexGameWin = this.myIsRandomHexGameWin;
            return "Win the random starting hex game";
         }
         if (starting.myIsRandomGameWin != this.myIsRandomGameWin)
         {
            starting.myIsRandomGameWin = this.myIsRandomGameWin;
            return "Win the random starting options game";
         }
         if (starting.myIsFunGameWin != this.myIsFunGameWin)
         {
            starting.myIsFunGameWin = this.myIsFunGameWin;
            return "Win the fun options game";
         }
         //--------------------------------------
         if (starting.myIsLowWitWin != this.myIsLowWitWin)
         {
            starting.myIsLowWitWin = this.myIsLowWitWin;
            return "Win with a Wit and Wiles equal to two";
         }
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
            starting.myIsRoyalHelmWin = this.myIsRoyalHelmWin;
            return "Win with the Royal Helm ";
         }
         if (starting.myIsHuldraDefeatedInBattleWin != this.myIsHuldraDefeatedInBattleWin)
         {
            starting.myIsHuldraDefeatedInBattleWin = this.myIsHuldraDefeatedInBattleWin;
            return "Win defeating Huldra in battle";
         }
         if (starting.myIsHuldraDesposedWin != this.myIsHuldraDesposedWin)
         {
            starting.myIsHuldraDesposedWin = this.myIsHuldraDesposedWin;
            return "Win desposing Huldra from throne during an audience";
         }
         if (starting.myIsLostOnTime != this.myIsLostOnTime)
         {
            starting.myIsLostOnTime = this.myIsLostOnTime;
            return "Lost due to time expiring";
         }
         if (starting.myIsLostAxeDeath != this.myIsLostAxeDeath)
         {
            starting.myIsLostAxeDeath = this.myIsLostAxeDeath;
            return "Lost due to head chopped off";
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
         if (starting.myIsTrueLoveAdded != this.myIsTrueLoveAdded)
         {
            starting.myIsTrueLoveAdded = this.myIsTrueLoveAdded;
            return "You found your true love";
         }
         //--------------------------------------
         if (starting.myIsResistenceRingUsed != this.myIsResistenceRingUsed)
         {
            starting.myIsResistenceRingUsed = this.myIsResistenceRingUsed;
            return "Use Resistence Ring in battle";
         }
         if (starting.myIsHydraTeethUsed != this.myIsHydraTeethUsed)
         {
            starting.myIsHydraTeethUsed = this.myIsHydraTeethUsed;
            return "Used Hydra Teeth in battle";
         }
         if (starting.myIsRescueHeir != this.myIsRescueHeir)
         {
            starting.myIsRescueHeir = this.myIsRescueHeir;
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
         if (starting.myIsLadyAeravirAccused != this.myIsLadyAeravirAccused)
         {
            starting.myIsLadyAeravirAccused = this.myIsLadyAeravirAccused;
            return "Accuse Lady Aeravir of promiscuous behavior"; // e145
         }
         //--------------------------------------
         if (starting.myIsDragonKiller != this.myIsDragonKiller)
         {
            starting.myIsDragonKiller = this.myIsDragonKiller;
            return "Killed a dragon";
         }
         if (starting.myIsBanditKiller != this.myIsBanditKiller)
         {
            string msg = "Killed " + this.myNumBanditKill + " bandits";
            this.myNumBanditKill++; // give one for free so game feat does not show up again
            starting.myIsBanditKiller = this.myIsBanditKiller;
            return msg;
         }
         if (starting.myIsOrcKiller != this.myIsOrcKiller)
         {
            string msg = "Killed " + this.myNumOrcKill + " orcs";
            this.myNumOrcKill++; // give one for free so game feat does not show up again
            starting.myIsOrcKiller = this.myIsOrcKiller;
            return msg;
         }
         if (starting.myIsGoblinKiller != this.myIsGoblinKiller)
         {
            string msg = "Killed " + this.myNumGoblinKill + " goblins";
            this.myNumGoblinKill++; // give one for free so game feat does not show up again
            starting.myIsGoblinKiller = this.myIsGoblinKiller;
            return msg;
         }
         if (starting.myIsWolfKiller != this.myIsWolfKiller)
         {
            string msg = "Killed " + this.myNumWolfKill + " wolves";
            this.myNumWolfKill++; // give one for free so game feat does not show up again
            starting.myIsWolfKiller = this.myIsWolfKiller;
            return msg;
         }
         if (starting.myIsNightsInJail != this.myIsNightsInJail)
         {
            string msg = "Spend " + this.myNumNightsInJail + " in jail";
            starting.myIsNightsInJail = this.myIsNightsInJail;
            this.myNumNightsInJail++; // give one for free so game feat does not show up again
            return msg;
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
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("{ ");
         sb.Append("wOrg=");
         sb.Append(myIsOriginalGameWin.ToString());
         sb.Append(",wWW=");
         sb.Append(myIsLowWitWin.ToString());
         sb.Append(", w500=");
         sb.Append(myIs500GoldWin.ToString());
         sb.Append(", wAlly=");
         sb.Append(myIsNobleAllyWin.ToString());
         sb.Append(", wBless=");
         sb.Append(myIsBlessedWin.ToString());
         sb.Append(", wStaff=");
         sb.Append(myIsStaffOfCommandWin.ToString());
         sb.Append(", wHelm=");
         sb.Append(myIsRoyalHelmWin.ToString());
         sb.Append(", wHuldra1=");
         sb.Append(myIsHuldraDefeatedInBattleWin.ToString());
         sb.Append(", wHuldra2=");
         sb.Append(myIsHuldraDesposedWin.ToString());
         sb.Append(", tAir=");
         sb.Append(myIsAirTravel.ToString());
         sb.Append(", tRaft=");
         sb.Append(myIsRaftTravel.ToString());
         sb.Append(", tArch=");
         sb.Append(myIsArchTravel.ToString());
         sb.Append(", kDragon=");
         sb.Append(myIsDragonKiller.ToString());
         sb.Append(", #bandits=");
         sb.Append(myNumBanditKill.ToString());
         sb.Append(", kBandit=");
         sb.Append(myIsBanditKiller.ToString());
         sb.Append(", #orcs=");
         sb.Append(myNumOrcKill.ToString());
         sb.Append(", kOrc=");
         sb.Append(myIsOrcKiller.ToString());
         sb.Append(", #goblins=");
         sb.Append(myNumGoblinKill.ToString());
         sb.Append(", kGoblin=");
         sb.Append(myIsGoblinKiller.ToString());
         sb.Append(", #wolfs=");
         sb.Append(myNumWolfKill.ToString());
         sb.Append(", kWolf=");
         sb.Append(myIsWolfKiller.ToString());
         sb.Append(", #nights=");
         sb.Append(myNumNightsInJail.ToString());
         sb.Append(", knights=");
         sb.Append(myIsNightsInJail.ToString());
         sb.Append(", foul=");
         sb.Append(myIsPurchaseFoulbane.ToString());
         sb.Append(", chaga=");
         sb.Append(myIsPurchaseChaga.ToString());
         if ( 0 < myVisitedTowns.Count )
         {
            sb.Append(", vTowns=[");
            for( int i=0; i<myVisitedTowns.Count; ++i)
            {
               sb.Append(myVisitedTowns[i]);
               if( i != myVisitedTowns.Count - 1 )
                  sb.Append(',');
            }
            sb.Append("]");
         }
         sb.Append(", town?=");
         sb.Append(myIsVisitAllTowns.ToString());
         if (0 < myVisitedCastles.Count)
         {
            sb.Append(", vCastles=[");
            for (int i = 0; i < myVisitedCastles.Count; ++i)
            {
               sb.Append(myVisitedCastles[i]);
               if (i != myVisitedCastles.Count - 1)
                  sb.Append(',');
            }
            sb.Append("]");
         }
         sb.Append(", castle?=");
         sb.Append(myIsVisitAllCastles.ToString());
         if (0 < myVisitedTemples.Count)
         {
            sb.Append(", vTemples=[");
            for (int i = 0; i < myVisitedTemples.Count; ++i)
            {
               sb.Append(myVisitedTemples[i]);
               if (i != myVisitedTemples.Count - 1)
                  sb.Append(',');
            }
            sb.Append("]");
         }
         sb.Append(", temple?=");
         sb.Append(myIsVisitAllTemples.ToString());
         if (0 < myVisitedRuins.Count)
         {
            sb.Append(", vRuins=[");
            for (int i = 0; i < myVisitedRuins.Count; ++i)
            {
               sb.Append(myVisitedRuins[i]);
               if (i != myVisitedRuins.Count - 1)
                  sb.Append(',');
            }
            sb.Append("]");
         }
         sb.Append(", ruin?=");
         sb.Append(myIsVisitAllRuins.ToString());
         if (0 < myVisitedRuins.Count)
         {
            sb.Append(", vOasis=[");
            for (int i = 0; i < myVisitedOasises.Count; ++i)
            {
               sb.Append(myVisitedOasises[i]);
               if (i != myVisitedOasises.Count - 1)
                  sb.Append(',');
            }
            sb.Append("]");
         }
         sb.Append(", oasis?=");
         sb.Append(myIsVisitAllOasis.ToString());
         sb.Append(" }");
         return sb.ToString();
      }
   };

}
