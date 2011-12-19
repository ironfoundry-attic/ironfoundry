namespace IronFoundry.VsExtension.Ui.Controls.Mvvm
{
    public class ProgressMessage 
    {
        public ProgressMessage(int value, string text)
        {
            this.Value = value;
            this.Text = text;
        }

        public int Value { get; private set; }
        public string Text { get; private set; }
    }
}