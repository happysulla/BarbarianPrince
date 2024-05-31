using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace BarbarianPrince
{
   internal class StarvationUnitTest : IUnitTest
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
      public StarvationUnitTest(EventViewer ev)
      {
         myIndexName = 0;
         //------------------------------------------
         myHeaderNames.Add("10-Henchmen no food or coin");
         myHeaderNames.Add("10-Henchmen w/ Giant & Eagles");
         myHeaderNames.Add("10-Party w/ Slaves & True Love");
         myHeaderNames.Add("10-Henchmen");
         myHeaderNames.Add("10-Henchmen Group - Witches Paid");
         myHeaderNames.Add("10-Henchmen Group w/ Mistrel Playing");
         myHeaderNames.Add("10-Henchmen Group w/ Mistrel");
         myHeaderNames.Add("10-Party w/ 10 food 0 SD");
         myHeaderNames.Add("10-Party w/ 8 food, multi SD");
         myHeaderNames.Add("10-Party w/ 10 food, 3 Mounts");
         myHeaderNames.Add("10-Party w/ 3 food, 3 SD, 4 Mounts");
         myHeaderNames.Add("10-Party w/ 3 food, 3 SD, 4 Mounts, Party Fed");
         myHeaderNames.Add("10-Party w/ 3 food, 3 SD, 4 Mounts, Mounts Fed");
         myHeaderNames.Add("10-Party w/ 30 food, 3 SD, 4 Mounts, Desert");
         //------------------------------------------
         myHeaderNames.Add("10-Prince w heal wounds");
         myHeaderNames.Add("10-Prince w heal poison");
         myHeaderNames.Add("10-Prince w/5 food 1 Starve Day");
         myHeaderNames.Add("10-Prince w/5 food 3 SD");
         myHeaderNames.Add("10-Prince w/1 food 1 SD");
         myHeaderNames.Add("10-Prince w/0 food 1 SD");
         myHeaderNames.Add("10-Prince w/2 food, 0 SD, 1 Mount");
         myHeaderNames.Add("10-Prince w/2 food, 0 SD, 1 Mount, party/mount Fed");
         myHeaderNames.Add("10-Prince w/2 food, 0 SD, 1 Mount, party Fed");
         myHeaderNames.Add("10-Prince w/2 food, 0 SD, 1 Mount, mount Fed");
         myHeaderNames.Add("10-Prince w/5 food, 0 SD, 2 Mount");
         myHeaderNames.Add("10-Prince w/2 food, 0 SD, 2 Mount");
         myHeaderNames.Add("10-Finish");
         //------------------------------------------
         myCommandNames.Add("00-Feed");
         myCommandNames.Add("01-Feed");
         myCommandNames.Add("02-Feed");
         myCommandNames.Add("03-Feed");
         myCommandNames.Add("04-Feed");
         myCommandNames.Add("05-Feed");
         myCommandNames.Add("06-Feed");
         myCommandNames.Add("07-Feed");
         myCommandNames.Add("08-Feed");
         myCommandNames.Add("09-Feed");
         myCommandNames.Add("10-Feed");
         myCommandNames.Add("11-Feed");
         myCommandNames.Add("12-Feed");
         myCommandNames.Add("13-Feed");
         myCommandNames.Add("14-Feed");
         myCommandNames.Add("15-Feed");
         myCommandNames.Add("16-Feed");
         myCommandNames.Add("17-Feed");
         myCommandNames.Add("18-Feed");
         myCommandNames.Add("19-Feed");
         myCommandNames.Add("20-Feed");
         myCommandNames.Add("21-Feed");
         myCommandNames.Add("22-Feed");
         myCommandNames.Add("23-Feed");
         myCommandNames.Add("24-Feed");
         myCommandNames.Add("25-Feed");
         myCommandNames.Add("Finish");
         //------------------------------------------
         if (null == ev)
         {
            Logger.Log(LogEnum.LE_ERROR, "StarvationUnitTest(): ev=null");
            CtorError = true;
            return;
         }
         myEventViewer = ev;
      }
      public bool Command(ref IGameInstance gi)
      {
         gi.PartyMembers.Clear();
         gi.IsMinstrelPlaying = false; // e049 - minstrel & eagles
         gi.IsPartyFed = false;
         gi.IsMountsFed = false;
         gi.IsMagicianProvideGift = false;
         ITerritory t = gi.Territories.Find("0101");
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): t=null");
            return false;
         }
         gi.Prince.Territory = t;
         gi.Prince.Reset();
         gi.Prince.Food = 0;
         gi.Prince.Coin = 0;
         gi.Prince.SetWounds(1, 1);
         gi.Prince.AddSpecialItemToShare(SpecialEnum.HealingPoition);
         gi.Prince.AddSpecialItemToShare(SpecialEnum.CurePoisonVial);
         gi.PartyMembers.Add(gi.Prince);
         if (CommandName == myCommandNames[0])
         {
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Wages = 2;
            gi.PartyMembers.Add(companion1);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[1]) 
         {
            gi.Prince.Food = 30;
            gi.Prince.Coin = 30;
            //-----------------------------
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Wages = 2;
            gi.PartyMembers.Add(companion1);
            //-----------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            gi.PartyMembers.Add(companion2);
            //-----------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion5 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion5.Wages = 1;
            gi.PartyMembers.Add(companion5);
            //-----------------------------
            string giantName = "Giant" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem giant = new MapItem(giantName, 1.0, false, false, false, "c61Giant", "c61Giant", t, 8, 9, 10);
            giant.StarveDayNum = 2;
            gi.AddCompanion(giant);
            //-----------------------------
            for (int i = 0; i < 3; ++i)
            {
               string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
               eagle.IsFlying = true;
               gi.AddCompanion(eagle);
            }
            gi.AddNewMountToParty();
            gi.AddNewMountToParty();
            gi.AddNewMountToParty();
            gi.AddNewMountToParty();
            gi.AddNewMountToParty();
            gi.AddNewMountToParty();
            gi.PartyMembers.Reverse();
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         if (CommandName == myCommandNames[2])
         {
            gi.Prince.Food = 6;
            gi.Prince.StarveDayNum = 0;
            //---------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Food = 0;
            companion1.StarveDayNum = 1;
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            companion2.Food = 0;
            companion2.StarveDayNum = 2;
            gi.PartyMembers.Add(companion2);
            //---------------------------------------------------
            string porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", t, 0, 0, 0);
            porter.Food = 0;
            porter.StarveDayNum = 4;
            gi.AddCompanion(porter);
            //---------------------------------------------------
            porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", t, 0, 0, 0);
            porter.Food = 0;
            porter.StarveDayNum = 5;
            gi.AddCompanion(porter);
            //---------------------------------------------------
            //string slaveGirlName = "SlaveGirl" + Utilities.MapItemNum.ToString();
            //++Utilities.MapItemNum;
            //IMapItem slaveGirl = new MapItem(slaveGirlName, 1.0, false, false, false, "c41SlaveGirl", "c41SlaveGirl", t, 0, 0, 0);
            //slaveGirl.Food = 0;
            //slaveGirl.StarveDayNum = 5;
            //gi.AddCompanion(slaveGirl);
            //---------------------------------------------------
            //slaveGirlName = "SlaveGirl" + Utilities.MapItemNum.ToString();
            //++Utilities.MapItemNum;
            //slaveGirl = new MapItem(slaveGirlName, 1.0, false, false, false, "c41SlaveGirl", "c41SlaveGirl", t, 0, 0, 0);
            //slaveGirl.Food = 0;
            //slaveGirl.StarveDayNum = 2;
            //gi.AddCompanion(slaveGirl);
            //---------------------------------------------------
            string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", t, 0, 0, 0);
            trueLove.Food = 0;
            trueLove.StarveDayNum = 0;
            gi.AddCompanion(trueLove);
            //---------------------------------------------------
            //trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            //++Utilities.MapItemNum;
            //trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", t, 0, 0, 0);
            //trueLove.Food = 0;
            //trueLove.StarveDayNum = 0;
            //gi.AddCompanion(trueLove);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[3]) //Henchmen
         {
            gi.Prince.Coin = 10;
            gi.Prince.Food = 10;
            gi.Prince.StarveDayNum = 0;
            //---------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Wages = 2;
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            gi.PartyMembers.Add(companion2);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion5 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion5.Wages = 1;
            gi.PartyMembers.Add(companion5);
            //---------------------------------------------------
            gi.PartyMembers.Reverse();
            gi.IsMagicianProvideGift = true; // <<<<<<<<<<<<<<<<<<<<=================================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[4]) //Henchmen Group
         {
            gi.Prince.Coin = 10;
            gi.Prince.Food = 10;
            gi.Prince.StarveDayNum = 0;
            //---------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Wages = 2;
            companion1.GroupNum = 1;
            companion1.PayDay = gi.Days - 1;
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion2.Wages = 2;
            companion2.GroupNum = 1;
            companion2.PayDay = gi.Days - 1;
            gi.PartyMembers.Add(companion2);
            //---------------------------------------------------
            miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion3.Wages = 2;
            companion3.GroupNum = 1;
            companion3.PayDay = gi.Days - 1;
            gi.PartyMembers.Add(companion3);
            //---------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion4 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            gi.PartyMembers.Add(companion4);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion5 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion5.Wages = 1;
            companion5.GroupNum = 2;
            companion5.PayDay = gi.Days - 1;
            gi.PartyMembers.Add(companion5);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion6 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion6.Wages = 1;
            companion6.GroupNum = 2;
            companion6.PayDay = gi.Days - 1;
            gi.PartyMembers.Add(companion6);
            //---------------------------------------------------
            gi.PartyMembers.Reverse();
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[5]) //Henchmen Group w/ Minstrel Playing
         {
            gi.Prince.Coin = 10;
            gi.Prince.Food = 10;
            gi.Prince.StarveDayNum = 0;
            //---------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Wages = 2;
            companion1.GroupNum = 1;
            companion1.PayDay = gi.Days - 1;
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion2.Wages = 2;
            companion2.GroupNum = 1;
            companion2.PayDay = gi.Days - 1;
            gi.PartyMembers.Add(companion2);
            //---------------------------------------------------
            miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion3.Wages = 2;
            companion3.GroupNum = 1;
            companion3.PayDay = gi.Days - 1;
            gi.PartyMembers.Add(companion3);
            //---------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion4 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            companion4.Wages = 2; 
            gi.PartyMembers.Add(companion4);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion5 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion5.Wages = 1;
            companion5.GroupNum = 2;
            companion5.PayDay = gi.Days + 1;
            gi.PartyMembers.Add(companion5);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion6 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion6.Wages = 1;
            companion6.GroupNum = 2;
            companion6.PayDay = gi.Days + 1;
            gi.PartyMembers.Add(companion6);
            //--------------------------------------------------
            string minstrelName = "Minstrel" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem minstrel = new MapItem(minstrelName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", t, 0, 0, 0);
            gi.AddCompanion(minstrel);
            gi.IsMinstrelPlaying = true; // <<<<<<<<<<<<<<<<<<<<=================================
            gi.IsMagicianProvideGift = true; // <<<<<<<<<<<<<<<<<<<<=================================
            //--------------------------------------------------
            gi.PartyMembers.Reverse();
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[6]) //Henchmen Group w/ Minstrel
         {
            gi.Prince.Coin = 10;
            gi.Prince.Food = 10;
            gi.Prince.StarveDayNum = 0;
            //---------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Wages = 2;
            companion1.GroupNum = 1;
            companion1.PayDay = gi.Days - 1;
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion2.Wages = 2;
            companion2.GroupNum = 1;
            companion2.PayDay = gi.Days - 1;
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion3.Wages = 2;
            companion3.GroupNum = 1;
            companion3.PayDay = gi.Days - 1;
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion4 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            companion4.Wages = 2;
            gi.PartyMembers.Add(companion4);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion5 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion5.Wages = 1;
            companion5.GroupNum = 2;
            companion5.PayDay = gi.Days + 1;
            gi.PartyMembers.Add(companion5);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion6 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion6.Wages = 1;
            companion6.GroupNum = 2;
            companion6.PayDay = gi.Days + 1;
            gi.PartyMembers.Add(companion6);
            //--------------------------------------------------
            string minstrelName = "Minstrel" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem minstrel = new MapItem(minstrelName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", t, 0, 0, 0);
            gi.AddCompanion(minstrel);
            //--------------------------------------------------
            gi.PartyMembers.Reverse();
            gi.IsMagicianProvideGift = true; // <<<<<<<<<<<<<<<<<<<<=================================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[7]) 
         {
            gi.Prince.Food = 10;
            gi.Prince.StarveDayNum = 0;
            //---------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            gi.PartyMembers.Add(companion2);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            gi.PartyMembers.Add(companion3);
            //---------------------------------------------------
            gi.PartyMembers.Reverse();
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[8])
         {
            gi.Prince.Food = 2;
            gi.Prince.StarveDayNum = 0;
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Food = 2;
            companion1.StarveDayNum = 1;
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            companion2.Food = 2;
            companion2.StarveDayNum = 2;
            gi.PartyMembers.Add(companion2);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion3.Food = 2;
            companion3.StarveDayNum = 3;
            gi.PartyMembers.Add(companion3);
            //--------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[9])
         {
            gi.Prince.Food = 4;
            gi.Prince.StarveDayNum = 0;
            //---------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Food = 2;
            companion1.StarveDayNum = 1;
            companion1.AddNewMount();
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            companion2.Food = 2;
            companion2.StarveDayNum = 2;
            companion2.AddNewMount();
            gi.PartyMembers.Add(companion2);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion3.Food = 2;
            companion3.StarveDayNum = 3;
            companion3.AddNewMount();
            gi.PartyMembers.Add(companion3);
            //---------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[10])
         {
            gi.Prince.Food = 3;
            gi.Prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            //---------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Food = 0;
            companion1.StarveDayNum = 1;
            companion1.AddNewMount();
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            companion2.Food = 0;
            companion2.StarveDayNum = 2;
            companion2.AddNewMount();
            gi.PartyMembers.Add(companion2);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion3.Food = 0;
            companion3.StarveDayNum = 3;
            companion3.AddNewMount();
            gi.PartyMembers.Add(companion3);
            //--------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[11])
         {
            gi.Prince.Food = 3;
            gi.Prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            //---------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Food = 0;
            companion1.StarveDayNum = 1;
            companion1.AddNewMount();
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            companion2.Food = 0;
            companion2.StarveDayNum = 2;
            companion2.AddNewMount();
            gi.PartyMembers.Add(companion2);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion3.Food = 0;
            companion3.StarveDayNum = 3;
            companion3.SetWounds(1, 2);
            companion3.AddNewMount();
            gi.PartyMembers.Add(companion3);
            //--------------------------------------------------
            gi.IsPartyFed = true; // <<<<<<<<<<<<<<<<<<<<=================================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[12])
         {
            gi.Prince.Food = 3;
            gi.Prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            //---------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Food = 0;
            companion1.StarveDayNum = 1;
            companion1.AddNewMount();
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            companion2.Food = 0;
            companion2.StarveDayNum = 2;
            companion2.AddNewMount();
            gi.PartyMembers.Add(companion2);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion6 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion6.Food = 0;
            companion6.StarveDayNum = 3;
            companion6.AddNewMount();
            gi.PartyMembers.Add(companion6);
            //---------------------------------------------------
            gi.IsMountsFed = true; // <<<<<<<<<<<<<<<<<<<<=================================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[13])
         {
            t = gi.Territories.Find("0306"); // Mountains
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t=null");
               return false;
            }
            gi.Prince.Territory = t;
            gi.Prince.Food = 6;
            gi.Prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            //---------------------------------------------------
            string miName = "Mercenary" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion1 = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            companion1.Food = 8;
            companion1.StarveDayNum = 1;
            companion1.AddNewMount();
            gi.PartyMembers.Add(companion1);
            //---------------------------------------------------
            miName = "Porter" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
            companion2.Food = 8;
            companion2.StarveDayNum = 2;
            companion2.AddNewMount();
            gi.PartyMembers.Add(companion2);
            //---------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum;
            Utilities.MapItemNum++;
            IMapItem companion6 = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            companion6.Food = 8;
            companion6.StarveDayNum = 3;
            companion6.AddNewMount();
            gi.PartyMembers.Add(companion6);
            //---------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[14]) //<<<<<<<<<<<<<<<<<<<<<<<<<<<========================== PRINCE ONLY
         {
            gi.Prince.Food = 2;
            gi.Prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[15])
         {
            gi.Prince.Food = 2;
            gi.Prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[16])
         {
            gi.Prince.Food = 5;
            gi.Prince.StarveDayNum = 1;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[17])
         {
            gi.Prince.Food = 5;
            gi.Prince.StarveDayNum = 3;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[18])
         {
            gi.Prince.Food = 1;
            gi.Prince.StarveDayNum = 1;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[19])
         {
            gi.Prince.Food = 0;
            gi.Prince.StarveDayNum = 1;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[20])
         {
            gi.Prince.Food = 2;
            gi.Prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1); // 1 mount
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[21])
         {
            gi.Prince.Food = 2;
            gi.Prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1); // 1 mount
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[22])
         {
            gi.Prince.Food = 2;
            gi.Prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1); // 1 mount
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[23])
         {
            gi.Prince.Food = 2;
            gi.Prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            gi.IsPartyFed = true;
            gi.IsMountsFed = true;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[24])
         {
            gi.Prince.Food = 5;
            gi.Prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 2);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[25])
         {
            gi.Prince.Food = 2;
            gi.Prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 2);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): Reached default index=" + myIndexName.ToString());
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
         else if (HeaderName == myHeaderNames[14])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[15])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[16])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[17])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[18])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[19])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[20])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[21])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[22])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[23])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[24])
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
      //-----------------------------------------------------------
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
            MapItem pegasus = new MapItem(name, 1.0, false, false, false, "MPegasus", "", prince.Territory, 0, 0, 0);
            pegasus.StarveDayNum = 5;
            prince.Mounts.Add(pegasus);
         }
         if (2 < numMounts)
         {
            string name = "Unicorn" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem unicorn = new MapItem(name, 1.0, false, false, false, "MUnicorn", "", prince.Territory, 0, 0, 0);
            prince.Mounts.Add(unicorn);
         }
      }
   }
}
