using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace renstech.NET.SupernovaDispatcher.Control
{
    /// <summary>
    /// Interaction logic for UserLineControl.xaml
    /// </summary>
    public partial class UserLineControl : UserControl
    {
        static public int ButtonHeight;
        static public int ColCount;
        static public RoutedEventHandler ItemButton_Click;

        public UserLineControl()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.DataContext != null)
            {
                List<object> items = this.DataContext as List<object>;
                if (items != null)
                    ArrangeUsers(items);
            }
        }

        private void ArrangeUsers(List<object> Items)
        {
            for (int i = 0; i < ColCount; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(1, GridUnitType.Star);
                UserLineGrid.ColumnDefinitions.Add(col);                
            }

            for (int i = 0; i < Items.Count; i++)
            {
                Button button = new Button();
                button.Style = (Style)this.Resources["UserButtonBaseStyle"];
                button.Height = ButtonHeight;
                button.DataContext = Items[i];
                if (ItemButton_Click != null)
                    button.Click += ItemButton_Click;

                Grid.SetColumn(button, i);
                UserLineGrid.Children.Add(button);
            }
        }
    }
}
