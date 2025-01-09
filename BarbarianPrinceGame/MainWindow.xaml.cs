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
using Microsoft.Win32;

namespace BarbarianPrince
{
   //-----------------------------------------------------------------------------------
   public partial class MainWindow : Window
   {
      public static string theAssemblyDirectory = "";
      private IGameEngine myGameEngine = null;
      private GameViewerWindow myGameViewerWindow = null;
      public MainWindow()
      {
         InitializeComponent();
         try
         {
            //--------------------------------------------
            Assembly assem = Assembly.GetExecutingAssembly();
            string codeBase = assem.Location;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            theAssemblyDirectory = System.IO.Path.GetDirectoryName(path);
            MapImage.theImageDirectory = theAssemblyDirectory + @"\images\";
            ConfigFileReader.theConfigDirectory = theAssemblyDirectory + @"\config\";
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Logger.theLogDirectory = appDataDir + @"\BarbarianPrince\Logs\";
            GameLoadMgr.theGamesDirectory = appDataDir + @"\BarbarianPrince\Games\";
            GameFeat.theGameFeatDirectory = appDataDir + @"\BarbarianPrince\GameFeat\";
            if (false == Directory.Exists(GameFeat.theGameFeatDirectory)) // create directory if does not exists
               Directory.CreateDirectory(GameFeat.theGameFeatDirectory);
            //--------------------------------------------
            Utilities.InitializeRandomNumGenerators();
            //--------------------------------------------
            string iconFilename = MapImage.theImageDirectory + "BarbarianPrince.ico";
            Uri iconUri = new Uri(iconFilename, UriKind.Absolute);
            this.Icon = BitmapFrame.Create(iconUri); 
            //--------------------------------------------
            IGameInstance gi = new GameInstance(true);
            if (true == gi.CtorError)
            {
               Application.Current.Shutdown();
               return;
            }
            myGameEngine = new GameEngine(this);                         // GameEngine initiates logger
            myGameViewerWindow = new GameViewerWindow(myGameEngine, gi); // Start the main view
            if (true == myGameViewerWindow.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): GameViewerWindow() ctor error");
               Application.Current.Shutdown();
               return;
            }
            myGameViewerWindow.Icon = this.Icon;
            myGameViewerWindow.Show(); // Finished initializing so show the window
            //--------------------------------------------
            try // copy user documentation to folder where user data is kept
            {
#if DEBUG
               string docs1Src = theAssemblyDirectory + @"\Docs\BP2-eventsbook_singleA4.pdf";
               string docs2Src = theAssemblyDirectory + @"\Docs\BP2-rulesbook_singleA4.pdf";
#else
               string docs1Src = theAssemblyDirectory + @"\Docs\BP2-eventsbook_singleA4.pdf";
               string docs2Src = theAssemblyDirectory + @"\Docs\BP2-rulesbook_singleA4.pdf";
#endif
               string docsDir = appDataDir + @"\BarbarianPrince\Docs\";
               if (false == Directory.Exists(docsDir))
                  Directory.CreateDirectory(docsDir);
               string docs1Dest = docsDir + @"BP2-eventsbook_singleA4.pdf";
               if ( false == File.Exists(docs1Dest))
                  File.Copy(docs1Src, docs1Dest);
               string docs2Dest = docsDir + @"BP2-rulesbook_singleA4.pdf";
               if (false == File.Exists(docs2Dest))
                  File.Copy(docs2Src, docs2Dest);
            }
            catch (Exception e)
            {
               Logger.Log(LogEnum.LE_ERROR, "MainWindow(): Copying docs to new folder caused exception e=" + e.ToString());
            }
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
