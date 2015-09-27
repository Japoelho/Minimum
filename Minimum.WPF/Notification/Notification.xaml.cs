using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Minimum.WPF
{
    public partial class Notification : UserControl, INotifyPropertyChanged
    {
        private IList<Message> _notifications;
        public IList<Message> Notifications { get { return _notifications; } set { _notifications = value; OnPropertyChanged("Notifications"); } }

        public Notification()
        {
            Notifications = new ObservableCollection<Message>();

            InitializeComponent();
        }

        public void Notify(string title, string message, NotificationState status)
        {
            Message notification = new Message()
            {
                Title = title,
                Text = message,
                Status = status,
                Duration = TimeSpan.FromMilliseconds(2750)
            };

            Notifications.Add(notification);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (s, e) =>
            {
                Notifications.Remove(notification);
                timer.Stop();
            };
            timer.Start();
        }

        public void Notify(string title, string message, NotificationState status, TimeSpan duration)
        {
            Message notification = new Message()
            {
                Title = title,
                Text = message,
                Status = status,
                Duration = duration
            };

            Notifications.Add(notification);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = duration;
            timer.Tick += (s, e) =>
            {
                Notifications.Remove(notification);
                timer.Stop();
            };
            timer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void MessageViewer_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer messageViewer = sender as ScrollViewer;
            Message message = messageViewer.DataContext as Message;

            DoubleAnimation animation = new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(250));            
            animation.BeginTime = message.Duration;
            messageViewer.BeginAnimation(Rectangle.HeightProperty, animation);

            DoubleAnimation fadeout = new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(500));
            fadeout.BeginTime = message.Duration - TimeSpan.FromMilliseconds(500);
            messageViewer.BeginAnimation(UIElement.OpacityProperty, fadeout);
        }

        private void MessageClick(object sender, EventArgs e)
        {
            Message message = (sender as Border).DataContext as Message;
            if (message != null) { Notifications.Remove(message); }
        }
    }

    public class Message
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public NotificationState Status { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public enum NotificationState
    {
        Sucess, Alert, Error, Undefined
    }

    public class StatusToBorderConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((NotificationState)value)
            {
                case NotificationState.Sucess:
                    { return new SolidColorBrush(Colors.Green); }
                case NotificationState.Error:
                    { return new SolidColorBrush(Colors.Red); }
                case NotificationState.Alert:
                    { return new SolidColorBrush(Colors.Orange); }
                default:
                    { return new SolidColorBrush(Colors.Silver); }
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class StatusToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((NotificationState)value)
            {
                case NotificationState.Sucess:
                    {
                        LinearGradientBrush brush = new LinearGradientBrush();
                        brush.StartPoint = new Point(0, 0);
                        brush.EndPoint = new Point(0, 1);
                        brush.GradientStops.Add(new GradientStop(Color.FromRgb(175, 225, 175), 1.0d));
                        brush.GradientStops.Add(new GradientStop(Color.FromRgb(225, 255, 225), 0.0d));
                        return brush;
                    }
                case NotificationState.Error:
                    {
                        LinearGradientBrush brush = new LinearGradientBrush();
                        brush.StartPoint = new Point(0, 0);
                        brush.EndPoint = new Point(0, 1);
                        brush.GradientStops.Add(new GradientStop(Color.FromRgb(225, 175, 175), 1.0d));
                        brush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 225, 225), 0.0d));
                        return brush;
                    }
                case NotificationState.Alert:
                    {
                        LinearGradientBrush brush = new LinearGradientBrush();
                        brush.StartPoint = new Point(0, 0);
                        brush.EndPoint = new Point(0, 1);
                        brush.GradientStops.Add(new GradientStop(Color.FromRgb(205, 205, 155), 1.0d));
                        brush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 255, 225), 0.0d));
                        return brush;
                    }
                default:
                    {
                        LinearGradientBrush brush = new LinearGradientBrush();
                        brush.StartPoint = new Point(0, 0);
                        brush.EndPoint = new Point(0, 1);
                        brush.GradientStops.Add(new GradientStop(Color.FromRgb(190, 190, 190), 1.0d));
                        brush.GradientStops.Add(new GradientStop(Color.FromRgb(235, 235, 235), 0.0d));
                        //brush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 255, 255), 1.0d));
                        //brush.GradientStops.Add(new GradientStop(Color.FromRgb(210, 210, 210), 0.0d));
                        return brush;
                    }
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class StatusIsCheck : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((NotificationState)value == NotificationState.Sucess) { return Visibility.Visible; }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class StatusIsAlert : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((NotificationState)value == NotificationState.Alert) { return Visibility.Visible; }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class StatusIsError : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((NotificationState)value == NotificationState.Error) { return Visibility.Visible; }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class StatusIsUndefined : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((NotificationState)value == NotificationState.Undefined) { return Visibility.Visible; }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
