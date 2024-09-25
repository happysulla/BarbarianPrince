using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace BarbarianPrince
{
   public partial class OptionSelectionDialog : Window
   {
      public bool CtorError { get; }
      public IGameInstance myGameInstance = null;
      private Options myOptions { get; set; } = null;
      public Options NewOptions { get => myOptions; }
      private bool myIsRandomGame = false;
      public OptionSelectionDialog(Options options)
      {
         InitializeComponent();
         myOptions = options.GetDeepCopy(); // make copy b/c do not want to change unless OK button selected by user
         myRadioButtonOriginal.ToolTip = "Play the game as God Intended. Be prepared to lose often.";
         myRadioButtonRandomParty.ToolTip = "Choose random party members.";
         myRadioButtonRandomStart.ToolTip = "Choose random starting hex.";
         myRadioButtonAllRandom.ToolTip = "Everything is random.";
         myRadioButtonMaxFun.ToolTip = "Recommended choices for maximizing fun.";
         myRadioButtonCustom.ToolTip = "Custom choices for all choices.";
         //--------------------
         myCheckBoxAutoSetup.ToolTip = "Skip initial tutorial when starting.";
         myCheckBoxAutoWealth.ToolTip = "All wealth rolls under 5 are automatically rolled.";
         //--------------------
         myCheckBoxPrinceHorse.ToolTip = "Prince starts with a hex.";
         myCheckBoxPrincePegasus.ToolTip = "Prince starts with a Pegasus mount.";
         myCheckBoxPrinceCoin.ToolTip = "Prince starts with 50-99 gold pieces.";
         myCheckBoxPrinceFood.ToolTip = "Prince starts with 10 food.";
         myCheckBoxStartNerveGas.ToolTip = "Prince starts with Nerve Gas Bomb.";
         myCheckBoxStartNecklass.ToolTip = "Prince starts with Resurrection Necklass.";
         //--------------------
         myRadioButtonPartyOriginal.ToolTip = "Prince starts by himself.";
         myRadioButtonPartyRandom10.ToolTip = "10 random party members are added.";
         myRadioButtonPartyRandom8.ToolTip = "8 random party members are added.";
         myRadioButtonPartyRandom5.ToolTip = "5 random party members are added.";
         myRadioButtonPartyRandom3.ToolTip = "3 random party members are added.";
         myRadioButtonPartyRandom1.ToolTip = "1 random party member is added.";
         myRadioButtonPartyCustom.ToolTip = "Pick and choose what party members begin the game.";
         //--------------------
         myCheckBoxPartyMounted.ToolTip = "All party members get horse unless they are mounts.";
         myCheckBoxPartyAirborne.ToolTip = "All party members get Pegasus unless they are flying characters.";
         //--------------------
         myRadioButtonHexOriginal.ToolTip = "Party starts as originial game intended.";
         myRadioButtonHexRandom.ToolTip = "Party starts on random hex on the map.";
         myRadioButtonHexRandomTown.ToolTip = "Party starts on random town hex.";
         myRadioButtonHexRandomLeft.ToolTip = "Party starts on random left edge.";
         myRadioButtonHexRandomRight.ToolTip = "Party starts on random right edge.";
         myRadioButtonHexRandomBottom.ToolTip = "Party starts on random bottom edge.";
         //--------------------
         myRadioButtonMonsterNormal.ToolTip = "Monsters start with original game attributes.";
         myRadioButtonMonsterLessEasy.ToolTip = "Monsters subtract 1 from endurance and combat.";
         myRadioButtonMonsterEasy.ToolTip = "Monsters subtract 2 from endurance and combat.";
         myRadioButtonMonsterEasiest.ToolTip = "Monsters have one endurance and combat.";
         //--------------------
         myCheckBoxAutoLostIncrement.ToolTip = "Lost chance decreases on consecutive lost rolls.";
         myCheckBoxExtendTime.ToolTip = "Extend end time from 70 days to 105 days.";
         myCheckBoxReducedLodgingCosts.ToolTip = "Lodging in structures is half price.";
         myCheckBoxAddIncome.ToolTip = "Add 3-6gp at end of each day performing menial tasks during daily activities if not incapacitated.";
         myCheckBoxEasyRoute.ToolTip = "Route on a 5 or 6 for bandits, wolves, goblins, and orcs";
         //--------------------
         myCheckBoxNoLostRoll.ToolTip = "Skip Lost Rolls.";
         myCheckBoxNoLostEvent.ToolTip = "Lost encounters never occur.";
         myCheckBoxNoEvent.ToolTip = "Travel encounters never occur. Lost encounters may still occur.";
         myCheckBoxNoRoadEvent.ToolTip = "Road encounters never occur.";
         myCheckBoxNoCrossEvent.ToolTip = "Crossing river encounters never occur.";
         myCheckBoxNoRaftEvent.ToolTip = "Raft encounters never occur when rafting.";
         myCheckBoxNoAirEvent.ToolTip = "Air encounters never occur when flying.";
         myCheckBoxForceLostEvent.ToolTip = "Lost encounters always occur when lost roll is made.";
         myCheckBoxForceLostAfterCross.ToolTip = "Lost encounter always occurs after crossing river.";
         myCheckBoxForceEvent.ToolTip = "Ground encounter always occurs.";
         myCheckBoxForceCrossEvent.ToolTip = "River crossing encounter always occurs.";
         myCheckBoxForceRaftEvent.ToolTip = "Raft encounter always occurs.";
         myCheckBoxForceAirEvent.ToolTip = "Air encounter always occurs when flying.";
         if (false == UpdateDisplay(myOptions))
         {
            Logger.Log(LogEnum.LE_ERROR, "OptionSelectionDialog(): UpdateDisplay() returned false");
            CtorError = true;
         }
      }
      //----------------------------------------------------------------
      private bool UpdateDisplay(Options options)
      {
         bool isCustomConfig = false;
         bool isRandomPartyConfig = false;
         bool isCustomPartyConfig = false;
         bool isRandomHexConfig = false;
         bool isCustomHexConfig = false;
         bool isFunOption = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Auto Rolls
         string name = "AutoSetup";
         Option option = options.Find(name);
         if( null == option )
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxAutoSetup.IsChecked = option.IsEnabled;
         //-------------------------
         name = "AutoWealthRollForUnderFive";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxAutoWealth.IsChecked = option.IsEnabled;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Prince 
         name = "PrinceHorse";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxPrinceHorse.IsChecked = option.IsEnabled;
         if( true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "PrincePegasus";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxPrincePegasus.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "PrinceCoin";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxPrinceCoin.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         //-------------------------
         name = "PrinceFood";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxPrinceFood.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         //-------------------------
         name = "StartWithNerveGame";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxStartNerveGas.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         //-------------------------
         name = "StartWithNecklass";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxStartNecklass.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Party Members
         myCheckBoxDwarf.IsEnabled = false;
         myCheckBoxEagle.IsEnabled = false;
         myCheckBoxElf.IsEnabled = false;
         myCheckBoxElfWarrior.IsEnabled = false;
         myCheckBoxFalcon.IsEnabled = false;
         myCheckBoxGriffon.IsEnabled = false;
         myCheckBoxHarpy.IsEnabled = false;
         myCheckBoxMagician.IsEnabled = false;
         myCheckBoxMercenary.IsEnabled = false;
         myCheckBoxMerchant.IsEnabled = false;
         myCheckBoxMinstrel.IsEnabled = false;
         myCheckBoxMonk.IsEnabled = false;
         myCheckBoxPorterSlave.IsEnabled = false;
         myCheckBoxPriest.IsEnabled = false;
         myCheckBoxTrueLove.IsEnabled = false;
         myCheckBoxWizard.IsEnabled = false;
         name = "RandomParty10";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         if ( true == option.IsEnabled )
         {
            myRadioButtonPartyRandom10.IsChecked = true;
            isRandomPartyConfig = true;
            isFunOption = false;
         }
         else
         {
            name = "RandomParty08";
            option = options.Find(name);
            if (null == option)
            {
               option = new Option(name, false);
               myOptions.Add(option);
            }
            if (true == option.IsEnabled)
            {
               myRadioButtonPartyRandom8.IsChecked = true;
               isRandomPartyConfig = true;
               isFunOption = false;
            }
            else
            {
               name = "RandomParty05";
               option = options.Find(name);
               if (null == option)
               {
                  option = new Option(name, false);
                  myOptions.Add(option);
               }
               if (true == option.IsEnabled)
               {
                  myRadioButtonPartyRandom5.IsChecked = true;
                  isRandomPartyConfig = true;
               }
               else
               {
                  name = "RandomParty03";
                  option = options.Find(name);
                  if (null == option)
                  {
                     option = new Option(name, false);
                     myOptions.Add(option);
                  }
                  if (true == option.IsEnabled)
                  {
                     myRadioButtonPartyRandom3.IsChecked = true;
                     isRandomPartyConfig = true;
                     isFunOption = false;
                  }
                  else
                  {
                     name = "RandomParty01";
                     option = options.Find(name);
                     if (null == option)
                     {
                        option = new Option(name, false);
                        myOptions.Add(option);
                     }
                     if (true == option.IsEnabled)
                     {
                        myRadioButtonPartyRandom1.IsChecked = true;
                        isRandomPartyConfig = true;
                        isFunOption = false;
                     }
                     else
                     {
                        name = "PartyCustom";
                        option = options.Find(name);
                        if (null == option)
                        {
                           option = new Option(name, false);
                           myOptions.Add(option);
                        }
                        if (true == option.IsEnabled)
                        {
                           myRadioButtonPartyCustom.IsChecked = true;
                           myCheckBoxDwarf.IsEnabled = true;
                           myCheckBoxEagle.IsEnabled = true;
                           myCheckBoxElf.IsEnabled = true;
                           myCheckBoxElfWarrior.IsEnabled = true;
                           myCheckBoxFalcon.IsEnabled = true;
                           myCheckBoxGriffon.IsEnabled = true;
                           myCheckBoxHarpy.IsEnabled = true;
                           myCheckBoxMagician.IsEnabled = true;
                           myCheckBoxMercenary.IsEnabled = true;
                           myCheckBoxMerchant.IsEnabled = true;
                           myCheckBoxMinstrel.IsEnabled = true;
                           myCheckBoxMonk.IsEnabled = true;
                           myCheckBoxPorterSlave.IsEnabled = true;
                           myCheckBoxPriest.IsEnabled = true;
                           myCheckBoxTrueLove.IsEnabled = true;
                           myCheckBoxWizard.IsEnabled = true;
                           //-------------------------
                           name = "Dwarf";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxDwarf.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "Eagle";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxEagle.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "Elf";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxElf.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "ElfWarrior";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxElfWarrior.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "Falcon";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxFalcon.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "Griffon";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxGriffon.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "Harpy";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxHarpy.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "Magician";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxMagician.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "Mercenary";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxMercenary.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "Merchant";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxMerchant.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "Minstrel";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxMinstrel.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "Monk";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxMonk.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "PorterSlave";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxPorterSlave.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "Priest";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxPriest.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "TrueLove";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxTrueLove.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                           //-------------------------
                           name = "Wizard";
                           option = options.Find(name);
                           if (null == option)
                           {
                              option = new Option(name, false);
                              myOptions.Add(option);
                           }
                           myCheckBoxWizard.IsChecked = option.IsEnabled;
                           if (true == option.IsEnabled)
                           {
                              isCustomPartyConfig = true;
                              isFunOption = false;
                           }
                        }
                     }
                  }
               }
            }
         }
         if( (false == isRandomPartyConfig ) && (false == isCustomPartyConfig) )
            myRadioButtonPartyOriginal.IsChecked = true;
         //------------------------------------------------
         name = "PartyMounted";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxPartyMounted.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         name = "PartyAirborne";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxPartyAirborne.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         name = "RandomHex";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexRandom.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         //-------------------------
         name = "RandomTown";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexRandomTown.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "RandomLeft";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexRandomLeft.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "RandomRight";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexRandomRight.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "RandomBottom";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexRandomBottom.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "0109";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexTown.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "0206";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexRuin.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isRandomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "0708";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexRiver.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "0711";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexTemple.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "1212";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexHuldra.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "0323";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexDrogat.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "1923";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexLadyAeravir.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "0418";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexFarmland.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "0722";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexCountryside.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "0409";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexForest.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "0406";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexHill.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------   
         name = "1611";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexMountain.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "0411";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexSwamp.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "1507";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexDesert.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "1905";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexRoad.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "1723";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonHexBottom.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomHexConfig = true;
            isFunOption = false;
         }
         if ((false == isRandomHexConfig) && (false == isCustomHexConfig))
            myRadioButtonHexOriginal.IsChecked = true;
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Monsters
         name = "EasiestMonsters";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myRadioButtonMonsterEasiest.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         else
         {
            name = "EasyMonsters";
            option = options.Find(name);
            if (null == option)
            {
               option = new Option(name, false);
               myOptions.Add(option);
            }
            myRadioButtonMonsterEasy.IsChecked = option.IsEnabled;
            if (true == option.IsEnabled)
            {
               isCustomConfig = true;
            }
            else
            {
               isFunOption = false;
               name = "LessHardMonsters";
               option = options.Find(name);
               if (null == option)
               {
                  option = new Option(name, false);
                  myOptions.Add(option);
               }
               myRadioButtonMonsterLessEasy.IsChecked = option.IsEnabled;
               if (true == option.IsEnabled)
               {
                  isCustomConfig = true;
                  isFunOption = false;
               }
               else
               {
                  myRadioButtonMonsterNormal.IsChecked = true;
               }
            }
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Game Options
         name = "AutoLostDecrease";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxAutoLostIncrement.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         name = "ExtendEndTime";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxExtendTime.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         name = "ReduceLodgingCosts";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxReducedLodgingCosts.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         name = "SteadyIncome";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxAddIncome.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         name = "EasyRoute";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxEasyRoute.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
         }
         else
         {
            isFunOption = false;
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         name = "NoLostRoll";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxNoLostRoll.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "ForceNoLostEvent";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxNoLostEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "ForceLostEvent";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxForceLostEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "ForceNoEvent";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxNoEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "ForceEvent";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxForceEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "ForceNoRoadEvent";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxNoRoadEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "ForceNoAirEvent";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxNoAirEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "ForceAirEvent";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxForceAirEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "ForceNoCrossEvent";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxNoCrossEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "ForceCrossEvent";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxForceCrossEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "ForceLostAfterCrossEvent";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxForceLostAfterCross.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "ForceNoRaftEvent";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxNoRaftEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //-------------------------
         name = "ForceRaftEvent";
         option = options.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         myCheckBoxForceRaftEvent.IsChecked = option.IsEnabled;
         if (true == option.IsEnabled)
         {
            isCustomConfig = true;
            isFunOption = false;
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         // Summary Selection
         if (true == isFunOption)
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonMaxFun.IsChecked = true;
            myRadioButtonCustom.IsChecked = false;
         }
         else if (true == myIsRandomGame)
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = true;
            myRadioButtonMaxFun.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         else if ((true == isCustomConfig) || (true == isCustomPartyConfig) || (true == isCustomHexConfig))
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonMaxFun.IsChecked = false;
            myRadioButtonCustom.IsChecked = true;
         }
         else if ( (true == isRandomPartyConfig) && (true == isRandomHexConfig) )
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = true;
            myRadioButtonMaxFun.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         else if (true == isRandomPartyConfig)
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = true;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonMaxFun.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         else if (true == isRandomHexConfig)
         {
            myRadioButtonOriginal.IsChecked = false;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = true;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonMaxFun.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         else
         {
            myRadioButtonOriginal.IsChecked = true;
            myRadioButtonRandomParty.IsChecked = false;
            myRadioButtonRandomStart.IsChecked = false;
            myRadioButtonAllRandom.IsChecked = false;
            myRadioButtonMaxFun.IsChecked = false;
            myRadioButtonCustom.IsChecked = false;
         }
         //++++++++++++++++++++++++++++++++++++++++++++++++
         return true;
      }
      private void ResetPrince()
      {
         Option option = null;
         option = myOptions.Find("PrinceHorse");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("PrinceHorse", false);
         option = myOptions.Find("PrincePegasus");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("PrincePegasus", false);
         option = myOptions.Find("PrinceCoin");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("PrinceCoin", false);
         option = myOptions.Find("PrinceFood");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("PrinceFood", false);
         option = myOptions.Find("StartWithNerveGame");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("StartWithNerveGame", false);
         option = myOptions.Find("StartWithNecklass");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("StartWithNecklass", false);
      }
      private void ResetParty()
      {
         Option option = null;
         option = myOptions.Find("RandomParty10");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("RandomParty10", false);
         option = myOptions.Find("RandomParty08");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("RandomParty08", false);
         option = myOptions.Find("RandomParty05");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("RandomParty05", false);
         option = myOptions.Find("RandomParty03");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("RandomParty03", false);
         option = myOptions.Find("RandomParty01");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("RandomParty01", false);
         option = myOptions.Find("PartyCustom");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("PartyCustom", false);
      }
      private void ResetPartyMembers()
      {
         Option option = null;
         option = myOptions.Find("Dwarf");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Dwarf", false);
         option = myOptions.Find("Eagle");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Eagle", false);
         option = myOptions.Find("Elf");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Elf", false);
         option = myOptions.Find("ElfWarrior");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("ElfWarrior", false);
         option = myOptions.Find("Falcon");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Falcon", false);
         option = myOptions.Find("Griffon");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Griffon", false);
         option = myOptions.Find("Harpy");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Harpy", false);
         option = myOptions.Find("Magician");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Magician", false);
         option = myOptions.Find("Mercenary");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Mercenary", false);
         option = myOptions.Find("Merchant");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Merchant", false);
         option = myOptions.Find("Minstrel");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Minstrel", false);
         option = myOptions.Find("Monk");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Monk", false);
         option = myOptions.Find("PorterSlave");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("PorterSlave", false);
         option = myOptions.Find("Priest");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Priest", false);
         option = myOptions.Find("TrueLove");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("TrueLove", false);
         option = myOptions.Find("Wizard");
         if (null != option)
            option.IsEnabled = false;
         else
            option = new Option("Wizard", false);
      }
      private void ResetPartyOptions()
      {
         Option option = null;
         option = myOptions.Find("PartyMounted");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyOptions(): not found PartyMounted");
         option = myOptions.Find("PartyAirborne");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetPartyOptions(): not found PartyAirborne");
      }
      private void ResetHex()
      {
         Option option = null;
         myRadioButtonHexRandom.IsChecked = false;
         option = myOptions.Find("RandomHex");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found RandomHex");
         myRadioButtonHexRandomTown.IsChecked = false;
         option = myOptions.Find("RandomTown");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found RandomTown");
         myRadioButtonHexRandomLeft.IsChecked = false;
         option = myOptions.Find("RandomLeft");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found RandomLeft");
         myRadioButtonHexRandomRight.IsChecked = false;
         option = myOptions.Find("RandomRight");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found RandomRight");
         myRadioButtonHexRandomBottom.IsChecked = false;
         option = myOptions.Find("RandomBottom");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found RandomBottom");
         myRadioButtonHexTown.IsChecked = false;
         option = myOptions.Find("0109");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0109");
         myRadioButtonHexRuin.IsChecked = false;
         option = myOptions.Find("0206");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0206");
         myRadioButtonHexRiver.IsChecked = false;
         option = myOptions.Find("0708");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0708");
         myRadioButtonHexTemple.IsChecked = false;
         option = myOptions.Find("0711");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0711");
         myRadioButtonHexHuldra.IsChecked = false;
         option = myOptions.Find("1212");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 1212");
         myRadioButtonHexDrogat.IsChecked = false;
         option = myOptions.Find("0323");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0323");
         myRadioButtonHexLadyAeravir.IsChecked = false;
         option = myOptions.Find("1923");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 1923");
         myRadioButtonHexFarmland.IsChecked = false;
         option = myOptions.Find("0418");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0418");
         myRadioButtonHexCountryside.IsChecked = false;
         option = myOptions.Find("0722");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0722");
         myRadioButtonHexForest.IsChecked = false;
         option = myOptions.Find("0409");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0409");
         myRadioButtonHexHill.IsChecked = false;
         option = myOptions.Find("0406");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0406");
         myRadioButtonHexMountain.IsChecked = false;
         option = myOptions.Find("1611"); 
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 1611");
         myRadioButtonHexSwamp.IsChecked = false;
         option = myOptions.Find("0411");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 0411");
         myRadioButtonHexDesert.IsChecked = false;
         option = myOptions.Find("1507");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 1507");
         myRadioButtonHexRoad.IsChecked = false;
         option = myOptions.Find("1905");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 1905");
         myRadioButtonHexBottom.IsChecked = false;
         option = myOptions.Find("1723");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found 1723");
      }
      private void ResetMonsters()
      {
         Option option = null;
         option = myOptions.Find("LessHardMonsters");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found LessHardMonsters");
         option = myOptions.Find("EasyMonsters");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found EasyMonsters");
         option = myOptions.Find("EasiestMonsters");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found EasiestMonsters");
      }
      private void ResetGameOptions()
      {
         Option option = null;
         option = myOptions.Find("AutoLostDecrease");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetGameOptions(): not found AutoLostDecrease");
         option = myOptions.Find("ExtendEndTime");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetGameOptions(): not found ExtendEndTime");
         option = myOptions.Find("ReduceLodgingCosts");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetGameOptions(): not found ReduceLodgingCosts");
         option = myOptions.Find("SteadyIncome");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetGameOptions(): not found SteadyIncome");
         option = myOptions.Find("EasyRoute");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetGameOptions(): not found EasyRoute");
      }
      private void ResetEvents()
      {
         Option option = null;
         myCheckBoxNoLostRoll.IsChecked = false;
         option = myOptions.Find("NoLostRoll");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found NoLostRoll");
         myCheckBoxNoLostEvent.IsChecked = false;
         option = myOptions.Find("ForceNoLostEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoLostEvent");
         myCheckBoxNoEvent.IsChecked = false;
         option = myOptions.Find("ForceNoEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoEvent");
         myCheckBoxNoRoadEvent.IsChecked = false;
         option = myOptions.Find("ForceNoRoadEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoRoadEvent");
         myCheckBoxNoCrossEvent.IsChecked = false;
         option = myOptions.Find("ForceNoCrossEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoCrossEvent");
         myCheckBoxNoRaftEvent.IsChecked = false;
         option = myOptions.Find("ForceNoRaftEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoRaftEvent");
         myCheckBoxNoAirEvent.IsChecked = false;
         option = myOptions.Find("ForceNoAirEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoAirEvent");
         myCheckBoxForceLostEvent.IsChecked = false;
         option = myOptions.Find("ForceLostEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceLostEvent");
         myCheckBoxForceLostAfterCross.IsChecked = false;
         option = myOptions.Find("ForceLostAfterCrossEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceLostAfterCrossEvent");
         myCheckBoxForceEvent.IsChecked = false;
         option = myOptions.Find("ForceEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceEvent");
         myCheckBoxForceRaftEvent.IsChecked = false;
         option = myOptions.Find("ForceRaftEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceRaftEvent");
         myCheckBoxForceAirEvent.IsChecked = false;
         option = myOptions.Find("ForceNoAirEvent");
         if (null != option)
            option.IsEnabled = false;
         else
            Logger.Log(LogEnum.LE_ERROR, "ResetHex(): not found ForceNoAirEvent");
      }
      private void SelectRandomPartyChoice()
      {
         int choice = Utilities.RandomGenerator.Next(6);
         Option option = null;
         switch(choice)
         {
            case 0:  break;
            case 1: option = myOptions.Find("RandomParty10"); break;
            case 2: option = myOptions.Find("RandomParty08"); break;
            case 3: option = myOptions.Find("RandomParty05"); break;
            case 4: option = myOptions.Find("RandomParty03"); break;
            case 5: option = myOptions.Find("RandomParty01"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyChoice: reached default choice=" + choice.ToString()); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyChoice(): myOptions.Find() for choice=" + choice.ToString());
         else
            option.IsEnabled = !option.IsEnabled;
      }
      private void SelectRandomHexChoice()
      {
         int choice = Utilities.RandomGenerator.Next(21);
         Option option = null;
         switch (choice)
         {
            case 0: break;
            case 1: option = myOptions.Find("RandomHex"); break;
            case 2: option = myOptions.Find("RandomTown"); break;
            case 3: option = myOptions.Find("RandomLeft"); break;
            case 4: option = myOptions.Find("RandomRight"); break;
            case 5: option = myOptions.Find("RandomBottom"); break;
            case 6: option = myOptions.Find("0109"); break;
            case 7: option = myOptions.Find("0206"); break;
            case 8: option = myOptions.Find("0711"); break;
            case 9: option = myOptions.Find("1212"); break;
            case 10: option = myOptions.Find("0323"); break;
            case 11: option = myOptions.Find("1923"); break;
            case 12: option = myOptions.Find("0418"); break;
            case 13: option = myOptions.Find("0722"); break;
            case 14: option = myOptions.Find("0409"); break;
            case 15: option = myOptions.Find("0406"); break;
            case 16: option = myOptions.Find("1611"); break;
            case 17: option = myOptions.Find("0411"); break;
            case 18: option = myOptions.Find("1507"); break;
            case 19: option = myOptions.Find("1905"); break;
            case 20: option = myOptions.Find("1723"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "SelectRandomHexChoice: reached default choice=" + choice.ToString()); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectRandomHexChoice(): myOptions.Find() for choice=" + choice.ToString());
         else
            option.IsEnabled = !option.IsEnabled;
      }
      private void SelectRandomPrinceChoice()
      {
         int choice = Utilities.RandomGenerator.Next(2);
         if( 1 == choice )
         {
            Option option = myOptions.Find("PrinceHorse");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=PrinceHorse");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("PrincePegasus");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=PrincePegasus");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("PrinceCoin");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=PrinceCoin");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("PrinceFood");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=PrinceFood");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("StartWithNerveGame");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=StartWithNerveGame");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("StartWithNecklass");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=StartWithNecklass");
            else
               option.IsEnabled = !option.IsEnabled;
         }
      }
      private void SelectRandomPartyOptionChoice()
      {
         int choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("PartyMounted");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=PartyMounted");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("PartyAirborne");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPrinceChoice(): myOptions.Find() for option=PartyAirborne");
            else
               option.IsEnabled = !option.IsEnabled;
         }
      }
      private void SelectRandomGameOptionChoice()
      {
         int choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("AutoLostDecrease");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=AutoLostDecrease");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("ExtendEndTime");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=ExtendEndTime");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("ReduceLodgingCosts");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=ReduceLodgingCosts");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("SteadyIncome");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=SteadyIncome");
            else
               option.IsEnabled = !option.IsEnabled;
         }
         choice = Utilities.RandomGenerator.Next(2);
         if (1 == choice)
         {
            Option option = myOptions.Find("EasyRoute");
            if (null == option)
               Logger.Log(LogEnum.LE_ERROR, "SelectRandomPartyOptionChoice(): myOptions.Find() for option=EasyRoute");
            else
               option.IsEnabled = !option.IsEnabled;
         }
      }
      private void SelectFunGameOptions()
      {
         Option option = myOptions.Find("AutoSetup");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=AutoSetup");
         else
            option.IsEnabled = true;
         option = myOptions.Find("AutoWealthRollForUnderFive");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=AutoWealthRollForUnderFive");
         else
            option.IsEnabled = true;
         option = myOptions.Find("AutoLostDecrease");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=AutoLostDecrease");
         else
            option.IsEnabled = true;
         option = myOptions.Find("ExtendEndTime");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=ExtendEndTime");
         else
            option.IsEnabled = true;
         option = myOptions.Find("ReduceLodgingCosts");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=ReduceLodgingCosts");
         else
            option.IsEnabled = true;
         option = myOptions.Find("SteadyIncome");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=SteadyIncome");
         else
            option.IsEnabled = true;
         option = myOptions.Find("EasyRoute");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=EasyRoute");
         else
            option.IsEnabled = true;
         option = myOptions.Find("EasyMonsters");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=EasyMonsters");
         else
            option.IsEnabled = true;
         option = myOptions.Find("PrinceFood");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=PrinceFood");
         else
            option.IsEnabled = true;
         option = myOptions.Find("PrinceCoin");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=PrinceCoin");
         else
            option.IsEnabled = true;
         option = myOptions.Find("StartWithNerveGame");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=StartWithNerveGame");
         else
            option.IsEnabled = true;
         option = myOptions.Find("StartWithNecklass");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=StartWithNecklass");
         else
            option.IsEnabled = true;
         option = myOptions.Find("RandomParty05");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=RandomParty05");
         else
            option.IsEnabled = true;
         option = myOptions.Find("RandomHex");
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "SelectFunGameOptions(): myOptions.Find() for option=RandomHex");
         else
            option.IsEnabled = true;
      }
      //----------------------CONTROLLER FUNCTIONS----------------------
      private void ButtonOk_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = true;
      }
      private void ButtonCancel_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void StackPanelSummary_Click(object sender, RoutedEventArgs e)
      {
         RadioButton rb = (RadioButton)sender;
         ResetPrince();
         ResetParty();
         ResetPartyMembers();
         ResetPartyOptions();
         ResetHex();
         ResetMonsters();
         ResetGameOptions();
         ResetEvents();
         myIsRandomGame = false;
         switch (rb.Name)
         {
            case "myRadioButtonOriginal":
               break;
            case "myRadioButtonRandomParty":
               SelectRandomPartyChoice();
               break;
            case "myRadioButtonRandomStart":
               SelectRandomHexChoice();
               break;
            case "myRadioButtonAllRandom":
               myIsRandomGame = true;
               SelectRandomPartyChoice();
               SelectRandomHexChoice();
               SelectRandomPrinceChoice();
               SelectRandomGameOptionChoice();
               SelectRandomPartyOptionChoice();
               break;
            case "myRadioButtonMaxFun":
               SelectFunGameOptions();
               break;
            case "myRadioButtonCustom":
               break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): reached default name=" + rb.Name); return;
         }
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): UpdateDisplay() returned false for name=" + rb.Name);
      }
      private void StackPanelOptions_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         switch (cb.Name)
         {
            case "myCheckBoxAutoSetup": 
               option = myOptions.Find("AutoSetup");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            case "myCheckBoxAutoWealth": 
               option = myOptions.Find("AutoWealthRollForUnderFive");
               if (null == option)
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): myOptions.Find() for name=" + cb.Name);
               else
                  option.IsEnabled = !option.IsEnabled;
               break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelOptions_Click(): reached default name=" + cb.Name); return;
         }
      }
      private void StackPanelPrince_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         myIsRandomGame = false;
         switch (cb.Name)
         {
            case "myCheckBoxPrinceHorse": option = myOptions.Find("PrinceHorse"); break;
            case "myCheckBoxPrincePegasus": option = myOptions.Find("PrincePegasus"); break;
            case "myCheckBoxPrinceCoin": option = myOptions.Find("PrinceCoin"); break;
            case "myCheckBoxPrinceFood": option = myOptions.Find("PrinceFood"); break;
            case "myCheckBoxStartNerveGas": option = myOptions.Find("StartWithNerveGame"); break;
            case "myCheckBoxStartNecklass": option = myOptions.Find("StartWithNecklass"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelPrince_Click(): reached default name=" + cb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPrince_Click(): myOptions.Find() for name=" + cb.Name);
         else 
            option.IsEnabled = !option.IsEnabled;
         if ( false == UpdateDisplay(myOptions) )
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPrince_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void StackPanelParty_Click(object sender, RoutedEventArgs e)
      {
         RadioButton rb = (RadioButton)sender;
         Option option = null;
         ResetParty();
         switch (rb.Name)
         {
            case "myRadioButtonPartyOriginal": ResetPartyMembers(); break;
            case "myRadioButtonPartyRandom10": ResetPartyMembers(); myIsRandomGame = false; option = myOptions.Find("RandomParty10"); break;
            case "myRadioButtonPartyRandom8": ResetPartyMembers(); myIsRandomGame = false; option = myOptions.Find("RandomParty08"); break;
            case "myRadioButtonPartyRandom5": ResetPartyMembers(); myIsRandomGame = false; option = myOptions.Find("RandomParty05"); break;
            case "myRadioButtonPartyRandom3": ResetPartyMembers(); myIsRandomGame = false; option = myOptions.Find("RandomParty03"); break;
            case "myRadioButtonPartyRandom1": ResetPartyMembers(); myIsRandomGame = false; option = myOptions.Find("RandomParty01"); break;
            case "myRadioButtonPartyCustom": option = myOptions.Find("PartyCustom"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): reached default name=" + rb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): myOptions.Find() for name=" + rb.Name);
         else
            option.IsEnabled = true;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelParty_Click(): UpdateDisplay() returned false for name=" + rb.Name);
      }
      private void StackPanelPartyMember_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         string name = "Rat";
         myIsRandomGame = false;
         switch (cb.Name)
         {
            case "myCheckBoxDwarf": name = "Dwarf";  break;
            case "myCheckBoxEagle": name = "Eagle"; break;
            case "myCheckBoxElf": name = "Elf";  break;
            case "myCheckBoxElfWarrior": name = "ElfWarrior";break;
            case "myCheckBoxFalcon": name = "Falcon"; break;
            case "myCheckBoxGriffon": name = "Griffon"; break;
            case "myCheckBoxHarpy": name = "Harpy";  break;
            case "myCheckBoxMagician": name = "Magician";  break;
            case "myCheckBoxMercenary": name = "Mercenary"; break;
            case "myCheckBoxMerchant": name = "Merchant";  break;
            case "myCheckBoxMinstrel": name = "Minstrel"; break;
            case "myCheckBoxMonk": name = "Monk"; break;
            case "myCheckBoxPorterSlave": name = "PorterSlave"; break;
            case "myCheckBoxPriest": name = "Priest"; break;
            case "myCheckBoxTrueLove": name = "TrueLove";  break;
            case "myCheckBoxWizard": name = "Wizard"; break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyMember_Click(): reached default name=" + cb.Name); return;
         }
         Option option = myOptions.Find(name);
         if (null == option)
         {
            option = new Option(name, false);
            myOptions.Add(option);
         }
         option.IsEnabled = !option.IsEnabled;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyMember_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void StackPanelPartyOption_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         switch (cb.Name)
         {
            case "myCheckBoxPartyMounted": myIsRandomGame = false; option = myOptions.Find("PartyMounted"); break;
            case "myCheckBoxPartyAirborne": myIsRandomGame = false; option = myOptions.Find("PartyAirborne"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyOption_Click(): reached default name=" + cb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyOption_Click(): myOptions.Find() for name=" + cb.Name);
         else
            option.IsEnabled = !option.IsEnabled;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelPartyOption_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void StackPanelGameOption_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         string name = "";
         switch (cb.Name)
         {
            case "myCheckBoxAutoLostIncrement": name = "AutoLostDecrease"; break;
            case "myCheckBoxExtendTime": name = "ExtendEndTime"; break;
            case "myCheckBoxReducedLodgingCosts": name = "ReduceLodgingCosts"; break;
            case "myCheckBoxAddIncome": name = "SteadyIncome"; break;
            case "myCheckBoxEasyRoute": name = "EasyRoute"; break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOption_Click(): reached default name=" + cb.Name); return;
         }
         option = myOptions.Find(name);
         if (null == option)
         {
            Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOption_Click(): myOptions.Find() for name=" + cb.Name);
            option = new Option(name, false);
         }
         else
         {
            option.IsEnabled = !option.IsEnabled;
         }
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelGameOption_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
      private void StackPanelHex_Click(object sender, RoutedEventArgs e)
      {
         RadioButton rb = (RadioButton)sender;
         Option option = null;
         ResetHex();
         switch (rb.Name)
         {
            case "myRadioButtonHexOriginal":
               if (false == UpdateDisplay(myOptions))
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelHex_Click(): UpdateDisplay() returned false for name=" + rb.Name);
               return;
            case "myRadioButtonHexRandom": option = myOptions.Find("RandomHex"); break;
            case "myRadioButtonHexRandomTown": option = myOptions.Find("RandomTown"); break;
            case "myRadioButtonHexRandomLeft": option = myOptions.Find("RandomLeft"); break;
            case "myRadioButtonHexRandomRight": option = myOptions.Find("RandomRight"); break;
            case "myRadioButtonHexRandomBottom": option = myOptions.Find("RandomBottom"); break;
            case "myRadioButtonHexTown": option = myOptions.Find("0109"); break;
            case "myRadioButtonHexRuin": option = myOptions.Find("0206"); break;
            case "myRadioButtonHexRiver": option = myOptions.Find("0708"); break;
            case "myRadioButtonHexTemple": option = myOptions.Find("0711"); break;
            case "myRadioButtonHexHuldra": option = myOptions.Find("1212"); break;
            case "myRadioButtonHexDrogat": option = myOptions.Find("0323"); break;
            case "myRadioButtonHexLadyAeravir": option = myOptions.Find("1923"); break;
            case "myRadioButtonHexFarmland": option = myOptions.Find("0418"); break;
            case "myRadioButtonHexCountryside": option = myOptions.Find("0722"); break;
            case "myRadioButtonHexForest": option = myOptions.Find("0409"); break;
            case "myRadioButtonHexHill": option = myOptions.Find("0406"); break;
            case "myRadioButtonHexMountain": option = myOptions.Find("1611"); break;
            case "myRadioButtonHexSwamp": option = myOptions.Find("0411"); break;
            case "myRadioButtonHexDesert": option = myOptions.Find("1507"); break;
            case "myRadioButtonHexRoad": option = myOptions.Find("1905"); break;
            case "myRadioButtonHexBottom": option = myOptions.Find("1723"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelHex_Click(): reached default name=" + rb.Name); return;
         }
         myIsRandomGame = false;
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelHex_Click(): myOptions.Find() for name=" + rb.Name);
         else
            option.IsEnabled = true;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelHex_Click(): UpdateDisplay() returned false for name=" + rb.Name);
      }
      private void StackPanelMonster_Click(object sender, RoutedEventArgs e)
      {
         RadioButton rb = (RadioButton)sender;
         Option option = null;
         ResetMonsters();
         myIsRandomGame = false;
         switch (rb.Name)
         {
            case "myRadioButtonMonsterNormal":
               if (false == UpdateDisplay(myOptions))
                  Logger.Log(LogEnum.LE_ERROR, "StackPanelMonster_Click(): UpdateDisplay() returned false for name=" + rb.Name); 
               return;
            case "myRadioButtonMonsterLessEasy": option = myOptions.Find("LessHardMonsters"); break;
            case "myRadioButtonMonsterEasy": option = myOptions.Find("EasyMonsters"); break;
            case "myRadioButtonMonsterEasiest": option = myOptions.Find("EasiestMonsters"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelMonster_Click(): reached default name=" + rb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelMonster_Click(): myOptions.Find() for name=" + rb.Name);
         else
            option.IsEnabled = true;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelMonster_Click(): UpdateDisplay() returned false for name=" + rb.Name);
      }
      private void StackPanelEvent_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         Option option = null;
         myIsRandomGame = false;
         switch (cb.Name)
         {
            case "myCheckBoxNoLostRoll": option = myOptions.Find("NoLostRoll"); break;
            case "myCheckBoxNoLostEvent": option = myOptions.Find("ForceNoLostEvent"); break;
            case "myCheckBoxNoEvent": option = myOptions.Find("ForceNoEvent"); break;
            case "myCheckBoxNoRoadEvent": option = myOptions.Find("ForceNoRoadEvent"); break;
            case "myCheckBoxNoCrossEvent": option = myOptions.Find("ForceNoCrossEvent"); break;
            case "myCheckBoxNoRaftEvent": option = myOptions.Find("ForceNoRaftEvent"); break;
            case "myCheckBoxNoAirEvent": option = myOptions.Find("ForceNoAirEvent"); break;
            case "myCheckBoxForceLostEvent": option = myOptions.Find("ForceLostEvent"); break;
            case "myCheckBoxForceLostAfterCross": option = myOptions.Find("ForceLostAfterCrossEvent"); break;
            case "myCheckBoxForceEvent": option = myOptions.Find("ForceEvent"); break;
            case "myCheckBoxForceCrossEvent": option = myOptions.Find("ForceCrossEvent"); break;
            case "myCheckBoxForceRaftEvent": option = myOptions.Find("ForceRaftEvent"); break;
            case "myCheckBoxForceAirEvent": option = myOptions.Find("ForceAirEvent"); break;
            default: Logger.Log(LogEnum.LE_ERROR, "StackPanelEvent_Click(): reached default name=" + cb.Name); return;
         }
         if (null == option)
            Logger.Log(LogEnum.LE_ERROR, "StackPanelEvent_Click(): myOptions.Find() for name=" + cb.Name);
         else
            option.IsEnabled = !option.IsEnabled;
         if (false == UpdateDisplay(myOptions))
            Logger.Log(LogEnum.LE_ERROR, "StackPanelEvent_Click(): UpdateDisplay() returned false for name=" + cb.Name);
      }
   }
}
