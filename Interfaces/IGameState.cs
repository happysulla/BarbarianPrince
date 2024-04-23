namespace BarbarianPrince
{
    public interface IGameState
    {
        string PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll);
    }
}
