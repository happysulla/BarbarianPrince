using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbarianPrince
{
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
      public bool myIsMinstelAdded;
      public bool myIsEagleAdded;
      public bool myIsFalconAdded;
      //-------------------------------------
      public bool myIsMerchantAdded;
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
      public Territories myVisitedTowns = new Territories();
      public Territories myVisitedTemples = new Territories();
      public Territories myVisitedCastles = new Territories();
      public Territories myVisitedRuins = new Territories();
      public Territories myVisitedOasises = new Territories();
      public bool myIsVisitAllTowns;
      public bool myIsVisitAllCastles;
      public bool myIsVisitAllTemples;
      public bool myIsVisitAllRuins;
      public bool myIsVisitAllOasis;
      //-------------------------------------
      public bool myIsHydraTeethUsed;          // use hydra teeth
      public bool myIsRescueHier;              // Rescue Huldra Heir from Hill Tribe
      public bool myIsSneakAttack;             // Perform sneak attack on Huldra
      public bool myIsPurchaseFoulbane;        // Purchase foulbane
      public bool myIsStealGems;               // Steal Gems from Dragot using Foulbane
   };
}
