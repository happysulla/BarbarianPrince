namespace BarbarianPrince
{
    public interface IView
    {
        void UpdateView(ref IGameInstance gi, GameAction action);
    }
}
