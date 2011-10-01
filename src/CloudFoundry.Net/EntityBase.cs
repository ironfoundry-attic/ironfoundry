namespace CloudFoundry.Net
{
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.ComponentModel;

    [Serializable]
    public abstract class EntityBase : INotifyPropertyChanged
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override string ToString()
        {
            return ToJson();
        }

        public static T FromJson<T>(string argJson)
        {
            return JsonConvert.DeserializeObject<T>(argJson);
        }        

        #region INotifyPropertyChanged Implementation
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}