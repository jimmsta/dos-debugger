using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

// The following code is adapted (and modified) from
// http://www.codeproject.com/Tips/169512/Image-that-is-grayed-when-disabled-for-use-in-butt
// Author:  Thomas Willwacher
// Date Updated: 20 Mar 2011
// License: The Code Project Open License (CPOL)
namespace Util.Windows.Controls
{
    public class AutoGrayImage : Image
    {
        // TBD: do we need to dispose these objects?
        private BitmapSource DisabledSource;

        public AutoGrayImage()
        {
            this.IsEnabledChanged += new
                DependencyPropertyChangedEventHandler(OnIsEnabledChanged);
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsEnabled)
            {
                base.Source = this.Source;
                base.Opacity = 1.0;
            }
            else
            {
                base.Source = this.DisabledSource;
                base.Opacity = 0.5;
            }
        }
        
        public static new readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", 
                typeof(BitmapSource), typeof(AutoGrayImage),
                new PropertyMetadata(null, OnSourceChanged));

        public new BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void OnSourceChanged(
            DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            AutoGrayImage image = sender as AutoGrayImage;
            if (image == null)
                return;

            if (image.Source == null)
            {
                image.DisabledSource = null;
            }
            else
            {
                image.DisabledSource = new FormatConvertedBitmap(
                    image.Source, PixelFormats.Gray8, null, 0);
                image.OpacityMask = new ImageBrush(image.Source);
            }
            image.OnIsEnabledChanged(image, new DependencyPropertyChangedEventArgs());
        }
    }
}
