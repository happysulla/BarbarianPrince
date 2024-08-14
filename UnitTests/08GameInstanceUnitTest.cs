using System.Collections.Generic;

namespace BarbarianPrince
{
   public class GameInstanceUnitTest : IUnitTest
   {
      public bool CtorError { get; } = false;
      GameInstanceDialog myDialog = null;
      //-----------------------------------------------------------
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      //-----------------------------------------------------------
      public GameInstanceUnitTest(IGameInstance gi)
      {
         //------------------------------------------
         myIndexName = 0;
         myHeaderNames.Add("08-Show Dialog");
         myHeaderNames.Add("08-Duplicate Mounts?");
         myHeaderNames.Add("08-Transfer Mounts");
         myHeaderNames.Add("08-Add Mounts");
         myHeaderNames.Add("08-Add Mounts2");
         myHeaderNames.Add("08-True Love Add");
         myHeaderNames.Add("08-True Love Remove");
         myHeaderNames.Add("08-True Love Triangle");
         myHeaderNames.Add("08-Add Food 11");
         myHeaderNames.Add("08-Reduce Food 5");
         myHeaderNames.Add("08-Add Coin 55");
         myHeaderNames.Add("08-Reduce Coin 25");
         myHeaderNames.Add("08-Add Coin 110");
         myHeaderNames.Add("08-Add Food 3");
         myHeaderNames.Add("08-Reduce Coin 166");
         myHeaderNames.Add("08-Add Coin 152 w/ Looter");
         myHeaderNames.Add("08-Add Coin 500 w/ 2 Looters");
         myHeaderNames.Add("08-Kill PartyMembers");
         myHeaderNames.Add("08-Add Food 12");
         myHeaderNames.Add("08-Remove Leaderless Party");
         myHeaderNames.Add("08-Add Food 12");
         myHeaderNames.Add("08-Remove Abandoner-14");
         myHeaderNames.Add("08-Finish");
         //------------------------------------------
         myCommandNames.Add("00-Show Dialog");
         myCommandNames.Add("01-Duplicate Mounts");
         myCommandNames.Add("02-Transfer Mounts");
         myCommandNames.Add("03-Add Mounts");
         myCommandNames.Add("04-Add Mounts2");
         myCommandNames.Add("05-Add True Love");
         myCommandNames.Add("06-Remove True Love");
         myCommandNames.Add("07-True Love Traingle");
         myCommandNames.Add("08-Change Food-1");
         myCommandNames.Add("09-Change Food-2");
         myCommandNames.Add("10-Change Coin-3");
         myCommandNames.Add("11-Change Coin-4");
         myCommandNames.Add("12-Change Coin-5");
         myCommandNames.Add("13-Change Food-6");
         myCommandNames.Add("14-Change Coin-7");
         myCommandNames.Add("15-Change Coin-8");
         myCommandNames.Add("16-Change Coin-9");
         myCommandNames.Add("17-Kill Party Members");
         myCommandNames.Add("18-Add Food");
         myCommandNames.Add("19-Remove Leaderless");
         myCommandNames.Add("20-Add Food-2");
         myCommandNames.Add("21-Remove Abandoner");
         myCommandNames.Add("22-Finish");
         //------------------------------------------
         gi.PartyMembers.Clear();
         gi.Prince.Reset();
         gi.PartyMembers.Add(gi.Prince);
         //------------------------------------------
         string magicianName = "Magician" + Utilities.MapItemNum.ToString();
         ++Utilities.MapItemNum;
         IMapItem magician = new MapItem(magicianName, 1.0, false, false, false, "c16Magician", "c16Magician", gi.Prince.Territory, 5, 5, 0);
         gi.AddCompanion(magician);
         //------------------------------------------
         string dwarfName = "Dwarf" + Utilities.MapItemNum.ToString();
         ++Utilities.MapItemNum;
         IMapItem dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
         gi.AddCompanion(dwarf);
         //------------------------------------------
         string miName2 = "Runaway" + Utilities.MapItemNum.ToString();
         ++Utilities.MapItemNum;
         IMapItem runaway = new MapItem(miName2, 1.0, false, false, false, "c09Runaway", "c09Runaway", gi.Prince.Territory, 4, 2, 0);
         gi.AddCompanion(runaway);
         //------------------------------------------
         string mercenaryName = "Mercenary" + Utilities.MapItemNum.ToString();
         ++Utilities.MapItemNum;
         IMapItem mercenary = new MapItem(mercenaryName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 5, 5, 0);
         mercenary.Food = 5;
         mercenary.Coin = 98;
         //mercenary.AddNewMount();  // riding
         mercenary.AddNewMount(MountEnum.Pegasus); // flying
         gi.AddCompanion(mercenary);
         //------------------------------------------
         string porterName2 = "PorterSlave" + Utilities.MapItemNum.ToString();
         ++Utilities.MapItemNum;
         IMapItem porter2 = new MapItem(porterName2, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", gi.Prince.Territory, 0, 0, 0);
         gi.AddCompanion(porter2);
         //------------------------------------------
         string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
         ++Utilities.MapItemNum;
         IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
         eagle.IsFlying = true;
         gi.AddCompanion(eagle);
         //------------------------------------------
         myDialog = new GameInstanceDialog(gi);
      }
      public bool Command(ref IGameInstance gi)
      {
         gi.WitAndWile = 3;
         if (CommandName == myCommandNames[0]) // Show Dialog
         {
            myDialog.Show();
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[1]) // Add duplicate mount and check for error
         {
            gi.PartyMembers.Clear();
            gi.Prince.Reset();
            gi.PartyMembers.Add(gi.Prince);
            //------------------------------------------
            string magicianName = "Magician" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem magician = new MapItem(magicianName, 1.0, false, false, false, "c16Magician", "c16Magician", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(magician);
            //------------------------------------------
            IMapItems mounts = new MapItems();
            for (int i = 0; i < 4; i++)
               gi.AddNewMountToParty();
            string name = "Horse" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem horse = new MapItem(name, 1.0, false, false, false, "MHorse", "", gi.Prince.Territory, 0, 0, 0);
            gi.Prince.AddMount(horse);
            for (int i = 0; i < 2; i++)
               gi.AddNewMountToParty();
            MapItem horse1 = new MapItem(name, 1.0, false, false, false, "MHorse", "", gi.Prince.Territory, 0, 0, 0);
            magician.AddMount(horse1);
            if( false == gi.IsDuplicateMount() )
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): 1-IsDuplicateMount() returned false and should have returned true");
               return false;
            }
            //------------------------------------------
            if (true == gi.IsDuplicateMount()) // the second time should work since duplicate was removed
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): 2-IsDuplicateMount() returned false and should have returned true");
               return false;
            }
            //------------------------------------------
            gi.Prince.Mounts.Rotate(1); // See if rotate of many horses causes error
            if (true == gi.IsDuplicateMount()) // the second time should work since duplicate was removed
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): 3-IsDuplicateMount() returned false and should have returned true");
               return false;
            }
            //------------------------------------------
            for(int i = 0; i<5; ++i)
               gi.Prince.Mounts.Rotate(1); // See if rotate of many horses causes error
            if (true == gi.IsDuplicateMount()) // the second time should work since duplicate was removed
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): 4-IsDuplicateMount() returned false and should have returned true");
               return false;
            }
            //------------------------------------------
            gi.Prince.Mounts.Clear();
            gi.AddNewMountToParty();
            gi.Prince.Mounts.Rotate(1); // See if rotate of one horse causes error
            if (true == gi.IsDuplicateMount()) // the second time should work since duplicate was removed
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): 5-IsDuplicateMount() returned false and should have returned true");
               return false;
            }
            gi.Prince.Mounts.Rotate(1); // See if rotate of one horse causes error
            if (true == gi.IsDuplicateMount()) // the second time should work since duplicate was removed
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): 6-IsDuplicateMount() returned false and should have returned true");
               return false;
            }
         }
         else if (CommandName == myCommandNames[2]) // Transfer Mounts
         {
            gi.PartyMembers.Clear();
            gi.Prince.Reset();
            gi.PartyMembers.Add(gi.Prince);
            //------------------------------------------
            string magicianName = "Magician" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem magician = new MapItem(magicianName, 1.0, false, false, false, "c16Magician", "c16Magician", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(magician);
            //------------------------------------------
            string dwarfName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(dwarf);
            //------------------------------------------
            string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
            eagle.IsFlying = true;
            gi.AddCompanion(eagle);
            IMapItems mounts = new MapItems();
            for (int i = 0; i < 12; i++)
            {
               string name = "Horse" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               MapItem horse = new MapItem(name, 1.0, false, false, false, "MHorse", "", gi.Prince.Territory, 0, 0, 0);
               mounts.Add(horse);
            }
            for (int i = 0; i < 2; i++)
            {
               string name = "Pegasus" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               MapItem pegasus = new MapItem(name, 1.0, false, false, false, "MPegasus", "", gi.Prince.Territory, 0, 0, 0);
               mounts.Add(pegasus);
            }
            gi.TransferMounts(mounts);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[3]) // Add Mounts
         {
            gi.PartyMembers.Clear();
            gi.Prince.Reset();
            gi.PartyMembers.Add(gi.Prince);
            //------------------------------------------
            string magicianName = "Magician" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem magician = new MapItem(magicianName, 1.0, false, false, false, "c16Magician", "c16Magician", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(magician);
            //------------------------------------------
            string dwarfName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(dwarf);
            //------------------------------------------
            string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
            eagle.IsFlying = true;
            gi.AddCompanion(eagle);
            string griffonName = "Griffon" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem griffon = new MapItem(griffonName, 1.0, false, false, false, "c63Griffon", "c63Griffon", gi.Prince.Territory, 3, 4, 1);
            griffon.IsFlying = true;
            gi.AddCompanion(griffon);
            IMapItems mounts = new MapItems();
            for (int i = 0; i < 3; i++)
            {
               string name = "Horse" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               MapItem horse = new MapItem(name, 1.0, false, false, false, "MHorse", "", gi.Prince.Territory, 0, 0, 0);
               mounts.Add(horse);
            }
            for (int i = 0; i < 1; i++)
            {
               string name = "Pegasus" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               MapItem pegasus = new MapItem(name, 1.0, false, false, false, "MPegasus", "", gi.Prince.Territory, 0, 0, 0);
               mounts.Add(pegasus);
            }
            mounts.Add(griffon);
            gi.TransferMounts(mounts);
            gi.AddNewMountToParty(MountEnum.Horse);
            gi.AddNewMountToParty(MountEnum.Pegasus);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[4]) // 
         {
            gi.PartyMembers.Clear();
            gi.PartyMembers.Add(gi.Prince);
            //------------------------------------------
            string magicianName = "Magician" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem magician = new MapItem(magicianName, 1.0, false, false, false, "c16Magician", "c16Magician", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(magician);
            //------------------------------------------
            string dwarfName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(dwarf);
            //------------------------------------------
            string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
            eagle.IsFlying = true;
            gi.AddCompanion(eagle);
            //------------------------------------------
            IMapItems mounts = new MapItems();
            for (int i = 0; i < 12; i++)
            {
               string name = "Horse" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               MapItem horse = new MapItem(name, 1.0, false, false, false, "MHorse", "", gi.Prince.Territory, 0, 0, 0);
               mounts.Add(horse);
            }
            for (int i = 0; i < 2; i++)
            {
               string name = "Pegasus" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               MapItem pegasus = new MapItem(name, 1.0, false, false, false, "MPegasus", "", gi.Prince.Territory, 0, 0, 0);
               mounts.Add(pegasus);
            }
            gi.TransferMounts(mounts);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[5]) // Add True Love
         {
            gi.PartyMembers.Clear();
            gi.PartyMembers.Add(gi.Prince);
            //------------------------------------------
            string magicianName = "Magician" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem magician = new MapItem(magicianName, 1.0, false, false, false, "c16Magician", "c16Magician", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(magician);
            //------------------------------------------
            string dwarfName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(dwarf);
            //------------------------------------------
            string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(trueLove);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[6]) // Remove True Love
         {
            gi.PartyMembers.Clear();
            gi.PartyMembers.Add(gi.Prince);
            //------------------------------------------
            string magicianName = "Magician" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem magician = new MapItem(magicianName, 1.0, false, false, false, "c16Magician", "c16Magician", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(magician);
            //------------------------------------------
            string dwarfName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(dwarf);
            //------------------------------------------
            string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(trueLove);
            gi.RemoveAbandonedInParty(trueLove);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[7]) // Remove True Love Triangle
         {
            gi.PartyMembers.Clear();
            gi.PartyMembers.Add(gi.Prince);
            //------------------------------------------
            string magicianName = "Magician" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem magician = new MapItem(magicianName, 1.0, false, false, false, "c16Magician", "c16Magician", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(magician);
            //------------------------------------------
            string dwarfName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(dwarf);
            //------------------------------------------
            string eagleName = "Eagle" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem eagle = new MapItem(eagleName, 1.0, false, false, false, "c62Eagle", "c62Eagle", gi.Prince.Territory, 3, 4, 1);
            eagle.IsFlying = true;
            gi.AddCompanion(eagle);
            string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(trueLove);
            trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(trueLove);
            gi.RemoveAbandonedInParty(trueLove);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[8]) // Add Foods 
         {
            gi.AddFoods(11);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[9]) // Reduce Foods
         {
            gi.ReduceFoods(5);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[10])  // Add Coin
         {
            gi.AddCoins(355);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[11])
         {
            gi.ReduceCoins(25);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[12])
         {
            gi.AddCoins(110);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[13])
         {
            gi.AddFoods(3);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[14])
         {
            gi.ReduceCoins(166);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[15])
         {
            gi.AddCoins(152);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[16])
         {
            gi.AddCoins(500);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[17]) // kill party members
         {
            foreach (IMapItem mi in gi.PartyMembers)
            {
               if (("Prince" == mi.Name) || (true == mi.IsKilled))
                  continue;
               mi.IsKilled = true;
               break;
            }
            foreach (IMapItem mi in gi.PartyMembers)
            {
               if (("Prince" == mi.Name) || (true == mi.IsKilled))
                  continue;
               mi.IsUnconscious = true;
               break;
            }
            gi.ProcessIncapacitedPartyMembers("UnitTest");
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[18])
         {
            gi.AddFoods(12);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[19])
         {
            gi.RemoveLeaderlessInParty();
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[20])
         {
            gi.AddFoods(12);
            gi.AddCoins(733);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[21])
         {
            foreach (IMapItem mi in gi.PartyMembers) // set one to unconscious
            {
               if ("Prince" == mi.Name)
                  continue;
               gi.RemoveAbandonerInParty(mi);
               break;
            }
            myDialog.UpdateGridRows(gi);
         }
         else
         {
            if (false == Cleanup(ref gi))
               Logger.Log(LogEnum.LE_ERROR, "Command(): Cleanup() returned error");
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
         else if (HeaderName == myHeaderNames[14]) // make looter for 7
         {
            ++myIndexName;
            foreach (IMapItem mi in gi.PartyMembers)
            {
               if (("Prince" == mi.Name) || (true == mi.IsLooter))
                  continue;
               mi.IsLooter = true;
               break;
            }
         }
         else if (HeaderName == myHeaderNames[15]) // make looter for 8
         {
            ++myIndexName;
            foreach (IMapItem mi in gi.PartyMembers)
            {
               if (("Prince" == mi.Name) || (true == mi.IsLooter))
                  continue;
               mi.IsLooter = true;
               break;
            }
         }
         else if (HeaderName == myHeaderNames[16]) // reconstitute party
         {
            ++myIndexName;
            gi.PartyMembers.Clear();
            gi.PartyMembers.Add(gi.Prince);
            //------------------------------------------
            string magicianName = "Magician" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem magician = new MapItem(magicianName, 1.0, false, false, false, "c16Magician", "c16Magician", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(magician);
            //------------------------------------------
            string dwarfName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(dwarf);
            //------------------------------------------
            string miName2 = "Runaway" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem runaway = new MapItem(miName2, 1.0, false, false, false, "c09Runaway", "c09Runaway", gi.Prince.Territory, 4, 2, 0);
            gi.AddCompanion(runaway);
            //------------------------------------------
            string mercenaryName = "Mercenary" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem mercenary = new MapItem(mercenaryName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 5, 5, 0);
            mercenary.Food = 5;
            mercenary.Coin = 98;
            //mercenary.AddNewMount();  // riding
            mercenary.AddNewMount(MountEnum.Pegasus); // flying
            gi.AddCompanion(mercenary);
            //------------------------------------------
            string porterName2 = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem porter2 = new MapItem(porterName2, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(porter2);
            //------------------------------------------
            foreach (IMapItem mi in gi.PartyMembers) // set one to unconscious
            {
               if (("Prince" == mi.Name) && (false == mi.IsUnconscious))
                  continue;
               mi.IsUnconscious = true;
               break;
            }
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
            gi.PartyMembers.Clear();
            gi.PartyMembers.Add(gi.Prince);
            //------------------------------------------
            string magicianName = "Magician" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem magician = new MapItem(magicianName, 1.0, false, false, false, "c16Magician", "c16Magician", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(magician);
            //------------------------------------------
            string dwarfName = "Dwarf" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem dwarf = new MapItem(dwarfName, 1.0, false, false, false, "c08Dwarf", "c08Dwarf", gi.Prince.Territory, 5, 5, 0);
            gi.AddCompanion(dwarf);
            //------------------------------------------
            string miName2 = "Runaway" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem runaway = new MapItem(miName2, 1.0, false, false, false, "c09Runaway", "c09Runaway", gi.Prince.Territory, 4, 2, 0);
            gi.AddCompanion(runaway);
            //------------------------------------------
            string mercenaryName = "Mercenary" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem mercenary = new MapItem(mercenaryName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 5, 5, 0);
            mercenary.Food = 5;
            mercenary.Coin = 98;
            //mercenary.AddNewMount();  // riding
            mercenary.AddNewMount(MountEnum.Pegasus); // flying
            gi.AddCompanion(mercenary);
            //------------------------------------------
            string porterName2 = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem porter2 = new MapItem(porterName2, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", gi.Prince.Territory, 0, 0, 0);
            gi.AddCompanion(porter2);
            //------------------------------------------
         }
         else if (HeaderName == myHeaderNames[20])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[21])
         {
            ++myIndexName;
         }
         else
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup() returned false");
               return false;
            }
         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi)
      {
         myDialog.Close();
         gi.PartyMembers.Clear();
         gi.PartyMembers.Add(gi.Prince);
         ++gi.GameTurn;
         return true;
      }
   }
}
