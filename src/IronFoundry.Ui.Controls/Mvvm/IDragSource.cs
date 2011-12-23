namespace IronFoundry.Ui.Controls.Mvvm
{
    using System.Windows;

    public interface IDragSource
    {
        DragDropEffects GetDragEffects(object dataContext);
        object GetData(object dataContext);
    }
}