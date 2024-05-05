using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BarbarianPrince
{
    public partial class GameInstanceDialog : Window
    {
        private const int STARTING_ASSIGNED_ROW = 4;
        private const int MAX_GRID_ROW = 20;
        public struct GridRow
        {
            public IMapItem myMapItem;
            public GridRow(IMapItem mi) { myMapItem = mi; }
        };
        public bool CtorError { get; } = false;
        private int myMaxRowCount = 0;
        private GridRow[] myGridRows = null;
        private readonly FontFamily myFontFam = new FontFamily("Tahoma");
        //------------------------------------------------------------------------------------
        public GameInstanceDialog(IGameInstance gi)
        {
            InitializeComponent();
            if (false == UpdateGridRows(gi))
            {
                Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
                CtorError = true;
                return;
            }
        }
        public bool UpdateGridRows(IGameInstance gi)
        {
            myGridRows = new GridRow[MAX_GRID_ROW];
            myMaxRowCount = gi.PartyMembers.Count;
            //--------------------------------------------------
            int j = 0;
            IMapItem prince = null;
            foreach (IMapItem mi in gi.PartyMembers)
            {
                if (null == mi)
                {
                    Logger.Log(LogEnum.LE_ERROR, "OpenChest(): mi=null");
                    return false;
                }
                if ("Prince" == mi.Name)
                    prince = mi;
                myGridRows[j] = new GridRow(mi);
                ++j;
            }
            if (null == prince)
            {
                Logger.Log(LogEnum.LE_ERROR, "OpenChest(): prince=null");
                return false;
            }
            //--------------------------------------------------
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
                int row = i + STARTING_ASSIGNED_ROW;
                IMapItem mi = myGridRows[i].myMapItem;
                //------------------------------------
                Button b = CreateButton(mi);
                myGrid.Children.Add(b);
                Grid.SetRow(b, row);
                Grid.SetColumn(b, 0);
                //------------------------------------
                Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = mi.Food.ToString() };
                myGrid.Children.Add(label1);
                Grid.SetRow(label1, row);
                Grid.SetColumn(label1, 1);
                //------------------------------------
                Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = mi.StarveDayNum.ToString() };
                myGrid.Children.Add(label2);
                Grid.SetRow(label2, row);
                Grid.SetColumn(label2, 2);
                //------------------------------------
                Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = mi.Coin.ToString() };
                myGrid.Children.Add(label3);
                Grid.SetRow(label3, row);
                Grid.SetColumn(label3, 3);
                //------------------------------------
                Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = mi.Mounts.Count.ToString() };
                myGrid.Children.Add(label4);
                Grid.SetRow(label4, row);
                Grid.SetColumn(label4, 4);
                //------------------------------------
                int freeLoad = mi.GetFreeLoad();
                Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = freeLoad.ToString() };
                myGrid.Children.Add(label5);
                Grid.SetRow(label5, row);
                Grid.SetColumn(label5, 5);
            }
            return true;
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
