using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Minimum.WPF
{
    public partial class TextBoxAutoComplete : UserControl, INotifyPropertyChanged
    {
        public string Text
        {
            get { return GetValue(TextProperty).ToString(); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextBoxAutoComplete), new FrameworkPropertyMetadata 
        {
            BindsTwoWayByDefault = true
        });
        
        private IList<String> _textValues;
        public IList<String> TextValues { get { return _textValues; } set { _textValues = value; OnPropertyChanged("TextValues"); } }

        public IList Values
        {
            get { return GetValue(ValuesProperty) as IList; }
            set 
            {
                SetValue(ValuesProperty, value);
                
                _textValues.Clear();
                
                if (value != null)
                {
                    if (value.Count == 1)
                    {
                        string newValue = null;

                        if (PropertyName != null) { newValue = value[0].GetType().GetProperty(PropertyName).GetValue(value[0], null).ToString(); }
                        else { newValue = value[0].ToString(); }

                        if (newValue == Text) { return; }
                    }

                    for (int i = 0; i < value.Count; i++)
                    {
                        string newValue = null;

                        if (PropertyName != null) { newValue = value[i].GetType().GetProperty(PropertyName).GetValue(value[i], null).ToString(); }
                        else { newValue = value[i].ToString(); }

                        _textValues.Add(newValue);
                    }
                }
                
                OnPropertyChanged("TextValues");
            }
        }

        public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register("Values", typeof(IList), typeof(TextBoxAutoComplete), new FrameworkPropertyMetadata 
        {
            BindsTwoWayByDefault = true,
            PropertyChangedCallback = new PropertyChangedCallback(ValuesPropertyChanged)
        });


        public static void ValuesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TextBoxAutoComplete)d).Values = e.NewValue as IList;            
        }

        public string PropertyName
        {
            get { return GetValue(PropertyNameProperty).ToString(); }
            set { SetValue(PropertyNameProperty, value); }
        }

        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register("PropertyName", typeof(string), typeof(TextBoxAutoComplete));

        public TextBoxAutoComplete()
        {
            _textValues = new ObservableCollection<String>();

            InitializeComponent();            
        }

        public event EventHandler TextChanged;
        
        private void TextBox_Focus(object sender, RoutedEventArgs e)
        {
        }

        private void TextBox_Blur(object sender, RoutedEventArgs e)
        {
            if (TextValues == null) { return; }

            TextValues.Clear();
            OnPropertyChanged("TextValues");
        }
        
        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TextChanged != null) { TextChanged(this, e); }

            Text = TextBox.Text;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (ValuesList.Items.Count > 0)
                {
                    if (ValuesList.SelectedIndex < 0)
                    {
                        ValuesList.SelectedIndex = 0;
                    }
                    else
                    {
                        ValuesList.SelectedIndex = ValuesList.SelectedIndex + 1;
                    }
                }
            }
            else if (e.Key == Key.Up)
            {
                if (ValuesList.Items.Count > 0)
                {
                    if (ValuesList.SelectedIndex - 1 < 0)
                    {
                        ValuesList.SelectedIndex = -1;
                    }
                    else if (ValuesList.SelectedIndex >= 0)
                    {
                        ValuesList.SelectedIndex = ValuesList.SelectedIndex - 1;
                    }
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (ValuesList.SelectedIndex >= 0)
                {
                    TextBox.Text = ValuesList.SelectedItem.ToString();
                    TextBox.Select(TextBox.Text.Length, 0);
                    TextValues.Clear();
                    OnPropertyChanged("TextValues");
                }
            }
        }

        private void ListView_MouseClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock contentText = e.OriginalSource as TextBlock;
            if (contentText != null)
            {
                TextBox.Text = contentText.Text;
                TextBox.Select(TextBox.Text.Length, 0);
                TextValues.Clear();
                OnPropertyChanged("TextValues");
                return;
            }

            Border contentBorder = e.OriginalSource as Border;
            if (contentBorder != null)
            {
                ContentPresenter content = contentBorder.Child as ContentPresenter;
                TextBox.Text = content.Content.ToString();
                TextBox.Select(TextBox.Text.Length, 0);
                TextValues.Clear();
                OnPropertyChanged("TextValues");
                return;                
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class ListHasValues : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && (value as IList).Count > 0)
            { return true; }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}