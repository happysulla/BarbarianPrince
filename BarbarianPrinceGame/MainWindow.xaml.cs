using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Resources;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Window = System.Windows.Window;
using BarbarianPrince.Properties;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace BarbarianPrince
{
   //-----------------------------------------------------------------------------------
   public partial class MainWindow : Window
   {
      private IGameEngine myGameEngine = null;
      private GameViewerWindow myGameViewerWindow = null;
      public MainWindow()
      {
         InitializeComponent();
         try
         {
            //--------------------------------------------
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            string assemplyDirectory = System.IO.Path.GetDirectoryName(path);
            MapImage.theImageDirectory = assemplyDirectory + @"\images\";
            ConfigFileReader.theConfigDirectory = assemplyDirectory + @"\config\";
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Logger.theLogDirectory = appDataDir + @"\BarbarianPrince\Logs\";
            GameLoadMgr.theGamesDirectory = appDataDir + @"\BarbarianPrince\Games\";
            //--------------------------------------------
            Utilities.InitializeRandomNumGenerators();
            //--------------------------------------------
            string iconFilename = MapImage.theImageDirectory + "BarbarianPrince.ico";
            Uri iconUri = new Uri(iconFilename, UriKind.Absolute);
            this.Icon = BitmapFrame.Create(iconUri); 
            //--------------------------------------------
            IGameInstance gi = new GameInstance();
            if (true == gi.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): GameInstance() ctor error");
               Application.Current.Shutdown();
               return;
            }
            myGameEngine = new GameEngine(this);
            myGameViewerWindow = new GameViewerWindow(myGameEngine, gi); // Start the main view
            if (true == myGameViewerWindow.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): GameViewerWindow() ctor error");
               Application.Current.Shutdown();
               return;
            }
            myGameViewerWindow.Icon = this.Icon;
            myGameViewerWindow.Show(); // Finished initializing so show the window
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "MainWindow() e=" + e.ToString());
            Application.Current.Shutdown();
            return;
         }
      }
      //-----------------------------------------------------------------------
      public void UpdateViews(ref IGameInstance gi, GameAction action)
      {
         foreach (IView v in myGameEngine.Views)
            v.UpdateView(ref gi, action);
      }
   } // end class
}
