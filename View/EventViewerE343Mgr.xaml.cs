using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace BarbarianPrince
{
   /// <summary>
   /// Interaction logic for EventViewerE343Mgr.xaml
   /// </summary>
   public partial class EventViewerE343Mgr : UserControl
   {
      public delegate bool EndE343Callback();
      private const int STARTING_ASSIGNED_ROW = 6;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myRollResult;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myRollResult = Utilities.NO_RESULT;
         }
      };
      public enum E343Enum
      {
         VICTIM_CHECK,
         END_TALISMAN,          // Check for talisman disappearing
         END_TALISMAN_SHOW,     // Show last die roll
         SHOW_END_ROUND,
         SHOW_RESULTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private int mySpecialistCount = 0;
      private bool myIsResistenceTalismanHeldByParty = false;
      private int myRound = 1;
      //---------------------------------------------
      private E343Enum myState = E343Enum.VICTIM_CHECK;
      private EndE343Callback myCallback = null;
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
      public EventViewerE343Mgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE203Mgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE203Mgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE203Mgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE203Mgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE203Mgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool FindVictim(EndE343Callback callback)
      {
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "FindVictim(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, need two party members
         {
            Logger.Log(LogEnum.LE_ERROR, "FindVictim(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myState = E343Enum.VICTIM_CHECK;
         myMaxRowCount = myGameInstance.PartyMembers.Count;
         myIsRollInProgress = false;
         myRollResulltRowNum = 0;
         myCallback = callback;
         myIsResistenceTalismanHeldByParty = myGameInstance.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman);
         myRound = 1;
         mySpecialistCount = 0;
         //--------------------------------------------------
         int i = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "FindVictim(): mi=null");
               return false;
            }
            myGridRows[i] = new GridRow(mi);
            if ("e082" == myGameInstance.EventStart)
            {
               if (true == mi.IsSpecialist())
                  ++mySpecialistCount;
            }
            ++i;
         }
         //--------------------------------------------------
         if ("e082" == myGameInstance.EventStart)
         {
            if ((1 == myGameInstance.PartyMembers.Count) && (false == myIsResistenceTalismanHeldByParty)) // if only prince and no talisman, game over dude
            {
               IMapItem prince = myGameInstance.Prince;
               prince.IsDisappear = true;
               prince.IsKilled = true;
               myGridRows[STARTING_ASSIGNED_ROW].myRollResult = 6;
               myState = E343Enum.SHOW_RESULTS;
            }
            else if ((1 == mySpecialistCount) && (false == myIsResistenceTalismanHeldByParty)) // if only one specialist in party, they are the victim
            {
               myState = E343Enum.END;
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  IMapItem mi = myGridRows[j].myMapItem;
                  if (true == mi.IsSpecialist())
                  {
                     mi.IsDisappear = true;
                     mi.IsKilled = true;
                     myGridRows[j].myRollResult = 6;
                     break;
                  }
               }
               myState = E343Enum.SHOW_RESULTS;
            }
            else
            {
               for (int j = 0; j < myMaxRowCount; ++j)
               {
                  IMapItem mi = myGridRows[j].myMapItem;
                  if (false == mi.IsSpecialist())  
                     myGridRows[j].myRollResult = 0; // If specialist in party and this is not specialist, indicate it is not a possible selection
                  else
                     myGridRows[j].myRollResult = Utilities.NO_RESULT;
               }
            }
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "FindVictim(): UpdateGrid() return false");
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
         if (E343Enum.END == myState)
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
         if (E343Enum.END == myState)
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
         myTextBlockHeader.Text = "e343 Victim Selection - Round#" + myRound.ToString();
         myTextBlockInstructions.Inlines.Clear();
         switch (myState)
         {
            case E343Enum.VICTIM_CHECK:
               if ("e082" == myGameInstance.EventStart)
               {
                  if ((1 == myGameInstance.PartyMembers.Count) && (true == myIsResistenceTalismanHeldByParty))
                     myTextBlockInstructions.Inlines.Add(new Run("Click on talisman to avoid spectre attack."));
                  else if (true == myIsResistenceTalismanHeldByParty)
                     myTextBlockInstructions.Inlines.Add(new Run("Click on talisman or roll one die for each character. If six, they are the victim."));
                  else
                     myTextBlockInstructions.Inlines.Add(new Run("Roll one die for each character. If six, they are the victim."));
               }
               else
               {
                  myTextBlockInstructions.Inlines.Add(new Run("Roll one die for each character. If six, they are the victim."));
               }
               break;
            case E343Enum.END_TALISMAN:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die to check for talisman destruction."));
               break;
            case E343Enum.END_TALISMAN_SHOW:
            case E343Enum.SHOW_END_ROUND:
            case E343Enum.SHOW_RESULTS:
            case E343Enum.END:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
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
            case E343Enum.SHOW_END_ROUND:
            case E343Enum.VICTIM_CHECK:
               if (1 < myGameInstance.PartyMembers.Count)
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(Utilities.theImageDirectoryPath + "Luck.gif", UriKind.Relative);
                  bmi.EndInit();
                  double size = Utilities.ZOOM * Utilities.theMapItemSize;
                  Image img = new Image { Source = bmi, Width = 1.75 * size, Height = size };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myStackPanelAssignable.Children.Add(img);
               }
               else
               {
                  Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r0);
               }
               if ((true == myIsResistenceTalismanHeldByParty) && ("e082" == myGameInstance.EventStart) )
               {
                  Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r0);
                  Image img2 = new Image { Name = "TalismanResistance", Source = MapItem.theMapImages.GetBitmapImage("TalismanResistance"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img2);
               }
               break;
            case E343Enum.END_TALISMAN:
            case E343Enum.END_TALISMAN_SHOW:
            case E343Enum.SHOW_RESULTS:
            case E343Enum.END:
               Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r1);
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
         if ((E343Enum.END_TALISMAN == myState) || (E343Enum.END_TALISMAN_SHOW == myState))
         {
            if (false == UpdateGridRowsTalismanEnd())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsTalismanEnd() return false");
               return false;
            }
            return true;
         }
         //------------------------------------------------------------
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItem;
            //------------------------------------
            if ((0 < mySpecialistCount) && (false == mi.IsSpecialist()))  // If specialist in party and this is not speclist, skip
               continue;
            Button b = CreateButton(mi);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //------------------------------------
            if (0 < myGridRows[i].myRollResult)
            {
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myRollResult.ToString() };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 1);
            }
            else
            {
               if ((E343Enum.SHOW_RESULTS != myState) && (1 < myGameInstance.PartyMembers.Count))
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri(Utilities.theImageDirectoryPath + "dieRoll.gif", UriKind.Relative);
                  bmi.EndInit();
                  Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, rowNum);
                  Grid.SetColumn(img, 1);
               }
            }
            if (6 == myGridRows[i].myRollResult)
            {
               Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "yes" };
               myGrid.Children.Add(label);
               Grid.SetRow(label, rowNum);
               Grid.SetColumn(label, 2);
            }
         }
         return true;
      }
      private bool UpdateGridRowsTalismanEnd()
      {
         myTextBlock2.Text = "Destroyed?";
         //--------------------------------------------------------
         bool isOneDiceResultsShown = false;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi = myGridRows[i].myMapItem;
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsDrug(): mi=null");
               return false;
            }
            //----------------------------------------------
            int rowNum = i + STARTING_ASSIGNED_ROW;
            Button b = CreateButton(mi);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //----------------------------------------------
            if (true == mi.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
            {
               if (0 < myGridRows[i].myRollResult)
               {
                  isOneDiceResultsShown = true;
                  Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myRollResult.ToString() };
                  myGrid.Children.Add(labelforResult);
                  Grid.SetRow(labelforResult, rowNum);
                  Grid.SetColumn(labelforResult, 1);
                  if (6 == myGridRows[i].myRollResult)
                  {
                     BitmapImage bmi0 = new BitmapImage();
                     bmi0.BeginInit();
                     bmi0.UriSource = new Uri(Utilities.theImageDirectoryPath + "TalismanResistanceDestroy.gif", UriKind.Relative);
                     bmi0.EndInit();
                     Image img0 = new Image { Source = bmi0, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     ImageBehavior.SetAnimatedSource(img0, bmi0);
                     ImageBehavior.SetAutoStart(img0, true);
                     ImageBehavior.SetRepeatBehavior(img0, new RepeatBehavior(1));
                     myGrid.Children.Add(img0);
                     Grid.SetRow(img0, rowNum);
                     Grid.SetColumn(img0, 2);
                     if (false == myGameInstance.RemoveSpecialItem(SpecialEnum.ResistanceTalisman, mi))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): RemoveSpecialItem() returned false");
                        return false;
                     }
                  }
                  else
                  {
                     Image img5 = new Image { Name = "TalismanResistance", Source = MapItem.theMapImages.GetBitmapImage("TalismanResistance"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myGrid.Children.Add(img5);
                     Grid.SetRow(img5, rowNum);
                     Grid.SetColumn(img5, 2);
                  }
               }
               else
               {
                  if (E343Enum.END_TALISMAN == myState)
                  {
                     BitmapImage bmi0 = new BitmapImage();
                     bmi0.BeginInit();
                     bmi0.UriSource = new Uri(Utilities.theImageDirectoryPath + "dieRoll.gif", UriKind.Relative);
                     bmi0.EndInit();
                     Image img0 = new Image { Source = bmi0, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img0, bmi0);
                     myGrid.Children.Add(img0);
                     Grid.SetRow(img0, rowNum);
                     Grid.SetColumn(img0, 1);
                  }
               }
            }
         }
         if (true == isOneDiceResultsShown)
            myState = E343Enum.END_TALISMAN_SHOW;
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
         int j = myRollResulltRowNum - STARTING_ASSIGNED_ROW;
         if (j < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): invalid state j=" + j.ToString());
            return;
         }
         IMapItem mi = myGridRows[j].myMapItem;
         myGridRows[j].myRollResult = dieRoll;
         //-----------------------------------------
         if (E343Enum.VICTIM_CHECK == myState) // If only one possible victim, short circuit to show results
         {
            if (6 == dieRoll) // victim selected
            {
               myState = E343Enum.SHOW_RESULTS;
               IMapItem victim = myGridRows[j].myMapItem;
               if ("e082" == myGameInstance.EventStart) // spectre
               {
                  victim.IsDisappear = true;
                  victim.IsKilled = true;
               }
               else if ("e091" == myGameInstance.EventStart) // poison snake
               {
                  victim.SetWounds(0, myGameInstance.DieResults["e091"][0]);
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): invalid state ae=" + myGameInstance.EventStart);
                  return;
               }
            }
            else
            {
               myState = E343Enum.SHOW_END_ROUND;
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  if (Utilities.NO_RESULT == myGridRows[i].myRollResult)
                     myState = E343Enum.VICTIM_CHECK;
               }
            }
         }
         else if (E343Enum.END_TALISMAN == myState)
         {
            myState = E343Enum.END_TALISMAN_SHOW;
         }
         //-----------------------------------------
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      //-----------------------------------------------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (E343Enum.SHOW_END_ROUND == myState)
         {
            myState = E343Enum.VICTIM_CHECK;
            ++myRound;
            for (int i = 0; i < myMaxRowCount; ++i)
            {
               IMapItem mi = myGridRows[i].myMapItem;
               if (("e082" == myGameInstance.EventStart) && (0 < mySpecialistCount) && (false == mi.IsSpecialist()))  // If specialist in party and this is not speclist, skip
                  myGridRows[i].myRollResult = 0;
               else
                  myGridRows[i].myRollResult = Utilities.NO_RESULT;
            }
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         if ((E343Enum.SHOW_RESULTS == myState) || (E343Enum.END_TALISMAN_SHOW == myState))
         {
            myState = E343Enum.END;
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
                  if (ui1 is Image img) // Check all images within the myStackPanelAssignable
                  {
                     if (result.VisualHit == img)
                     {
                        string name = (string)img.Name;
                        if ("TalismanResistance" == name)
                        {
                           myState = E343Enum.END_TALISMAN;
                           for (int i = 0; i < myMaxRowCount; ++i)
                              myGridRows[i].myRollResult = Utilities.NO_RESULT;
                        }
                        if (false == UpdateGrid())
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                        return;
                     }
                  }
               }
            }
            else if (ui is Image img1) // next check all images within the Grid Rows
            {
               if (result.VisualHit == img1)
               {
                  if (false == myIsRollInProgress)
                  {
                     myRollResulltRowNum = Grid.GetRow(img1);
                     myIsRollInProgress = true;
                     myDieRoller.RollMovingDie(myCanvas, ShowDieResults);
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
