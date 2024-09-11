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
    public partial class EventViewerE134Mgr : UserControl
    {
        public delegate bool EndE134Callback(bool isRepeat);
        private const int STARTING_ASSIGNED_ROW = 6;
        //---------------------------------------------
        public struct GridRow
        {
            public IMapItem myMapItem;
            public bool myIsWoundedRolled;
            public bool myIsWounded;
            public int myWounds;
            public GridRow(IMapItem mi)
            {
                myMapItem = mi;
                myIsWoundedRolled = false;
                myIsWounded = false;
                myWounds = Utilities.NO_RESULT;
            }
        };
        public enum E134Enum
        {
            WOUND_CHECK,
            SHOW_RESULTS,
            END
        };
        //---------------------------------------------
        public bool CtorError { get; } = false;
        private bool myIsRepeatSearch = false;
        //---------------------------------------------
        private E134Enum myState = E134Enum.WOUND_CHECK;
        private EndE134Callback myCallback = null;
        private int myMaxRowCount = 0;
        private int myWoundedCount = 0;
        private GridRow[] myGridRows = null;
        //---------------------------------------------
        private IGameInstance myGameInstance = null;
        private readonly Canvas myCanvas = null;
        private readonly ScrollViewer myScrollViewer = null;
        private RuleDialogViewer myRulesMgr = null;
        //---------------------------------------------
        private IDieRoller myDieRoller = null;
        private int myRollResulltRowNum = 0;
        private int myRollResulltColNum = 0;
        private bool myIsRollInProgress = false;
        //---------------------------------------------
        private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
        private readonly FontFamily myFontFam = new FontFamily("Tahoma");
        private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
        //-----------------------------------------------------------------------------------------
        public EventViewerE134Mgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
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
        public bool CheckRubbleDamage(EndE134Callback callback)
        {
            if (null == myGameInstance.PartyMembers)
            {
                Logger.Log(LogEnum.LE_ERROR, "CheckRubbleDamage(): partyMembers=null");
                return false;
            }
            if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
            {
                Logger.Log(LogEnum.LE_ERROR, "CheckRubbleDamage(): myGameInstance.PartyMembers.Count < 1");
                return false;
            }
            //--------------------------------------------------
            myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
            myState = E134Enum.WOUND_CHECK;
            myMaxRowCount = myGameInstance.PartyMembers.Count;
            myIsRollInProgress = false;
            myRollResulltRowNum = 0;
            myRollResulltColNum = 0;
            myCallback = callback;
            //--------------------------------------------------
            int i = 0;
            IMapItem prince = null;
            foreach (IMapItem mi in myGameInstance.PartyMembers)
            {
                if (null == mi)
                {
                    Logger.Log(LogEnum.LE_ERROR, "CheckRubbleDamage(): mi=null");
                    return false;
                }
                if ("Prince" == mi.Name)
                    prince = mi;
                myGridRows[i] = new GridRow(mi);
                ++i;
            }
            if (null == prince)
            {
                Logger.Log(LogEnum.LE_ERROR, "CheckRubbleDamage(): prince=null");
                return false;
            }
            //--------------------------------------------------
            // Add the unassignable mapitems that never move or change to the Grid Rows
            if (false == UpdateGrid())
            {
                Logger.Log(LogEnum.LE_ERROR, "CheckRubbleDamage(): UpdateGrid() return false");
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
            if (E134Enum.END == myState)
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
            if (E134Enum.END == myState)
            {
                if (null == myCallback)
                {
                    Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
                    return false;
                }
                if (false == myCallback(myIsRepeatSearch))
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
                case E134Enum.WOUND_CHECK:
                    myTextBlockInstructions.Inlines.Add(new Run("The ruins are unstable. Roll one die for each character in your party. If a 6+ "));
                    myTextBlockInstructions.Inlines.Add(new LineBreak());
                    myTextBlockInstructions.Inlines.Add(new Run("is rolled, he is injured in the rubble. Roll two dice for the wounds suffered."));
                    break;
                case E134Enum.SHOW_RESULTS:
                case E134Enum.END:
                    bool isSearchPossible = (myWoundedCount < myMaxRowCount); // If everybody is wounded, a search is not possible
                    if (true == isSearchPossible)
                    {
                        myTextBlockInstructions.Inlines.Add(new Run("Click the campfire to end the search."));
                        myTextBlockInstructions.Inlines.Add(new LineBreak());
                        myTextBlockInstructions.Inlines.Add(new Run("Click the shaky walls continue to search this day."));
                    }
                    else
                    {
                        myTextBlockInstructions.Inlines.Add(new Run("No additonal search is possible this day since all characters are wounded."));
                        myTextBlockInstructions.Inlines.Add(new LineBreak());
                        myTextBlockInstructions.Inlines.Add(new Run("Click the campfire to continue."));
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
                case E134Enum.WOUND_CHECK:
                    Rectangle r8 = new Rectangle()
                    {
                        Visibility = Visibility.Hidden,
                        Width = Utilities.ZOOM * Utilities.theMapItemSize,
                        Height = Utilities.ZOOM * Utilities.theMapItemSize
                    };
                    myStackPanelAssignable.Children.Add(r8);
                    break;
                case E134Enum.SHOW_RESULTS:
                case E134Enum.END:
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
                    //-------------------------------------------
                    bool isSearchPossible = (myWoundedCount < myMaxRowCount); // If everybody is wounded, a search is not possible
                    if (true == isSearchPossible)
                    {
                        BitmapImage bmi7 = new BitmapImage();
                        bmi7.BeginInit();
                        bmi7.UriSource = new Uri(MapImage.theImageDirectory + "ShakyWalls.gif", UriKind.Absolute);
                        bmi7.EndInit();
                        Image img7 = new Image { Tag = "ShakyWalls", Source = bmi7, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                        ImageBehavior.SetAnimatedSource(img7, bmi7);
                        myStackPanelAssignable.Children.Add(img7);
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
            for (int i = 0; i < myMaxRowCount; ++i)
            {
                int rowNum = i + STARTING_ASSIGNED_ROW;
                IMapItem mi = myGridRows[i].myMapItem;
                Button b = CreateButton(mi);
                myGrid.Children.Add(b);
                Grid.SetRow(b, rowNum);
                Grid.SetColumn(b, 0);
                //------------------------------------
                if (true == myGridRows[i].myIsWoundedRolled)
                {
                    CheckBox cb = new CheckBox() { IsChecked = myGridRows[i].myIsWounded, IsEnabled = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                    myGrid.Children.Add(cb);
                    Grid.SetRow(cb, rowNum);
                    Grid.SetColumn(cb, 1);
                    if (Utilities.NO_RESULT != myGridRows[i].myWounds)
                    {
                        Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myWounds.ToString() };
                        myGrid.Children.Add(label);
                        Grid.SetRow(label, rowNum);
                        Grid.SetColumn(label, 2);
                    }
                    else
                    {
                        BitmapImage bmi = new BitmapImage();
                        bmi.BeginInit();
                        bmi.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
                        bmi.EndInit();
                        Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                        ImageBehavior.SetAnimatedSource(img, bmi);
                        myGrid.Children.Add(img);
                        Grid.SetRow(img, rowNum);
                        Grid.SetColumn(img, 2);
                    }
                }
                else
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
            if (1 == myRollResulltColNum)
            {
                myGridRows[rowNum].myIsWoundedRolled = true;
                if (6 <= dieRoll)
                    myGridRows[rowNum].myIsWounded = true;
                else
                    myGridRows[rowNum].myWounds = 0;
            }
            else if (2 == myRollResulltColNum)
            {
                IMapItem mi = myGridRows[rowNum].myMapItem;
                ++myWoundedCount;
                mi.SetWounds(dieRoll, 0);
                myGridRows[rowNum].myWounds = dieRoll;
            }
            else
            {
                Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default myRollResulltColNum=" + myRollResulltColNum.ToString());
            }
            //-----------------------------------------------------------------
            myState = E134Enum.SHOW_RESULTS;
            for (int i = 0; i < myMaxRowCount; ++i)
            {
                if (false == myGridRows[i].myIsWoundedRolled)
                    myState = E134Enum.WOUND_CHECK;
                else if (Utilities.NO_RESULT == myGridRows[i].myWounds)
                    myState = E134Enum.WOUND_CHECK;
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
                                {
                                    myState = E134Enum.END;
                                    myIsRepeatSearch = false;
                                }
                                else if ("ShakyWalls" == name)
                                {
                                    myState = E134Enum.END;
                                    myIsRepeatSearch = true;
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
                            myRollResulltRowNum = Grid.GetRow(img1);
                            myRollResulltColNum = Grid.GetColumn(img1);
                            myIsRollInProgress = true;
                            RollEndCallback callback = ShowDieResults;
                            if (1 == myRollResulltColNum)
                                myDieRoller.RollMovingDie(myCanvas, callback);
                            else if (2 == myRollResulltColNum)
                                myDieRoller.RollMovingDice(myCanvas, callback);
                            else
                                Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): reached default col=" + myRollResulltColNum.ToString());
                            img1.Visibility = Visibility.Hidden;
                        }
                        return;
                    }
                }
            }
        }
    }
}
