using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelperLibrary.WEB;
using FlvPlayer;
using Microsoft.Practices.Prism.ViewModel;
using MvPlayer.Service;
using MvPlayer.Service.Model;

namespace MvPlayer
{
    public class MainViewModel : NotificationObject
    {
        #region IsWindowLoaded (INotifyPropertyChanged Property)

        private bool _isWindowLoaded;

        public bool IsWindowLoaded
        {
            get { return _isWindowLoaded; }
            set
            {
                if (_isWindowLoaded.Equals(value)) return;
                _isWindowLoaded = value;
                RaisePropertyChanged("IsWindowLoaded");
            }
        }

        #endregion

        #region PlayPara (INotifyPropertyChanged Property)

        private PlayerParameters _playPara;

        public PlayerParameters PlayPara
        {
            get { return _playPara; }
            set
            {
                if (_playPara != null && _playPara.Equals(value)) return;
                _playPara = value;
                RaisePropertyChanged("PlayPara");
            }
        }

        #endregion

        #region MvList (INotifyPropertyChanged Property)

        private ObservableCollection<MusicVideo> _mvList;
        public ObservableCollection<MusicVideo> MvList
        {
            get { return _mvList ?? (_mvList=new ObservableCollection<MusicVideo>()); }
            set
            {
                if (_mvList != null && _mvList.Equals(value)) return;
                _mvList = value;
                RaisePropertyChanged("MvList");
            }
        }
        #endregion

        private MainViewModel()
        {
            Task.Run(() =>
            {
                YinYueTai.GetIndexMvList(YinYueTai.IndexMvType.Premiere, YinYueTai.IndexMvArea.All)
                         .ForEach(s => MvList.Add(s));
                if (MvList.Count == 0) return;
                //Note: Important!!! Player must be called after window loaded. 
                if (IsWindowLoaded) 
                {
                    var mv = MvList[0];
                    PlayPara = new PlayerParameters
                    {
                        CaptureUrl = MvList[0].Image
                    };
                    mv.FlvUrl = mv.PlayPageUrl.FlvUrl();
                    PlayPara = new PlayerParameters(mv.FlvUrl);
                }
                //TODO :if not loaded, then...wait...
                //else
            });
        }

        public static readonly MainViewModel Instance = new MainViewModel();
    }
}
