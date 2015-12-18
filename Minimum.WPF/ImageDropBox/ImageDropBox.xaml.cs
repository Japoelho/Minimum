using Microsoft.Win32;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using System.ComponentModel;

namespace Minimum.WPF
{
    public partial class ImageDropBox : UserControl
    {
        private bool _isWorking;        

        public byte[] Image { get; private set; }
        public string Filename { get; private set; }

        public ImageSource Source
        {
            get { return GetValue(ImageSourceProperty) as ImageSource; }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(ImageDropBox), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });

        public event EventHandler ImageChanged;

        public ImageDropBox()
        {
            _isWorking = false;

            InitializeComponent();
        }

        private void FileDrop(object sender, DragEventArgs e)
        {
            if (_isWorking) { return; }

            DataObject paste = e.Data as DataObject;

            // - Tudo menos o IE
            if (paste.GetFormats().Contains("Text"))
            {
                string url = (string)paste.GetData("Text");

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (s, ev) =>
                {
                    WebClient client = new WebClient();

                    try
                    {
                        Uri uri = new Uri(url);
                        byte[] imageData = client.DownloadData(uri);

                        ev.Result = imageData;
                    }
                    catch { ev.Result = null; }                    
                };
                worker.RunWorkerCompleted += (s, ev) =>
                {
                    byte[] imageData = ev.Result as byte[];
                    if (imageData == null) 
                    {
                        Filter.Visibility = Visibility.Collapsed;
                        _isWorking = false; 
                        return; 
                    }

                    using (MemoryStream stream = new MemoryStream(imageData))
                    {                        
                        try
                        {
                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = stream;
                            image.EndInit();

                            ImageBlock.Source = image;
                            Image = imageData;

                            Filename = url.LastIndexOf('/') > -1 ? url.Substring(url.LastIndexOf('/') + 1) : url;

                            if (ImageChanged != null) { ImageChanged(this, e); }
                        }
                        catch { }
                    }

                    Filter.Visibility = Visibility.Collapsed;
                    _isWorking = false;
                };
                worker.RunWorkerAsync();
                _isWorking = true;
                Filter.Visibility = Visibility.Visible;
                
                return;
            }

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
            if (_isWorking) { return; }

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