namespace IronFoundry.Ui.Controls.Mvvm
{
    using System.Windows;

    public class DropTargetBehavior
    {
        public static readonly DependencyProperty DropTargetProperty =
            DependencyProperty.RegisterAttached("DropTarget", typeof(IDropTarget), typeof(DropTargetBehavior),
                                                new PropertyMetadata(null, OnPropertyChanged));

        public static IDropTarget GetDropTarget(DependencyObject d)
        {
            return (IDropTarget)d.GetValue(DropTargetProperty);
        }

        public static void SetDropTarget(DependencyObject d, IDropTarget value)
        {
            d.SetValue(DropTargetProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (UIElement)d;

            if (e.NewValue != null)
            {
                element.DragOver += DragOver;
                element.Drop += Drop;
            }
            else
            {
                element.DragOver -= DragOver;
                element.Drop -= Drop;
            }
        }

        private static void Drop(object sender, DragEventArgs e)
        {
            var dropTarget = GetDropTarget((DependencyObject)sender);

            dropTarget.Drop(e.Data);
            e.Handled = true;
        }

        private static void DragOver(object sender, DragEventArgs e)
        {
            var dropTarget = GetDropTarget((DependencyObject)sender);

            e.Effects = dropTarget.GetDropEffects(e.Data);
            e.Handled = true;
        }
    }
}