using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Navigation;

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
         if (CommandName == myCommandNames[0]) //Giant
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 30;
            prince.Coin = 30;
            IMapItem companion1 = AddHireling(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            IMapItem companion3 = AddHireling(ref gi, "Witch");
            if (null == companion3)
               return false;
            companion3.Wages = 1;
            string giantName = "Giant" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem giant = new MapItem(giantName, 1.0, false, false, false, "c61Giant", "c61Giant", t, 8, 9, 10);
            giant.StarveDayNum = 2;
            gi.AddCompanion(giant);
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
         if (CommandName == myCommandNames[1])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 6;
            prince.StarveDayNum = 0;
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            companion1.Food = 0;
            companion1.StarveDayNum = 1;
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            companion2.Food = 0;
            companion2.StarveDayNum = 2;
            string porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", t, 0, 0, 0);
            porter.Food = 0;
            porter.StarveDayNum = 4;
            gi.AddCompanion(porter);
            porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", t, 0, 0, 0);
            porter.Food = 0;
            porter.StarveDayNum = 5;
            gi.AddCompanion(porter);
            //string slaveGirlName = "SlaveGirl" + Utilities.MapItemNum.ToString();
            //++Utilities.MapItemNum;
            //IMapItem slaveGirl = new MapItem(slaveGirlName, 1.0, false, false, false, "c41SlaveGirl", "c41SlaveGirl", t, 0, 0, 0);
            //slaveGirl.Food = 0;
            //slaveGirl.StarveDayNum = 5;
            //gi.AddCompanion(slaveGirl);
            //slaveGirlName = "SlaveGirl" + Utilities.MapItemNum.ToString();
            //++Utilities.MapItemNum;
            //slaveGirl = new MapItem(slaveGirlName, 1.0, false, false, false, "c41SlaveGirl", "c41SlaveGirl", t, 0, 0, 0);
            //slaveGirl.Food = 0;
            //slaveGirl.StarveDayNum = 2;
            //gi.AddCompanion(slaveGirl);
            string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", t, 0, 0, 0);
            trueLove.Food = 0;
            trueLove.StarveDayNum = 0;
            gi.AddCompanion(trueLove);
            //trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            //++Utilities.MapItemNum;
            //trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", t, 0, 0, 0);
            //trueLove.Food = 0;
            //trueLove.StarveDayNum = 0;
            //gi.AddCompanion(trueLove);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[2]) //Henchmen
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Coin = 10;
            prince.Food = 10;
            prince.StarveDayNum = 0;
            IMapItem companion1 = AddHireling(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            IMapItem companion3 = AddHireling(ref gi, "Witch");
            companion3.Wages = 1;
            if (null == companion3)
               return false;
            gi.PartyMembers.Reverse();
            gi.IsMagicianProvideGift = true; // <<<<<<<<<<<<<<<<<<<<=================================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[3]) //Henchmen Group
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Coin = 10;
            prince.Food = 10;
            prince.StarveDayNum = 0;
            IMapItem companion1 = AddHirelingGroup(ref gi, "Mercenary", 1, gi.Days - 1);
            if (null == companion1)
               return false;
            IMapItem companion1a = AddHirelingGroup(ref gi, "Mercenary", 1, gi.Days - 1);
            if (null == companion1a)
               return false;
            IMapItem companion1b = AddHirelingGroup(ref gi, "Mercenary", 1, gi.Days - 1);
            if (null == companion1b)
               return false;
            IMapItem companion2 = AddHireling(ref gi, "Porter");
            if (null == companion2)
               return false;
            IMapItem companion3 = AddHirelingGroup(ref gi, "Witch", 2, gi.Days - 1); // already paid for today
            companion3.Wages = 1;
            if (null == companion3)
               return false;
            IMapItem companion3a = AddHirelingGroup(ref gi, "Witch", 2, gi.Days - 1); // already paid for today
            companion3a.Wages = 1;
            if (null == companion3a)
               return false;
            gi.PartyMembers.Reverse();
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[4]) //Henchmen Group w/ Minstrel Playing
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Coin = 10;
            prince.Food = 10;
            prince.StarveDayNum = 0;
            IMapItem companion1 = AddHirelingGroup(ref gi, "Mercenary", 1, gi.Days - 1);
            if (null == companion1)
               return false;
            IMapItem companion1a = AddHirelingGroup(ref gi, "Mercenary", 1, gi.Days - 1);
            if (null == companion1a)
               return false;
            IMapItem companion1b = AddHirelingGroup(ref gi, "Mercenary", 1, gi.Days - 1);
            if (null == companion1b)
               return false;
            IMapItem companion2 = AddHireling(ref gi, "Porter");
            if (null == companion2)
               return false;
            IMapItem companion3 = AddHirelingGroup(ref gi, "Witch", 2, gi.Days + 1);
            companion3.Wages = 1;
            if (null == companion3)
               return false;
            IMapItem companion3a = AddHirelingGroup(ref gi, "Witch", 2, gi.Days + 1);
            companion3a.Wages = 1;
            if (null == companion3a)
               return false;
            string minstrelName = "Minstrel" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem minstrel = new MapItem(minstrelName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", t, 0, 0, 0);
            gi.AddCompanion(minstrel);
            gi.IsMinstrelPlaying = true; // <<<<<<<<<<<<<<<<<<<<=================================
            gi.IsMagicianProvideGift = true; // <<<<<<<<<<<<<<<<<<<<=================================
            gi.PartyMembers.Reverse();
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[5]) //Henchmen Group w/ Minstrel
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Coin = 10;
            prince.Food = 10;
            prince.StarveDayNum = 0;
            IMapItem companion1 = AddHirelingGroup(ref gi, "Mercenary", 1, gi.Days - 1);
            if (null == companion1)
               return false;
            IMapItem companion1a = AddHirelingGroup(ref gi, "Mercenary", 1, gi.Days - 1);
            if (null == companion1a)
               return false;
            IMapItem companion1b = AddHirelingGroup(ref gi, "Mercenary", 1, gi.Days - 1);
            if (null == companion1b)
               return false;
            IMapItem companion2 = AddHireling(ref gi, "Porter");
            if (null == companion2)
               return false;
            IMapItem companion3 = AddHirelingGroup(ref gi, "Witch", 2, gi.Days + 1);
            companion3.Wages = 1;
            if (null == companion3)
               return false;
            IMapItem companion3a = AddHirelingGroup(ref gi, "Witch", 2, gi.Days + 1);
            companion3a.Wages = 1;
            if (null == companion3a)
               return false;
            string minstrelName = "Minstrel" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem minstrel = new MapItem(minstrelName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", t, 0, 0, 0);
            gi.AddCompanion(minstrel);
            gi.PartyMembers.Reverse();
            gi.IsMagicianProvideGift = true; // <<<<<<<<<<<<<<<<<<<<=================================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[6]) 
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 10;
            prince.StarveDayNum = 0;
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            gi.PartyMembers.Reverse();
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[7])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 2;
            prince.StarveDayNum = 0;
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            companion1.Food = 2;
            companion1.StarveDayNum = 1;
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            companion2.Food = 2;
            companion2.StarveDayNum = 2;
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            companion3.Food = 2;
            companion3.StarveDayNum = 3;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[8])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 4;
            prince.StarveDayNum = 0;
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            companion1.Food = 2;
            companion1.StarveDayNum = 1;
            companion1.AddNewMount();
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            companion2.Food = 2;
            companion2.StarveDayNum = 2;
            companion2.AddNewMount();
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            companion3.Food = 2;
            companion3.StarveDayNum = 3;
            companion3.AddNewMount();
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[9])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 3;
            prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            companion1.Food = 0;
            companion1.StarveDayNum = 1;
            companion1.AddNewMount();
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            companion2.Food = 0;
            companion2.StarveDayNum = 2;
            companion2.AddNewMount();
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            companion3.Food = 0;
            companion3.StarveDayNum = 3;
            companion3.AddNewMount();
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[10])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 3;
            prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            companion1.Food = 0;
            companion1.StarveDayNum = 1;
            companion1.AddNewMount();
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            companion2.Food = 0;
            companion2.StarveDayNum = 2;
            companion2.AddNewMount();
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            companion3.Food = 0;
            companion3.StarveDayNum = 3;
            companion3.SetWounds(1, 2);
            companion3.AddNewMount();
            gi.IsPartyFed = true; // <<<<<<<<<<<<<<<<<<<<=================================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[11])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 3;
            prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            companion1.Food = 0;
            companion1.StarveDayNum = 1;
            companion1.AddNewMount();
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            companion2.Food = 0;
            companion2.StarveDayNum = 2;
            companion2.AddNewMount();
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            companion3.Food = 0;
            companion3.StarveDayNum = 3;
            companion3.AddNewMount();
            gi.IsMountsFed = true; // <<<<<<<<<<<<<<<<<<<<=================================
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[12])
         {
            t = gi.Territories.Find("0306"); // Mountains
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t=null");
               return false;
            }
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 6;
            prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            IMapItem companion1 = AddCompanion(ref gi, "Mercenary");
            if (null == companion1)
               return false;
            companion1.Food = 8;
            companion1.StarveDayNum = 1;
            companion1.AddNewMount();
            IMapItem companion2 = AddCompanion(ref gi, "Porter");
            if (null == companion2)
               return false;
            companion2.Food = 8;
            companion2.StarveDayNum = 2;
            companion2.AddNewMount();
            IMapItem companion3 = AddCompanion(ref gi, "Witch");
            if (null == companion3)
               return false;
            companion3.Food = 8;
            companion3.StarveDayNum = 3;
            companion3.AddNewMount();
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[13]) //<<<<<<<<<<<<<<<<<<<<<<<<<<<========================== PRINCE ONLY
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 2;
            prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[14])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 2;
            prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[15])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 5;
            prince.StarveDayNum = 1;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[16])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 5;
            prince.StarveDayNum = 3;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[17])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 1;
            prince.StarveDayNum = 1;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[18])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 0;
            prince.StarveDayNum = 1;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[19])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 2;
            prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1); // 1 mount
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[20])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 2;
            prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1); // 1 mount
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[21])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 2;
            prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1); // 1 mount
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[22])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 2;
            prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 1);
            gi.IsPartyFed = true;
            gi.IsMountsFed = true;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[23])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 5;
            prince.StarveDayNum = 0;
            AddPrinceMounts(ref gi, 2);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireStarvationCheck);
         }
         else if (CommandName == myCommandNames[24])
         {
            IMapItem prince = AddPrince(ref gi, t);
            if (null == prince)
               return false;
            prince.Food = 2;
            prince.StarveDayNum = 0;
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
      private IMapItem AddPrince(ref IGameInstance gi, ITerritory t )
      {
         IMapItems partyMembers = gi.PartyMembers;
         partyMembers.Clear();
         IMapItem prince = gi.MapItems.Find("Prince");
         if (null == prince)
            Logger.Log(LogEnum.LE_ERROR, "AddPrince(): mi=null");
         prince.Territory = t;
         prince.Reset();
         prince.SetWounds(1, 1);
         prince.AddSpecialItemToShare(SpecialEnum.HealingPoition);
         prince.AddSpecialItemToShare(SpecialEnum.CurePoisonVial);
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
      private IMapItem AddCompanion(ref IGameInstance gi, string name)
      {
         IMapItems partyMembers = gi.PartyMembers;
         IMapItem companion1 = gi.MapItems.Find(name);
         if (null == companion1)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddCompanions(): mi=null for name=" + name);
            return null;
         }
         IMapItem clone = new MapItem(companion1);
         //clone.SetWounds(2, 1);
         //clone.AddSpecialItemToKeep(SpecialEnum.HealingPoition);
         //clone.AddSpecialItemToKeep(SpecialEnum.CurePoisonVial);
         partyMembers.Add(clone);
         return companion1;
      }
      private IMapItem AddHireling(ref IGameInstance gi, string name)
      {
         IMapItems partyMembers = gi.PartyMembers;
         IMapItem companion1 = gi.MapItems.Find(name);
         if (null == companion1)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddCompanions(): mi=null for name=" + name);
            return null;
         }
         IMapItem clone = new MapItem(companion1);
         clone.Wages = 2;
         partyMembers.Add(clone);
         return clone;
      }
      private IMapItem AddHirelingGroup(ref IGameInstance gi, string name, int groupNum, int days)
      {
         IMapItems partyMembers = gi.PartyMembers;
         IMapItem companion1 = gi.MapItems.Find(name);
         if (null == companion1)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddHirelingGroup(): mi=null for name=" + name);
            return null;
         }
         IMapItem clone = new MapItem(companion1);
         clone.Wages = 2;
         clone.GroupNum = groupNum;
         clone.PayDay = days;
         partyMembers.Add(clone);
         return clone;
      }
   }
}
