namespace IronFoundry.Ui.Controls.Mvvm
{
    using System;
    using System.Windows;

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
}