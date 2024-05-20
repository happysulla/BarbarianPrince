using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace BarbarianPrince
{
   public class CombatUnitTest : IUnitTest
   {
      public bool CtorError { get; } = false;
      private EventViewer myEventViewer = null;
      private IGameInstance myGameInstance = null;
      //-----------------------------------------------------------
      private int myIndexName = 0;
      private int myIndexCombat = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      //-----------------------------------------------------------
      public CombatUnitTest(DockPanel dp, IGameInstance gi, EventViewer ev, IDieRoller dr)
      {
         //------------------------------------------
         myIndexName = 0;
         myIndexCombat = 0;
         myHeaderNames.Add("14-Black Knight");
         myHeaderNames.Add("14-Wolves");
         myHeaderNames.Add("14-Halfling");
         myHeaderNames.Add("14-Elf w/ Nerve Gas");
         myHeaderNames.Add("14-Wizard");
         myHeaderNames.Add("14-Poison");
         myHeaderNames.Add("14-Hunting Cat");
         myHeaderNames.Add("14-Wild Boar");
         myHeaderNames.Add("14-Spiders");
         myHeaderNames.Add("14-Cavalry");
         myHeaderNames.Add("14-Protector");
         myHeaderNames.Add("14-Mirror");
         myHeaderNames.Add("14-ResistenceRing");
         myHeaderNames.Add("14-Shield");
         myHeaderNames.Add("14-HydraTeeth");
         myHeaderNames.Add("14-Spectre");
         myHeaderNames.Add("14-More Encountered");
         myHeaderNames.Add("14-Equal Members");
         myHeaderNames.Add("14-More Party");
         //------------------------------------------
         myCommandNames.Add("00-Show Combat");
         myCommandNames.Add("01-Show Combat");
         myCommandNames.Add("02-Show Combat");
         myCommandNames.Add("03-Show Combat");
         myCommandNames.Add("04-Show Combat");
         myCommandNames.Add("05-Show Combat");
         myCommandNames.Add("06-Show Combat");
         myCommandNames.Add("07-Show Combat");
         myCommandNames.Add("08-Show Combat");
         myCommandNames.Add("09-Show Combat");
         myCommandNames.Add("10-Show Combat");
         myCommandNames.Add("11-Show Combat");
         myCommandNames.Add("12-Show Combat");
         myCommandNames.Add("13-Show Combat");
         myCommandNames.Add("14-Show Combat");
         myCommandNames.Add("15-Show Combat");
         myCommandNames.Add("16-Show Combat");
         myCommandNames.Add("17-Show Combat");
         myCommandNames.Add("18-Show Combat");
         //--------------------------------------------
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "CombatUnitTest(): svTB=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         gi.WitAndWile = 4;
         //--------------------------------------------
         if (null == ev)
         {
            Logger.Log(LogEnum.LE_ERROR, "CombatUnitTest(): ev=null");
            CtorError = true;
            return;
         }
         myEventViewer = ev;
      }
      public bool Command(ref IGameInstance gi)
      {
         gi.PartyMembers.Clear();
         gi.EncounteredMembers.Clear();
         gi.HydraTeethCount = 0;
         gi.IsCavalryEscort = false;
         gi.EventStart = "e029";
         if (CommandName == myCommandNames[0]) // Black Knight
         {
            gi.EventStart = "e123b";
            gi.EventActive = "e304";
            gi.Prince.ResetPartial();
            gi.Prince.Territory = gi.Territories.Find("0305");
            gi.Prince.AddSpecialItemToShare(SpecialEnum.ShieldOfLight);
            gi.Prince.AddSpecialItemToShare(SpecialEnum.ResistanceRing);
            gi.Prince.SetWounds(7, 0);
            gi.PartyMembers.Add(gi.Prince);
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            gi.PartyMembers.Add(companion1);
            IMapItem companion2 = AddCompanion(ref gi, "Mercenary");
            if (null == companion2)
               return false;
            gi.PartyMembers.Add(companion2);
            //---------------------
            string blackKnightName = "BlackNight" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem blackKnight = new MapItem(blackKnightName, 1.0, false, false, false, "c80BlackKnight", "c80BlackKnight", gi.Prince.Territory, 8, 8, 30);
            gi.EncounteredMembers.Add(blackKnight);
            gi.EventDisplayed = gi.EventActive = "e307";
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[1]) // Wolves
         {
            gi.EventStart = "e075b";
            gi.EventActive = "e309";
            gi.Prince.ResetPartial();
            gi.Prince.Territory = gi.Territories.Find("0305");
            gi.Prince.AddSpecialItemToShare(SpecialEnum.ShieldOfLight);
            gi.PartyMembers.Add(gi.Prince);
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            companion1.AddSpecialItemToShare(SpecialEnum.ResistanceRing);
            gi.PartyMembers.Add(companion1);
            IMapItem companion2 = AddCompanion(ref gi, "Mercenary");
            if (null == companion2)
               return false;
            gi.PartyMembers.Add(companion2);
            //---------------------
            gi.AddNewMountToParty();
            gi.AddNewMountToParty();
            gi.AddNewMountToParty();
            gi.AddNewMountToParty();
            gi.AddNewMountToParty();
            //---------------------
            gi.EncounteredMembers.Clear();
            for (int i = 0; i < 5; ++i)
            {
               string miName = "Wolf" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem wolf = new MapItem(miName, 1.0, false, false, false, "c71Wolf", "c71Wolf", gi.Prince.Territory, 3, 3, 0);
               gi.EncounteredMembers.Add(wolf);
            }
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[2]) // Halfling
         {
            gi.EventStart = "e008";
            gi.EventActive = "e304";
            gi.Prince.ResetPartial();
            gi.Prince.Territory = gi.Territories.Find("0305");
            gi.Prince.AddSpecialItemToKeep(SpecialEnum.ShieldOfLight);
            gi.PartyMembers.Add(gi.Prince);
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            companion1.AddSpecialItemToKeep(SpecialEnum.ResistanceRing);
            gi.PartyMembers.Add(companion1);
            IMapItem companion2 = AddCompanion(ref gi, "Mercenary");
            if (null == companion2)
               return false;
            gi.PartyMembers.Add(companion2);
            //---------------------
            gi.IsElfWitAndWileActive = true;
            gi.EncounteredMembers.Clear();
            string halflingLeaderName = "HalflingWarrior" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem halflingLeader = new MapItem(halflingLeaderName, 1.0, false, false, false, "c70HalflingLead", "c70HalflingLead", gi.Prince.Territory, 10, 3, 4);
            gi.EncounteredMembers.Add(halflingLeader);
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[3]) // Elf with Nerve Gas
         {
            gi.EventStart = "e007";
            gi.EventActive = "e310";
            gi.Prince.ResetPartial();
            gi.Prince.Territory = gi.Territories.Find("0305");
            gi.Prince.AddSpecialItemToKeep(SpecialEnum.ShieldOfLight);
            gi.PartyMembers.Add(gi.Prince);
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            companion1.AddSpecialItemToKeep(SpecialEnum.ResistanceRing);
            gi.PartyMembers.Add(companion1);
            IMapItem companion2 = AddCompanion(ref gi, "Mercenary");
            if (null == companion2)
               return false;
            gi.PartyMembers.Add(companion2);
            //---------------------
            gi.IsElfWitAndWileActive = true;
            gi.EncounteredMembers.Clear();
            string elfLeaderName = "ElfWarrior" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem elfLeader = new MapItem(elfLeaderName, 1.0, false, false, false, "c69ElfLead", "c69ElfLead", gi.Prince.Territory, 7, 6, 21);
            elfLeader.AddSpecialItemToShare(SpecialEnum.NerveGasBomb);
            gi.EncounteredMembers.Add(elfLeader);
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[4]) // Wizard is Encountered
         {
            gi.EventStart = "e023";
            if ("e307" == gi.EventActive) // on first iteration - wizard strikes first
               gi.EventActive = "e304";
            else
               gi.EventActive = "e307";
            IMapItem prince = AddPrince(ref gi, "0101");
            if (null == prince)
               return false;
            prince.AddSpecialItemToKeep(SpecialEnum.ResistanceRing);
            gi.AddSpecialItem(SpecialEnum.ResistanceTalisman, prince);
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            companion1.AddSpecialItemToKeep(SpecialEnum.ResistanceRing);
            gi.PartyMembers.Add(companion1);
            IMapItem companion2 = AddCompanion(ref gi, "Mercenary");
            if (null == companion2)
               return false;
            gi.PartyMembers.Add(companion2);
            //---------------------
            string miName = "Wizard" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem character = new MapItem(miName, 1.0, false, false, false, "c12Wizard", "c12Wizard", gi.Prince.Territory, 1, 1, 60);
            //encountered1.SetWounds(1, 0);
            gi.EncounteredMembers.Add(character);
            miName = "Mercenary" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            character = new MapItem(miName, 1.0, false, false, false, "c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
            gi.EncounteredMembers.Add(character);
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[5]) // Poison Drug
         {
            if ("e304" == gi.EventActive)
               gi.EventActive = "e307";
            else
               gi.EventActive = "e304";
            IMapItem prince = AddPrince(ref gi, "0101");
            if (null == prince)
               return false;
            prince.AddSpecialItemToShare(SpecialEnum.PoisonDrug);
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            companion1.AddSpecialItemToKeep(SpecialEnum.PoisonDrug);
            gi.PartyMembers.Add(companion1);
            //---------------------
            string porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(porter);
            porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(porter);
            //---------------------
            string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(trueLove);
            trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(trueLove);
            //---------------------
            string minstrelName = "Minstrel" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem minstrel = new MapItem(minstrelName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(minstrel);
            //---------------------
            IMapItem encountered2 = AddEncounteredMember(ref gi, "Wizard");
            if (null == encountered2)
               return false;
            IMapItem encountered3 = AddEncounteredMember(ref gi, "Mercenary");
            if (null == encountered3)
               return false;
            IMapItem encountered4 = AddEncounteredMember(ref gi, "Runaway");
            if (null == encountered4)
               return false;
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[6]) // hunting cat
         {
            gi.EventStart = "e076";
            gi.EventActive = "e310";
            IMapItem prince = AddPrince(ref gi, "0105");
            if (null == prince)
               return false;
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            gi.PartyMembers.Add(companion1);
            IMapItem companion2 = AddCompanion(ref gi, "Mercenary");
            if (null == companion2)
               return false;
            gi.PartyMembers.Add(companion2);
            //---------------------
            string porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(porter);
            porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(porter);
            //---------------------
            string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(trueLove);
            trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(trueLove);
            //---------------------
            string miName = "HuntingCat" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem huntingCat = new MapItem(miName, 1.0, false, false, false, "c59HuntingCat", "c59HuntingCat", gi.Prince.Territory, 3, 6, 0);
            gi.EncounteredMembers.Add(huntingCat);
            //---------------------
            string minstrelName = "Minstrel" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem minstrel = new MapItem(minstrelName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(minstrel);
            //---------------------
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[7]) // boar
         {
            gi.EventStart = "e083";
            gi.EventActive = "e310";
            IMapItem prince = AddPrince(ref gi, "0105");
            if (null == prince)
               return false;
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            gi.PartyMembers.Add(companion1);
            IMapItem companion2 = AddCompanion(ref gi, "Mercenary");
            if (null == companion2)
               return false;
            gi.PartyMembers.Add(companion2);
            //---------------------
            string porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(porter);
            porterName = "PorterSlave" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            porter = new MapItem(porterName, 1.0, false, false, false, "c42SlavePorter", "c42SlavePorter", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(porter);
            //---------------------
            string trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(trueLove);
            trueLoveName = "TrueLove" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            trueLove = new MapItem(trueLoveName, 1.0, false, false, false, "c44TrueLove", "c44TrueLove", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(trueLove);
            //---------------------
            string minstrelName = "Minstrel" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem minstrel = new MapItem(minstrelName, 1.0, false, false, false, "c60Minstrel", "c60Minstrel", prince.Territory, 0, 0, 0);
            myGameInstance.AddCompanion(minstrel);
            //---------------------
            string miName = "Boar" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem boar = new MapItem(miName, 1.0, false, false, false, "c58Boar", "c58Boar", gi.Prince.Territory, 5, 8, 0);
            gi.EncounteredMembers.Add(boar);
            //---------------------
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[8]) // Spiders
         {
            gi.EventStart = "e074";
            gi.EventActive = "e309";
            IMapItem prince = AddPrince(ref gi, "0105");
            if (null == prince)
               return false;
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            gi.PartyMembers.Add(companion1);
            //-----------------------------------------
            for (int i = 0; i < 4; ++i)
            {
               string miName = "Spider" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem spider = new MapItem(miName, 1.0, false, false, false, "c54Spider", "c54Spider", gi.Prince.Territory, 1, 1, 0);
               gi.EncounteredMembers.Add(spider);
            }
            //-----------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[9])  // Cavalry
         {
            gi.EventActive = "e304";
            IMapItem prince = AddPrince(ref gi, "0101");
            if (null == prince)
               return false;
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            gi.PartyMembers.Add(companion1);
            //-----------------------------------------
            gi.IsCavalryEscort = true;
            string cavalryName = "Cavalry" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem cavalry = new MapItem(cavalryName, 1.0, false, false, false, "Cavalry", "Cavalry", prince.Territory, 0, 0, 0);
            cavalry.IsGuide = true;
            gi.AddCompanion(cavalry);
            //-----------------------------------------
            string farmerName = "Farmer" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem farmer = new MapItem(farmerName, 1.0, false, false, false, "c17Farmer", "c17Farmer", prince.Territory, 1, 1, 2);
            gi.EncounteredMembers.Add(farmer);
            IMapItem encountered2 = AddEncounteredMember(ref gi, "Dragon");
            if (null == encountered2)
               return false;
            IMapItem encountered3 = AddEncounteredMember(ref gi, "Witch");
            if (null == encountered3)
               return false;
            //-----------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[10])  // Protector coming
         {
            gi.EventStart = "e012b"; // indicates protector is coming
            if ("e304" == gi.EventActive)
               gi.EventActive = "e307";
            else
               gi.EventActive = "e304";
            IMapItem prince = AddPrince(ref gi, "0101");
            if (null == prince)
               return false;
            prince.AddSpecialItemToShare(SpecialEnum.ResistanceRing);
            //-----------------------------------------
            string farmerName = "Farmer" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem farmer = new MapItem(farmerName, 1.0, false, false, false, "c17Farmer", "c17Farmer", gi.Prince.Territory, 1, 1, 2);
            gi.EncounteredMembers.Add(farmer);
            //-----------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[11])  // Mirror Image
         {
            gi.EventStart = "e047";
            gi.EventActive = "e307";
            IMapItem prince = AddPrince(ref gi, "0101");
            if (null == prince)
               return false;
            prince.AddSpecialItemToShare(SpecialEnum.PoisonDrug);
            prince.AddSpecialItemToShare(SpecialEnum.ShieldOfLight);
            prince.AddSpecialItemToKeep(SpecialEnum.MagicSword);
            prince.AddSpecialItemToKeep(SpecialEnum.ResistanceTalisman);
            prince.AddSpecialItemToShare(SpecialEnum.HydraTeeth);
            gi.HydraTeethCount = 3;
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            gi.PartyMembers.Add(companion1);
            //-----------------------------------------
            IMapItem mirror = new MapItem(gi.Prince);
            mirror.Name = "Mirror";
            mirror.TopImageName = "c34PrinceMirror";
            IMapImage mii = MapItem.theMapImages.Find(mirror.TopImageName);
            if (null == mii)
            {
               mii = (IMapImage)new MapImage(mirror.TopImageName);
               MapItem.theMapImages.Add(mii);
            }
            mirror.BottomImageName = "c34PrinceMirror";
            mirror.OverlayImageName = "";
            gi.EncounteredMembers.Add(mirror);
            //-----------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[12]) // Resistence Ring
         {
            gi.EventActive = "e310";
            IMapItem prince = AddPrince(ref gi, "0101");
            if (null == prince)
               return false;
            prince.AddSpecialItemToShare(SpecialEnum.ResistanceRing);
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            gi.PartyMembers.Add(companion1);
            IMapItem encountered2 = AddEncounteredMember(ref gi, "Dragon");
            if (null == encountered2)
               return false;
            IMapItem encountered3 = AddEncounteredMember(ref gi, "Witch");
            if (null == encountered3)
               return false;
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[13]) // Shield
         {
            gi.EventActive = "e300";
            IMapItem prince = AddPrince(ref gi, "0101");
            if (null == prince)
               return false;
            prince.AddSpecialItemToShare(SpecialEnum.PoisonDrug);
            prince.AddSpecialItemToShare(SpecialEnum.ShieldOfLight);
            prince.AddSpecialItemToKeep(SpecialEnum.MagicSword);
            prince.AddSpecialItemToKeep(SpecialEnum.ResistanceTalisman);
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            companion1.AddSpecialItemToKeep(SpecialEnum.NerveGasBomb);
            companion1.AddSpecialItemToKeep(SpecialEnum.MagicSword);
            companion1.AddSpecialItemToKeep(SpecialEnum.PoisonDrug);
            companion1.AddSpecialItemToKeep(SpecialEnum.ShieldOfLight);
            gi.PartyMembers.Add(companion1);
            IMapItem encountered2 = AddEncounteredMember(ref gi, "Wizard");
            if (null == encountered2)
               return false;
            IMapItem encountered3 = AddEncounteredMember(ref gi, "Mercenary");
            if (null == encountered3)
               return false;
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[14]) // Hydra Teeth
         {
            gi.EventActive = "e304";
            IMapItem prince = AddPrince(ref gi, "0101");
            if (null == prince)
               return false;
            prince.AddSpecialItemToShare(SpecialEnum.HydraTeeth);
            gi.HydraTeethCount = 3;
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            gi.PartyMembers.Add(companion1);
            IMapItem encountered2 = AddEncounteredMember(ref gi, "Wizard");
            if (null == encountered2)
               return false;
            IMapItem encountered3 = AddEncounteredMember(ref gi, "Mercenary");
            if (null == encountered3)
               return false;
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[15]) // Spectre
         {
            gi.EventActive = "e304";
            IMapItem prince = AddPrince(ref gi, "0101");
            if (null == prince)
               return false;
            prince.AddSpecialItemToShare(SpecialEnum.ResistanceTalisman);
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            companion1.IsFickle = true;
            gi.PartyMembers.Add(companion1);
            IMapItem companion2 = AddCompanion(ref gi, "Witch");
            if (null == companion2)
               return false;
            gi.PartyMembers.Add(companion2);
            IMapItem companion4 = AddCompanion(ref gi, "Mercenary");
            if (null == companion4)
               return false;
            companion4.IsFickle = true;
            gi.PartyMembers.Add(companion4);
            IMapItem companion5 = AddCompanion(ref gi, "Monk");
            if (null == companion5)
               return false;
            gi.PartyMembers.Add(companion5);
            IMapItem encountered1 = AddEncounteredMember(ref gi, "Spectre");
            if (null == encountered1)
               return false;
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[16])
         {
            SetCombatEvent(gi);
            IMapItem prince = AddPrince(ref gi, "0101");
            if (null == prince)
               return false;
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            companion1.IsFickle = true;
            gi.PartyMembers.Add(companion1);
            //----------------------------------------------
            IMapItem encountered1 = AddEncounteredMember(ref gi, "Runaway");
            if (null == encountered1)
               return false;
            IMapItem encountered2 = AddEncounteredMember(ref gi, "Wizard");
            if (null == encountered2)
               return false;
            IMapItem encountered3 = AddEncounteredMember(ref gi, "Mercenary");
            if (null == encountered3)
               return false;
            //----------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[17])
         {
            SetCombatEvent(gi);
            IMapItem prince = AddPrince(ref gi, "0101");
            if (null == prince)
               return false;
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            gi.PartyMembers.Add(companion1);
            //----------------------------------------------
            IMapItem encountered1 = AddEncounteredMember(ref gi, "Witch");
            if (null == encountered1)
               return false;
            IMapItem encountered2 = AddEncounteredMember(ref gi, "Wizard");
            if (null == encountered2)
               return false;
            //----------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         else if (CommandName == myCommandNames[18])
         {
            SetCombatEvent(gi);
            IMapItem prince = AddPrince(ref gi, "0101");
            if (null == prince)
               return false;
            IMapItem companion1 = AddCompanion(ref gi, "Dwarf");
            if (null == companion1)
               return false;
            gi.PartyMembers.Add(companion1);
            IMapItem companion2 = AddCompanion(ref gi, "Mercenary");
            if (null == companion2)
               return false;
            gi.PartyMembers.Add(companion2);
            //----------------------------------------------
            IMapItem encountered1 = AddEncounteredMember(ref gi, "Witch");
            if (null == encountered1)
               return false;
            IMapItem encountered2 = AddEncounteredMember(ref gi, "Wizard");
            if (null == encountered2)
               return false;
            //----------------------------------------------
            myEventViewer.UpdateView(ref gi, GameAction.EncounterCombat);
         }
         return true;
      }
      public bool NextTest(ref IGameInstance gi)
      {
         myIndexCombat = 0;
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
      private void SetCombatEvent(IGameInstance gi)
      {
         switch (myIndexCombat)
         {
            case 0: gi.EventActive = "e300"; break;
            case 1: gi.EventActive = "e301"; break;
            case 2: gi.EventActive = "e302"; break;
            case 3: gi.EventActive = "e303"; break;
            case 4: gi.EventActive = "e304"; break;
            case 5: gi.EventActive = "e305"; break;
            case 6: gi.EventActive = "e306"; break;
            case 7: gi.EventActive = "e307"; break;
            case 8: gi.EventActive = "e308"; break;
            case 9: gi.EventActive = "e309"; break;
            case 10: gi.EventActive = "e310"; break;
            default: myIndexCombat = 0; break;  // reset
         }
         ++myIndexCombat;
      }
      private IMapItem AddPrince(ref IGameInstance gi, string tName)
      {
         IMapItem prince = gi.MapItems.Find("Prince");
         if (null == prince)
            Logger.Log(LogEnum.LE_ERROR, "AddPrince(): mi=null");
         ITerritory t = gi.Territories.Find(tName); // Mountains
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): t=null");
            return null;
         }
         IMapItem clone = new MapItem(prince);
         clone.Reset();
         clone.Territory = t;
         //clone.AddSpecialItemToShare(SpecialEnum.MagicSword);
         //clone.AddSpecialItemToShare(SpecialEnum.NerveGasBomb);
         gi.PartyMembers.Add(clone);
         gi.Prince = clone;
         return clone;
      }
      private IMapItem AddCompanion(ref IGameInstance gi, string name)
      {
         IMapItem companion1 = gi.MapItems.Find(name);
         if (null == companion1)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddCompanions(): mi=null for name=" + name);
            return null;
         }
         companion1.Reset();
         //companion1.AddSpecialItemToKeep(SpecialEnum.PoisonDrug);
         //companion1.AddSpecialItemToKeep(SpecialEnum.MagicSword);
         //companion1.AddSpecialItemToShare(SpecialEnum.ResistanceTalisman);
         IMapItem clone = new MapItem(companion1);
         clone.Name = name + Utilities.MapItemNum.ToString();
         ++Utilities.MapItemNum;
         return clone;
      }
      private IMapItem AddEncounteredMember(ref IGameInstance gi, string name)
      {
         IMapItem enemy = gi.MapItems.Find(name);
         if (null == enemy)
         {
            Logger.Log(LogEnum.LE_ERROR, "AddEncounteredMember(): mi=null for name=" + name);
            return null;
         }
         enemy.Reset();
         IMapItem clone = new MapItem(enemy);
         clone.Mounts.Clear();
         clone.Coin = 10;
         clone.Name = name + Utilities.MapItemNum.ToString();
         ++Utilities.MapItemNum;
         gi.EncounteredMembers.Add(clone);
         AddEncounteredMount(clone, 2);
         return clone;
      }
      private void AddEncounteredMount(IMapItem encountered, int numMounts)
      {
         encountered.Mounts.Clear();
         for (int i = 0; i < numMounts; ++i)
         {
            string name = "Horse" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            MapItem horse = new MapItem(name, 1.0, false, false, false, "MHorse", "", encountered.Territory, 0, 0, 0);
            encountered.Mounts.Add(horse);
         }
      }
   }
}
