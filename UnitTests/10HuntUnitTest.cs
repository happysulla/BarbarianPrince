using System.Collections.Generic;

namespace BarbarianPrince
{
   internal class HuntUnitTest : IUnitTest
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
      //-----------------------------------------------------------
      public HuntUnitTest(EventViewer ev)
      {
         //------------------------------------------
         myIndexName = 0;
         myHeaderNames.Add("09-Hunt Town Giant & 12gp");
         myHeaderNames.Add("09-Hunt Town Dwarf & 8gp");
         myHeaderNames.Add("09-Hunt W/ Countryside w/ Prince w/ 0gp ");
         myHeaderNames.Add("09-Hunt Town Dwarf & 8gp & Rested");
         myHeaderNames.Add("09-Hunt Town Dwarf & 8gp & 2 Mounts rested");
         myHeaderNames.Add("09-Hunt Town Dwarf & 3gp & 2 Mounts rested");
         myHeaderNames.Add("09-Hunt Farmland");
         myHeaderNames.Add("09-Hunt w/ Party & Rested");
         myHeaderNames.Add("09-Finish");
         //------------------------------------------
         myCommandNames.Add("00-Show HuntMgr");
         myCommandNames.Add("01-Show HuntMgr");
         myCommandNames.Add("02-Show HuntMgr");
         myCommandNames.Add("03-Show HuntMgr");
         myCommandNames.Add("04-Show HuntMgr");
         myCommandNames.Add("05-Show HuntMgr");
         myCommandNames.Add("06-Show HuntMgr");
         myCommandNames.Add("07-Show HuntMgr");
         myCommandNames.Add("Finish");
         //------------------------------------------
         if (null == ev)
         {
            Logger.Log(LogEnum.LE_ERROR, "HuntUnitTest(): ev=null");
            CtorError = true;
            return;
         }
         myEventViewer = ev;
      }
      public bool Command(ref IGameInstance gi)
      {
         ITerritory t = gi.Territories.Find("0101");
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): t=null");
            return false;
         }
         IMapItems partyMembers = gi.PartyMembers;
         partyMembers.Clear();
         IMapItem mi = gi.MapItems.Find("Prince");
         if (null == mi)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): mi=null");
            return false;
         }
         mi.Reset();
         IMapItem clone = new MapItem(mi);
         gi.Prince = clone;
         partyMembers.Add(clone);
         gi.IsPartyRested = false;
         //-------------------------------------------------
         //-------------------------------------------------
         if (CommandName == myCommandNames[0])  // Town
         {
            AddMounts(ref gi, clone, 3); // <<<<== mounts
            clone.Territory = t;
            clone.Coin = 30;
            //-------------------------------
            mi = gi.MapItems.Find("Dwarf");
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): mi=null");
               return false;
            }
            mi.Reset();
            clone = new MapItem(mi);
            partyMembers.Add(clone);
            //-------------------------------
            string giantName = "Giant" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem giant = new MapItem(giantName, 1.0, false, false, false, "c61Giant", "c61Giant", t, 8, 9, 10);
            giant.StarveDayNum = 2;
            gi.AddCompanion(giant);
            //-------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.Hunt);
         }
         if (CommandName == myCommandNames[1])  // Town
         {
            AddMounts(ref gi, clone, 3); // <<<<== mounts
            clone.Territory = t;
            clone.Coin = 3;
            //-------------------------------
            mi = gi.MapItems.Find("Dwarf");
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): mi=null");
               return false;
            }
            mi.Reset();
            clone = new MapItem(mi);
            clone.Coin = 5;
            partyMembers.Add(clone);
            //-------------------------------
            gi.CheapLodgings.Add(t); // <<<<<===== cheap lodgings
            myEventViewer.UpdateView(ref gi, GameAction.Hunt);
         }
         else if (CommandName == myCommandNames[2])
         {
            ITerritory t1 = gi.Territories.Find("0104"); // Countryside
            if (null == t1)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t1=null");
               return false;
            }
            clone.Territory = t1;
            myEventViewer.UpdateView(ref gi, GameAction.Hunt);
         }
         else if (CommandName == myCommandNames[3])  // Town and rested
         {
            clone.Territory = t;
            clone.Coin = 3;
            //-------------------------------
            mi = gi.MapItems.Find("Dwarf");
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): mi=null");
               return false;
            }
            mi.Reset();
            clone = new MapItem(mi);
            clone.Coin = 5;
            partyMembers.Add(clone);
            //-------------------------------
            gi.IsPartyRested = true; // <<<<== reseted
            myEventViewer.UpdateView(ref gi, GameAction.Hunt);
         }
         else if (CommandName == myCommandNames[4])  // Town and rested w/ mounts
         {
            clone.Territory = t;
            clone.Coin = 3;
            AddMounts(ref gi, clone, 2); // <<<<== mounts
            //-------------------------------
            mi = gi.MapItems.Find("Dwarf");
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): mi=null");
               return false;
            }
            mi.Reset();
            clone = new MapItem(mi);
            clone.Coin = 5;
            partyMembers.Add(clone);
            //-------------------------------
            gi.IsPartyRested = true; // <<<<== reseted
            myEventViewer.UpdateView(ref gi, GameAction.Hunt);
         }
         else if (CommandName == myCommandNames[5])  // Town and rested w/ mounts, not enough coin
         {
            clone.Territory = t;
            clone.Coin = 3;
            AddMounts(ref gi, clone, 2); // <<<<== 2 mounts
            //-------------------------------
            mi = gi.MapItems.Find("Dwarf");
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): mi=null");
               return false;
            }
            mi.Reset();
            clone = new MapItem(mi);
            clone.Coin = 0;
            partyMembers.Add(clone);
            //-------------------------------
            gi.IsPartyRested = true; // <<<<== reseted
            myEventViewer.UpdateView(ref gi, GameAction.Hunt);
         }
         else if (CommandName == myCommandNames[6]) // Farmland - NOT RESTED SO NOT HELPING HUNTERS
         {
            clone.Territory = t;
            //-------------------------------
            mi = gi.MapItems.Find("Dwarf");
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): mi=null");
               return false;
            }
            mi.Reset();
            clone = new MapItem(mi);
            partyMembers.Add(clone);
            myEventViewer.UpdateView(ref gi, GameAction.Hunt);
         }
         else if (CommandName == myCommandNames[7]) // Countryside & Rested Party - OTHERS CAN HELP IN HUNT
         {
            ITerritory t2 = gi.Territories.Find("0104");
            if (null == t2)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t2=null");
               return false;
            }
            clone.Territory = t2;
            //-------------------------------
            mi = gi.MapItems.Find("Dwarf");
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): mi=null");
               return false;
            }
            mi.Reset();
            clone = new MapItem(mi);
            clone.IsGuide = true;
            clone.GuideTerritories.Add(t);
            partyMembers.Add(clone);
            //-------------------------------
            mi = gi.MapItems.Find("Witch");
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): mi=null");
               return false;
            }
            mi.Reset();
            clone = new MapItem(mi);
            clone.IsGuide = true;
            ITerritory t1 = gi.Territories.Find("1011");// act in guide long ways away mans this is not a guide for this hex
            if (null == t1)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t1=null");
               return false;
            }
            clone.GuideTerritories.Add(t1);
            partyMembers.Add(clone);
            //-------------------------------
            gi.IsPartyRested = true; // <<<<== reseted
            myEventViewer.UpdateView(ref gi, GameAction.Hunt);
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
         else
         {
            if (false == Cleanup(ref gi))
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup returned error");
         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi)
      {
         ++gi.GameTurn;
         return true;
      }
      private void AddMounts(ref IGameInstance gi, IMapItem mi, int numMounts)
      {
         if (0 < numMounts)
         {
            string name = "Horse" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem horse = new MapItem(name, 1.0, false, false, false, "MHorse", "", mi.Territory, 0, 0, 0);
            mi.Mounts.Add(horse);
         }
         if (1 < numMounts)
         {
            string name = "Pegasus" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem horse = new MapItem(name, 1.0, false, false, false, "MPegasus", "", mi.Territory, 0, 0, 0);
            mi.Mounts.Add(horse);
         }
         if (2 < numMounts)
         {
            string name = "Unicorn" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem horse = new MapItem(name, 1.0, false, false, false, "MUnicorn", "", mi.Territory, 0, 0, 0);
            mi.Mounts.Add(horse);
         }
      }
   }
}
