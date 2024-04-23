using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace BarbarianPrince
{
    public interface IMapImage
    {
        string Name { get; set; }
        bool IsAnimated { get; set; }
        Image ImageControl { get; set; }
        ImageAnimationController AnimationController { get; set; }
    }
    public interface IMapImages : System.Collections.IEnumerable
    {
        int Count { get; }
        void Add(IMapImage mii);
        IMapImage RemoveAt(int index);
        void Insert(int index, IMapImage mii);
        void Clear();
        bool Contains(IMapImage mii);
        int IndexOf(IMapImage mii);
        void Remove(IMapImage mii);
        IMapImage Find(string pathToMatch);
        BitmapImage GetBitmapImage(string pathToMatch);
        IMapImage this[int index] { get; set; }
    }
}
