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
         myHeaderNames.Add("00-Show Dialog");
         myHeaderNames.Add("01-Trasnfer Mounts");
         myHeaderNames.Add("02-Add Mounts");
         myHeaderNames.Add("03-True Love Add");
         myHeaderNames.Add("04-True Love Remove");
         myHeaderNames.Add("05-True Love Triangle");
         myHeaderNames.Add("06-Add Food 11");
         myHeaderNames.Add("07-Reduce Food 5");
         myHeaderNames.Add("08-Add Coin 55");
         myHeaderNames.Add("09-Reduce Coin 25");
         myHeaderNames.Add("10-Add Coin 110");
         myHeaderNames.Add("11-Add Food 3");
         myHeaderNames.Add("12-Reduce Coin 166");
         myHeaderNames.Add("13-Add Coin 152 w/ Looter");
         myHeaderNames.Add("14-Add Coin 500 w/ 2 Looters");
         myHeaderNames.Add("15-Kill PartyMembers");
         myHeaderNames.Add("16-Add Food 12");
         myHeaderNames.Add("17-Remove Leaderless Party");
         myHeaderNames.Add("18-Add Food 12");
         myHeaderNames.Add("19-Remove Abandoner-14");
         myHeaderNames.Add("20-Finish");
         //------------------------------------------
         myCommandNames.Add("Show Dialog");
         myCommandNames.Add("Trasnfer Mounts");
         myCommandNames.Add("Add Mounts");
         myCommandNames.Add("Add True Love");
         myCommandNames.Add("Remove True Love");
         myCommandNames.Add("True Love Traingle");
         myCommandNames.Add("Change Food-1");
         myCommandNames.Add("Change Food-2");
         myCommandNames.Add("Change Coin-3");
         myCommandNames.Add("Change Coin-4");
         myCommandNames.Add("Change Coin-5");
         myCommandNames.Add("Change Food-6");
         myCommandNames.Add("Change Coin-7");
         myCommandNames.Add("Change Coin-8");
         myCommandNames.Add("Change Coin-9");
         myCommandNames.Add("Kill Party Members");
         myCommandNames.Add("Add Food");
         myCommandNames.Add("Remove Leaderless");
         myCommandNames.Add("Add Food-2");
         myCommandNames.Add("Remove Abandoner");
         myCommandNames.Add("Finish");
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
         if (CommandName == myCommandNames[0])
         {
            myDialog.Show();
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[1])
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
         else if (CommandName == myCommandNames[2])
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
         else if (CommandName == myCommandNames[3])
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
         else if (CommandName == myCommandNames[4])
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
         else if (CommandName == myCommandNames[5])
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
         else if (CommandName == myCommandNames[6])
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
         else if (CommandName == myCommandNames[7])
         {
            gi.AddFoods(11);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[8])
         {
            gi.ReduceFoods(5);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[9])
         {
            gi.AddCoins(55);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[10])
         {
            gi.ReduceCoins(25);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[11])
         {
            gi.AddCoins(110);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[12])
         {
            gi.AddFoods(3);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[13])
         {
            gi.ReduceCoins(166);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[14])
         {
            gi.AddCoins(152);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[15])
         {
            gi.AddCoins(500);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[1164]) // kill party members
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
            gi.RemoveKilledInParty("UnitTest");
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[17])
         {
            gi.AddFoods(12);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[18])
         {
            gi.RemoveLeaderlessInParty();
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[19])
         {
            gi.AddFoods(12);
            gi.AddCoins(733);
            myDialog.UpdateGridRows(gi);
         }
         else if (CommandName == myCommandNames[20])
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
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup() returned error");
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
         else if (HeaderName == myHeaderNames[13]) // make looter for 7
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
         else if (HeaderName == myHeaderNames[14]) // make looter for 8
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
         else if (HeaderName == myHeaderNames[15]) // reconstitute party
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
         else if (HeaderName == myHeaderNames[19])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[20])
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
