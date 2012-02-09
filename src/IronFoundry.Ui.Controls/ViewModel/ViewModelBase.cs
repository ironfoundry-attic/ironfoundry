namespace IronFoundry.Ui.Controls.ViewModel
{
    public abstract class ViewModelBaseEx : GalaSoft.MvvmLight.ViewModelBase
    {
        protected void SetValue<T>(ref T originalValue, T newValue, string propertyName)
        {
            if (ValuesAreDifferent(originalValue, newValue))
            {
                originalValue = newValue;
                RaisePropertyChanged(propertyName);
            }
        }

        protected bool ValuesAreDifferent<T>(T originalValue, T newValue)
        {
            return (ReferenceEquals(originalValue, null) && false == ReferenceEquals(newValue, null)) ||
                   (false == ReferenceEquals(originalValue, null) && false == originalValue.Equals(newValue));
        }
    }
}