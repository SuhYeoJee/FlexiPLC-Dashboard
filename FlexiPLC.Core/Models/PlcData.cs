using System.Collections.Generic;
using System.ComponentModel;

namespace FlexiPLC.Core.Models
{
    public class PlcData : INotifyPropertyChanged
    {   
        // dictionary mapping -> name: value
        private Dictionary<string, object> _data = new Dictionary<string, object>();
        public event PropertyChangedEventHandler PropertyChanged; // for ui update

        public object this[string propertyName]
        {
            get
            {
                if (_data.ContainsKey(propertyName))
                {
                    return _data[propertyName];
                }
                return null;
            }
            set
            {
                if (!_data.ContainsKey(propertyName) || !Equals(_data[propertyName], value))
                {
                    _data[propertyName] = value;
                    OnPropertyChanged(propertyName);
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}