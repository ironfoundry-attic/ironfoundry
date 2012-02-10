namespace IronFoundry.Ui.Controls.ViewModel
{
    public abstract class ViewModelBaseEx : GalaSoft.MvvmLight.ViewModelBase
    {
        protected bool SetValue<T>(ref T originalValue, T newValue, string propertyName)
        {
            bool changed = false;

            if (ValuesAreDifferent(originalValue, newValue))
            {
                originalValue = newValue;
                RaisePropertyChanged(propertyName);
                changed = true;
            }

            return changed;
        }

        protected bool ValuesAreDifferent<T>(T originalValue, T newValue)
        {
            return (ReferenceEquals(originalValue, null) && false == ReferenceEquals(newValue, null)) ||
                   (false == ReferenceEquals(originalValue, null) && false == originalValue.Equals(newValue));
        }
    }
}