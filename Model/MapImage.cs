using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace BarbarianPrince
{
    [Serializable]
    public class MapImage : IMapImage
    {
        public System.Windows.Media.Imaging.BitmapImage myBitmapImage = null;
        public string Name { get; set; } = "";
        public bool IsAnimated { get; set; } = false;
        public Image ImageControl { get; set; } = null;
        public ImageAnimationController AnimationController { get; set; } = null;
        //--------------------------------------------
        public MapImage()
        {
        }
        public MapImage(string imageName)
        {
            try
            {
                Name = imageName;
                myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.UriSource = new Uri("../../Images/" + Utilities.RemoveSpaces(imageName) + ".gif", UriKind.Relative);
                myBitmapImage.EndInit();
                ImageControl = new Image { Source = myBitmapImage, Stretch = Stretch.Fill, Name = imageName };
                ImageBehavior.SetAnimatedSource(ImageControl, myBitmapImage);
                ImageBehavior.SetAutoStart(ImageControl, true);
                ImageBehavior.SetRepeatBehavior(ImageControl, new RepeatBehavior(1));
                ImageBehavior.AddAnimationCompletedHandler(ImageControl, ImageAnimationLoaded);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Unable to create img=" + imageName + " e=" + e.ToString());
            }
        }
        MapImage(MapImage mii)
        {
            Name = mii.Name;
            myBitmapImage = mii.myBitmapImage;
            ImageControl = mii.ImageControl;
            AnimationController = mii.AnimationController;
        }
        private void ImageAnimationLoaded(object sender, RoutedEventArgs e)
        {
            ImageControl = (Image)sender;
            // Logger.Log(LogEnum.LE_GAME_INIT, "ImageAnimationLoaded(): name=" + ImageControl.Name);
            AnimationController = ImageBehavior.GetAnimationController(ImageControl);
            if (null == AnimationController)
                Logger.Log(LogEnum.LE_ERROR, "ImageAnimationCompleted(): controller=null");
            else
                IsAnimated = true;
        }
    }
    //--------------------------------------------------------------------------
    [Serializable]
    public class MapImages : IEnumerable, IMapImages
    {
        private readonly ArrayList myList;
        public MapImages() { myList = new ArrayList(); }
        public void Add(IMapImage mii) { myList.Add(mii); }
        public IMapImage RemoveAt(int index)
        {
            IMapImage mii = (IMapImage)myList[index];
            myList.RemoveAt(index);
            return mii;
        }
        public void Insert(int index, IMapImage mii) { myList.Insert(index, mii); }
        public int Count { get { return myList.Count; } }
        public void Clear() { myList.Clear(); }
        public bool Contains(IMapImage mii) { return myList.Contains(mii); }
        public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
        public int IndexOf(IMapImage mii) { return myList.IndexOf(mii); }
        public void Remove(IMapImage mii) { myList.Remove(mii); }
        public IMapImage Find(string pathToMatch)
        {
            foreach (Object o in myList)
            {
                IMapImage mii = (IMapImage)o;
                if (mii.Name == pathToMatch)
                    return mii;
            }
            return null;
        }
        public BitmapImage GetBitmapImage(string pathToMatch)
        {
            foreach (Object o in myList)
            {
                MapImage mii = (MapImage)o;
                if (mii.Name == pathToMatch)
                    return mii.myBitmapImage;
            }
            MapImage miiToAdd = new MapImage(pathToMatch);
            myList.Add(miiToAdd);
            return miiToAdd.myBitmapImage;
        }
        public IMapImage this[int index]
        {
            get { return (IMapImage)(myList[index]); }
            set { myList[index] = value; }
        }
    }
}
