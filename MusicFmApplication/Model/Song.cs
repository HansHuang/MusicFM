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
    public class Song : ServiceModel.Song, INotifyPropertyChanged
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

        #region Picture (INotifyPropertyChanged Property)

        private string _picture;

        public new string Picture
        {
            get { return _picture; }
            set
            {
                if (_picture != null && _picture.Equals(value)) return;
                _picture = value;
                RaisePropertyChanged("Picture");
            }
        }

        #endregion

        #region Thumb (INotifyPropertyChanged Property)

        private string _thumb;

        public new string Thumb
        {
            get { return _thumb; }
            set
            {
                if (_thumb != null && _thumb.Equals(value)) return;
                _thumb = value;
                RaisePropertyChanged("Thumb");
            }
        }

        #endregion

        #region Like (INotifyPropertyChanged Property)

        private int _like;

        public new int Like
        {
            get { return _like; }
            set
            {
                if (_like.Equals(value)) return;
                _like = value;
                RaisePropertyChanged("Like");
            }
        }

        #endregion

        public Song(){}

        public Song(ServiceModel.Song source)
        {
            this.Copy(source);
        }

    }
}
