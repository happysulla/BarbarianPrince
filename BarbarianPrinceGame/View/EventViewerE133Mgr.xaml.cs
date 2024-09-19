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
   public partial class EventViewerE133Mgr : UserControl
   {
      public delegate bool EndE133Callback(bool isPrinceAffected);
      private const int STARTING_ASSIGNED_ROW = 6;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll;
         public bool myIsAffected;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myDieRoll = Utilities.NO_RESULT;
            myIsAffected = false;
         }
      };
      public enum E133Enum
      {
         PLAGUE_CHECK,
         SHOW_PRINCE_RESULTS,
         SHOW_PARTY_RESULTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      private bool myIsPrinceAffected = false;
      //---------------------------------------------
      private E133Enum myState = E133Enum.PLAGUE_CHECK;
      private EndE133Callback myCallback = null;
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
      public EventViewerE133Mgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE134Mgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE134Mgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE134Mgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE134Mgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE134Mgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool CheckPlague(EndE133Callback callback)
      {
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckPlague(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckPlague(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myState = E133Enum.PLAGUE_CHECK;
         myMaxRowCount = myGameInstance.PartyMembers.Count;
         myIsPrinceAffected = false;
         myIsRollInProgress = false;
         myRollResulltRowNum = 0;
         myCallback = callback;
         //--------------------------------------------------
         int i = 0;
         IMapItem prince = null;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "CheckPlague(): mi=null");
               return false;
            }
            if ("Prince" == mi.Name)
               prince = mi;
            myGridRows[i] = new GridRow(mi);
            ++i;
         }
         if (null == prince)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckPlague(): prince=null");
            return false;
         }
         //--------------------------------------------------
         // Add the unassignable mapitems that never move or change to the Grid Rows
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckPlague(): UpdateGrid() return false");
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
         if (E133Enum.END == myState)
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
         if (E133Enum.END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            if (false == myCallback(myIsPrinceAffected))
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
            case E133Enum.PLAGUE_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll one die for each character in your party. A 3+ means madness"));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("destroyed their mind, and the character crumples into a heap and soon "));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("dies with foaming at the mouth."));
               break;
            case E133Enum.SHOW_PRINCE_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("You are a victim of the madness yourself, but your northern blood"));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("helps you to survive. You awake the next morning having gone "));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("without food: "));
               Button buttonStarve = new Button() { Content = "r216", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
               buttonStarve.Click += ButtonRule_Click;
               myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonStarve));
               myTextBlockInstructions.Inlines.Add(new Run(". You find all your surviving followers dead "));
               myTextBlockInstructions.Inlines.Add(new LineBreak());
               myTextBlockInstructions.Inlines.Add(new Run("or escaped with all your wealth. Click sunshine to start a new day."));
               break;
            case E133Enum.END:
            case E133Enum.SHOW_PARTY_RESULTS:
               Button buttonEscape = new Button() { Content = "r218a", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
               buttonEscape.Click += ButtonRule_Click;
               if (1 < myMaxRowCount)
               {
                  myTextBlockInstructions.Inlines.Add(new Run("All surviving members immediately escape from the hex: "));
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonEscape));
                  myTextBlockInstructions.Inlines.Add(new LineBreak());
                  myTextBlockInstructions.Inlines.Add(new Run("Mounts are unaffected so survivors can bring mounts, possessions,"));
                  myTextBlockInstructions.Inlines.Add(new LineBreak());
                  myTextBlockInstructions.Inlines.Add(new Run("and wealth of the entire party with them. Click campfire to continue."));
               }
               else
               {
                  myTextBlockInstructions.Inlines.Add(new Run("You immediately escape from the hex: "));
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonEscape));
                  myTextBlockInstructions.Inlines.Add(new LineBreak());
                  myTextBlockInstructions.Inlines.Add(new Run("Mounts are unaffected so you can bring mounts, possessions,"));
                  myTextBlockInstructions.Inlines.Add(new LineBreak());
                  myTextBlockInstructions.Inlines.Add(new Run("and wealth. Click campfire to continue."));
               }

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
         switch (myState)
         {
            case E133Enum.PLAGUE_CHECK:
               Rectangle r8 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myStackPanelAssignable.Children.Add(r8);
               break;
            case E133Enum.SHOW_PARTY_RESULTS:
            case E133Enum.END:
               BitmapImage bmi6 = new BitmapImage();
               bmi6.BeginInit();
               bmi6.UriSource = new Uri(MapImage.theImageDirectory + "CampFire2.gif", UriKind.Absolute);
               bmi6.EndInit();
               Image img6 = new Image { Tag = "Campfire", Source = bmi6, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img6, bmi6);
               myStackPanelAssignable.Children.Add(img6);
               //-------------------------------------------
               Rectangle r9 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Width = Utilities.ZOOM * Utilities.theMapItemOffset,
                  Height = Utilities.ZOOM * Utilities.theMapItemOffset
               };
               myStackPanelAssignable.Children.Add(r9);
               break;
            case E133Enum.SHOW_PRINCE_RESULTS:
               BitmapImage bmi7 = new BitmapImage();
               bmi7.BeginInit();
               bmi7.UriSource = new Uri(MapImage.theImageDirectory + "Sun1.gif", UriKind.Absolute);
               bmi7.EndInit();
               Image img7 = new Image { Tag = "Sunshine", Source = bmi7, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img7, bmi7);
               myStackPanelAssignable.Children.Add(img7);
               //-------------------------------------------
               Rectangle r7 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Width = Utilities.ZOOM * Utilities.theMapItemOffset,
                  Height = Utilities.ZOOM * Utilities.theMapItemOffset
               };
               myStackPanelAssignable.Children.Add(r7);
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
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItem;
            Button b = CreateButton(mi);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //------------------------------------
            CheckBox cb = new CheckBox() { IsChecked = myGridRows[i].myIsAffected, IsEnabled = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            myGrid.Children.Add(cb);
            Grid.SetRow(cb, rowNum);
            Grid.SetColumn(cb, 2);
            if (Utilities.NO_RESULT == myGridRows[i].myDieRoll)
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
               bmi.EndInit();
               Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img, bmi);
               myGrid.Children.Add(img);
               Grid.SetRow(img, rowNum);
               Grid.SetColumn(img, 1);
            }
            else
            {
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRoll.ToString() };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 1);
            }
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
         //-----------------------------------------------------------------
         int rowNum = myRollResulltRowNum - STARTING_ASSIGNED_ROW;
         IMapItem mi = myGridRows[rowNum].myMapItem;
         myGridRows[rowNum].myDieRoll = dieRoll;
         if (3 <= dieRoll)
         {
            myGridRows[rowNum].myIsAffected = true;
            mi.IsPlagued = true;
            if ("Prince" == myGridRows[rowNum].myMapItem.Name)
               myIsPrinceAffected = true;
         }
         //-----------------------------------------------------------------
         myState = E133Enum.SHOW_PARTY_RESULTS;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            if (Utilities.NO_RESULT == myGridRows[i].myDieRoll)
            {
               myState = E133Enum.PLAGUE_CHECK;
               break;
            }
         }
         if (E133Enum.SHOW_PARTY_RESULTS == myState)
         {
            if (true == myIsPrinceAffected)
               myState = E133Enum.SHOW_PRINCE_RESULTS;
         }
         //-----------------------------------------------------------------
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
                        string name = (string)img.Tag;
                        if ("Campfire" == name)
                           myState = E133Enum.END;
                        if ("Sunshine" == name)
                           myState = E133Enum.END;
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
                     myRollResulltRowNum = Grid.GetRow(img1);
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
