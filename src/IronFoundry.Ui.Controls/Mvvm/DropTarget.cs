namespace IronFoundry.Ui.Controls.Mvvm
{
    using System;
    using System.Windows;

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
}