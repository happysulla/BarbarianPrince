using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Xml.Linq;

namespace BarbarianPrince
{
   internal class TransportMgrUnitTest : IUnitTest
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
      public TransportMgrUnitTest(EventViewer ev)
      {
         myIndexName = 0;
         myHeaderNames.Add("13-Party Sunstroke");
         //------------------------------------------
         myHeaderNames.Add("13-Party Exhausted");
         myHeaderNames.Add("13-Prince w/ 3 Mounts");
         myHeaderNames.Add("13-Prince w/ 1 Mounts, 1 Starve Day");
         //------------------------------------------
         myHeaderNames.Add("13-Prince w/ Unc Dwarf");
         myHeaderNames.Add("13-Prince w/ 1 Unc, 1 Mount");
         myHeaderNames.Add("13-Prince w/ 1 Unc, 2 Mount");
         //------------------------------------------
         myHeaderNames.Add("13-Prince w/ Griffon");
         myHeaderNames.Add("13-Prince w/ 2 Mounts & Griffon");
         myHeaderNames.Add("13-Prince w/ Eagles");
         myHeaderNames.Add("13-Party w/ Eagles");
         myHeaderNames.Add("13-Party w/ 2 Mounts & Eagles");
         //------------------------------------------
         myHeaderNames.Add("13-Party w/ Unc");
         myHeaderNames.Add("13-Party w/ Unc, 3 Mounts, Eagles");
         myHeaderNames.Add("13-Party w/ Unc, 2 Mounts, 2 Griffon");
         myHeaderNames.Add("13-Party w/ 3 Unc, Eagles");
         myHeaderNames.Add("13-Party w/ 3 Unc, 1 Mount");
         myHeaderNames.Add("13-Party w/ 3 Unc, 2 Mount");
         myHeaderNames.Add("13-Party w/ 3 Unc, 3 Mount");
         myHeaderNames.Add("13-Finish");
         //------------------------------------------
         myCommandNames.Add("00-Show Loads");
         myCommandNames.Add("01-Show Loads");
         myCommandNames.Add("02-Show Loads");
         myCommandNames.Add("03-Show Loads");
         myCommandNames.Add("04-Show Loads");
         myCommandNames.Add("05-Show Loads");
         myCommandNames.Add("06-Show Loads");
         myCommandNames.Add("07-Show Loads");
         myCommandNames.Add("08-Show Loads");
         myCommandNames.Add("09-Show Loads");
         myCommandNames.Add("10-Show Loads");
         myCommandNames.Add("11-Show Loads");
         myCommandNames.Add("12-Show Loads");
         myCommandNames.Add("13-Show Loads");
         myCommandNames.Add("14-Show Loads");
         myCommandNames.Add("15-Show Loads");
         myCommandNames.Add("16-Show Loads");
         myCommandNames.Add("17-Show Loads");
         myCommandNames.Add("18-Show Loads");
         myCommandNames.Add("Finish");
         //------------------------------------------
         if (null == ev)
         {
            Logger.Log(LogEnum.LE_ERROR, "TransportMgrUnitTest(): ev=null");
            CtorError = true;
            return;
         }
         myEventViewer = ev;
      }
      public bool Command(ref IGameInstance gi)
      {
         gi.PartyMembers.Clear();
         gi.Prince.Reset();
         ITerritory t = gi.Territories.Find("1005"); // Mountains
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): t=null");
            return false;
         }
         gi.Prince.Territory = t;
         gi.Prince.Food = 1;
         gi.Prince.Coin = 1;
         if (CommandName == myCommandNames[0]) // Party with Sunstroke - e121
         {
            gi.Prince.IsSunStroke = true;
            //----------------------------------------------------------------
            string mercenaryName = "Mercenary" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem mercenary = new MapItem(mercenaryName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 5, 5, 0);
            mercenary.Food = 10;
            mercenary.Coin = 5;
            mercenary.IsSunStroke = true;
            gi.AddCompanion(mercenary);
            //----------------------------------------------------------------
            string dwarfName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            dwarf.Food = 4;
            dwarf.Coin = 301;
            //dwarf.IsRiding = true;  
            dwarf.AddNewMount();
            gi.AddCompanion(dwarf);
            //----------------------------------------------------------------
            string monkName = "Monk" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem monk = new MapItem(monkName, 1.0, false, false, false, "c19Monk", "c19Monk", gi.Prince.Territory, 5, 5, 0);
            monk.Food = 5;
            gi.AddCompanion(monk);
            //----------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[1]) // Party w/ Unc, 2 Mounts, 1 Griffon, and Exhausted
         {
            AddPrinceMounts(ref gi, 2);
            AddCompanions(ref gi);
            //----------------------------------------------------------------
            string miName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            dwarf.IsUnconscious = true;
            dwarf.Wound = dwarf.Endurance - 1;
            dwarf.Food = 0;
            dwarf.Coin = 0;
            gi.PartyMembers.Add(dwarf);
            //---------------------------------------------------------------
            string griffonName = "Griffon" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem griffon = new MapItem(griffonName, 1.0, false, false, false, "c63Griffon", "c63Griffon", gi.Prince.Territory, 3, 4, 1);
            griffon.IsFlying = true;
            griffon.IsRiding = true;
            gi.AddCompanion(griffon);
            //----------------------------------------- e120 - Exhaust the Party
            gi.IsExhausted = true;
            foreach (IMapItem mi in gi.PartyMembers)
            {
               if (null != mi.Rider)
               {
                  mi.Rider.Mounts.Remove(mi);  // Griffon removes its rider
                  mi.Rider = null;
               }
            }
            foreach (IMapItem mi in gi.PartyMembers)
            {
               mi.IsExhausted = true;
               mi.SetWounds(1, 0); // each party member suffers one wound
               foreach (IMapItem mount in mi.Mounts)
                  mount.IsExhausted = true;
               if ((false == mi.Name.Contains("Griffon")) && (false == mi.Name.Contains("Eagle")))
                  mi.IsRiding = false;
            }
            gi.RemoveKilledInParty("E120 Exhausted");
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[2]) //13-Prince w/ 3 Mounts
         {
            AddPrinceMounts(ref gi, 3);
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[3]) //Prince w/ 1 Mounts, 1 Starve Day
         {
            AddPrinceMounts(ref gi, 1);
            gi.Prince.StarveDayNum = 1;
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         //-----------------------------------------------------------
         else if (CommandName == myCommandNames[4]) //Prince w/ Unc Dwarf
         {
            // Prince does not have enough free load to carry the dwarf - one man can only carry half of a person
            //----------------------------------------------------------------
            string miName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            dwarf.IsUnconscious = true;
            dwarf.Wound = dwarf.Endurance - 1;
            dwarf.Food = 0;
            dwarf.Coin = 0;
            gi.PartyMembers.Add(dwarf);
            //---------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[5]) // Prince w/ 1 Unc, 1 Mount
         {
            AddPrinceMounts(ref gi, 1);
            //----------------------------------------------------------------
            string miName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            dwarf.IsUnconscious = true;
            dwarf.Wound = dwarf.Endurance - 1;
            dwarf.Food = 0;
            dwarf.Coin = 0;
            gi.PartyMembers.Add(dwarf);
            //---------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[6]) //Prince w/ 1 Unc, 2 Mount
         {
            AddPrinceMounts(ref gi, 2);
            //----------------------------------------------------------------
            string miName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            dwarf.IsUnconscious = true;
            dwarf.Wound = dwarf.Endurance - 1;
            dwarf.Food = 0;
            dwarf.Coin = 0;
            gi.PartyMembers.Add(dwarf);
            //---------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         //-----------------------------------------------------------
         else if (CommandName == myCommandNames[7]) // Prince w/ Griffon 
         {;
            gi.Prince.Food = 5;
            //----------------------------------------------------------------
            string griffonName = "Griffon" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem griffon = new MapItem(griffonName, 1.0, false, false, false, "c63Griffon", "c63Griffon", gi.Prince.Territory, 3, 4, 1);
            griffon.IsFlying = true;
            gi.AddCompanion(griffon);
            //----------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[8]) // Party w/ 2 Mounts & Griffon
         {
            gi.Prince.Food = 10;
            AddCompanions(ref gi);
            AddPrinceMounts(ref gi, 2);
            //----------------------------------------------------------------
            string griffonName = "Griffon" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem griffon = new MapItem(griffonName, 1.0, false, false, false, "c63Griffon", "c63Griffon", gi.Prince.Territory, 3, 4, 1);
            griffon.IsFlying = true;
            gi.AddCompanion(griffon);
            //----------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[9]) // Prince w/ Eagles
         {;
            for (int i = 0; i < 3; ++i)
            {
               string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
               eagle.IsFlying = true;
               gi.AddCompanion(eagle);
            }
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[10])  // Party w/ Eagles
         {
            AddCompanions(ref gi);
            for (int i = 0; i < 3; ++i)
            {
               string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
               eagle.IsFlying = true;
               gi.AddCompanion(eagle);
            }
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[11]) // Party w/ 2 Mounts & Eagles
         {
            AddPrinceMounts(ref gi, 2);
            AddCompanions(ref gi);
            for (int i = 0; i < 3; ++i)
            {
               string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
               eagle.IsFlying = true;
               gi.AddCompanion(eagle);
            }
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         //-----------------------------------------------------------
         else if (CommandName == myCommandNames[12]) //Party w/ Unc
         {
            AddCompanions(ref gi);
            //----------------------------------------------------------------
            string miName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            dwarf.IsUnconscious = true;
            dwarf.Wound = dwarf.Endurance - 1;
            dwarf.Food = 0;
            dwarf.Coin = 0;
            gi.PartyMembers.Add(dwarf);
            //---------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[13]) //Party w/ Unc, 3 Mounts, Eagles
         {
            AddPrinceMounts(ref gi, 3);
            //----------------------------------------------------------------
            AddCompanions(ref gi);
            //----------------------------------------------------------------
            string miName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            dwarf.IsUnconscious = true;
            dwarf.Wound = dwarf.Endurance - 1;
            dwarf.Food = 0;
            dwarf.Coin = 0;
            gi.PartyMembers.Add(dwarf);
            //----------------------------------------------------------------
            for (int i = 0; i < 3; ++i)
            {
               string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
               eagle.IsFlying = true;
               gi.AddCompanion(eagle);
            }
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[14]) // Party w/ Unc, 2 Mounts, 2 Griffon
         {
            AddPrinceMounts(ref gi, 2);
            AddCompanions(ref gi);
            //----------------------------------------------------------------
            string miName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            dwarf.IsUnconscious = true;
            dwarf.Wound = dwarf.Endurance - 1;
            dwarf.Food = 0;
            dwarf.Coin = 0;
            gi.PartyMembers.Add(dwarf);
            //----------------------------------------------------------------
            for (int i = 0; i < 2; ++i)
            {
               string griffonName = "Griffon" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem griffon = new MapItem(griffonName, 1.0, false, false, false, "c63Griffon", "c63Griffon", gi.Prince.Territory, 3, 4, 1);
               griffon.IsFlying = true;
               gi.AddCompanion(griffon);
            }
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[15])
         {
            //----------------------------------------------------------------
            AddCompanions(ref gi);
            //----------------------------------------------------------------
            string miName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            dwarf.IsUnconscious = true;
            dwarf.Wound = dwarf.Endurance - 1;
            dwarf.Food = 0;
            dwarf.Coin = 0;
            gi.PartyMembers.Add(dwarf);
            //----------------------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem witch = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            witch.IsUnconscious = true;
            witch.Wound = witch.Endurance - 1;
            witch.Food = 0;
            witch.Coin = 0;
            gi.PartyMembers.Add(witch);
            //----------------------------------------------------------------
            miName = "Runaway" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem runaway = new MapItem(miName, 1.0, false, false, false, "c09Runaway", "c09Runaway", gi.Prince.Territory, 4, 4, 0);
            runaway.IsUnconscious = true;
            runaway.Wound = witch.Endurance - 1;
            runaway.Food = 0;
            runaway.Coin = 0;
            gi.PartyMembers.Add(runaway);
            //----------------------------------------------------------------
            for (int i = 0; i < 3; ++i)
            {
               string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
               eagle.IsFlying = true;
               gi.AddCompanion(eagle);
            }
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[16])
         {
            AddPrinceMounts(ref gi, 1);
            //----------------------------------------------------------------
            AddCompanions(ref gi);
            //----------------------------------------------------------------
            string miName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            dwarf.IsUnconscious = true;
            dwarf.Wound = dwarf.Endurance - 1;
            dwarf.Food = 0;
            dwarf.Coin = 0;
            gi.PartyMembers.Add(dwarf);
            //----------------------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem witch = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            witch.IsUnconscious = true;
            witch.Wound = witch.Endurance - 1;
            witch.Food = 0;
            witch.Coin = 0;
            gi.PartyMembers.Add(witch);
            //----------------------------------------------------------------
            miName = "Runaway" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem runaway = new MapItem(miName, 1.0, false, false, false, "c09Runaway", "c09Runaway", gi.Prince.Territory, 4, 4, 0);
            runaway.IsUnconscious = true;
            runaway.Wound = witch.Endurance - 1;
            runaway.Food = 0;
            runaway.Coin = 0;
            gi.PartyMembers.Add(runaway);
            //----------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[17])
         {
            AddPrinceMounts(ref gi, 2);
            //----------------------------------------------------------------
            AddCompanions(ref gi);
            //----------------------------------------------------------------
            string miName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            dwarf.IsUnconscious = true;
            dwarf.Wound = dwarf.Endurance - 1;
            dwarf.Food = 0;
            dwarf.Coin = 0;
            gi.PartyMembers.Add(dwarf);
            //----------------------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem witch = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            witch.IsUnconscious = true;
            witch.Wound = witch.Endurance - 1;
            witch.Food = 0;
            witch.Coin = 0;
            gi.PartyMembers.Add(witch);
            //----------------------------------------------------------------
            miName = "Runaway" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem runaway = new MapItem(miName, 1.0, false, false, false, "c09Runaway", "c09Runaway", gi.Prince.Territory, 4, 4, 0);
            runaway.IsUnconscious = true;
            runaway.Wound = witch.Endurance - 1;
            runaway.Food = 0;
            runaway.Coin = 0;
            gi.PartyMembers.Add(runaway);
            //----------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
         }
         else if (CommandName == myCommandNames[18])
         {
            AddPrinceMounts(ref gi, 3);
            //----------------------------------------------------------------
            AddCompanions(ref gi);
            //----------------------------------------------------------------
            string miName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(miName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            dwarf.IsUnconscious = true;
            dwarf.Wound = dwarf.Endurance - 1;
            dwarf.Food = 0;
            dwarf.Coin = 0;
            gi.PartyMembers.Add(dwarf);
            //----------------------------------------------------------------
            miName = "Witch" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem witch = new MapItem(miName, 1.0, false, false, false, "c13Witch", "c13Witch", gi.Prince.Territory, 3, 1, 5);
            witch.IsUnconscious = true;
            witch.Wound = witch.Endurance - 1;
            witch.Food = 0;
            witch.Coin = 0;
            gi.PartyMembers.Add(witch);
            //----------------------------------------------------------------
            miName = "Runaway" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem runaway = new MapItem(miName, 1.0, false, false, false, "c09Runaway", "c09Runaway", gi.Prince.Territory, 4, 4, 0);
            runaway.IsUnconscious = true;
            runaway.Wound = witch.Endurance - 1;
            runaway.Food = 0;
            runaway.Coin = 0;
            gi.PartyMembers.Add(runaway);
            //----------------------------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.CampfireLoadTransport);
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
      //-------------------------------------------------------------
      private void AddCompanions(ref IGameInstance gi)
      {
         IMapItems partyMembers = gi.PartyMembers;
         string miName = "Mercenary" + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         IMapItem companion = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
         companion.Food = 2;
         companion.Coin = 2;
         partyMembers.Add(companion);
         //---------------------------------------
         miName = "Porter" + Utilities.MapItemNum;
         Utilities.MapItemNum++;
         IMapItem companion2 = new MapItem(miName, 1.0, false, false, false, "c11Porter", "c11Porter", gi.Prince.Territory, 0, 0, 0);
         companion2.Food = 3;
         companion2.Coin = 3;
         partyMembers.Add(companion2);
         //---------------------------------------
         miName = "Wizard" + Utilities.MapItemNum;
         Utilities.MapItemNum++;
         IMapItem companion3 = new MapItem(miName, 1.0, false, false, false, "c12Wizard", "c12Wizard", gi.Prince.Territory, 4, 4, 60);
         companion3.Reset();
         companion3.Food = 4;
         companion3.Coin = 4;
         partyMembers.Add(companion3);
         //---------------------------------------
      }
      private void AddPrinceMounts(ref IGameInstance gi, int numMounts)
      {
         IMapItem prince = gi.Prince;
         if (null == prince)
            Logger.Log(LogEnum.LE_ERROR, "AddPrince(): mi=null");
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
   }
}
