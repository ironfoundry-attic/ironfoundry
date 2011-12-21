namespace IronFoundry.Types
{
    using System;
    using System.ComponentModel;
    using Newtonsoft.Json;

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
            T rv = JsonConvert.DeserializeObject<T>(argJson);

            var message = rv as Message;
            if (null != message)
            {
                message.RawJson = argJson;
            }

            return rv;
        }        

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}