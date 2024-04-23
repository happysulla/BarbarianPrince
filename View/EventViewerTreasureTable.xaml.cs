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
      private Dictionary<string, string[]> myReferences = null;
      private Dictionary<string, string[]> myEvents = null;
      //---------------------------------------------
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
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
         //--------------------------------------------------
         myReferences = new Dictionary<string, string[]>();
         myReferences["Farmland"] = new String[6] { "e009", "r231", "r232", "r233", "r234", "r235" };
         myReferences["Countryside"] = new String[6] { "r232", "r236", "r237", "r238", "r239", "r240" };
         myReferences["Forest"] = new String[6] { "r232", "r241", "r242", "r243", "r244", "r240" };
         myReferences["Hills"] = new String[6] { "r232", "r245", "r246", "r247", "r248", "r249" };
         myReferences["Mountains"] = new String[6] { "r232", "r250", "r251", "r252", "r253", "r248" };
         myReferences["Swamp"] = new String[6] { "r232", "r254", "r255", "r256", "r257", "r258" };
         myReferences["Desert"] = new String[6] { "r259", "r260", "r261", "r262", "r263", "r264" };
         myReferences["Cross River"] = new String[6] { "r232", "r265", "r266", "r267", "r268", "r269" };
         myReferences["On Road"] = new String[6] { "r270", "r271", "r272", "r273", "r274", "r275" };
         myReferences["Airborne"] = new String[6] { "r276", "r277", "r278", "r279", "r280", "r281" };
         myReferences["Rafting"] = new String[6] { "r230", "r230", "r230", "r230", "r230", "r230" };
         myEvents = new Dictionary<string, string[]>();
         myEvents["r231"] = new String[6] { "e018", "e018", "e022", "e022", "e023", "e130" };
         myEvents["r232"] = new String[6] { "e003", "e004", "e005", "e006", "e007", "e008" };
         myEvents["r233"] = new String[6] { "e128", "e128", "e128", "e128", "e129", "e017" };
         myEvents["r234"] = new String[6] { "e049", "e048", "e032", "e081", "e050", "e050" };
         myEvents["r235"] = new String[6] { "e078", "e078", "e079", "e079", "e009", "e009" };
         myEvents["r236"] = new String[6] { "e009", "e009", "e050", "e018", "e022", "e023" };
         myEvents["r237"] = new String[6] { "e052", "e055", "e057", "e051", "e034", "e072" };
         myEvents["r238"] = new String[6] { "e077", "e075", "e075", "e075", "e076", "e081" };
         myEvents["r239"] = new String[6] { "e044", "e046", "e067", "e064", "e068", "e069" };
         myEvents["r240"] = new String[6] { "e078", "e078", "e078", "e078", "e079", "e079" };
         myEvents["r241"] = new String[6] { "e074", "e074", "e073", "e009", "e051", "e128" };
         myEvents["r242"] = new String[6] { "e072", "e072", "e052", "e082", "e080", "e080" };
         myEvents["r243"] = new String[6] { "e083", "e083", "e084", "e084", "e076", "e075" };
         myEvents["r244"] = new String[6] { "e165", "e166", "e065", "e064", "e087", "e087" };
         myEvents["r245"] = new String[6] { "e098", "e112", "e023", "e051", "e068", "e022" };
         myEvents["r246"] = new String[6] { "e028", "e028", "e058", "e070", "e055", "e056" };
         myEvents["r247"] = new String[6] { "e076", "e076", "e076", "e075", "e128", "e128" };
         myEvents["r248"] = new String[6] { "e118", "e052", "e059", "e067", "e066", "e064" };
         myEvents["r249"] = new String[6] { "e078", "e078", "e078", "e085", "e079", "e079" };
         myEvents["r250"] = new String[6] { "e099", "e100", "e023", "e068", "e101", "e112" };
         myEvents["r251"] = new String[6] { "e028", "e028", "e058", "e055", "e052", "e054" };
         myEvents["r252"] = new String[6] { "e078", "e078", "e079", "e079", "e088", "e065" };
         myEvents["r253"] = new String[6] { "e085", "e085", "e086", "e086", "e086", "e095" };
         myEvents["r254"] = new String[6] { "e022", "e009", "e073", "e051", "e051", "e074" };
         myEvents["r255"] = new String[6] { "e034", "e082", "e164", "e052", "e057", "e098" };
         myEvents["r256"] = new String[6] { "e091", "e091", "e094", "e094", "e092", "e092" };
         myEvents["r257"] = new String[6] { "e089", "e089", "e089", "e090", "e064", "e093" };
         myEvents["r258"] = new String[6] { "e078", "e078", "e078", "e095", "e095", "e097" };
         myEvents["r259"] = new String[6] { "e022", "e129", "e128", "e051", "e023", "e068" };
         myEvents["r260"] = new String[6] { "e028", "e082", "e055", "e003", "e004", "e028" };
         myEvents["r261"] = new String[6] { "e005", "e120", "e120", "e120", "e067", "e066" };
         myEvents["r262"] = new String[6] { "e034", "e164", "e164", "e091", "e091", "e120" };
         myEvents["r263"] = new String[6] { "e064", "e064", "e121", "e121", "e121", "e093" };
         myEvents["r264"] = new String[6] { "e078", "e078", "e078", "e078", "e096", "e096" };
         myEvents["r265"] = new String[6] { "e122", "e122", "e122", "e009", "e051", "e074" };
         myEvents["r266"] = new String[6] { "e123", "e123", "e057", "e057", "e052", "e055" };
         myEvents["r267"] = new String[6] { "e094", "e094", "e091", "e091", "e075", "e084" };
         myEvents["r268"] = new String[6] { "e083", "e076", "e077", "e124", "e124", "e124" };
         myEvents["r269"] = new String[6] { "e122", "e122", "e122", "e125", "e126", "e127" };
         myEvents["r270"] = new String[6] { "e018", "e022", "e023", "e073", "e009", "e009" };
         myEvents["r271"] = new String[6] { "e050", "e051", "e051", "e051", "e003", "e003" };
         myEvents["r272"] = new String[6] { "e004", "e004", "e005", "e006", "e006", "e008" };
         myEvents["r273"] = new String[6] { "e007", "e007", "e057", "e130", "e128", "e128" };
         myEvents["r274"] = new String[6] { "e049", "e048", "e081", "e128", "e129", "e129" };
         myEvents["r275"] = new String[6] { "e078", "e078", "e079", "e079", "e128", "e129" };
         myEvents["r276"] = new String[6] { "e102", "e102", "e103", "e103", "e104", "e104" };
         myEvents["r277"] = new String[6] { "e112", "e112", "e112", "e112", "e108", "e108" };
         myEvents["r278"] = new String[6] { "e106", "e106", "e105", "e105", "e079", "e079" };
         myEvents["r279"] = new String[6] { "e107", "e109", "e077", "e101", "e110", "e111" };
         myEvents["r280"] = new String[6] { "e099", "e098", "e100", "e101", "e064", "e065" };
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
            myGameInstance.AddCoins(myCoin);
            if (SpecialEnum.None != myItem)
            {
               if (SpecialEnum.PegasusMount == myItem)
                  myGameInstance.AddNewMountToParty(MountEnum.Pegasus);
               else
                  myGameInstance.AddSpecialItem(myItem, myMapItem);
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
         bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
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
         Point p = e.GetPosition((UIElement)sender);
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
