using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public partial class EventViewerTreasureTable : UserControl
   {
      public delegate bool EndTreasureTableCallback();
      private const int STARTING_ASSIGNED_ROW = 6;
      //---------------------------------------------
      public enum TreasureTableEnum
      {
         TT_COIN_ROLL,
         TT_ITEM_ROLL,
         TT_COIN_SHOW_RESULTS,
         TT_ITEM_SHOW_RESULTS,
         TT_ITEM_SHOW_REROLL,
         TT_END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      private int myWealthCode = 0;
      private IMapItem myMapItem = null;
      private int myCoin = 0;
      private SpecialEnum myItem = SpecialEnum.None;
      private string myItemLabel = "";
      private bool myIsReroll = false;  // only used for e039 on a roll of 2 and 2. Switch to Special Treasure Rol A and reroll
      private PegasusTreasureEnum myIsPegasusTreasure = PegasusTreasureEnum.Mount;
      //---------------------------------------------
      private TreasureTableEnum myState = TreasureTableEnum.TT_COIN_ROLL;
      private EndTreasureTableCallback myCallback = null;
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private bool myIsRollInProgress = false;
      private int myRollCoin = Utilities.NO_RESULT;
      private int myRollItem = Utilities.NO_RESULT;
      //---------------------------------------------
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      private readonly FontFamily myFontFam2 = new FontFamily("Tahoma");
      //-----------------------------------------------------------------------------------------
      public EventViewerTreasureTable(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemHuntViewer(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemHuntViewer(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemHuntViewer(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemHuntViewer(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemHuntViewer(): cfm=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);  // used for dotted lines
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool GetTreasure(EndTreasureTableCallback callback, int wealthCode, IMapItem mi, PegasusTreasureEnum pegasusType)
      {
         myMapItem = mi;
         myIsReroll = false; // only used for e039 on a roll of 2 and 2. Switch to Special Treasure Rol A and reroll
         myIsPegasusTreasure = pegasusType;  // is it e188 or e188a
         if (null == callback)
         {
            Logger.Log(LogEnum.LE_ERROR, "GetTreasure(): callback=null");
            return false;
         }
         myCallback = callback;
         //--------------------------------------------------
         switch (wealthCode)
         {
            case 0:
            case 1:
            case 2:
            case 4:
            case 5:
            case 7:
            case 10:
            case 12:
            case 15:
            case 21:
            case 25:
            case 30:
            case 50:
            case 60:
            case 70:
            case 100:
            case 110:
               myWealthCode = wealthCode;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetTreasure(): invalid parameter wc=" + wealthCode.ToString());
               return false;
         }
         //--------------------------------------------------
         myIsRollInProgress = false;
         myRollCoin = Utilities.NO_RESULT;
         myRollItem = Utilities.NO_RESULT;
         myItemLabel = "";
         myState = TreasureTableEnum.TT_COIN_ROLL;
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "GetTreasure(): UpdateGrid() return false");
            return false;
         }
         myScrollViewer.Content = myGrid;
         return true;
      }
      //-----------------------------------------------------------------------------------------
      private bool UpdateGrid()
      {
         if (false == UpdateEndState())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdatedEndState() returned false");
            return false;
         }
         if (TreasureTableEnum.TT_END == myState)
            return true;
         if (false == UpdateUserInstructions())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
            return false;
         }
         if (false == UpdateAssignablePanel())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
            return false;
         }
         if (false == UpdateGridRows())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
            return false;
         }
         return true;
      }
      private bool UpdateEndState()
      {
         if (TreasureTableEnum.TT_END == myState)
         {
            //------------------------------------------
            myGameInstance.AddCoins("EventViewerTreasureTable", myCoin);
            if (SpecialEnum.None != myItem)
            {
               if (SpecialEnum.PegasusMount == myItem)
               {
                  if (false == myGameInstance.AddNewMountToParty(MountEnum.Pegasus))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): AddNewMountToParty() return false");
                     return false;
                  }
               }
               else
               {
                  myGameInstance.AddSpecialItem(myItem, myMapItem);
               }
            }
            //------------------------------------------
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            if (false == myCallback())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateUserInstructions()
      {
         myTextBlockInstructions.Inlines.Clear();
         switch (myState)
         {
            case TreasureTableEnum.TT_COIN_ROLL:
               myTextBlockInstructions.Inlines.Add(new Run("Roll  for gold:"));
               break;
            case TreasureTableEnum.TT_ITEM_ROLL:
               if (true == myIsReroll)
                  myTextBlockInstructions.Inlines.Add(new Run("Unavailable for e039 - Reroll on Special Item Row A:"));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Roll  for special item:"));
               break;
            case TreasureTableEnum.TT_COIN_SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case TreasureTableEnum.TT_ITEM_SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click item for details or click anywhere else to continue."));
               break;
            case TreasureTableEnum.TT_END:
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default s=" + myState.ToString());
               return false; ;
         }
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear();
         //-------------------------------------------------------------------
         Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
         //-------------------------------------------------------------------
         BitmapImage bmi = new BitmapImage();
         bmi.BeginInit();
         bmi.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
         bmi.EndInit();
         Image img1 = new Image { Name = "DieRoll", Source = bmi, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         ImageBehavior.SetAnimatedSource(img1, bmi);
         //-------------------------------------------------------------------
         Image img2 = new Image { Name = "Coin", Source = MapItem.theMapImages.GetBitmapImage("Coin"), Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         //-------------------------------------------------------------------
         Label label = new Label() { FontFamily = myFontFam2, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         //-------------------------------------------------------------------
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case TreasureTableEnum.TT_COIN_ROLL:
               myStackPanelAssignable.Children.Add(img1);
               myStackPanelAssignable.Children.Add(r0);
               break;
            case TreasureTableEnum.TT_ITEM_ROLL:
               myStackPanelAssignable.Children.Add(r0);
               myStackPanelAssignable.Children.Add(img2);
               label.Content = " = " + myCoin.ToString();
               myStackPanelAssignable.Children.Add(label);
               myStackPanelAssignable.Children.Add(r1);
               myStackPanelAssignable.Children.Add(img1);
               break;
            case TreasureTableEnum.TT_COIN_SHOW_RESULTS:
               myStackPanelAssignable.Children.Add(r0);
               myStackPanelAssignable.Children.Add(img2);
               label.Content = " = " + myCoin.ToString();
               myStackPanelAssignable.Children.Add(label);
               myStackPanelAssignable.Children.Add(r1);
               break;
            case TreasureTableEnum.TT_ITEM_SHOW_RESULTS:
               myStackPanelAssignable.Children.Add(r0);
               myStackPanelAssignable.Children.Add(img2);
               label.Content = " = " + myCoin.ToString();
               myStackPanelAssignable.Children.Add(label);
               myStackPanelAssignable.Children.Add(r1);
               if (false == UpdateAssignablePanelShowItem(myItem))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): UpdateAssignablePanelShowItem() returned false");
                  return false;
               }
               break;
            case TreasureTableEnum.TT_END:
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateGridRows()
      {
         //------------------------------------------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //------------------------------------------------------------
         switch (myState)
         {
            case TreasureTableEnum.TT_COIN_ROLL:
            case TreasureTableEnum.TT_COIN_SHOW_RESULTS:
               if (false == UpdateCoinRow(myWealthCode, myRollCoin))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateCoinRow() returned false for s=" + myState.ToString());
                  return false;
               }
               break;
            case TreasureTableEnum.TT_ITEM_ROLL:
            case TreasureTableEnum.TT_ITEM_SHOW_RESULTS:
               if (false == UpdateCoinRow(myWealthCode, myRollCoin))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateCoinRow() returned false for s=" + myState.ToString());
                  return false;
               }
               if (false == UpdateItemRow(myWealthCode, myRollCoin))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateCoinRow() returned false for s=" + myState.ToString());
                  return false;
               }
               break;
            case TreasureTableEnum.TT_END:
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateCoinRow(int wealthCode, int dieRoll)
      {
         Label label = new Label() { FontFamily = myFontFam1, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "Wealth Code " + myWealthCode.ToString() };
         myGrid.Children.Add(label);
         Grid.SetRow(label, STARTING_ASSIGNED_ROW);
         Grid.SetColumn(label, 0);
         //------------------------------------------------
         int[] coins = GameEngine.theTreasureMgr.GetCoinRowContent(wealthCode);
         try
         {
            Button[] buttons = new Button[6];
            buttons[0] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[1] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[2] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[3] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[4] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[5] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            for (int i = 0; i < 6; i++)
            {
               if (i + 1 == myRollCoin)
                  buttons[i].IsEnabled = true;
               myGrid.Children.Add(buttons[i]);
               Grid.SetRow(buttons[i], STARTING_ASSIGNED_ROW);
               Grid.SetColumn(buttons[i], i + 1);
               buttons[i].Content = coins[i].ToString();
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateCoinRow(): e=" + e.ToString());
            return false;
         }
         return true;
      }
      private bool UpdateItemRow(int wealthCode, int dieRoll)
      {
         Label label = new Label() { FontFamily = myFontFam1, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myItemLabel };
         myGrid.Children.Add(label);
         Grid.SetRow(label, STARTING_ASSIGNED_ROW + 1);
         Grid.SetColumn(label, 0);
         string[] items = null;
         //------------------------------------------------
         if (true == myIsReroll)
            items = GameEngine.theTreasureMgr.GetItemRowContent(wealthCode, 1, myIsPegasusTreasure);
         else
            items = GameEngine.theTreasureMgr.GetItemRowContent(wealthCode, myRollCoin, myIsPegasusTreasure);
         try
         {
            Button[] buttons = new Button[6];
            buttons[0] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[1] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[2] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[3] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[4] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[5] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            for (int i = 0; i < 6; i++)
            {
               if (i + 1 == myRollItem)
                  buttons[i].IsEnabled = true;
               myGrid.Children.Add(buttons[i]);
               Grid.SetRow(buttons[i], STARTING_ASSIGNED_ROW + 1);
               Grid.SetColumn(buttons[i], i + 1);
               buttons[i].Content = items[i];
               buttons[i].Click += ButtonRule_Click;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateItemRow(): e=" + e.ToString());
            return false;
         }
         return true;
      }
      private bool UpdateAssignablePanelShowItem(SpecialEnum item)
      {
         switch (item)
         {
            case SpecialEnum.HealingPoition:
               Image img1 = new Image { Name = "r180", Source = MapItem.theMapImages.GetBitmapImage("PotionHeal"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img1);
               break;
            case SpecialEnum.CurePoisonVial:
               Image img2 = new Image { Name = "r181", Source = MapItem.theMapImages.GetBitmapImage("PotionCure"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img2);
               break;
            case SpecialEnum.GiftOfCharm:
               Image img3 = new Image { Name = "r182", Source = MapItem.theMapImages.GetBitmapImage("CharmGift"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img3);
               break;
            case SpecialEnum.EnduranceSash:
               Image img4 = new Image { Name = "r183", Source = MapItem.theMapImages.GetBitmapImage("Sash"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img4);
               break;
            case SpecialEnum.ResistanceTalisman:
               Image img5 = new Image { Name = "r184", Source = MapItem.theMapImages.GetBitmapImage("TalismanResistance"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img5);
               break;
            case SpecialEnum.PoisonDrug:
               Image img6 = new Image { Name = "r185", Source = MapItem.theMapImages.GetBitmapImage("PoisonDrug"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img6);
               break;
            case SpecialEnum.MagicSword:
               Image img7 = new Image { Name = "r186", Source = MapItem.theMapImages.GetBitmapImage("Sword"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img7);
               break;
            case SpecialEnum.AntiPoisonAmulet:
               Image img8 = new Image { Name = "r187", Source = MapItem.theMapImages.GetBitmapImage("AmuletAntiPoison"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img8);
               break;
            case SpecialEnum.PegasusMount:
               Image img9 = new Image { Name = "r188", Source = MapItem.theMapImages.GetBitmapImage("Pegasus"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img9);
               break;
            case SpecialEnum.PegasusMountTalisman:
               Image img9a = new Image { Name = "r188a", Source = MapItem.theMapImages.GetBitmapImage("TalismanPegasus"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img9a);
               break;
            case SpecialEnum.CharismaTalisman:
               Image img10 = new Image { Name = "r189", Source = MapItem.theMapImages.GetBitmapImage("TalismanCharisma"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img10);
               break;
            case SpecialEnum.NerveGasBomb:
               Image img11 = new Image { Name = "r190", Source = MapItem.theMapImages.GetBitmapImage("NerveGasBomb"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img11);
               break;
            case SpecialEnum.ResistanceRing:
               Image img12 = new Image { Name = "r191", Source = MapItem.theMapImages.GetBitmapImage("RingResistence"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img12);
               break;
            case SpecialEnum.ResurrectionNecklace:
               Image img13 = new Image { Name = "r192", Source = MapItem.theMapImages.GetBitmapImage("Necklace"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img13);
               break;
            case SpecialEnum.ShieldOfLight:
               Image img14 = new Image { Name = "r193", Source = MapItem.theMapImages.GetBitmapImage("Shield"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img14);
               break;
            case SpecialEnum.RoyalHelmOfNorthlands:
               Image img15 = new Image { Name = "r194", Source = MapItem.theMapImages.GetBitmapImage("Helmet"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img15);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanelShowItem(): reached default item=" + item.ToString());
               return false;
         }
         return true;
      }
      //-----------------------------------------------------------------------------------------
      public void ShowDieResults(int dieRoll)
      {
         switch (myState)
         {
            case TreasureTableEnum.TT_COIN_ROLL:
               if ("e154e" == myGameInstance.EventStart) // add one to die if Mayor's Daughter
               {
                  if( dieRoll < 6 )
                     ++dieRoll;
               }
               myRollCoin = dieRoll;
               myCoin = GameEngine.theTreasureMgr.GetCoin(myWealthCode, dieRoll);
               if (myCoin < 0)
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): GetCoin() returned error c=" + myCoin.ToString());
               myState = TreasureTableEnum.TT_COIN_SHOW_RESULTS;
               switch (myWealthCode)
               {
                  case 5:
                     switch (dieRoll)
                     {
                        case 2: case 4: case 6: myItemLabel = "Item Row A"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        default: break;
                     }
                     break;
                  case 12:
                     switch (dieRoll)
                     {
                        case 2: myItemLabel = "Item Row C"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        case 3: case 5: myItemLabel = "Item Row A"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        default: break;
                     }
                     break;
                  case 25:
                     switch (dieRoll)
                     {
                        case 1: case 3: case 5: myItemLabel = "Item Row A"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        default: break;
                     }
                     break;
                  case 60:
                     switch (dieRoll)
                     {
                        case 1: myItemLabel = "Item Row A"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        case 2: myItemLabel = "Item Row C"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        case 4: myItemLabel = "Item Row B"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        case 5: myItemLabel = "Item Row A"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        default: break;
                     }
                     break;
                  case 110:
                     switch (dieRoll)
                     {
                        case 1: myItemLabel = "Item Row B"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        case 2: myItemLabel = "Item Row C"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        case 3: myItemLabel = "Item Row B"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        case 4: myItemLabel = "Item Row A"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        case 5: myItemLabel = "Item Row C"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        case 6: myItemLabel = "Item Row A"; myState = TreasureTableEnum.TT_ITEM_ROLL; break;
                        default: break;
                     }
                     break;
                  default: break;
               }
               break;
            case TreasureTableEnum.TT_ITEM_ROLL:
               myRollItem = dieRoll;
               if ((PegasusTreasureEnum.Reroll == myIsPegasusTreasure) && ("Item Row C" == myItemLabel) && (2 == dieRoll) && (60 == myWealthCode)) // special case for e039
               {
                  myIsReroll = true;
                  myRollItem = Utilities.NO_RESULT;
                  myItemLabel = "Item Row A";
                  myState = TreasureTableEnum.TT_ITEM_ROLL;
               }
               else
               {
                  if (true == myIsReroll)
                     myItem = GameEngine.theTreasureMgr.GetSpecialPossession(myWealthCode, 1, dieRoll, myIsPegasusTreasure);
                  else
                     myItem = GameEngine.theTreasureMgr.GetSpecialPossession(myWealthCode, myRollCoin, dieRoll, myIsPegasusTreasure);
                  if (SpecialEnum.None == myItem)
                     Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): GetCoin() returned error c=" + myCoin.ToString());
                  myState = TreasureTableEnum.TT_ITEM_SHOW_RESULTS;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default myState=" + myState.ToString());
               break;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      //-----------------------------------------------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGrid.Children)
         {
            if (ui is StackPanel panel)
            {
               foreach (UIElement ui1 in panel.Children)
               {
                  if (ui1 is Image img) // Check all images within the myStackPanelAssignable
                  {
                     if (result.VisualHit == img)
                     {
                        if ("DieRoll" == img.Name)
                        {
                           if (false == myIsRollInProgress) // myStackPanelAssignable image is clicked
                           {
                              myIsRollInProgress = true;
                              myDieRoller.RollMovingDie(myCanvas, ShowDieResults);
                              img.Visibility = Visibility.Hidden;
                              return;
                           }
                        }
                        else if (true == img.Name.StartsWith("r"))
                        {
                           if (false == myRulesMgr.ShowRule(img.Name))
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ShowRule() returned false img.Name=" + img.Name);
                           return;
                        }
                     }
                  }
               }
            }
         }
         //---------------------------------------------------
         if ((TreasureTableEnum.TT_COIN_SHOW_RESULTS == myState) || (TreasureTableEnum.TT_ITEM_SHOW_RESULTS == myState))
         {
            myState = TreasureTableEnum.TT_END;
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
      }
      private void ButtonRule_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         String content = (String)b.Content;
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr=null");
            return;
         }
         if (true == content.StartsWith("r")) // rules based click
         {
            if (false == myRulesMgr.ShowRule(content))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowRule() returned false");
               return;
            }
         }
         else if (true == content.StartsWith("t")) // rules based click
         {
            if (false == myRulesMgr.ShowTable(content))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowTable() returned false");
               return;
            }
         }
      }
   }
}
