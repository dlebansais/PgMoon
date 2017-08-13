using PgMoon;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Selectors
{
    public class MoonPhaseSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            FrameworkElement parent = element;
            MainWindow DataContext = element.DataContext as MainWindow;

            for (;;)
            {
                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
                if (parent == null)
                    break;

                Canvas ParentPanel;
                if ((ParentPanel = parent as Canvas) != null)
                {
                    DataContext = ParentPanel.DataContext as MainWindow;
                    break;
                }
            }

            string TemplateName;
            if (DataContext != null && ((int)DataContext.PhaseCalculator.MoonPhase).ToString() == item as string)
                TemplateName = "SelectedStatueTemplate";
            else
                TemplateName = "UnselectedStatueTemplate";

            DataTemplate Result = element.TryFindResource(TemplateName) as DataTemplate;
            return Result;
        }
    }
}
