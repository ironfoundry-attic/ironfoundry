using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Mvvm
{
    public interface IDragSource
    {
        DragDropEffects GetDragEffects(object dataContext);
        object GetData(object dataContext);
    }

    public interface IDropTarget
    {
        DragDropEffects GetDropEffects(IDataObject dataObject);
        void Drop(IDataObject dataObject);
    }

    public class DragSource<T> : IDragSource
    {
        private readonly Func<T, DragDropEffects> _getSupportedEffects;
        private readonly Func<T, object> _getData;

        public DragSource(Func<T, DragDropEffects> getSupportedEffects, Func<T, object> getData)
        {
            #region Argument checks
            if (getSupportedEffects == null)
                throw new ArgumentNullException("getSupportedEffects");

            if (getData == null)
                throw new ArgumentNullException("getData");
            #endregion

            _getSupportedEffects = getSupportedEffects;
            _getData = getData;
        }

        public DragDropEffects GetDragEffects(object dataContext)
        {
            return _getSupportedEffects((T)dataContext);
        }

        public object GetData(object dataContext)
        {
            return _getData((T)dataContext);
        }
    }

    public class DropTarget<T> : IDropTarget
    {
        private readonly Func<T, DragDropEffects> _getEffects;
        private readonly Action<T> _drop;

        public DropTarget(Func<T, DragDropEffects> getEffects, Action<T> drop)
        {
            #region Argument checks
            if (getEffects == null)
                throw new ArgumentNullException("getEffects");

            if (drop == null)
                throw new ArgumentNullException("drop");
            #endregion

            _getEffects = getEffects;
            _drop = drop;
        }

        public DragDropEffects GetDropEffects(IDataObject dataObject)
        {
            if (!dataObject.GetDataPresent(typeof(T)))
                return DragDropEffects.None;

            return _getEffects((T)dataObject.GetData(typeof(T)));
        }

        public void Drop(IDataObject dataObject)
        {
            _drop((T)dataObject.GetData(typeof(T)));
        }
    }

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
