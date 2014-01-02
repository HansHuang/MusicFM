using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
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

        public Dispatcher UiDispatcher { get; set; }

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

        private MainViewModel(Dispatcher dispatcher)
        {
            UiDispatcher = dispatcher;
            Task.Run((Action) GetMvList);
        }

        private static MainViewModel _instance;
        public static MainViewModel GetInstance(Dispatcher dispatcher)
        {
            return _instance ?? (_instance = new MainViewModel(dispatcher));
        }

        private void GetMvList()
        {
            var list = YinYueTai.GetIndexMvList(YinYueTai.IndexMvType.Premiere, YinYueTai.IndexMvArea.All);
            if (list.Count == 0 || UiDispatcher == null) return;
            UiDispatcher.BeginInvoke((Action) (() => list.ForEach(s => MvList.Add(s))));
            //Note: Important!!! Player must be called after window loaded. 
            if (IsWindowLoaded)
            {
                var mv = list[0];
                PlayPara = new PlayerParameters
                {
                    CaptureUrl = list[0].Image
                };
                mv.FlvUrl = mv.PlayPageUrl.FlvUrl();
                PlayPara = new PlayerParameters(mv.FlvUrl);
            }
            //TODO :if not loaded, then...wait...
            //else
        }

        
    }
}
