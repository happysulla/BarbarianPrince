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
using static BarbarianPrince.GameViewerWindow;

namespace BarbarianPrince
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
         public int myNumFoulbane;
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
            myNumFoulbane = 0;
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
         public bool myIsFoulbane;
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
            myIsFoulbane = false;
            myIsMagicBox = false;
            myIsHydraTeeth = false;
            myIsStaffOfCommand = false;
         }
      };
      //----------------------------------------------------------------
      public bool CtorError = false;
      private double myColumnWidth = (1.5 * Utilities.theMapItemSize) + 17;
      private double myRowHeight = ((0.1 + Utilities.ZOOM) * Utilities.theMapItemSize) + 10;
      private RuleDialogViewer myRulesManager = null;
      private GridRowHeading myGridRowHeading = new GridRowHeading(false);
      private GridRow[] myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      //----------------------------------------------------------------
      public InventoryDisplayDialog(IGameInstance gi, RuleDialogViewer rm)
      {
         InitializeComponent();
         if ( null == rm )
         {
            Logger.Log(LogEnum.LE_ERROR, "InventoryDisplayDialog(): rv=null");
            CtorError = true;
            return;
         }
         myRulesManager = rm;
         this.Height = gi.PartyMembers.Count * myRowHeight + myColumnWidth + 30; // ColumnWidth represents the initial Row's Height -  20 is for border and header space
         SetGridRowData(gi);
         UpdateGridRowHeader();
         UpdateGridRows(gi);
      }
      private void SetGridRowData(IGameInstance gi)
      {
         for (int i = 0; i < gi.PartyMembers.Count; ++i)
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
            myGridRows[i].myNumFoulbane = mi.GetNumSpecialItem(SpecialEnum.Foulbane);
            if (0 < myGridRows[i].myNumFoulbane)
               myGridRowHeading.myIsFoulbane = true;
            myGridRows[i].myNumMagicBox = mi.GetNumSpecialItem(SpecialEnum.MagicBox);
            if (0 < myGridRows[i].myNumMagicBox)
               myGridRowHeading.myIsMagicBox = true;
            int numTeeth = mi.GetNumSpecialItem(SpecialEnum.HydraTeeth);
            if (0 < numTeeth)
            {
               myGridRowHeading.myIsHydraTeeth = true;
               myGridRows[i].myNumHydraTeeth = gi.HydraTeethCount;
            }
            myGridRows[i].myNumStaffOfCommand = mi.GetNumSpecialItem(SpecialEnum.StaffOfCommand);
            if (0 < myGridRows[i].myNumStaffOfCommand)
               myGridRowHeading.myIsStaffOfCommand = true;
         }
      }
      private void UpdateGridRowHeader()
      {
         const int rowNum = 0;
         //--------------------------------------------------------
         {
            Rectangle rect = new Rectangle { Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize, Visibility=Visibility.Hidden};
            myGrid.Children.Add(rect);
            Grid.SetRow(rect, rowNum);
            Grid.SetColumn(rect, 0);
         }
         //--------------------------------------------------------
         {
            Image img = new Image { Source = MapItem.theMapImages.GetBitmapImage("Food") };
            Button button = CreateButton(img);
            button.Name = "r215";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, 1);
         }
         //--------------------------------------------------------
         {
            Image img = new Image { Source = MapItem.theMapImages.GetBitmapImage("Coin")};
            Button button = CreateButton(img);
            button.Name = "r225";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, 2);
         }
         //--------------------------------------------------------
         int colNum = 3;
         if (true == myGridRowHeading.myIsHorse)
         {
            Image img = new Image { Source = MapItem.theMapImages.GetBitmapImage("Mount")};
            Button button = CreateButton(img);
            button.Name = "r206";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsPegasus)
         {
            Image img = new Image { Source = MapItem.theMapImages.GetBitmapImage("Pegasus")};
            Button button = CreateButton(img);
            button.Name = "r188";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsHealingPoition)
         {
            Image img = new Image {Name= "PotionHeal", Source = MapItem.theMapImages.GetBitmapImage("PotionHeal")};
            Button button = CreateButton(img);
            button.Name = "r180";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsCurePoisonVial)
         {
            Image img = new Image { Name = "PotionCure", Source = MapItem.theMapImages.GetBitmapImage("PotionCure")};
            Button button = CreateButton(img);
            button.Name = "r181";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsGiftOfCharm)
         {
            Image img = new Image { Name = "CharmGift", Source = MapItem.theMapImages.GetBitmapImage("CharmGift")};
            Button button = CreateButton(img);
            button.Name = "r182";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsEnduranceSash)
         {
            Image img = new Image { Name = "Sash", Source = MapItem.theMapImages.GetBitmapImage("Sash")};
            Button button = CreateButton(img);
            button.Name = "r183";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsResistanceTalisman)
         {
            Image img = new Image { Name = "TalismanResistance", Source = MapItem.theMapImages.GetBitmapImage("TalismanResistance")};
            Button button = CreateButton(img);
            button.Name = "r184";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsPoisonDrug)
         {
            Image img = new Image { Name = "PoisonDrug", Source = MapItem.theMapImages.GetBitmapImage("PoisonDrug")};
            Button button = CreateButton(img);
            button.Name = "r185";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsMagicSword)
         {
            Image img = new Image { Name = "Sword", Source = MapItem.theMapImages.GetBitmapImage("Sword")};
            Button button = CreateButton(img);
            button.Name = "r186";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsAntiPoisonAmulet)
         {
            Image img = new Image { Name = "AmuletAntiPoison", Source = MapItem.theMapImages.GetBitmapImage("AmuletAntiPoison")};
            Button button = CreateButton(img);
            button.Name = "r187";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsPegasusMountTalisman)
         {
            Image img = new Image { Name = "TalismanPegasus", Source = MapItem.theMapImages.GetBitmapImage("TalismanPegasus")};
            Button button = CreateButton(img);
            button.Name = "r188a";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsCharismaTalisman)
         {
            Image img = new Image { Name = "TalismanCharisma", Source = MapItem.theMapImages.GetBitmapImage("TalismanCharisma")};
            Button button = CreateButton(img);
            button.Name = "r189";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsNerveGasBomb)
         {
            Image img = new Image { Name = "NerveGasBomb", Source = MapItem.theMapImages.GetBitmapImage("NerveGasBomb")};
            Button button = CreateButton(img);
            button.Name = "r190";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsResistanceRing)
         {
            Image img = new Image { Name = "RingResistence", Source = MapItem.theMapImages.GetBitmapImage("RingResistence")};
            Button button = CreateButton(img);
            button.Name = "r191";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsResurrectionNecklace)
         {
            Image img = new Image { Name = "Necklace", Source = MapItem.theMapImages.GetBitmapImage("Necklace")};
            Button button = CreateButton(img);
            button.Name = "r192";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsShieldOfLight)
         {
            Image img = new Image { Name = "Shield", Source = MapItem.theMapImages.GetBitmapImage("Shield")};
            Button button = CreateButton(img);
            button.Name = "r193";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsRoyalHelmOfNorthlands)
         {
            Image img = new Image { Name = "Helmet", Source = MapItem.theMapImages.GetBitmapImage("Helmet") };
            Button button = CreateButton(img);
            button.Name = "r194";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsTrollSkin)
         {
            Image img = new Image { Name = "TrollSkin", Source = MapItem.theMapImages.GetBitmapImage("TrollSkin") };
            Button button = CreateButton(img);
            button.Name = "r057";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsDragonEye)
         {
            Image img = new Image { Name = "DragonEye", Source = MapItem.theMapImages.GetBitmapImage("DragonEye") };
            Button button = CreateButton(img);
            button.Name = "r098";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsRocBeak)
         {
            Image img = new Image { Name = "RocBeak", Source = MapItem.theMapImages.GetBitmapImage("RocBeak")};
            Button button = CreateButton(img);
            button.Name = "r099";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsGriffonClaw)
         {
            Image img = new Image { Name = "GriffonClaw", Source = MapItem.theMapImages.GetBitmapImage("GriffonClaw") };
            Button button = CreateButton(img);
            button.Name = "r100";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsFoulbane)
         {
            Image img = new Image { Name = "Foulbane", Source = MapItem.theMapImages.GetBitmapImage("FoulBane") };
            Button button = CreateButton(img);
            button.Name = "r146";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsMagicBox)
         {
            Image img = new Image { Name = "BoxUnopened", Source = MapItem.theMapImages.GetBitmapImage("BoxUnopened") };
            Button button = CreateButton(img);
            button.Name = "r140";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsHydraTeeth)
         {
            Image img = new Image { Name = "Teeth", Source = MapItem.theMapImages.GetBitmapImage("Teeth") };
            Button button = CreateButton(img);
            button.Name = "r141";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         if (true == myGridRowHeading.myIsStaffOfCommand)
         {
            Image img = new Image { Name = "Staff", Source = MapItem.theMapImages.GetBitmapImage("Staff") };
            Button button = CreateButton(img);
            button.Name = "r212m";
            myGrid.Children.Add(button);
            Grid.SetRow(button, rowNum);
            Grid.SetColumn(button, colNum);
            ++colNum;
         }
         //--------------------------------------------------------
         this.Width = colNum * myColumnWidth;
      }
      private void UpdateGridRows(IGameInstance gi)
      {
         for (int i = 0; i < gi.PartyMembers.Count; ++i)
         {
            IMapItem mi = gi.PartyMembers[i];
            int rowNum = i + 1;
            Button b = CreateButton(mi);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //--------------------------------------------------------
            {
               int count = mi.Food;
               if (0 < count)
               {
                  Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = count.ToString() };
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, rowNum);
                  Grid.SetColumn(label, 1);
               }
            }
            //--------------------------------------------------------
            {
               int count = mi.Coin;
               if( 0 < count )
               {
                  Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = count.ToString() };
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, rowNum);
                  Grid.SetColumn(label, 2);
               }
            }
            //--------------------------------------------------------
            int colNum = 3;
            if (true == myGridRowHeading.myIsHorse)
            {
               int count = myGridRows[i].myNumHorse;
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
               int count = myGridRows[i].myNumPegasus;
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
               int count = myGridRows[i].myNumHealingPoition;
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
            if (true == myGridRowHeading.myIsCurePoisonVial)
            {
               int count = myGridRows[i].myNumCurePoisonVial;
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
            if (true == myGridRowHeading.myIsGiftOfCharm)
            {
               int count = myGridRows[i].myNumGiftOfCharm;
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
            if (true == myGridRowHeading.myIsEnduranceSash)
            {
               int count = myGridRows[i].myNumEnduranceSash;
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
            if (true == myGridRowHeading.myIsResistanceTalisman)
            {
               int count = myGridRows[i].myNumResistanceTalisman;
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
            if (true == myGridRowHeading.myIsPoisonDrug)
            {
               int count = myGridRows[i].myNumPoisonDrug;
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
            if (true == myGridRowHeading.myIsMagicSword)
            {
               int count = myGridRows[i].myNumMagicSword;
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
            if (true == myGridRowHeading.myIsAntiPoisonAmulet)
            {
               int count = myGridRows[i].myNumAntiPoisonAmulet;
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
            if (true == myGridRowHeading.myIsPegasusMountTalisman)
            {
               int count = myGridRows[i].myNumPegasusMountTalisman;
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
            if (true == myGridRowHeading.myIsCharismaTalisman)
            {
               int count = myGridRows[i].myNumCharismaTalisman;
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
            if (true == myGridRowHeading.myIsNerveGasBomb)
            {
               int count = myGridRows[i].myNumNerveGasBomb;
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
            if (true == myGridRowHeading.myIsResistanceRing)
            {
               int count = myGridRows[i].myNumResistanceRing;
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
            if (true == myGridRowHeading.myIsResurrectionNecklace)
            {
               int count = myGridRows[i].myNumResurrectionNecklace;
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
            if (true == myGridRowHeading.myIsShieldOfLight)
            {
               int count = myGridRows[i].myNumShieldOfLight;
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
            if (true == myGridRowHeading.myIsRoyalHelmOfNorthlands)
            {
               int count = myGridRows[i].myNumRoyalHelmOfNorthlands;
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
            if (true == myGridRowHeading.myIsTrollSkin)
            {
               int count = myGridRows[i].myNumTrollSkin;
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
            if (true == myGridRowHeading.myIsDragonEye)
            {
               int count = myGridRows[i].myNumDragonEye;
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
            if (true == myGridRowHeading.myIsRocBeak)
            {
               int count = myGridRows[i].myNumRocBeak;
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
            if (true == myGridRowHeading.myIsGriffonClaw)
            {
               int count = myGridRows[i].myNumGriffonClaw;
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
            if (true == myGridRowHeading.myIsFoulbane)
            {
               int count = myGridRows[i].myNumFoulbane;
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
            if (true == myGridRowHeading.myIsMagicBox)
            {
               int count = myGridRows[i].myNumMagicBox;
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
            if (true == myGridRowHeading.myIsHydraTeeth)
            {
               int count = myGridRows[i].myNumHydraTeeth;
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
            if (true == myGridRowHeading.myIsStaffOfCommand)
            {
               int count = myGridRows[i].myNumStaffOfCommand;
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
         b.Margin = new Thickness(5, 5, 0, 0);
         b.Name = Utilities.RemoveSpaces(mi.Name);
         b.Width = (0.1+Utilities.ZOOM) * Utilities.theMapItemSize;
         b.Height = (0.1+Utilities.ZOOM) * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(1);
         b.BorderBrush = Brushes.Black;
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         MapItem.SetButtonContent(b, mi, true, false, false, false); 
         return b;
      }
      private Button CreateButton(Image img)
      { 
         System.Windows.Controls.Button b = new Button {Name=img.Name, Width= 1.5 * Utilities.theMapItemSize, Height= 1.5 * Utilities.theMapItemSize, Foreground=Brushes.Transparent, Background=Brushes.Transparent, BorderBrush=Brushes.Transparent, Margin=new Thickness(5)};
         b.Click += ButtonShowRule_Click;
         Viewbox vb = new Viewbox();
         vb.Child = img;
         b.Content = vb;
         return b;
      }
      private void ButtonShowRule_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         if (null == myRulesManager)
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr=null");
         else if (false == myRulesManager.ShowRule(b.Name))
            Logger.Log(LogEnum.LE_ERROR, "ButtonShowRule_Click(): myRulesMgr.ShowRule() returned false for c=" + b.Name);
      }
   }
}
