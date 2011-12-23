namespace IronFoundry.Ui.Controls.Mvvm
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    public class DragSourceBehavior
    {
        private static Point? _startPoint;

        public static readonly DependencyProperty DragSourceProperty =
            DependencyProperty.RegisterAttached("DragSource", typeof(IDragSource), typeof(DragSourceBehavior),
                                                new PropertyMetadata(null, OnPropertyChanged));


        public static IDragSource GetDragSource(DependencyObject dependencyObject)
        {
            return (IDragSource)dependencyObject.GetValue(DragSourceProperty);
        }

        public static void SetDragSource(DependencyObject dependencyObject, IDragSource value)
        {
            dependencyObject.SetValue(DragSourceProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var element = (UIElement)dependencyObject;

            if (e.NewValue != null)
            {
                element.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDown;
                element.PreviewMouseMove += PreviewMouseMove;
                element.MouseLeave += MouseLeave;
            }
            else
            {
                element.PreviewMouseLeftButtonDown -= PreviewMouseLeftButtonDown;
                element.PreviewMouseMove -= PreviewMouseMove;
                element.MouseLeave -= MouseLeave;
            }
        }

        private static void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private static void MouseLeave(object sender, MouseEventArgs e)
        {
            _startPoint = null;
        }

        private static void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _startPoint == null)
                return;

            if (!HasMouseMovedFarEnough(e))
                return;

            var dependencyObject = (FrameworkElement)sender;
            var dataContext = dependencyObject.GetValue(FrameworkElement.DataContextProperty);
            var dragSource = GetDragSource(dependencyObject);

            if (dragSource.GetDragEffects(dataContext) == DragDropEffects.None)
                return;

            DragDrop.DoDragDrop(dependencyObject,
                                dragSource.GetData(dataContext),
                                dragSource.GetDragEffects(dataContext));

        }

        private static bool HasMouseMovedFarEnough(MouseEventArgs e)
        {
            Vector delta = _startPoint.Value - e.GetPosition(null);

            return Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance;
        }
    }
}