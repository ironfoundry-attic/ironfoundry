namespace IronFoundry.Ui.Controls.Mvvm
{
    public class ProgressError
    {
        public ProgressError(string text)
        {
            this.Text = text;
        }

        public string Text { get; private set; }
    }
}