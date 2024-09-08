using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace BarbarianPrince
{
    /// <summary>
    /// Interaction logic for EventViewerE203Mgr.xaml
    /// </summary>
    public partial class EventViewerE203Mgr : UserControl
    {
        public delegate bool EndE203Callback();
        private const int STARTING_ASSIGNED_ROW = 6;
        //---------------------------------------------
        public struct GridRow
        {
            public IMapItem myMapItem;
            public int myEscapeRoll;
            public GridRow(IMapItem mi)
            {
                myMapItem = mi;
                myEscapeRoll = Utilities.NO_RESULT;
            }
        };
        public enum E203Enum
        {
            JAIL_BREAK_CHECK,
            SHOW_RESULTS,
            END
        };
        //---------------------------------------------
        public bool CtorError { get; } = false;
        //---------------------------------------------
        private E203Enum myState = E203Enum.JAIL_BREAK_CHECK;
        private EndE203Callback myCallback = null;
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
        public EventViewerE203Mgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
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
        public bool CheckPrisonBreak(EndE203Callback callback)
        {
            if (null == myGameInstance.PartyMembers)
            {
                Logger.Log(LogEnum.LE_ERROR, "CheckPrisonBreak(): partyMembers=null");
                return false;
            }
            if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
            {
                Logger.Log(LogEnum.LE_ERROR, "CheckPrisonBreak(): myGameInstance.PartyMembers.Count < 1");
                return false;
            }
            //--------------------------------------------------
            myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
            myState = E203Enum.JAIL_BREAK_CHECK;
            myMaxRowCount = myGameInstance.PartyMembers.Count;
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
                    Logger.Log(LogEnum.LE_ERROR, "CheckPrisonBreak(): mi=null");
                    return false;
                }
                myGridRows[i] = new GridRow(mi);
                if ("Prince" == mi.Name)
                {
                    prince = mi;
                    myGridRows[i].myEscapeRoll = 0;  // prince automatically passes 
                }
                ++i;
            }
            if (null == prince)
            {
                Logger.Log(LogEnum.LE_ERROR, "CheckPrisonBreak(): prince=null");
                return false;
            }
            //--------------------------------------------------
            if (false == UpdateGrid())
            {
                Logger.Log(LogEnum.LE_ERROR, "CheckPrisonBreak(): UpdateGrid() return false");
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
            if (E203Enum.END == myState)
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
            if (1 == myGameInstance.PartyMembers.Count) // if there are no party members, perform the callback
                myState = E203Enum.END;
            if (E203Enum.END == myState)
            {
                for (int i = 0; i < myMaxRowCount; ++i) // remove party members who disappear
                {
                    if (2 < myGridRows[i].myEscapeRoll)
                        myGameInstance.PartyMembers.Remove(myGridRows[i].myMapItem);
                }
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
                case E203Enum.JAIL_BREAK_CHECK:
                    myTextBlockInstructions.Inlines.Add(new Run("Roll one die for each character. If < 3, they escape with you."));
                    break;
                case E203Enum.SHOW_RESULTS:
                case E203Enum.END:
                    myTextBlockInstructions.Inlines.Add(new Run("Click the campfire to escape with party."));
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
                case E203Enum.JAIL_BREAK_CHECK:
                    Image img1 = new Image { Name = "Jail", Source = MapItem.theMapImages.GetBitmapImage("JailBreak"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                    myStackPanelAssignable.Children.Add(img1);
                    break;
                case E203Enum.SHOW_RESULTS:
                case E203Enum.END:
                    BitmapImage bmi2 = new BitmapImage();
                    bmi2.BeginInit();
                    bmi2.UriSource = new Uri(Utilities.theImageDirectoryPath + "CampFire2.gif", UriKind.Relative);
                    bmi2.EndInit();
                    Image img2 = new Image { Name = "Campfire", Source = bmi2, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                    ImageBehavior.SetAnimatedSource(img2, bmi2);
                    myStackPanelAssignable.Children.Add(img2);
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
                if (0 <= myGridRows[i].myEscapeRoll)
                {
                    string content = myGridRows[i].myEscapeRoll.ToString();
                    if ((0 == myGridRows[i].myEscapeRoll) || (7 == myGridRows[i].myEscapeRoll))
                        content = "NA";
                    Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = content };
                    myGrid.Children.Add(label);
                    Grid.SetRow(label, rowNum);
                    Grid.SetColumn(label, 1);
                }
                else
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
                if (0 <= myGridRows[i].myEscapeRoll)
                {
                    Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                    myGrid.Children.Add(label);
                    Grid.SetRow(label, rowNum);
                    Grid.SetColumn(label, 2);
                    if (2 < myGridRows[i].myEscapeRoll)
                        label.Content = "no";
                    else
                        label.Content = "yes";
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
            int j = myRollResulltRowNum - STARTING_ASSIGNED_ROW;
            if (j < 0)
            {
                Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): invalid state j=" + j.ToString());
                return;
            }
            myGridRows[j].myEscapeRoll = dieRoll;
            //-----------------------------------------
            if (3 < dieRoll) // if any party member is abandoned, all fickle characters disappear
            {
                for (int i = 0; i < myMaxRowCount; ++i)
                {
                    if ((true == myGridRows[i].myMapItem.IsFickle) && (i != j))
                        myGridRows[i].myEscapeRoll = 7;
                }
            }
            //-----------------------------------------
            myState = E203Enum.SHOW_RESULTS;
            for (int i = 0; i < myMaxRowCount; ++i)
            {
                if (Utilities.NO_RESULT == myGridRows[i].myEscapeRoll)
                    myState = E203Enum.JAIL_BREAK_CHECK;
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
                                string name = (string)img.Name;
                                if ("Campfire" == name)
                                    myState = E203Enum.END;
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
