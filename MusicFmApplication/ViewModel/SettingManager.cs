using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using CommonHelperLibrary;
using CustomControlResources;
using MusicFmApplication.Model;

namespace MusicFmApplication.ViewModel
{
    public class SettingManager : ViewModelBase
    {
        #region Fields
        protected MainViewModel ViewModel;

        private const string DownloadFolderCacheName = "DownloadFolder";
        private const string BgTypeCacheName = "BackgroundType";
        private const string EnGlobleHotkeyCacheName = "EnableGlobleHotkey";
        private const string ChannelOfflieSizeName = "ChannelOfflieSize";
        #endregion

        #region NotifyProperties
        #region DownloadFolder (INotifyPropertyChanged Property)

        private string _downloadFolder;

        public string DownloadFolder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_downloadFolder))
                {
                    _downloadFolder = SettingHelper.GetSetting(DownloadFolderCacheName, App.Name);
                    if (string.IsNullOrWhiteSpace(_downloadFolder))
                        _downloadFolder = Environment.CurrentDirectory + "\\DownloadSongs\\";
                }
                return _downloadFolder;
            }
            set
            {
                if (_downloadFolder != null && _downloadFolder.Equals(value)) return;
                _downloadFolder = value;
                SettingHelper.SetSetting(DownloadFolderCacheName, _downloadFolder, App.Name);
                RaisePropertyChanged("DownloadFolder");
            }
        }

        #endregion

        #region CanAdjustSystemVolume (INotifyPropertyChanged Property)

        private bool? _canAdjustSystemVolume;

        public bool? CanAdjustSystemVolume
        {
            get { return _canAdjustSystemVolume; }
            set
            {
                if (_canAdjustSystemVolume.Equals(value)) return;
                _canAdjustSystemVolume = value;
                RaisePropertyChanged("CanAdjustSystemVolume");
            }
        }

        #endregion

        #region BackgroundType (INotifyPropertyChanged Property)

        private BackgroundType? _backgroundType;

        public BackgroundType? BackgroundType
        {
            get
            {
                if (_backgroundType == null)
                {
                    BackgroundType bgType;
                    Enum.TryParse(SettingHelper.GetSetting(BgTypeCacheName, App.Name), out bgType);
                    _backgroundType = bgType;
                }
                return _backgroundType;
            }
            set
            {
                if (_backgroundType != null && _backgroundType.Equals(value)) return;
                _backgroundType = value;
                SettingHelper.SetSetting(BgTypeCacheName, _backgroundType.ToString(), App.Name);
                RaisePropertyChanged("BackgroundType");
            }
        }

        #endregion

        #region EnableGlobleHotkey (INotifyPropertyChanged Property)

        private bool? _enableGlobleHotkey;

        public bool? EnableGlobleHotkey
        {
            get
            {
                if (_enableGlobleHotkey == null)
                {
                    bool enable;
                    _enableGlobleHotkey = !bool.TryParse(SettingHelper.GetSetting(EnGlobleHotkeyCacheName, App.Name), out enable) || enable;
                }
                return _enableGlobleHotkey;
            }
            set
            {
                if (_enableGlobleHotkey != null && _enableGlobleHotkey.Equals(value)) return;
                _enableGlobleHotkey = value;
                SettingHelper.SetSetting(EnGlobleHotkeyCacheName, _enableGlobleHotkey.ToString(), App.Name);
                RaisePropertyChanged("EnableGlobleHotkey");
            }
        }

        #endregion

        #region ChannelOfflineSize (INotifyPropertyChanged Property)

        private int? _channelOfflineSize;

        public int? ChannelOfflineSize
        {
            get
            {
                if (_channelOfflineSize == null)
                {
                    int size;
                    int.TryParse(SettingHelper.GetSetting(ChannelOfflieSizeName, App.Name), out size);
                    _channelOfflineSize = size < 1 ? 20 : size;
                }
                return _channelOfflineSize;
            }
            set
            {
                if (_channelOfflineSize != null && _channelOfflineSize.Equals(value)) return;
                _channelOfflineSize = value;
                SettingHelper.SetSetting(ChannelOfflieSizeName, value.ToString(), App.Name);
                RaisePropertyChanged("ChannelOfflineSize");
            }
        }

        #endregion
        #endregion

        #region RelayCommand ChangedDownloadFolderCmd

        private RelayCommand _changedDownloadFolderCmd;

        public ICommand ChangedDownloadFolderCmd
        {
            get { return _changedDownloadFolderCmd ?? (_changedDownloadFolderCmd = new RelayCommand(s => ChangedDownloadFolderExecute())); }
        }

        private void ChangedDownloadFolderExecute()
        {
            var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != DialogResult.OK) return;
            DownloadFolder = fbd.SelectedPath;
        }

        #endregion


        public SettingManager(MainViewModel viewModel)
        {
            ViewModel = viewModel;

        }

    }
}
