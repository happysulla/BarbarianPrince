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

namespace BarbarianPrince
{
   public partial class EventViewerE031Mgr : UserControl
   {
      public delegate bool EndE031Callback();
      private const int STARTING_ASSIGNED_ROW = 6;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myDieRoll = Utilities.NO_RESULT;
         }
      };
      public enum E031Enum
      {
         SELECT_OPENER,
         OPEN_COMPARTMENT_SUCCESS,
         OPEN_COMPARTMENT_FAIL,
         TRAP_POISON_NEEDLE,
         TRAP_EXPLODE_ACID,
         TRAP_POISON_GAS,
         TRAP_PLAGUE_DUST,
         TRAP_FLYING_SPIKES,
         TRAP_NONE,
         ROLL_FOR_DESTRUCTION,
         SHOW_RESULTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      private int myDieRollAssignPanel = Utilities.NO_RESULT;
      private bool myIsDamageDoneToOpener = false;
      //---------------------------------------------
      private E031Enum myState = E031Enum.SELECT_OPENER;
      private EndE031Callback myCallback = null;
      private int myMaxRowCount = 0;
      private GridRow[] myGridRows = null;
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private int myRollResulltRowNum = 0;
      private bool myIsRollInProgress = false;
      //---------------------------------------------
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-----------------------------------------------------------------------------------------
      public EventViewerE031Mgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE031Mgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE031Mgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE031Mgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE031Mgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE031Mgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool OpenTomb(EndE031Callback callback)
      {
         //--------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenChest(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenChest(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myState = E031Enum.SELECT_OPENER;
         myMaxRowCount = myGameInstance.PartyMembers.Count;
         myIsRollInProgress = false;
         myRollResulltRowNum = 0;
         myDieRollAssignPanel = Utilities.NO_RESULT;
         myCallback = callback;
         myIsDamageDoneToOpener = false;
         //--------------------------------------------------
         int i = 0;
         IMapItem prince = null;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "OpenChest(): mi=null");
               return false;
            }
            if ("Prince" == mi.Name)
               prince = mi;
            myGridRows[i] = new GridRow(mi);
            ++i;
         }
         if (null == prince)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenChest(): prince=null");
            return false;
         }
         //--------------------------------------------------
         // Add the unassignable mapitems that never move or change to the Grid Rows
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenChest(): UpdateGrid() return false");
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
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateEndState() returned false");
            return false;
         }
         if (E031Enum.END == myState)
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
         if ((E031Enum.END == myState) || (true == myGameInstance.Prince.IsKilled))
         {
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
            case E031Enum.SELECT_OPENER:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die for one person to open compartment or select campfire to skip."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               break;
            case E031Enum.OPEN_COMPARTMENT_SUCCESS:
               myTextBlockInstructions.Inlines.Add(new Run("Compartment opened! Opener finds 50 gold coins."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("Also found Gift of Charm: "));
               Button buttonGiftCharm = new Button() { Content = "r182", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
               buttonGiftCharm.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonGiftCharm));
               myTextBlockInstructions.Inlines.Add(new Run(".  Roll for condition."));
               break;
            case E031Enum.OPEN_COMPARTMENT_FAIL:
               myTextBlockInstructions.Inlines.Add(new Run("Compartment has a trap!"));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("Roll for type of trap. Opens if opener survives."));
               break;
            case E031Enum.TRAP_POISON_NEEDLE:
               myTextBlockInstructions.Inlines.Add(new Run("Poison Needle: Opener suffers one poison wound."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case E031Enum.TRAP_EXPLODE_ACID:
               myTextBlockInstructions.Inlines.Add(new Run("Buring acid eplodes afflicting opener."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               if (false == myIsDamageDoneToOpener)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll one die for number of wounds suffered."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click acid cloud to continue."));
               break;
            case E031Enum.TRAP_POISON_GAS:
               myTextBlockInstructions.Inlines.Add(new Run("Poison gas burst afflicting opener."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               if (false == myIsDamageDoneToOpener)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll one die for number of poison suffered."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click gas cloud to continue."));
               break;
            case E031Enum.TRAP_PLAGUE_DUST:
               myTextBlockInstructions.Inlines.Add(new Run("Plague dust sickens opener."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               if (false == myIsDamageDoneToOpener)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll one die and 1/2 rounding up. Wounds suffered at evening meal."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click plague germ to continue."));
               break;
            case E031Enum.TRAP_FLYING_SPIKES:
               myTextBlockInstructions.Inlines.Add(new Run("Flying knives fly toward opener."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               if (false == myIsDamageDoneToOpener)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll one die, add 3, for number of wounds."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click flying knives to continue."));
               break;
            case E031Enum.TRAP_NONE:
               myTextBlockInstructions.Inlines.Add(new Run("Trap failed to work."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case E031Enum.SHOW_RESULTS:
               if (myDieRollAssignPanel < 5)
                  myTextBlockInstructions.Inlines.Add(new Run("Gift of charm ready to use."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Gift of charm crumbles into dust."));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("Click campfire to continue"));
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         BitmapImage bmi0 = new BitmapImage();
         bmi0.BeginInit();
         bmi0.UriSource = new Uri(MapImage.theImageDirectory + "DieRoll.gif", UriKind.Absolute);
         bmi0.EndInit();
         Image img0 = new Image { Tag = "DieRoll", Source = bmi0, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         ImageBehavior.SetAnimatedSource(img0, bmi0);
         switch (myState)
         {
            case E031Enum.SELECT_OPENER:
               BitmapImage bmi1 = new BitmapImage();
               bmi1.BeginInit();
               bmi1.UriSource = new Uri(MapImage.theImageDirectory + "CampFire2.gif", UriKind.Absolute);
               bmi1.EndInit();
               Image img1 = new Image { Tag = "Campfire", Source = bmi1, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img1, bmi1);
               myStackPanelAssignable.Children.Add(img1);
               break;
            case E031Enum.OPEN_COMPARTMENT_SUCCESS: // need to roll for if GiftOfCharm disappears
               Rectangle r10 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r10);
               Image img10 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CharmGift"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img10);
               Rectangle r11 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r11);
               myStackPanelAssignable.Children.Add(img0);
               break;
            case E031Enum.OPEN_COMPARTMENT_FAIL:
               myStackPanelAssignable.Children.Add(img0);
               break;
            case E031Enum.TRAP_POISON_NEEDLE:
               Image img2 = new Image { Tag = "TrapPin", Source = MapItem.theMapImages.GetBitmapImage("TrapPin"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img2);
               break;
            case E031Enum.TRAP_EXPLODE_ACID:
               Image img3 = new Image { Tag = "TrapAcid", Source = MapItem.theMapImages.GetBitmapImage("TrapAcid"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img3);
               break;
            case E031Enum.TRAP_POISON_GAS:
               Image img4 = new Image { Tag = "TrapGas", Source = MapItem.theMapImages.GetBitmapImage("TrapGas"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img4);
               break;
            case E031Enum.TRAP_PLAGUE_DUST:
               Image img5 = new Image { Tag = "TrapPlague", Source = MapItem.theMapImages.GetBitmapImage("TrapPlague"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img5);
               break;
            case E031Enum.TRAP_FLYING_SPIKES:
               Image img6 = new Image { Tag = "TrapFlyingKnives", Source = MapItem.theMapImages.GetBitmapImage("TrapFlyingKnives"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img6);
               break;
            case E031Enum.TRAP_NONE:
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "6" };
               myStackPanelAssignable.Children.Add(label);
               Rectangle r2 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r2);
               break;
            case E031Enum.SHOW_RESULTS:
               BitmapImage bmi7 = new BitmapImage();
               bmi7.BeginInit();
               bmi7.UriSource = new Uri(MapImage.theImageDirectory + "CampFire2.gif", UriKind.Absolute);
               bmi7.EndInit();
               Image img7 = new Image { Tag = "Campfire", Source = bmi7, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img7, bmi7);
               myStackPanelAssignable.Children.Add(img7);
               Rectangle r12 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r12);
               if (myDieRollAssignPanel < 5)
               {
                  Image img12 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CharmGift"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img12);
               }
               else
               {
                  Image img12 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CharmGiftDestroyed"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img12);
               }
               break;
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
         if (E031Enum.SELECT_OPENER == myState)
         {
            for (int j = 0; j < myMaxRowCount; ++j)
            {
               int rowNum = j + STARTING_ASSIGNED_ROW;
               IMapItem mi1 = myGridRows[j].myMapItem;
               if ((true == mi1.IsKilled) || (true == mi1.IsUnconscious))
                  continue;
               Button b1 = CreateButton(mi1);
               myGrid.Children.Add(b1);
               Grid.SetRow(b1, rowNum);
               Grid.SetColumn(b1, 0);
               //----------------------
               BitmapImage bmi1 = new BitmapImage();
               bmi1.BeginInit();
               bmi1.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
               bmi1.EndInit();
               Image img1 = new Image { Source = bmi1, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img1, bmi1);
               myGrid.Children.Add(img1);
               Grid.SetRow(img1, rowNum);
               Grid.SetColumn(img1, 1);
            }
            return true;
         }
         //------------------------------------------------------------
         int i = myRollResulltRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): 0 > i=" + i.ToString() + " myState=" + myState.ToString());
            return false;
         }
         IMapItem mi = myGridRows[i].myMapItem;
         //------------------------------------
         Button b = CreateButton(mi);
         myGrid.Children.Add(b);
         Grid.SetRow(b, myRollResulltRowNum);
         Grid.SetColumn(b, 0);
         //--------------------------------
         Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRoll.ToString() };
         //--------------------------------
         switch (myState)
         {
            case E031Enum.OPEN_COMPARTMENT_SUCCESS:
               myGrid.Children.Add(label);
               Grid.SetRow(label, myRollResulltRowNum);
               Grid.SetColumn(label, 1);
               break;
            case E031Enum.OPEN_COMPARTMENT_FAIL:
               myGrid.Children.Add(label);
               Grid.SetRow(label, myRollResulltRowNum);
               Grid.SetColumn(label, 1);
               break;
            case E031Enum.TRAP_POISON_NEEDLE:
               myGrid.Children.Add(label);
               Grid.SetRow(label, myRollResulltRowNum);
               Grid.SetColumn(label, 1);
               break;
            case E031Enum.TRAP_POISON_GAS:
            case E031Enum.TRAP_EXPLODE_ACID:
            case E031Enum.TRAP_PLAGUE_DUST:
            case E031Enum.TRAP_FLYING_SPIKES:
               if (Utilities.NO_RESULT == myGridRows[i].myDieRoll)
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
                  bmi.EndInit();
                  Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, myRollResulltRowNum);
                  Grid.SetColumn(img, 1);
               }
               else
               {
                  myGrid.Children.Add(label);
                  Grid.SetRow(label, myRollResulltRowNum);
                  Grid.SetColumn(label, 1);
               }
               break;
            case E031Enum.TRAP_NONE:
               myGrid.Children.Add(label);
               Grid.SetRow(label, myRollResulltRowNum);
               Grid.SetColumn(label, 1);
               break;
            case E031Enum.SHOW_RESULTS:
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): reached default myState=" + myState.ToString());
               return false;
         }
         return true;
      }
      //-----------------------------------------------------------------------------------------
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
      public void ShowDieResults(int dieRoll)
      {
         int i = myRollResulltRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString() + " myState=" + myState.ToString());
            return;
         }
         IMapItem mi = myGridRows[i].myMapItem;
         switch (myState)
         {
            case E031Enum.SELECT_OPENER:
               myGridRows[i].myDieRoll = dieRoll;
               myIsDamageDoneToOpener = false;
               if (dieRoll < myGameInstance.WitAndWile)
                  myState = E031Enum.OPEN_COMPARTMENT_SUCCESS;
               else
                  myState = E031Enum.OPEN_COMPARTMENT_FAIL;
               break;
            case E031Enum.OPEN_COMPARTMENT_FAIL:
               switch (dieRoll)
               {
                  case 1: myState = E031Enum.TRAP_POISON_NEEDLE; mi.SetWounds(0, 1); break;
                  case 2: myState = E031Enum.TRAP_EXPLODE_ACID; myGridRows[i].myDieRoll = Utilities.NO_RESULT; break;
                  case 3: myState = E031Enum.TRAP_POISON_GAS; myGridRows[i].myDieRoll = Utilities.NO_RESULT; break;
                  case 4: myState = E031Enum.TRAP_PLAGUE_DUST; myGridRows[i].myDieRoll = Utilities.NO_RESULT; break;
                  case 5: myState = E031Enum.TRAP_FLYING_SPIKES; myGridRows[i].myDieRoll = Utilities.NO_RESULT; break;
                  case 6: myState = E031Enum.TRAP_NONE; break;
                  default: Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): dieRoll=" + dieRoll.ToString()); return;
               }
               break;
            case E031Enum.TRAP_EXPLODE_ACID:
               myGridRows[i].myDieRoll = dieRoll;
               mi.SetWounds(dieRoll, 0);
               myIsDamageDoneToOpener = true;
               myState = E031Enum.OPEN_COMPARTMENT_SUCCESS;
               if (true == mi.IsKilled)
               {
                  myState = E031Enum.SELECT_OPENER;
                  myRollResulltRowNum = 0;
               }
               break;
            case E031Enum.TRAP_POISON_GAS:
               myGridRows[i].myDieRoll = dieRoll;
               mi.SetWounds(0, dieRoll);
               myIsDamageDoneToOpener = true;
               myState = E031Enum.OPEN_COMPARTMENT_SUCCESS;
               if (true == mi.IsKilled)
               {
                  myState = E031Enum.SELECT_OPENER;
                  myRollResulltRowNum = 0;
               }
               break;
            case E031Enum.TRAP_PLAGUE_DUST:
               myGridRows[i].myDieRoll = (int)Math.Ceiling((Double)dieRoll / 2);
               mi.PlagueDustWound = myGridRows[i].myDieRoll; // applied during evening meal - see r227
               mi.OverlayImageName = "OPlagueDust";
               myIsDamageDoneToOpener = true;
               myState = E031Enum.OPEN_COMPARTMENT_SUCCESS;
               break;
            case E031Enum.TRAP_FLYING_SPIKES:
               myGridRows[i].myDieRoll = dieRoll + 3;
               mi.SetWounds(myGridRows[i].myDieRoll, 0);
               myIsDamageDoneToOpener = true;
               myState = E031Enum.OPEN_COMPARTMENT_SUCCESS;
               if (true == mi.IsKilled)
               {
                  myState = E031Enum.SELECT_OPENER;
                  myRollResulltRowNum = 0;
               }
               break;
            case E031Enum.TRAP_NONE:
               myState = E031Enum.OPEN_COMPARTMENT_SUCCESS;
               break;
            case E031Enum.OPEN_COMPARTMENT_SUCCESS:
               myDieRollAssignPanel = dieRoll;  
               if (dieRoll < 5)
                  myGameInstance.AddSpecialItem(SpecialEnum.GiftOfCharm, mi);
               if (false == myGameInstance.AddCoins("EventViewerE031Mgr", 50))
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): AddCoins() returned false for myState=" + myState.ToString());
               myState = E031Enum.SHOW_RESULTS;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default myState=" + myState.ToString());
               return;
         }
         //-----------------------------------------------------------------
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      //-----------------------------------------------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if ((E031Enum.TRAP_NONE == myState) || (E031Enum.TRAP_POISON_NEEDLE == myState))
         {
            myState = E031Enum.OPEN_COMPARTMENT_SUCCESS;
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGrid.Children)
         {
            if (ui is StackPanel panel)
            {
               foreach (UIElement ui1 in panel.Children)
               {
                  if (ui1 is Image img0) // Check all images within the myStackPanelAssignable
                  {
                     if (result.VisualHit == img0)
                     {
                        string tag = (string)img0.Tag;
                        if ("Campfire" == tag)
                        {
                           myState = E031Enum.END;
                        }
                        else if ((true == myIsDamageDoneToOpener) && (("TrapAcid" == tag) || ("TrapGas" == tag) || ("TrapPlague" == tag) || ("TrapFlyingKnives" == tag)))
                        {
                           myState = E031Enum.OPEN_COMPARTMENT_SUCCESS;
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                        else if ("DieRoll" == tag)
                        {
                           if (false == myIsRollInProgress)
                           {
                              myIsRollInProgress = true;
                              RollEndCallback callback = ShowDieResults;
                              myDieRoller.RollMovingDie(myCanvas, callback);
                              img0.Visibility = Visibility.Hidden;
                           }
                           return;
                        }
                        if (false == UpdateGrid())
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                        return;
                     }
                  }
               }
            }
            if (ui is Image img1) // next check all images within the Grid Rows
            {
               if (result.VisualHit == img1)
               {
                  if (false == myIsRollInProgress)
                  {
                     if (E031Enum.SELECT_OPENER == myState)
                        myRollResulltRowNum = Grid.GetRow(img1);  // select the row number of the opener
                     myIsRollInProgress = true;
                     RollEndCallback callback = ShowDieResults;
                     myDieRoller.RollMovingDie(myCanvas, callback);
                     img1.Visibility = Visibility.Hidden;
                  }
                  return;
               }
            }
         }
      }
      private void ButtonRule_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         String content = (String)b.Content;
         if (null == myRulesMgr)
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr=null");
         else if (false == myRulesMgr.ShowRule(content))
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false for c=" + content);
      }
   }
}

