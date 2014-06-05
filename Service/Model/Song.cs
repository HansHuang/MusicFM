using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    [Serializable]
    public class Song : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged RaisePropertyChanged

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

        public int Sid { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string AlbumTitle { get; set; }
        public string AlbumId { get; set; }
        public string Company { get; set; }
        public string PublishTime { get; set; }
        public int Length { get; set; }
        public int Kbps { get; set; }
        public string Url { get; set; }
        public string LrcUrl { get; set; }

        #region Picture (INotifyPropertyChanged Property)

        private string _picture;

        public string Picture
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

        public string Thumb
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

        public int Like
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
    }
}
