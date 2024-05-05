namespace BarbarianPrince
{
    public interface IUnitTest
    {
        bool CtorError { get; }
        string HeaderName { get; }
        string CommandName { get; }
        bool NextTest(ref IGameInstance gi);
        bool Command(ref IGameInstance gi);
        bool Cleanup(ref IGameInstance gi);
    }
}
