using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace renstech.NET.SupernovaDispatcher.PADispatch
{
    /// <summary>
    /// Interaction logic for ZoneLineControl.xaml
    /// </summary>
    public partial class ZoneLineControl : UserControl
    {
        static public int ButtonHeight;
        static public Style ButtonStyle;
        static public int ColCount;
        static public RoutedEventHandler ItemButtonClick;

        public ZoneLineControl()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (DataContext != null)
            {
                List<object> items = DataContext as List<object>;
                if (items != null)
                    ArrangeItems(items);
            }
        }

        private void ArrangeItems(List<object> items)
        {
            for (int i = 0; i < ColCount; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(1, GridUnitType.Star);
                ButtonLineGrid.ColumnDefinitions.Add(col);                
            }

            for (int i = 0; i < items.Count; i++)
            {
                Button button = new Button();
                button.Style = ButtonStyle;
                button.Height = ButtonHeight;
                button.DataContext = items[i];
                if (ItemButtonClick != null)
                    button.Click += ItemButtonClick;

                Grid.SetColumn(button, i);
                ButtonLineGrid.Children.Add(button);
            }
        }
    }
}
