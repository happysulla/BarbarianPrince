using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;
using static BarbarianPrince.View.InventoryDisplayDialog;

namespace BarbarianPrince.View
{

   public partial class InventoryDisplayDialog : Window
   {
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myFood;
         public int myCoin;
         public int myNumHorse;
         public int myNumPegasus;
         public int myNumHealingPoition;
         public int myNumCurePoisonVial;
         public int myNumGiftOfCharm;
         public int myNumEnduranceSash;
         public int myNumResistanceTalisman;
         public int myNumPoisonDrug;
         public int myNumMagicSword;
         public int myNumAntiPoisonAmulet;
         public int myNumPegasusMountTalisman;
         public int myNumCharismaTalisman;
         public int myNumNerveGasBomb;
         public int myNumResistanceRing;
         public int myNumResurrectionNecklace;
         public int myNumShieldOfLight;
         public int myNumRoyalHelmOfNorthlands;
         public int myNumTrollSkin;
         public int myNumDragonEye;
         public int myNumRocBeak;
         public int myNumGriffonClaw;
         public int myNumMagicBox;
         public int myNumHydraTeeth;
         public int myNumStaffOfCommand;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myFood = 0;
            myCoin = 0;
            myNumHorse = 0;
            myNumPegasus = 0;
            myNumHealingPoition = 0;
            myNumCurePoisonVial = 0;
            myNumGiftOfCharm = 0;
            myNumEnduranceSash = 0;
            myNumResistanceTalisman = 0;
            myNumPoisonDrug = 0;
            myNumMagicSword = 0;
            myNumAntiPoisonAmulet = 0;
            myNumPegasusMountTalisman = 0;
            myNumCharismaTalisman = 0;
            myNumNerveGasBomb = 0;
            myNumResistanceRing = 0;
            myNumResurrectionNecklace = 0;
            myNumShieldOfLight = 0;
            myNumRoyalHelmOfNorthlands = 0;
            myNumTrollSkin = 0;
            myNumDragonEye = 0;
            myNumRocBeak = 0;
            myNumGriffonClaw = 0;
            myNumMagicBox = 0;
            myNumHydraTeeth = 0;
            myNumStaffOfCommand = 0;
         }
      };
      public struct GridRowHeading
      {
         public bool myIsHorse;
         public bool myIsPegasus;
         public bool myIsHealingPoition;
         public bool myIsCurePoisonVial;
         public bool myIsGiftOfCharm;
         public bool myIsEnduranceSash;
         public bool myIsResistanceTalisman;
         public bool myIsPoisonDrug;
         public bool myIsMagicSword;
         public bool myIsAntiPoisonAmulet;
         public bool myIsPegasusMountTalisman;
         public bool myIsCharismaTalisman;
         public bool myIsNerveGasBomb;
         public bool myIsResistanceRing;
         public bool myIsResurrectionNecklace;
         public bool myIsShieldOfLight;
         public bool myIsRoyalHelmOfNorthlands;
         public bool myIsTrollSkin;
         public bool myIsDragonEye;
         public bool myIsRocBeak;
         public bool myIsGriffonClaw;
         public bool myIsMagicBox;
         public bool myIsHydraTeeth;
         public bool myIsStaffOfCommand;
         public GridRowHeading(bool notUsed)
         {
            myIsHorse = false;
            myIsPegasus = false;
            myIsHealingPoition = false;
            myIsCurePoisonVial = false;
            myIsGiftOfCharm = false;
            myIsEnduranceSash = false;
            myIsResistanceTalisman = false;
            myIsPoisonDrug = false;
            myIsMagicSword = false;
            myIsAntiPoisonAmulet = false;
            myIsPegasusMountTalisman = false;
            myIsCharismaTalisman = false;
            myIsNerveGasBomb = false;
            myIsResistanceRing = false;
            myIsResurrectionNecklace = false;
            myIsShieldOfLight = false;
            myIsRoyalHelmOfNorthlands = false;
            myIsTrollSkin = false;
            myIsDragonEye = false;
            myIsRocBeak = false;
            myIsGriffonClaw = false;
            myIsMagicBox = false;
            myIsHydraTeeth = false;
            myIsStaffOfCommand = false;
         }
      };
      //----------------------------------------------------------------
      private GridRowHeading myGridRowHeading = new GridRowHeading(false);
      private GridRow[] myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      public InventoryDisplayDialog(IGameInstance gi)
      {
         InitializeComponent();
         SetGridRowData(gi);
         UpdateGridRows(gi);
      }
      private void SetGridRowData(IGameInstance gi)
      {
         for (int i = 1; i < gi.PartyMembers.Count; ++i)
         {
            IMapItem mi = gi.PartyMembers[i];
            myGridRows[i] = new GridRow(mi);
            foreach (IMapItem mount in mi.Mounts)
            {
               if (true == mount.Name.Contains("Pegasus"))
               {
                  ++myGridRows[i].myNumPegasus;
                  myGridRowHeading.myIsPegasus = true;
               }
               else if (true == mount.Name.Contains("Horse"))
               {
                  ++myGridRows[i].myNumHorse;
                  myGridRowHeading.myIsHorse = true;
               }
            }
            myGridRows[i].myNumHealingPoition = mi.GetNumSpecialItem(SpecialEnum.HealingPoition);
            if (0 < myGridRows[i].myNumHealingPoition)
               myGridRowHeading.myIsHealingPoition = true;
            myGridRows[i].myNumCurePoisonVial = mi.GetNumSpecialItem(SpecialEnum.CurePoisonVial);
            if (0 < myGridRows[i].myNumCurePoisonVial)
               myGridRowHeading.myIsCurePoisonVial = true;
            myGridRows[i].myNumGiftOfCharm = mi.GetNumSpecialItem(SpecialEnum.GiftOfCharm);
            if (0 < myGridRows[i].myNumGiftOfCharm)
               myGridRowHeading.myIsGiftOfCharm = true;
            myGridRows[i].myNumEnduranceSash = mi.GetNumSpecialItem(SpecialEnum.EnduranceSash);
            if (0 < myGridRows[i].myNumEnduranceSash)
               myGridRowHeading.myIsEnduranceSash = true;
            myGridRows[i].myNumResistanceTalisman = mi.GetNumSpecialItem(SpecialEnum.ResistanceTalisman);
            if (0 < myGridRows[i].myNumResistanceTalisman)
               myGridRowHeading.myIsResistanceTalisman = true;
            myGridRows[i].myNumPoisonDrug = mi.GetNumSpecialItem(SpecialEnum.PoisonDrug);
            if (0 < myGridRows[i].myNumPoisonDrug)
               myGridRowHeading.myIsPoisonDrug = true;
            myGridRows[i].myNumMagicSword = mi.GetNumSpecialItem(SpecialEnum.MagicSword);
            if (0 < myGridRows[i].myNumMagicSword)
               myGridRowHeading.myIsMagicSword = true;
            myGridRows[i].myNumAntiPoisonAmulet = mi.GetNumSpecialItem(SpecialEnum.AntiPoisonAmulet);
            if (0 < myGridRows[i].myNumAntiPoisonAmulet)
               myGridRowHeading.myIsAntiPoisonAmulet = true;
            myGridRows[i].myNumPegasusMountTalisman = mi.GetNumSpecialItem(SpecialEnum.PegasusMountTalisman);
            if (0 < myGridRows[i].myNumPegasusMountTalisman)
               myGridRowHeading.myIsPegasusMountTalisman = true;
            myGridRows[i].myNumCharismaTalisman = mi.GetNumSpecialItem(SpecialEnum.CharismaTalisman);
            if (0 < myGridRows[i].myNumCharismaTalisman)
               myGridRowHeading.myIsCharismaTalisman = true;
            myGridRows[i].myNumNerveGasBomb = mi.GetNumSpecialItem(SpecialEnum.NerveGasBomb);
            if (0 < myGridRows[i].myNumNerveGasBomb)
               myGridRowHeading.myIsNerveGasBomb = true;
            myGridRows[i].myNumResistanceRing = mi.GetNumSpecialItem(SpecialEnum.ResistanceRing);
            if (0 < myGridRows[i].myNumResistanceRing)
               myGridRowHeading.myIsResistanceRing = true;
            myGridRows[i].myNumResurrectionNecklace = mi.GetNumSpecialItem(SpecialEnum.ResurrectionNecklace);
            if (0 < myGridRows[i].myNumResurrectionNecklace)
               myGridRowHeading.myIsResurrectionNecklace = true;
            myGridRows[i].myNumShieldOfLight = mi.GetNumSpecialItem(SpecialEnum.ShieldOfLight);
            if (0 < myGridRows[i].myNumShieldOfLight)
               myGridRowHeading.myIsShieldOfLight = true;
            myGridRows[i].myNumRoyalHelmOfNorthlands = mi.GetNumSpecialItem(SpecialEnum.RoyalHelmOfNorthlands);
            if (0 < myGridRows[i].myNumRoyalHelmOfNorthlands)
               myGridRowHeading.myIsRoyalHelmOfNorthlands = true;
            myGridRows[i].myNumTrollSkin = mi.GetNumSpecialItem(SpecialEnum.TrollSkin);
            if (0 < myGridRows[i].myNumTrollSkin)
               myGridRowHeading.myIsTrollSkin = true;
            myGridRows[i].myNumDragonEye = mi.GetNumSpecialItem(SpecialEnum.DragonEye);
            if (0 < myGridRows[i].myNumDragonEye)
               myGridRowHeading.myIsDragonEye = true;
            myGridRows[i].myNumRocBeak = mi.GetNumSpecialItem(SpecialEnum.RocBeak);
            if (0 < myGridRows[i].myNumRocBeak)
               myGridRowHeading.myIsRocBeak = true;
            myGridRows[i].myNumGriffonClaw = mi.GetNumSpecialItem(SpecialEnum.GriffonClaws);
            if (0 < myGridRows[i].myNumGriffonClaw)
               myGridRowHeading.myIsGriffonClaw = true;
            myGridRows[i].myNumMagicBox = mi.GetNumSpecialItem(SpecialEnum.MagicBox);
            if (0 < myGridRows[i].myNumMagicBox)
               myGridRowHeading.myIsMagicBox = true;
            myGridRows[i].myNumHydraTeeth = mi.GetNumSpecialItem(SpecialEnum.HydraTeeth);
            if (0 < myGridRows[i].myNumHydraTeeth)
               myGridRowHeading.myIsHydraTeeth = true;
            myGridRows[i].myNumStaffOfCommand = mi.GetNumSpecialItem(SpecialEnum.StaffOfCommand);
            if (0 < myGridRows[i].myNumStaffOfCommand)
               myGridRowHeading.myIsStaffOfCommand = true;
         }
      }
      private void UpdateGridRows(IGameInstance gi)
      {
         for (int i = 1; i < gi.PartyMembers.Count; ++i)
         {
            IMapItem mi = gi.PartyMembers[i];
            myGridRows[i] = new GridRow(mi);
            int rowNum = i + 1;
            Button b = CreateButton(mi);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //--------------------------------------------------------
            {
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = mi.Food.ToString() };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 1);
            }
            //--------------------------------------------------------
            {
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = mi.Coin.ToString() };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 2);
            }
            //--------------------------------------------------------
            int colNum = 3;
            if (true == myGridRowHeading.myIsHorse)
            {
               int count = myGridRows[rowNum].myNumHorse;
               if ( 0 < count)
               {
                  Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = count.ToString() };
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, rowNum);
                  Grid.SetColumn(label, colNum);
               }
               ++colNum;
            }
            //--------------------------------------------------------
            if (true == myGridRowHeading.myIsPegasus)
            {
               int count = myGridRows[rowNum].myNumPegasus;
               if (0 < count)
               {
                  Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = count.ToString() };
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, rowNum);
                  Grid.SetColumn(label, colNum);
               }
               ++colNum;
            }
            //--------------------------------------------------------
            if (true == myGridRowHeading.myIsHealingPoition)
            {
               int count = myGridRows[rowNum].myNumHealingPoition;
               if (0 < count)
               {
                  Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = count.ToString() };
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, rowNum);
                  Grid.SetColumn(label, colNum);
               }
               ++colNum;
            }
         }
      }
      private Button CreateButton(IMapItem mi)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = Utilities.RemoveSpaces(mi.Name);
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(1);
         b.BorderBrush = Brushes.Black;
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         MapItem.SetButtonContent(b, mi, false, true); // This sets the image as the button's content
         return b;
      }
   }
}
