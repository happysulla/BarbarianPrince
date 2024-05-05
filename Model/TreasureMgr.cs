using System.Collections.Generic;

namespace BarbarianPrince
{
    public class TreasureMgr
    {
        private readonly string[] myA = new string[6] { "r180", "r181", "r182", "r183", "r184", "r185" };
        private readonly string[] myB = new string[6] { "r180", "r186", "r187", "r188", "r190", "r193" };
        private readonly string[] myBa = new string[6] { "r180", "r186", "r187", "r188a", "r190", "r193" };
        private readonly string[] myC = new string[6] { "r186", "r188", "r189", "r191", "r192", "r194" };
        private readonly string[] myCa = new string[6] { "r186", "r188a", "r189", "r191", "r192", "r194" };
        private Dictionary<int, int>[] myCoin = new Dictionary<int, int>[6];
        private Dictionary<int, SpecialEnum>[] mySpecialPossessions = new Dictionary<int, SpecialEnum>[6];
        //------------------------------------------------------
        public TreasureMgr()
        {
            myCoin[0] = new Dictionary<int, int>();
            myCoin[0][0] = 0;
            myCoin[0][1] = 0;
            myCoin[0][2] = 0;
            myCoin[0][4] = 2;
            myCoin[0][5] = 2;
            myCoin[0][7] = 3;
            myCoin[0][10] = 6;
            myCoin[0][12] = 5;
            myCoin[0][15] = 10;
            myCoin[0][21] = 15;
            myCoin[0][25] = 20;
            myCoin[0][30] = 23;
            myCoin[0][50] = 40;
            myCoin[0][60] = 45;
            myCoin[0][70] = 55;
            myCoin[0][100] = 85;
            myCoin[0][110] = 80;
            myCoin[1] = new Dictionary<int, int>();
            myCoin[1][0] = 0;
            myCoin[1][1] = 0;
            myCoin[1][2] = 1;
            myCoin[1][4] = 3;
            myCoin[1][5] = 4;
            myCoin[1][7] = 4;
            myCoin[1][10] = 8;
            myCoin[1][12] = 9;
            myCoin[1][15] = 12;
            myCoin[1][21] = 18;
            myCoin[1][25] = 22;
            myCoin[1][30] = 27;
            myCoin[1][50] = 45;
            myCoin[1][60] = 50;
            myCoin[1][70] = 60;
            myCoin[1][100] = 90;
            myCoin[1][110] = 90;
            myCoin[2] = new Dictionary<int, int>();
            myCoin[2][0] = 0;
            myCoin[2][1] = 1;
            myCoin[2][2] = 2;
            myCoin[2][4] = 4;
            myCoin[2][5] = 4;
            myCoin[2][7] = 6;
            myCoin[2][10] = 9;
            myCoin[2][12] = 11;
            myCoin[2][15] = 14;
            myCoin[2][21] = 20;
            myCoin[2][25] = 24;
            myCoin[2][30] = 29;
            myCoin[2][50] = 48;
            myCoin[2][60] = 55;
            myCoin[2][70] = 65;
            myCoin[2][100] = 95;
            myCoin[2][110] = 100;
            myCoin[3] = new Dictionary<int, int>();
            myCoin[3][0] = 0;
            myCoin[3][1] = 1;
            myCoin[3][2] = 2;
            myCoin[3][4] = 4;
            myCoin[3][5] = 6;
            myCoin[3][7] = 8;
            myCoin[3][10] = 11;
            myCoin[3][12] = 12;
            myCoin[3][15] = 16;
            myCoin[3][21] = 22;
            myCoin[3][25] = 26;
            myCoin[3][30] = 31;
            myCoin[3][50] = 52;
            myCoin[3][60] = 60;
            myCoin[3][70] = 70;
            myCoin[3][100] = 100;
            myCoin[3][110] = 110;
            myCoin[4] = new Dictionary<int, int>();
            myCoin[4][0] = 0;
            myCoin[4][1] = 2;
            myCoin[4][2] = 3;
            myCoin[4][4] = 5;
            myCoin[4][5] = 7;
            myCoin[4][7] = 10;
            myCoin[4][10] = 12;
            myCoin[4][12] = 15;
            myCoin[4][15] = 18;
            myCoin[4][21] = 24;
            myCoin[4][25] = 28;
            myCoin[4][30] = 33;
            myCoin[4][50] = 55;
            myCoin[4][60] = 70;
            myCoin[4][70] = 80;
            myCoin[4][100] = 110;
            myCoin[4][110] = 130;
            myCoin[5] = new Dictionary<int, int>();
            myCoin[5][0] = 0;
            myCoin[5][1] = 2;
            myCoin[5][2] = 4;
            myCoin[5][4] = 6;
            myCoin[5][5] = 8;
            myCoin[5][7] = 11;
            myCoin[5][10] = 14;
            myCoin[5][12] = 20;
            myCoin[5][15] = 20;
            myCoin[5][21] = 27;
            myCoin[5][25] = 30;
            myCoin[5][30] = 37;
            myCoin[5][50] = 60;
            myCoin[5][60] = 80;
            myCoin[5][70] = 90;
            myCoin[5][100] = 120;
            myCoin[5][110] = 150;
            //-----------------------------------------
            mySpecialPossessions[0] = new Dictionary<int, SpecialEnum>();  // Row A
            mySpecialPossessions[0][0] = SpecialEnum.HealingPoition;
            mySpecialPossessions[0][1] = SpecialEnum.CurePoisonVial;
            mySpecialPossessions[0][2] = SpecialEnum.GiftOfCharm;
            mySpecialPossessions[0][3] = SpecialEnum.EnduranceSash;
            mySpecialPossessions[0][4] = SpecialEnum.ResistanceTalisman;
            mySpecialPossessions[0][5] = SpecialEnum.PoisonDrug;
            //-----------------------------------------
            mySpecialPossessions[1] = new Dictionary<int, SpecialEnum>(); // Row B
            mySpecialPossessions[1][0] = SpecialEnum.HealingPoition;
            mySpecialPossessions[1][1] = SpecialEnum.MagicSword;
            mySpecialPossessions[1][2] = SpecialEnum.CurePoisonVial;
            mySpecialPossessions[1][3] = SpecialEnum.PegasusMount;
            mySpecialPossessions[1][4] = SpecialEnum.NerveGasBomb;
            mySpecialPossessions[1][5] = SpecialEnum.ShieldOfLight;
            //-----------------------------------------
            mySpecialPossessions[2] = new Dictionary<int, SpecialEnum>(); // Row C
            mySpecialPossessions[2][0] = SpecialEnum.MagicSword;
            mySpecialPossessions[2][1] = SpecialEnum.PegasusMount;
            mySpecialPossessions[2][2] = SpecialEnum.CharismaTalisman;
            mySpecialPossessions[2][3] = SpecialEnum.ResistanceRing;
            mySpecialPossessions[2][4] = SpecialEnum.ResurrectionNecklace;
            mySpecialPossessions[2][5] = SpecialEnum.RoyalHelmOfNorthlands;
            //-----------------------------------------
            mySpecialPossessions[3] = new Dictionary<int, SpecialEnum>(); // Row Ba
            mySpecialPossessions[3][0] = SpecialEnum.HealingPoition;
            mySpecialPossessions[3][1] = SpecialEnum.MagicSword;
            mySpecialPossessions[3][2] = SpecialEnum.CurePoisonVial;
            mySpecialPossessions[3][3] = SpecialEnum.PegasusMountTalisman;
            mySpecialPossessions[3][4] = SpecialEnum.NerveGasBomb;
            mySpecialPossessions[3][5] = SpecialEnum.ShieldOfLight;
            //-----------------------------------------
            mySpecialPossessions[4] = new Dictionary<int, SpecialEnum>(); // Row Ca
            mySpecialPossessions[4][0] = SpecialEnum.MagicSword;
            mySpecialPossessions[4][1] = SpecialEnum.PegasusMountTalisman;
            mySpecialPossessions[4][2] = SpecialEnum.CharismaTalisman;
            mySpecialPossessions[4][3] = SpecialEnum.ResistanceRing;
            mySpecialPossessions[4][4] = SpecialEnum.ResurrectionNecklace;
            mySpecialPossessions[4][5] = SpecialEnum.RoyalHelmOfNorthlands;

        }
        //------------------------------------------------------
        public int GetCoin(int wealthCode, int dieRoll)
        {

            int coin = -1;
            switch (wealthCode)
            {
                case 0:
                case 1:
                case 2:
                case 4:
                case 5:
                case 7:
                case 10:
                case 12:
                case 15:
                case 21:
                case 25:
                case 30:
                case 50:
                case 60:
                case 70:
                case 100:
                case 110:
                    break;
                default:
                    Logger.Log(LogEnum.LE_ERROR, "GetCoin(): invalid parameter wc=" + wealthCode.ToString());
                    return coin;
            }
            dieRoll -= 1; // zero based array
            if ((dieRoll < 0) && (5 < dieRoll))
            {
                Logger.Log(LogEnum.LE_ERROR, "GetCoin(): invalid parameter wc=" + wealthCode.ToString());
                return coin;
            }
            try
            {
                coin = myCoin[dieRoll][wealthCode];
                return coin;
            }
            catch (System.Collections.Generic.KeyNotFoundException e1)
            {
                Logger.Log(LogEnum.LE_ERROR, "GetCoin(): Unable to find e=" + e1.ToString() + " dr=" + dieRoll.ToString() + " wc=" + wealthCode.ToString());
                return coin;
            }
        }
        public int GetCoin(int wealthCode)
        {
            int coin = -1;
            switch (wealthCode)
            {
                case 0:
                case 1:
                case 2:
                case 4:
                case 5:
                case 7:
                case 10:
                case 12:
                case 15:
                case 21:
                case 25:
                case 30:
                case 50:
                case 60:
                case 70:
                case 100:
                case 110:
                    break;
                default:
                    Logger.Log(LogEnum.LE_ERROR, "GetCoin(): invalid parameter wc=" + wealthCode.ToString());
                    return coin;
            }
            int dieRoll = Utilities.RandomGenerator.Next(0, 6);
            if ((dieRoll < 0) && (5 < dieRoll))
            {
                Logger.Log(LogEnum.LE_ERROR, "GetCoin(): invalid parameter wc=" + wealthCode.ToString());
                return coin;
            }
            try
            {
                coin = myCoin[dieRoll][wealthCode];
            }
            catch (System.Collections.Generic.KeyNotFoundException e1)
            {
                Logger.Log(LogEnum.LE_ERROR, "GetCoin(): Unable to find e=" + e1.ToString() + " dr=" + dieRoll.ToString() + " wc=" + wealthCode.ToString());
            }
            return coin;
        }
        public SpecialEnum GetSpecialPossession(int wealthCode, int wealthCodeCol, int dieRoll, PegasusTreasureEnum pegasusTreasureType)
        {
            if ((dieRoll < 1) && (6 < dieRoll))
            {
                Logger.Log(LogEnum.LE_ERROR, "GetSpecialPossession(): Invalid param dr=" + dieRoll.ToString());
                return SpecialEnum.None;
            }
            //-----------------------------------------
            int index = -1;
            switch (wealthCode)
            {
                case 5:
                    switch (wealthCodeCol)
                    {
                        case 2:
                        case 4:
                        case 6:
                            index = 0;
                            break;
                        default:
                            Logger.Log(LogEnum.LE_ERROR, "GetSpecialPossession(): Invalid Parameters wc=" + wealthCode.ToString() + " col=" + wealthCodeCol.ToString());
                            return SpecialEnum.None;
                    }
                    break;
                case 12:
                    switch (wealthCodeCol)
                    {
                        case 2:
                            if (PegasusTreasureEnum.Mount == pegasusTreasureType)
                                index = 2;
                            else
                                index = 4;
                            break;
                        case 3:
                        case 5:
                            index = 0;
                            break;
                        default:
                            Logger.Log(LogEnum.LE_ERROR, "GetSpecialPossession(): Invalid Parameters wc=" + wealthCode.ToString() + " col=" + wealthCodeCol.ToString());
                            return SpecialEnum.None;
                    }
                    break;
                case 25:
                    switch (wealthCodeCol)
                    {
                        case 1:
                        case 3:
                        case 5:
                            index = 0;
                            break;
                        default:
                            Logger.Log(LogEnum.LE_ERROR, "GetSpecialPossession(): Invalid Parameters wc=" + wealthCode.ToString() + " col=" + wealthCodeCol.ToString());
                            return SpecialEnum.None;
                    }
                    break;
                case 60:
                    switch (wealthCodeCol)
                    {
                        case 1:
                            index = 0;
                            break;
                        case 2:
                            if (PegasusTreasureEnum.Mount == pegasusTreasureType)
                                index = 2;
                            else
                                index = 4;
                            break;
                        case 4:
                            if (PegasusTreasureEnum.Mount == pegasusTreasureType)
                                index = 1;
                            else
                                index = 3;
                            break;
                        case 5:
                            index = 0;
                            break;
                        default:
                            Logger.Log(LogEnum.LE_ERROR, "GetSpecialPossession(): Invalid Parameters wc=" + wealthCode.ToString() + " col=" + wealthCodeCol.ToString());
                            return SpecialEnum.None;
                    }
                    break;
                case 110:
                    switch (wealthCodeCol)
                    {
                        case 1:
                        case 3:
                            if (PegasusTreasureEnum.Mount == pegasusTreasureType)
                                index = 1;
                            else
                                index = 3;
                            break;
                        case 2:
                        case 5:
                            if (PegasusTreasureEnum.Mount == pegasusTreasureType)
                                index = 2;
                            else
                                index = 4;
                            break;
                        case 4:
                        case 6:
                            index = 0;
                            break;
                        default:
                            Logger.Log(LogEnum.LE_ERROR, "GetSpecialPossession(): Invalid Parameters wc=" + wealthCode.ToString() + " col=" + wealthCodeCol.ToString());
                            return SpecialEnum.None;
                    }
                    break;
                default:
                    Logger.Log(LogEnum.LE_ERROR, "GetSpecialPossession(): Invalid Parameters wc=" + wealthCode.ToString());
                    return SpecialEnum.None;
            }
            //-----------------------------------------
            if (index < 0 || 4 < index)
            {
                Logger.Log(LogEnum.LE_ERROR, "GetSpecialPossession(): Invalid logic wc=" + wealthCode.ToString() + " col=" + wealthCodeCol.ToString() + " index=" + index.ToString());
                return SpecialEnum.None;
            }
            //-----------------------------------------
            dieRoll -= 1; // zero based array
            try
            {
                SpecialEnum sp = mySpecialPossessions[index][dieRoll];
                return sp;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                Logger.Log(LogEnum.LE_ERROR, "GetSpecialPossession(): Exception dr=" + dieRoll.ToString() + " index=" + index.ToString());
                return SpecialEnum.None;
            }
        }
        //------------------------------------------------------
        public int[] GetCoinRowContent(int wealthCode)
        {
            switch (wealthCode)
            {
                case 0:
                case 1:
                case 2:
                case 4:
                case 5:
                case 7:
                case 10:
                case 12:
                case 15:
                case 21:
                case 25:
                case 30:
                case 50:
                case 60:
                case 70:
                case 100:
                case 110:
                    break;
                default:
                    Logger.Log(LogEnum.LE_ERROR, "GetCoinRowContent(): invalid parameter wc=" + wealthCode.ToString());
                    return null;
            }
            int[] coins = new int[6];
            coins[0] = myCoin[0][wealthCode];
            coins[1] = myCoin[1][wealthCode];
            coins[2] = myCoin[2][wealthCode];
            coins[3] = myCoin[3][wealthCode];
            coins[4] = myCoin[4][wealthCode];
            coins[5] = myCoin[5][wealthCode];
            return coins;
        }
        public string[] GetItemRowContent(int wealthCode, int wealthCodeCol, PegasusTreasureEnum pegasusTreasureType)
        {
            switch (wealthCode)
            {
                case 5:
                    switch (wealthCodeCol)
                    {
                        case 2: case 4: case 6: return myA;
                        default: Logger.Log(LogEnum.LE_ERROR, "GetItemRowContent(): invalid parameter wc=" + wealthCode.ToString() + " wcc=" + wealthCodeCol.ToString()); return null;
                    }
                case 12:
                    switch (wealthCodeCol)
                    {
                        case 2:
                            if (PegasusTreasureEnum.Mount == pegasusTreasureType)
                                return myC;
                            return myCa;
                        case 3: case 5: return myA;
                        default: Logger.Log(LogEnum.LE_ERROR, "GetItemRowContent(): invalid parameter wc=" + wealthCode.ToString() + " wcc=" + wealthCodeCol.ToString()); return null;
                    }
                case 25:
                    switch (wealthCodeCol)
                    {
                        case 1: case 3: case 5: return myA;
                        default: Logger.Log(LogEnum.LE_ERROR, "GetItemRowContent(): invalid parameter wc=" + wealthCode.ToString() + " wcc=" + wealthCodeCol.ToString()); return null;
                    }
                case 60:
                    switch (wealthCodeCol)
                    {
                        case 1: case 5: return myA;
                        case 2:
                            if (PegasusTreasureEnum.Mount == pegasusTreasureType)
                                return myC;
                            return myCa;
                        case 4:
                            if (PegasusTreasureEnum.Mount == pegasusTreasureType)
                                return myB;
                            return myBa;
                        default: Logger.Log(LogEnum.LE_ERROR, "GetItemRowContent(): invalid parameter wc=" + wealthCode.ToString() + " wcc=" + wealthCodeCol.ToString()); return null;
                    }
                case 110:
                    switch (wealthCodeCol)
                    {
                        case 1:
                        case 3:
                            if (PegasusTreasureEnum.Mount == pegasusTreasureType)
                                return myB;
                            return myBa;
                        case 2:
                        case 5:
                            if (PegasusTreasureEnum.Mount == pegasusTreasureType)
                                return myC;
                            return myCa;
                        case 4:
                        case 6:
                            return myA;
                        default: Logger.Log(LogEnum.LE_ERROR, " GetItemRowContent(): invalid parameter wc=" + wealthCode.ToString() + " wcc=" + wealthCodeCol.ToString()); return null;
                    }
                default: Logger.Log(LogEnum.LE_ERROR, "GetItemRowContent(): invalid parameter wc=" + wealthCode.ToString()); return null;
            }
        }
    }
}
