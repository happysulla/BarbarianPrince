using System.Collections.Generic;
using System.Xml.Linq;

namespace BarbarianPrince
{
   public class LodgingMgrUnitTest : IUnitTest
   {
      public bool CtorError { get; } = false;
      //-----------------------------------------------------------
      private EventViewer myEventViewer = null;
      //-----------------------------------------------------------
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      public LodgingMgrUnitTest(EventViewer ev)
      {
         //------------------------------------------
         myIndexName = 0;
         myHeaderNames.Add("11-Lodge Party w/ 2 Mount & Slaves");
         myHeaderNames.Add("11-Lodge Party w/ 2 Mount & Slaves");
         myHeaderNames.Add("11-Lodge Party w/ 2 Mount & Minstrel1");
         myHeaderNames.Add("11-Lodge Party w/ 2 Mount & Minstrel2");
         myHeaderNames.Add("11-Lodge Party w/ 10 coin & eagles");
         myHeaderNames.Add("11-Lodge Party w/ 4 Mount w/ 10 coin");
         myHeaderNames.Add("11-Lodge Party w/ 4 Mount w/ 5 coin");
         myHeaderNames.Add("11-Lodge Party w/ 6 Mount w/ 11 coin");
         myHeaderNames.Add("11-Lodge Party w/ 6 Mount w/ 8 coin");
         myHeaderNames.Add("11-Lodge Party w/ 6 Mount w/ 3 coin");
         myHeaderNames.Add("11-Lodge Prince 5 coin");
         myHeaderNames.Add("11-Lodge Prince w/ 1 Mount 5 coin");
         myHeaderNames.Add("11-Lodge Prince w/ 3 Mount 5 coin");
         myHeaderNames.Add("11-Lodge Prince w/ 2 Mount 2 coin");
         myHeaderNames.Add("11-Finish");
         //------------------------------------------
         myCommandNames.Add("00-Show Lodging");
         myCommandNames.Add("01-Show Lodging");
         myCommandNames.Add("02-Show Lodging");
         myCommandNames.Add("03-Show Lodging");
         myCommandNames.Add("04-Show Lodging");
         myCommandNames.Add("05-Show Lodging");
         myCommandNames.Add("06-Show Lodging");
         myCommandNames.Add("07-Show Lodging");
         myCommandNames.Add("08-Show Lodging");
         myCommandNames.Add("09-Show Lodging");
         myCommandNames.Add("10-Show Lodging");
         myCommandNames.Add("11-Show Lodging");
         myCommandNames.Add("12-Show Lodging");
         myCommandNames.Add("13-Show Lodging");
         myCommandNames.Add("Finish");
         if (null == ev)
         {
            Logger.Log(LogEnum.LE_ERROR, "LodgingMgrUnitTest(): ev=null");
            CtorError = true;
            return;
         }
         myEventViewer = ev;
      }
      public bool Command(ref IGameInstance gi)
      {
         gi.IsSecretTempleKnown = false;
         gi.IsPartyLodged = false;
         gi.IsMountsStabled = false;
         gi.IsMinstrelPlaying = false;
         gi.CheapLodgings.Clear();
         gi.ForbiddenAudiences.Clear();
         //-----------------------------------------------------------
         gi.PartyMembers.Clear();
         gi.Prince.Reset();
         gi.PartyMembers.Add(gi.Prince);
         ITerritory t = Territory.theTerritories.Find("0101");
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): t=null");
            return false;
         }
         gi.Prince.Territory = t;
         //-----------------------------------------------------------
         if (CommandName == myCommandNames[0])
         {
            gi.Prince.AddSpecialItemToKeep(SpecialEnum.TrollSkin);
            gi.Prince.AddSpecialItemToShare(SpecialEnum.TrollSkin);
            AddPrinceMounts(ref gi, 1);
            gi.Prince.Coin = 10;
            for (int i = 0; i < 3; ++i)
            {
               string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
               eagle.IsFlying = true;  
               gi.AddCompanion(eagle);
            }
            string porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(porter);
            porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(porter);
            string slaveGirlName = "SlaveGirl" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem slaveGirl = new MapItem(slaveGirlName, 1.0, false, false, false, "c41SlaveGirl", "c41SlaveGirl", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(slaveGirl);
            slaveGirlName = "SlaveGirl" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            slaveGirl = new MapItem(slaveGirlName, 1.0, false, false, false, "c41SlaveGirl", "c41SlaveGirl", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(slaveGirl);
            string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(trueLove);
            trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(trueLove);
            gi.IsPartyLodged = true;  // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            gi.IsMountsStabled = true; // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            gi.CheapLodgings.Add(gi.Prince.Territory); // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            gi.ForbiddenAudiences.AddClothesConstraint(gi.Prince.Territory); // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            ITerritory t1 = Territory.theTerritories.Find("0109");
            gi.ForbiddenAudiences.AddClothesConstraint(t1); // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[1])
         {
            AddPrinceMounts(ref gi, 2);
            gi.Prince.Coin = 10;
            string porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", gi.Prince.Territory, 0, 0, 0);
            AddCompanionMount(porter, true);
            gi.AddCompanion(porter);
            porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(porter);
            string slaveGirlName = "SlaveGirl" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem slaveGirl = new MapItem(slaveGirlName, 1.0, false, false, false, "c41SlaveGirl", "c41SlaveGirl", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(slaveGirl);
            slaveGirlName = "SlaveGirl" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            slaveGirl = new MapItem(slaveGirlName, 1.0, false, false, false, "c41SlaveGirl", "c41SlaveGirl", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(slaveGirl);
            string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", gi.Prince.Territory, 0, 0, 0);
            AddCompanionMount(trueLove, true);
            gi.AddCompanion(trueLove);
            trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(trueLove);
            gi.IsPartyLodged = true;  // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[2]) // Minstrel Playing
         {
            AddPrinceMounts(ref gi, 1);
            gi.Prince.Coin = 6;
            //----------------------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 5, 5, 0);
            companion1.Food = 10;
            companion1.Coin = 5;
            companion1.IsSunStroke = true;
            AddCompanionMount(companion1, true);
            gi.AddCompanion(companion1);
            //----------------------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            AddCompanionMount(companion2, true);
            gi.AddCompanion(companion2);
            //----------------------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            gi.AddCompanion(companion3);
            //----------------------------------------------------------------
            string minstrelName = "Minstrel" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem minstrel = new MapItem(minstrelName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(minstrel);
            //----------------------------------------------------------------
            gi.IsMinstrelPlaying = true;  // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[3]) // Minstrel Not Playing
         {
            AddPrinceMounts(ref gi, 1);
            gi.Prince.Coin = 6;
            //----------------------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 5, 5, 0);
            AddCompanionMount(companion1, true);
            gi.AddCompanion(companion1);
            //----------------------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            AddCompanionMount(companion2, true);
            gi.AddCompanion(companion2);
            //----------------------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            gi.AddCompanion(companion3);
            //----------------------------------------------------------------
            string minstrelName = "Minstrel" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem minstrel = new MapItem(minstrelName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(minstrel);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[4]) //11-Lodge Party w/ 10 coin and eagles
         {
            gi.Prince.Coin = 10;
            //----------------------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(companion1);
            //----------------------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(companion2);
            //----------------------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            gi.AddCompanion(companion3);
            //----------------------------------------------------------------
            for (int i = 0; i < 3; ++i)
            {
               string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
               eagle.IsFlying = true;
               gi.AddCompanion(eagle);
            }
            gi.IsSecretTempleKnown = true;  // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[5])
         {
            AddPrinceMounts(ref gi, 1);
            gi.Prince.Coin = 10;
            //----------------------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(companion1);
            AddCompanionMount(companion1, true);
            //----------------------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(companion2);
            AddCompanionMount(companion2, true);
            //----------------------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            gi.AddCompanion(companion3);
            AddCompanionMount(companion3, true);
            //----------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[6])
         {
            AddPrinceMounts(ref gi, 1);
            gi.Prince.Coin = 5;
            //----------------------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(companion1);
            AddCompanionMount(companion1, true);
            //----------------------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(companion2);
            AddCompanionMount(companion2, true);
            //----------------------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            gi.AddCompanion(companion3);
            AddCompanionMount(companion3, true);
            //----------------------------------------------------------------
            gi.IsSecretTempleKnown = true; // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[7])
         {
            AddPrinceMounts(ref gi, 1);
            gi.Prince.Coin = 11;
            //----------------------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(companion1);
            AddCompanionMount(companion1, true);
            //----------------------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(companion2);
            AddCompanionMount(companion2, true);
            //----------------------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            gi.AddCompanion(companion3);
            AddCompanionMount(companion3, true);
            AddCompanionMount(companion3, false);
            AddCompanionMount(companion3, false);
            //----------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[8])
         {
            AddPrinceMounts(ref gi, 3);
            gi.Prince.Coin = 8;
            //----------------------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(companion1);
            AddCompanionMount(companion1, true);
            //----------------------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(companion2);
            AddCompanionMount(companion2, true);
            //----------------------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            gi.AddCompanion(companion3);
            AddCompanionMount(companion3, true);
            //----------------------------------------------------------------
            gi.IsSecretTempleKnown = true; // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[9])
         {
            AddPrinceMounts(ref gi, 1);
            gi.Prince.Coin = 3;
            //----------------------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(companion1);
            AddCompanionMount(companion1, true);
            //----------------------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(companion2);
            AddCompanionMount(companion2, true);
            //----------------------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            gi.AddCompanion(companion3);
            AddCompanionMount(companion3, true);
            AddCompanionMount(companion3, false);
            AddCompanionMount(companion3, false);
            //----------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[10]) //<<<<<<<<<<<<<<<<<<<<<<<<<================================ PRINCE ONLY
         {
            gi.Prince.Coin = 5;
            gi.IsSecretTempleKnown = true;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[11]) 
         {
            gi.Prince.Coin = 5;
            AddPrinceMounts(ref gi, 1);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[12])
         {
            gi.Prince.Coin = 5;
            AddPrinceMounts(ref gi, 3);
            gi.IsSecretTempleKnown = true; // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[13])
         {
            gi.Prince.Coin = 2;
            AddPrinceMounts(ref gi, 2);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[14])
         { 
            // cleanup
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): reached default c=" + CommandName);
            return false;
         }
         return true;
      }
      public bool NextTest(ref IGameInstance gi)
      {
         if (HeaderName == myHeaderNames[0])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[1])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[2])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[3])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[4])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[5])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[6])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[7])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[8])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[9])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[10])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[11])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[12])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[13])
         {
            ++myIndexName;
         }
         else
         {
            if (false == Cleanup(ref gi))
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup returned error");
         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi)
      {
         gi.IsSecretTempleKnown = false;
         ++gi.GameTurn;
         return true;
      }
      //------------------------------------------------------------------
      private void AddPrinceMounts(ref IGameInstance gi, int numMounts)
      {
         IMapItem prince = gi.Prince;
         prince.Mounts.Clear();
         if (0 < numMounts)
         {
            string name = "Horse" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem horse = new MapItem(name, 1.0, false, false, false, "MHorse", "", prince.Territory, 0, 0, 0);
            prince.Mounts.Add(horse);
         }
         if (1 < numMounts)
         {
            string name = "Pegasus" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem horse = new MapItem(name, 1.0, false, false, false, "MPegasus", "", prince.Territory, 0, 0, 0);
            prince.Mounts.Add(horse);
         }
         if (2 < numMounts)
         {
            string name = "Unicorn" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem horse = new MapItem(name, 1.0, false, false, false, "MUnicorn", "", prince.Territory, 0, 0, 0);
            prince.Mounts.Add(horse);
         }
      }
      private void AddCompanionMount(IMapItem companion, bool isFreshMount)
      {
         if (true == isFreshMount)
         {
            companion.Mounts.Clear();
            string name = "Horse" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem horse = new MapItem(name, 1.0, false, false, false, "MHorse", "", companion.Territory, 0, 0, 0);
            companion.Mounts.Add(horse);
         }
         else
         {
            string name = "Pegasus" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem pegasus = new MapItem(name, 1.0, false, false, false, "MPegasus", "", companion.Territory, 0, 0, 0);
            companion.Mounts.Add(pegasus);
         }
      }
   }
}
