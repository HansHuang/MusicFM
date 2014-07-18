using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelperLibrary;
using ServiceModel = Service.Model;

namespace MusicFm.Model
{
    [Serializable]
    public class Channel : ServiceModel.Channel, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged RaisePropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region IsOfflined (INotifyPropertyChanged Property)

        private bool _isOfflined;

        public bool IsOfflined
        {
            get { return _isOfflined; }
            set
            {
                if (_isOfflined.Equals(value)) return;
                _isOfflined = value;
                RaisePropertyChanged("IsOfflined");
            }
        }

        #endregion

        #region DownloadProgress (INotifyPropertyChanged Property)

        private int _downloadProgress;

        public int DownloadProgress
        {
            get { return _downloadProgress; }
            set
            {
                if (_downloadProgress.Equals(value) || _downloadProgress < 0 || _downloadProgress > 100) return;
                _downloadProgress = value;
                RaisePropertyChanged("DownloadProgress");
            }
        }
        #endregion

        public Channel() { }

        public Channel(ServiceModel.Channel source)
        {
            this.Copy(source);
        }

    }
}
