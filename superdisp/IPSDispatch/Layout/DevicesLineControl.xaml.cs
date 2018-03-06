using System.Collections.Generic;
using System.Windows.Controls;

namespace renstech.NET.SupernovaDispatcher.IPSDispatch.Layout
{
    /// <summary>
    /// Interaction logic for DevicesLineControl.xaml
    /// </summary>
    public partial class DevicesLineControl : UserControl
    {
        public DevicesLineControl()
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
        }
    }
}
