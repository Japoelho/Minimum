using Microsoft.Win32;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Minimum.WPF
{
    public partial class ImageDropBox : UserControl
    {
        public byte[] Image { get; private set; }
        public string Filename { get; private set; }

        public ImageSource Source
        {
            get { return GetValue(ImageSourceProperty) as ImageSource; }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(ImageDropBox));

        public event EventHandler ImageChanged;

        public ImageDropBox()
        {
            InitializeComponent();
        }

        private void FileDrop(object sender, DragEventArgs e)
        {
            DataObject paste = e.Data as DataObject;
            if (paste.ContainsFileDropList())
            {
                StringCollection images = paste.GetFileDropList();
                if (images.Count > 0)
                {
                    using (FileStream file = File.Open(images[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = file;
                        image.EndInit();
                        
                        ImageBlock.Source = image;

                        Filename = images[0].LastIndexOf('\\') > -1 ? images[0].Substring(images[0].LastIndexOf('\\') + 1) : images[0];

                        using (MemoryStream stream = new MemoryStream())
                        {
                            file.Seek(0, SeekOrigin.Begin);
                            file.CopyTo(stream);
                            Image = stream.ToArray();
                        }

                        if (ImageChanged != null) { ImageChanged(this, e); }
                    }
                }
            }
        }

        private void ImageClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "JPG Files (*.jpg)|*.jpg|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|GIF Files (*.gif)|*.gif";

            if (fileDialog.ShowDialog() == true)
            {
                using (Stream file = fileDialog.OpenFile())
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = file;
                    image.EndInit();

                    ImageBlock.Source = image;

                    Filename = fileDialog.SafeFileName;

                    using (MemoryStream stream = new MemoryStream())
                    {
                        file.Seek(0, SeekOrigin.Begin);
                        file.CopyTo(stream);
                        Image = stream.ToArray();
                    }

                    if (ImageChanged != null) { ImageChanged(this, e); }
                }
            }
        }
    }
}