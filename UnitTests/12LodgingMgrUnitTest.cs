using System.Collections.Generic;

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
         ITerritory t = gi.Territories.Find("0101"); 
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): t=null");
            return false;
         }
         //-----------------------------------------------------------
         if (CommandName == myCommandNames[0])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.AddSpecialItemToKeep(SpecialEnum.TrollSkin);
            prince.AddSpecialItemToShare(SpecialEnum.TrollSkin);
            AddPrinceMounts(ref gi, 1);
            prince.Coin = 10;
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
            ITerritory t1 = gi.Territories.Find("0109");
            gi.ForbiddenAudiences.AddClothesConstraint(t1); // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[1])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            AddPrinceMounts(ref gi, 2);
            prince.Coin = 10;
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
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            AddPrinceMounts(ref gi, 1);
            prince.Coin = 6;
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            AddCompanionMount(companion1, true);
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            AddCompanionMount(companion2, true);
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            string minstrelName = "Minstrel" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem minstrel = new MapItem(minstrelName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", prince.Territory, 0, 0, 0);
            gi.AddCompanion(minstrel);
            gi.IsMinstrelPlaying = true;  // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[3]) // Minstrel Not Playing
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            AddPrinceMounts(ref gi, 1);
            prince.Coin = 6;
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            AddCompanionMount(companion1, true);
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            AddCompanionMount(companion2, true);
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            string minstrelName = "Minstrel" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem minstrel = new MapItem(minstrelName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", prince.Territory, 0, 0, 0);
            gi.AddCompanion(minstrel);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[4]) //11-Lodge Party w/ 10 coin and eagles
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Coin = 10;
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
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
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            AddPrinceMounts(ref gi, 1);
            prince.Coin = 10;
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            AddCompanionMount(companion1, true);
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            AddCompanionMount(companion2, true);
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            AddCompanionMount(companion3, true);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[6])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            AddPrinceMounts(ref gi, 1);
            prince.Coin = 5;
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            AddCompanionMount(companion1, true);
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            AddCompanionMount(companion2, true);
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            AddCompanionMount(companion3, true);
            gi.IsSecretTempleKnown = true; // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[7])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            AddPrinceMounts(ref gi, 1);
            prince.Coin = 11;
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            AddCompanionMount(companion1, true);
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            AddCompanionMount(companion2, true);
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            AddCompanionMount(companion3, true);
            AddCompanionMount(companion3, false);
            AddCompanionMount(companion3, false);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[8])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            AddPrinceMounts(ref gi, 3);
            prince.Coin = 8;
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            AddCompanionMount(companion1, true);
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            AddCompanionMount(companion2, true);
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            AddCompanionMount(companion3, true);
            gi.IsSecretTempleKnown = true; // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[9])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            AddPrinceMounts(ref gi, 1);
            prince.Coin = 3;
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            AddCompanionMount(companion1, true);
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            AddCompanionMount(companion2, true);
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            AddCompanionMount(companion3, true);
            AddCompanionMount(companion3, false);
            AddCompanionMount(companion3, false);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[10]) //<<<<<<<<<<<<<<<<<<<<<<<<<================================ PRINCE ONLY
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Coin = 5;
            gi.IsSecretTempleKnown = true;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[11]) 
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Coin = 5;
            AddPrinceMounts(ref gi, 1);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[12])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Coin = 5;
            AddPrinceMounts(ref gi, 3);
            gi.IsSecretTempleKnown = true; // <<<<<<<<<<<<<<<<<<<<<<<<=========================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
         }
         else if (CommandName == myCommandNames[13])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Coin = 2;
            AddPrinceMounts(ref gi, 2);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLodgingCheck);
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
      private IMapItem AddPrince(ref IGameInstance gi, ITerritory t)
      {
         IMapItems partyMembers = gi.PartyMembers;
         partyMembers.Clear();
         IMapItem prince = gi.MapItems.Find("Prince");
         if (null == prince)
            Logger.Log(LogEnum.LE_ERROR, "AddPrince(): mi=null");
         prince.Territory = t;
         prince.Reset();
         partyMembers.Add(prince);
         gi.Prince = prince;
         return prince;
      }
      private void AddPrinceMounts(ref IGameInstance gi, int numMounts)
      {
         IMapItem prince = gi.MapItems.Find("Prince");
         if (null == prince)
            Logger.Log(LogEnum.LE_ERROR, "AddPrince(): mi=null");
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
      private IMapItem AddCompanion(ref IGameInstance gi, string name)
      {
         IMapItems partyMembers = gi.PartyMembers;
         IMapItem companion1 = gi.MapItems.Find(name);
         if (null == companion1)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddCompanions(): mi=null for name=" + name);
            return null;
         }
         companion1.Reset();
         companion1.Mounts.Clear();
         partyMembers.Add(companion1);
         return companion1;
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
