using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAnimatedGif;
using static BarbarianPrince.EventViewerE343Mgr;

namespace BarbarianPrince
{
    public partial class EventViewerE073FrogMgr : UserControl
    {
        public delegate bool EndWitchCurse( bool isTalismanUsed, bool isPrinceFrog );
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
        public enum E073Enum
        {
            CURSE_CHECK,
            END_TALISMAN,          // Check for talisman disappearing
            END_TALISMAN_SHOW,     // Show last die roll
            SHOW_RESULTS,
            END
        };
        //---------------------------------------------
        public bool CtorError { get; } = false;
        //---------------------------------------------
        private E073Enum myState = E073Enum.CURSE_CHECK;
        private EndWitchCurse myCallback = null;
        private int myMaxRowCount = 0;
        private GridRow[] myGridRows = null;
        private bool myIsResistenceTalismanHeldByParty = false;
        private bool myIsPrinceFrog = false;
        private bool myIsTalismanUsed = false;
        //---------------------------------------------
        private IGameInstance myGameInstance = null;
        private readonly Canvas myCanvas = null;
        private readonly ScrollViewer myScrollViewer = null;
        private RuleDialogViewer myRulesMgr = null;
        //---------------------------------------------
        private IDieRoller myDieRoller = null;
        private int myDieRollRowNum = 0;
        private bool myIsRollInProgress = false;
        //---------------------------------------------
        private readonly FontFamily myFontFam = new FontFamily("Tahoma");
        //-----------------------------------------------------------------------------------------
        public EventViewerE073FrogMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
        {
            InitializeComponent();
            //--------------------------------------------------
            if (null == gi) // check parameter inputs
            {
                Logger.Log(LogEnum.LE_ERROR, "EventViewerE212CurseMgr(): gi=null");
                CtorError = true;
                return;
            }
            myGameInstance = gi;
            //--------------------------------------------------
            if (null == c) // check parameter inputs
            {
                Logger.Log(LogEnum.LE_ERROR, "EventViewerE212CurseMgr(): c=null");
                CtorError = true;
                return;
            }
            myCanvas = c;
            //--------------------------------------------------
            if (null == sv)
            {
                Logger.Log(LogEnum.LE_ERROR, "EventViewerE212CurseMgr(): sv=null");
                CtorError = true;
                return;
            }
            myScrollViewer = sv;
            //--------------------------------------------------
            if (null == rdv)
            {
                Logger.Log(LogEnum.LE_ERROR, "EventViewerE212CurseMgr(): rdv=null");
                CtorError = true;
                return;
            }
            myRulesMgr = rdv;
            //--------------------------------------------------
            if (null == dr)
            {
                Logger.Log(LogEnum.LE_ERROR, "EventViewerE212CurseMgr(): dr=true");
                CtorError = true;
                return;
            }
            myDieRoller = dr;
            //--------------------------------------------------
            myGrid.MouseDown += Grid_MouseDown;
        }
        public bool WitchCurseCheck(EndWitchCurse callback)
        {
            //--------------------------------------------------
            if (null == myGameInstance.PartyMembers)
            {
                Logger.Log(LogEnum.LE_ERROR, "WitchCurseCheck(): partyMembers=null");
                return false;
            }
            if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
            {
                Logger.Log(LogEnum.LE_ERROR, "WitchCurseCheck(): myGameInstance.PartyMembers.Count < 1");
                return false;
            }
            //--------------------------------------------------
            myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
            myState = E073Enum.CURSE_CHECK;
            myMaxRowCount = myGameInstance.PartyMembers.Count;
            myIsRollInProgress = false;
            myDieRollRowNum = 0;
            myCallback = callback;
            myIsPrinceFrog = false;
            myIsTalismanUsed = false;
            myIsResistenceTalismanHeldByParty = myGameInstance.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman);
            //--------------------------------------------------
            int i = 0;
            foreach (IMapItem mi in myGameInstance.PartyMembers)
            {
                if (null == mi)
                {
                    Logger.Log(LogEnum.LE_ERROR, "WitchCurseCheck(): mi=null");
                    return false;
                }
                myGridRows[i] = new GridRow(mi);
                ++i;
            }
            //--------------------------------------------------
            // Add the unassignable mapitems that never move or change to the Grid Rows
            if (false == UpdateGrid())
            {
                Logger.Log(LogEnum.LE_ERROR, "WitchCurseCheck(): UpdateGrid() return false");
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
            if (E073Enum.END == myState)
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
            if (E073Enum.END == myState)
            {
                if (null == myCallback)
                {
                    Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
                    return false;
                }
                //---------------------------------
                bool isAnyMemberFrog = false;
                for (int i = 0; i < myMaxRowCount; ++i) // Remove all members that deserted
                {
                    if (6 == myGridRows[i].myDieRoll)
                    {
                        myGameInstance.RemoveAbandonedInParty(myGridRows[i].myMapItem, true); // no possessions are transferred as part of escape
                        isAnyMemberFrog = true;
                    }
                }
                //---------------------------------
                if (true == isAnyMemberFrog) // If any member leaves, all fickle members leave
                {
                    IMapItems fickleMembers = new MapItems();
                    foreach (IMapItem mi in myGameInstance.PartyMembers)
                    {
                        if (true == mi.IsFickle)
                            fickleMembers.Add(mi);
                    }
                    foreach (IMapItem mi in fickleMembers)
                        myGameInstance.RemoveAbandonerInParty(mi);
                }
                //---------------------------------
                if (false == myCallback( myIsTalismanUsed, myIsPrinceFrog))
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
                case E073Enum.CURSE_CHECK:
                    if (true == myIsResistenceTalismanHeldByParty)
                        myTextBlockInstructions.Inlines.Add(new Run("Click talisman or roll for witch's spell."));
                    else
                        myTextBlockInstructions.Inlines.Add(new Run("Roll for witch's spell."));
                    break;
                case E073Enum.END_TALISMAN:
                    myTextBlockInstructions.Inlines.Add(new Run("Roll die to check for talisman destruction."));
                    break;
                case E073Enum.END_TALISMAN_SHOW:
                case E073Enum.SHOW_RESULTS:
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
                case E073Enum.CURSE_CHECK:
                    if (true == myIsResistenceTalismanHeldByParty)
                    {
                        Image img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanResistance"), Name = "TalismanResistance", Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                        myStackPanelAssignable.Children.Add(img3);
                    }
                    else
                    {
                        Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                        myStackPanelAssignable.Children.Add(r0);
                    }
                    break;
                case E073Enum.END_TALISMAN:
                case E073Enum.END_TALISMAN_SHOW:
                case E073Enum.SHOW_RESULTS:
                case E073Enum.END:
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
            if ((E073Enum.END_TALISMAN == myState) || (E073Enum.END_TALISMAN_SHOW == myState))
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
                Button b = CreateButton(mi);
                myGrid.Children.Add(b);
                Grid.SetRow(b, rowNum);
                Grid.SetColumn(b, 0);
                //--------------------------------
                if (Utilities.NO_RESULT == myGridRows[i].myDieRoll)
                {
                    BitmapImage bmi = new BitmapImage();
                    bmi.BeginInit();
                    bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
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
                    //-------------------------------

                    if (6 != myGridRows[i].myDieRoll)
                    {
                        Label labelResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "Escape" };
                        myGrid.Children.Add(labelResult);
                        Grid.SetRow(labelResult, rowNum);
                        Grid.SetColumn(labelResult, 2);
                    }
                    else
                    {
                        BitmapImage bmi = new BitmapImage();
                        bmi.BeginInit();
                        bmi.UriSource = new Uri("../../Images/Frog.gif", UriKind.Relative);
                        bmi.EndInit();
                        Image img = new Image { Source = bmi, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                        ImageBehavior.SetAnimatedSource(img, bmi);
                        myGrid.Children.Add(img);
                        Grid.SetRow(img, rowNum);
                        Grid.SetColumn(img, 2);
                    }
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
                    if (0 < myGridRows[i].myDieRoll)
                    {
                        isOneDiceResultsShown = true;
                        Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myDieRoll.ToString() };
                        myGrid.Children.Add(labelforResult);
                        Grid.SetRow(labelforResult, rowNum);
                        Grid.SetColumn(labelforResult, 1);
                        if (6 == myGridRows[i].myDieRoll)
                        {
                            BitmapImage bmi0 = new BitmapImage();
                            bmi0.BeginInit();
                            bmi0.UriSource = new Uri("../../Images/TalismanResistanceDestroy.gif", UriKind.Relative);
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
                        if (E073Enum.END_TALISMAN == myState)
                        {
                            BitmapImage bmi0 = new BitmapImage();
                            bmi0.BeginInit();
                            bmi0.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
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
                myState = E073Enum.END_TALISMAN_SHOW;
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
            int i = myDieRollRowNum - STARTING_ASSIGNED_ROW;
            if (i < 0)
            {
                Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): 0 > i=" + i.ToString() + " myState=" + myState.ToString());
                return;
            }
            myGridRows[i].myDieRoll = dieRoll;
            if (E073Enum.CURSE_CHECK == myState)
            {
                myState = E073Enum.SHOW_RESULTS;

                if (("Prince" == myGridRows[i].myMapItem.Name) && (6 == dieRoll))
                    myIsPrinceFrog = true;
                for (int j = 0; j < myMaxRowCount; ++j)
                {
                    IMapItem mi1 = myGridRows[j].myMapItem;
                    if (Utilities.NO_RESULT == myGridRows[j].myDieRoll)
                        myState = E073Enum.CURSE_CHECK;
                }
            }
            else if (E073Enum.END_TALISMAN == myState)
            {
                myState = E073Enum.END_TALISMAN_SHOW;
            }
            //-----------------------------------------------------------------
            if (false == UpdateGrid())
                Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
            myIsRollInProgress = false;
        }
        //-----------------------------------------------------------------------------------------
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((E073Enum.SHOW_RESULTS == myState) || (E073Enum.END_TALISMAN_SHOW == myState))
            {
                myState = E073Enum.END;
                if (false == UpdateGrid())
                    Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                return;
            }
            Point p = e.GetPosition((UIElement)sender);
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
                                string name = (string)img0.Name;
                                if ("TalismanResistance" == name)
                                {
                                    myState = E073Enum.END_TALISMAN;
                                    myIsTalismanUsed = true;    
                                    for (int i = 0; i < myMaxRowCount; ++i) // cannot chance past results
                                    {
                                        if( Utilities.NO_RESULT == myGridRows[i].myDieRoll)
                                           myGridRows[i].myDieRoll = 0;
                                    }
                                    if (false == UpdateGrid())
                                        Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                                    return;
                                }
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
                            myDieRollRowNum = Grid.GetRow(img1);  // select the row number of the opener
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
