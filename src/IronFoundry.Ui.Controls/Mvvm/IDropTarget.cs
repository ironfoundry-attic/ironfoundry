namespace IronFoundry.Ui.Controls.Mvvm
{
    using System.Windows;

    public interface IDropTarget
    {
        DragDropEffects GetDropEffects(IDataObject dataObject);
        void Drop(IDataObject dataObject);
    }
}