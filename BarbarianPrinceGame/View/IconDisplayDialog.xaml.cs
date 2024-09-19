﻿using System;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BarbarianPrince
{
   public partial class IconDisplayDialog : Window
   {
      public IconDisplayDialog()
      {
         InitializeComponent();
         int row = 0;
         int col = 0;
         Image img = new Image() {Height=50, Width=40, IsEnabled=false, Margin=new Thickness(0,5,0,0), Source = MapItem.theMapImages.GetBitmapImage("MarkOfCain") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("BoxUnopened") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("CharmGift") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("CoinBar") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("DenyAudience") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("DenyHire") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("CoinPileSingle") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("DragonEye") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("DrugChaga") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("ElvenTown") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("c41SlaveGirl") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         //--------------------------------------------
         row = 0;
         col = 2;
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Margin = new Thickness(0, 5, 0, 0), Source = MapItem.theMapImages.GetBitmapImage("ElvenCastle") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Exhausted") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("FineClothes") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Fugitive") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("FoulBane") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("God") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Gift") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("GriffonClaw") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Group") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("DenyTemple") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         //--------------------------------------------
         row = 0;
         col = 4;
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Margin = new Thickness(0, 5, 0, 0), Source = MapItem.theMapImages.GetBitmapImage("Helmet") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("HillTribeWarriors") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Muscle") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Negotiator") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("RocBeak") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Secrets") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Teeth") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("DwarfAdvice") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("HalflingTown") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("StructureDeny") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Deny") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         //--------------------------------------------
         row = 0;
         col = 6;
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Margin = new Thickness(0, 5, 0, 0), Source = MapItem.theMapImages.GetBitmapImage("Sun5") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("TowerGoblin") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("TowerOrc") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("TowerWizard") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Staff") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("TrollSkin") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("WizardAdvice") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Letter") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Ally") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("c60Minstrel") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
         img = new Image() { Height = 50, Width = 40, IsEnabled = false, Source = MapItem.theMapImages.GetBitmapImage("Cache") };
         myGrid.Children.Add(img);
         Grid.SetRow(img, ++row);
         Grid.SetColumn(img, col);
      }
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}