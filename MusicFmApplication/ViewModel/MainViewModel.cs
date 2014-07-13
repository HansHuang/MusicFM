using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommonHelperLibrary;
using CommonHelperLibrary.WEB;
using CustomControlResources;
using MusicFmApplication.Helper;
using MusicFmApplication.Model;
using Service;
using ServiceModel = Service.Model;

namespace MusicFmApplication.ViewModel
{
    /// <summary>
    /// Author : Hans Huang
    /// Date : 2013-10-19
    /// Class : MainViewModel
    /// Discription : ViewModel of MainWindow
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private static MainViewModel _instance;

        public MainWindow MainWindow { get; private set; }

        private const string SongListCacheName = "SongList";
        private const string SongListExpireCacheName = "SongListExpire";
        private const string SelectedSongServiceCacheName = "SelectedSongService";
        private const string SelectedChannelCacheName = "SelectedChannel";

        protected const int SearchOffset = 20;

        [ImportMany(typeof(ISongService))]
        public ObservableCollection<ISongService> AvalibleSongServices { get; set; }

        protected Dictionary<ISongService, List<Channel>> ServiceChannelsCache = new Dictionary<ISongService, List<Channel>>();

        public readonly TaskScheduler ContextTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        #endregion

        #region Events

        public delegate void SongServiceChangeHandle();

        public SongServiceChangeHandle SongServiceChanged;

        private void OnSongServiceChanged()
        {
            if (SongServiceChanged == null) return;
            if (CurrentChannel == null) SongServiceChanged();
            else
                Task.Factory.StartNew(() => SongServiceChanged(), new CancellationToken(), TaskCreationOptions.None,
                                      ContextTaskScheduler);
        }

        #endregion

        #region Notify Properties

        #region MediaManager (INotifyPropertyChanged Property)

        private MediaManager _mediaManager;

        public MediaManager MediaManager
        {
            get { return _mediaManager; }
            private set
            {
                if (_mediaManager != null && _mediaManager.Equals(value)) return;
                _mediaManager = value;
                RaisePropertyChanged("MediaManager");
            }
        }
        #endregion

        #region Account (INotifyPropertyChanged Property)

        private AccountManager _account;

        public AccountManager Account
        {
            get { return _account; }
            private set
            {
                if (_account != null && _account.Equals(value)) return;
                _account = value;
                RaisePropertyChanged("Account");
            }
        }

        #endregion

        #region WeatherMgr (INotifyPropertyChanged Property)

        private WeatherManager _weatherMgr;

        public WeatherManager WeatherMgr
        {
            get { return _weatherMgr; }
            private set
            {
                if (_weatherMgr != null && _weatherMgr.Equals(value)) return;
                _weatherMgr = value;
                RaisePropertyChanged("WeatherMgr");
            }
        }

        #endregion

        #region Setting (INotifyPropertyChanged Property)

        private SettingManager _setting;

        public SettingManager Setting
        {
            get { return _setting; }
            set
            {
                if (_setting != null && _setting.Equals(value)) return;
                _setting = value;
                RaisePropertyChanged("Setting");
            }
        }

        #endregion

        #region OfflineMgt (INotifyPropertyChanged Property)

        private OfflineManagement _offlineMgt;

        public OfflineManagement OfflineMgt
        {
            get { return _offlineMgt; }
            set
            {
                if (_offlineMgt != null && _offlineMgt.Equals(value)) return;
                _offlineMgt = value;
                RaisePropertyChanged("OfflineMgt");
            }
        }
        #endregion

        #region IsShowPlayerDetail (INotifyPropertyChanged Property)

        private bool _isShowPlayerDetail;
        public bool IsShowPlayerDetail
        {
            get { return _isShowPlayerDetail; }
            set
            {
                if (_isShowPlayerDetail.Equals(value)) return;
                _isShowPlayerDetail = value;
                RaisePropertyChanged("IsShowPlayerDetail");
            }
        }
        #endregion

        #region SongService (INotifyPropertyChanged Property)

        private ISongService _songService;

        public ISongService SongService
        {
            get { return _songService; }
            set
            {
                if (_songService != null && _songService.Equals(value)) return;
                _songService = value;
                RaisePropertyChanged("SongService");

                OnSongServiceChanged();
            }
        }
        #endregion

        #region HistorySongList (INotifyPropertyChanged Property)

        private ObservableCollection<Song> _historySongList;

        public ObservableCollection<Song> HistorySongList
        {
            get { return _historySongList ?? (_historySongList = new ObservableCollection<Song>()); }
            set
            {
                if (_historySongList != null && _historySongList.Equals(value)) return;
                _historySongList = value;
                RaisePropertyChanged("HistorySongList");
            }
        }
        #endregion

        #region SongList (INotifyPropertyChanged Property)

        private ObservableCollection<Song> _songList;

        public ObservableCollection<Song> SongList
        {
            get { return _songList ?? (_songList = new ObservableCollection<Song>()); }
            set
            {
                if (_songList != null && _songList.Equals(value)) return;
                _songList = value;
                RaisePropertyChanged("SongList");
            }
        }

        #endregion

        #region CurrentSong (INotifyPropertyChanged Property)

        private Song _currentSong;

        public Song CurrentSong
        {
            get { return _currentSong; }
            set
            {
                if (_currentSong != null && _currentSong.Equals(value)) return;
                _currentSong = value;
                RaisePropertyChanged("CurrentSong");
            }
        }
        #endregion

        #region IsDisylayLyric (INotifyPropertyChanged Property)

        private bool _isDisylayLyric;
        public bool IsDisylayLyric
        {
            get { return _isDisylayLyric; }
            set
            {
                if (_isDisylayLyric.Equals(value)) return;
                _isDisylayLyric = value;
                RaisePropertyChanged("IsDisylayLyric");
            }
        }
        #endregion

        #region Channels (INotifyPropertyChanged Property)

        private ObservableCollection<Channel> _channels;

        public ObservableCollection<Channel> Channels
        {
            get { return _channels ?? (_channels = new ObservableCollection<Channel>()); }
            set
            {
                if (_channels != null && _channels.Equals(value)) return;
                _channels = value;
                RaisePropertyChanged("Channels");
            }
        }
        #endregion

        #region CurrentChannel (INotifyPropertyChanged Property)

        private Channel _currentChannel;

        public Channel CurrentChannel
        {
            get { return _currentChannel; }
            set
            {
                if (_currentChannel != null && _currentChannel.Equals(value)) return;
                _currentChannel = value;
                RaisePropertyChanged("CurrentChannel");
            }
        }
        #endregion

        #region IsDownlading (INotifyPropertyChanged Property)

        private bool _isDownlading;

        public bool IsDownlading
        {
            get { return _isDownlading; }
            set
            {
                if (_isDownlading.Equals(value)) return;
                _isDownlading = value;
                RaisePropertyChanged("IsDownlading");
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
                if (_downloadProgress.Equals(value)) return;
                _downloadProgress = value;
                RaisePropertyChanged("DownloadProgress");
            }
        }

        #endregion

        #region SearchResult (INotifyPropertyChanged Property)

        private SearchResult _searchResult;

        public SearchResult SearchResult
        {
            get { return _searchResult; }
            set
            {
                if (_searchResult != null && _searchResult.Equals(value)) return;
                _searchResult = value;
                RaisePropertyChanged("SearchResult");
            }
        }

        #endregion

        #region CurrentArtist (INotifyPropertyChanged Property)

        private ServiceModel.Artist _currentArtist;

        public ServiceModel.Artist CurrentArtist
        {
            get { return _currentArtist; }
            set
            {
                if (_currentArtist != null && _currentArtist.Equals(value)) return;
                _currentArtist = value;
                RaisePropertyChanged("CurrentArtist");
            }
        }

        #endregion

        #endregion

        #region Delegate Commands

        #region RelayCommand NextSongCmd

        private RelayCommand _nextSongCmd;

        public ICommand NextSongCmd
        {
            get
            {
                return _nextSongCmd ?? (_nextSongCmd = new RelayCommand(s =>
                {
                    if (OfflineMgt.IsInternetConnected) NextSongExecute(s as bool?);
                    else OfflineMgt.NextSongExecute(s as bool?);
                }));
            }
        }

        private void NextSongExecute(bool? isEnded = false)
        {
            IsDownlading = false;
            DownloadProgress = 0;
            //If song is ended, add it to history(Display inverted order)
            if (isEnded.GetValueOrDefault())
            {
                HistorySongList.Insert(0, CurrentSong);
                if (Account.AccountInfo != null)
                {
                    var para = new ServiceModel.SongActionParameter(Account.AccountInfo)
                        .CurrentSongID(CurrentSong)
                        .CurrentChennal(CurrentChannel)
                        .PositionSeconds(CurrentSong.Length);
                    Task.Run(() => SongService.CompletedSong(para));
                }
            }

            Action<List<Song>> action = songs =>
                {
                    if (songs != null) songs.ForEach(s => SongList.Add(s));
                    if (SongList.Count < 1) return;
                    //Set current song & playing 
                    CurrentSong = SongList[0];
                    //Play current new song with new url
                    MediaManager.StartPlayerCmd.Execute(null);
                    //RaisePropertyChanged("MediaManager");
                    SongList.RemoveAt(0);
                };
            //Can only play after get song list
            if (SongList.Count < 1) GetSongList(action);
            //Directlly play next & get song list in another thread when list count less then elements
            else if (SongList.Count < 3)
            {
                //Task to get song list
                GetSongList(songs => songs.ForEach(s => SongList.Add(s)));
                //Play
                action(null);
            }
            //Directlly play next
            else action(null);
        }

        #endregion

        #region RelayCommand LikeSongCmd

        private RelayCommand _likeSongCmd;

        public ICommand LikeSongCmd
        {
            get
            {
                return _likeSongCmd ?? (_likeSongCmd = new RelayCommand(s =>
                {
                    if (OfflineMgt.IsInternetConnected) LikeSongExecute(s as string);
                    else OfflineMgt.LikeSongExecute((s as string));
                }));
            }
        }

        private void LikeSongExecute(string isHate)
        {
            ServiceModel.OperationType actionType;
            var ishate = !string.IsNullOrWhiteSpace(isHate);
            if (ishate)
            {
                actionType = ServiceModel.OperationType.Hate;
                NextSongCmd.Execute(false);
            }
            else
                actionType = CurrentSong.Like == 0 ? ServiceModel.OperationType.Like : ServiceModel.OperationType.DisLike;

            MediaManager.IsBuffering = true;
            Action<List<Song>> action = songs =>
            {
                MediaManager.IsBuffering = false;
                if (songs == null || songs.Count < 1) return;
                SongList.Clear();
                if (!ishate)
                    CurrentSong.Like = CurrentSong.Like == 0 ? 1 : 0;
                songs.ForEach(s => SongList.Add(s));
            };
            GetSongList(action, actionType);
        }
        #endregion

        #region RelayCommand ToggleLyricDisplayCmd

        private RelayCommand _toggleLyricDisplayCmd;

        public ICommand ToggleLyricDisplayCmd
        {
            get { return _toggleLyricDisplayCmd ?? (_toggleLyricDisplayCmd = new RelayCommand(s => ToggleLyricDisplayExecute())); }
        }

        private void ToggleLyricDisplayExecute()
        {
            IsDisylayLyric = !IsDisylayLyric;
        }
        #endregion

        #region RelayCommand SetChannelCmd

        private RelayCommand _setChannelCmd;

        public ICommand SetChannelCmd
        {
            get { return _setChannelCmd ?? (_setChannelCmd = new RelayCommand(s => SetChannelExecute(s as Channel))); }
        }

        private void SetChannelExecute(Channel channel)
        {
            if (channel == null) return;

            if (!Channels.Contains(channel)) Channels.Insert(0, channel);
            CurrentChannel = channel;
            CurrentArtist = null;
            SongList.Clear();
            NextSongCmd.Execute(false);
            IsShowPlayerDetail = false;
            Task.Run(() => SettingHelper.SetSetting(SelectedChannelCacheName, channel.Id.ToString(), App.Name));
        }
        #endregion

        #region RelayCommand DownloadSongCmd

        private RelayCommand _downloadSongCmd;

        public ICommand DownloadSongCmd
        {
            get { return _downloadSongCmd ?? (_downloadSongCmd = new RelayCommand(param => DownloadSongExetute())); }
        }

        private void DownloadSongExetute()
        {
            IsDownlading = true;
            DownloadProgress = 0;
            var name = CurrentSong.Artist + "-" + CurrentSong.Title + ".mp3";

            var list = new List<string>(MediaManager.Lyric.Mp3Urls) { CurrentSong.Url };
            HttpWebDealer.DownloadLargestFile(name, list, Setting.DownloadFolder, DownloadMonitor);
        }

        private void DownloadMonitor(object webClient, DownloadProgressChangedEventArgs e)
        {
            DownloadProgress = e.ProgressPercentage;
            if (DownloadProgress == 100) IsDownlading = false;
        }
        #endregion

        #region RelayCommand OpenDownloadFolderCmd

        private RelayCommand _openDownloadFolderCmd;

        public ICommand OpenDownloadFolderCmd
        {
            get { return _openDownloadFolderCmd ?? (_openDownloadFolderCmd = new RelayCommand(s => OpenDownloadFolderExecute())); }
        }

        private void OpenDownloadFolderExecute()
        {
            Task.Run(() =>
            {
                var folder = Setting.DownloadFolder;
                var name = CurrentSong.Artist + "-" + CurrentSong.Title + ".mp3";
                var path = folder.EndsWith(@"\") ? folder + name : folder + "\\" + name;
                Process.Start("Explorer.exe", "/select," + path);
            });
        }
        #endregion

        #region RelayCommand OpenSettingWindowCmd

        private RelayCommand _openSettingWindowCmd;

        public ICommand OpenSettingWindowCmd
        {
            get { return _openSettingWindowCmd ?? (_openSettingWindowCmd = new RelayCommand(s => OpenSettingWindowExecure())); }
        }

        private void OpenSettingWindowExecure()
        {
            if (SettingWindow.IsOpened) return;
            var wd = new SettingWindow(this) { Owner = MainWindow };
            wd.Show();
        }
        #endregion

        #region RelayCommand ToggleDesktopLyricCmd

        private RelayCommand _toggleDesktopLyricCmd;

        public ICommand ToggleDesktopLyricCmd
        {
            get { return _toggleDesktopLyricCmd ?? (_toggleDesktopLyricCmd = new RelayCommand(s => ToggleDesktopLyricExecute())); }
        }

        private void ToggleDesktopLyricExecute()
        {
            if (DesktopLyric.IsOpened)
            {
                var ownedWds = MainWindow.OwnedWindows;
                for (var i = 0; i < ownedWds.Count; i++)
                {
                    var wd = ownedWds[i];
                    if (!(wd is DesktopLyric)) continue;
                    wd.Close();
                    return;
                }
            }
            else
            {
                var wd = new DesktopLyric(this) { Owner = MainWindow };
                wd.Show();
            }
        }
        #endregion

        #region RelayCommand ChangeSongServiceCmd

        //private RelayCommand _changeSongServiceCmd;

        //public ICommand ChangeSongServiceCmd
        //{
        //    get { return _changeSongServiceCmd ?? (_changeSongServiceCmd = new RelayCommand(s => ChangeSongService(s as ISongService))); }
        //}

        //private void ChangeSongService(ISongService service)
        //{
        //    if (service == null) return;
        //    //SongService = (ISongService)Activator.CreateInstance(type);
        //    SongService = service;
        //    _channelCahe = CurrentChannel;
        //    //Clear search result (search result only work in specified service)
        //    SearchResult = null;
        //    //Re-set Current channel
        //    CurrentChannel = null;
        //    GetChannels();
        //    Task.Run(() => SettingHelper.SetSetting(SelectedSongServiceCacheName, service.Name, App.Name));
        //}

        #endregion

        #region RelayCommand SearchCmd

        private RelayCommand _searchCmd;

        public ICommand SearchCmd
        {
            get { return _searchCmd ?? (_searchCmd = new RelayCommand(SearchExecute)); }
        }

        private async void SearchExecute(object para)
        {
            var keyword = string.Empty;
            if (para is TextBox)
                keyword = ((TextBox)para).Text;
            else if (para is string)
                keyword = (string)para;
            if (string.IsNullOrWhiteSpace(keyword)) return;

            MediaManager.IsBuffering = true;
            SearchResult = null;
            var search = Task.Run(() => SongService.Search(keyword, SearchOffset));
            await search;
            SearchResult = new SearchResult(search.Result);
            MediaManager.IsBuffering = false;
        }

        #endregion

        #region RelayCommand AddSongCmd

        private RelayCommand _addSongCmd;

        public ICommand AddSongCmd
        {
            get { return _addSongCmd ?? (_addSongCmd = new RelayCommand(s => AddSongExecute(s as Song))); }
        }

        private void AddSongExecute(Song song)
        {
            if (song == null || SongList == null) return;
            SongList.Insert(0, song);
            NextSongCmd.Execute(false);
            IsShowPlayerDetail = false;
        }

        #endregion

        #region RelayCommand LoadMoreSearchResultCmd

        private RelayCommand _loadMoreSearchResultCmd;

        public ICommand LoadMoreSearchResultCmd
        {
            get { return _loadMoreSearchResultCmd ?? (_loadMoreSearchResultCmd = new RelayCommand(s => LoadMoreSearchResultExeture())); }
        }

        private async void LoadMoreSearchResultExeture()
        {
            if (SearchResult == null) return;

            MediaManager.IsBuffering = true;
            var count = SearchOffset + SearchResult.CurrentNr;
            var search = Task.Run(() => SongService.Search(SearchResult.Query, count));
            await search;
            SearchResult = new SearchResult(search.Result);
            MediaManager.IsBuffering = false;
        }

        #endregion

        #region RelayCommand PlayArtistCmd

        private RelayCommand _playArtistCmd;

        public ICommand PlayArtistCmd
        {
            get { return _playArtistCmd ?? (_playArtistCmd = new RelayCommand(s => PlayArtistExecute(s as ServiceModel.Artist))); }
        }

        private async void PlayArtistExecute(ServiceModel.Artist artist)
        {
            if (artist == null || string.IsNullOrWhiteSpace(artist.Name)) return;
            switch (SongService.Name)
            {
                case "DoubanFm":
                    break;
                case "BaiduMusic":
                    var tryGetSong = Task.Run(() => SongService.GetSongList(artist));
                    await tryGetSong;
                    if (tryGetSong.Result == null || tryGetSong.Result.Count == 0) return;
                    CurrentChannel = null;
                    SearchResult = null;
                    CurrentArtist = artist;
                    SongList.Clear();
                    SongList = new ObservableCollection<Song>(tryGetSong.Result.Select(s => new Song(s)));
                    NextSongCmd.Execute(false);
                    break;
            }
            IsShowPlayerDetail = false;
        }

        #endregion

        #endregion

        #region Construct Method
        /// <summary>
        /// Please call the GetInstance method
        /// </summary>
        /// <param name="window"></param>
        private MainViewModel(MainWindow window)
        {
            MainWindow = window;
            ComposeSongService();
            SongServiceChanged += HandleSongServiceChangd;

            MediaManager = new MediaManager(this);
            Account = new AccountManager(this);
            WeatherMgr = new WeatherManager(this);
            Setting = new SettingManager(this);
            OfflineMgt = new OfflineManagement(this);

            Task.Factory.StartNew(() =>
            {
                if (OfflineMgt.IsInternetConnected) StartPlayer();
                else OfflineMgt.StartOfflinePlayer();
            }, new CancellationToken(), TaskCreationOptions.None, ContextTaskScheduler);
        }

        public static MainViewModel GetInstance(MainWindow window = null)
        {
            return _instance ?? (_instance = new MainViewModel(window));
        }
        #endregion

        #region Processors

        private void StartPlayer()
        {
            //Selecte Song Service
            if (AvalibleSongServices == null || AvalibleSongServices.Count < 1) return;
            var serviceName = SettingHelper.GetSetting(SelectedSongServiceCacheName, App.Name);

            SongService = AvalibleSongServices.FirstOrDefault(s => s.Name == serviceName) ??
                          AvalibleSongServices.First(s => s is DoubanFm);
            //Try get account for song service
            Account.TryGetAccount();

            //GetChannels();
            var songListExpired = SettingHelper.GetSetting(SongListExpireCacheName, App.Name).Deserialize<DateTime>();
            var songList = songListExpired < DateTime.Now
                               ? new List<Song>()
                               : SettingHelper.GetSetting(SongListCacheName, App.Name).Deserialize<List<Song>>();
            if (songList != null && songList.Count > 0)
            {
                songList.ForEach(s => SongList.Add(s));
                NextSongCmd.Execute(false);
            }
            else if (CurrentChannel != null)
                SetChannelCmd.Execute(CurrentChannel);
        }

        private void HandleSongServiceChangd()
        {
            //Clear search result (search result only work in specified service)
            SearchResult = null;
            if (ServiceChannelsCache.ContainsKey(SongService))
                Channels = new ObservableCollection<Channel>(ServiceChannelsCache[SongService]);
            else
                GetChannels();

            Task.Run(() => SettingHelper.SetSetting(SelectedSongServiceCacheName, SongService.Name, App.Name));
        }

        private void GetChannels()
        {
            var basicChannels = SongService.GetChannels();
            Channels.Clear();
            basicChannels.ForEach(s => Channels.Add(new Channel(s)));

            if (CurrentChannel == null)
            {
                int selectedCId;
                var selected = SettingHelper.GetSetting(SelectedChannelCacheName, App.Name);
                int.TryParse(selected, out selectedCId);
                CurrentChannel = Channels.FirstOrDefault(s => s.Id == selectedCId) ?? Channels.First();
            }
            else
                CurrentChannel = Channels.FirstOrDefault(s => s.Same(CurrentChannel));


            Task.Run(() => 
            {
                var allChannels = SongService.GetChannels(false).Select(s => new Channel(s)).ToList();
                //check if already offlined
                if (Directory.Exists(OfflineMgt.OfflineFolder))
                {
                    var dirList = Directory.GetDirectories(OfflineMgt.OfflineFolder);
                    allChannels.ForEach(s =>
                    {
                        var name = string.Format("{0}({1})", s.StrId, s.Id);
                        s.IsOfflined = dirList.Any(d => d.EndsWith(name));
                        s.DownloadProgress = 100;
                    });
                }

                if (!ServiceChannelsCache.ContainsKey(SongService))
                    ServiceChannelsCache.Add(SongService, allChannels);
                else
                    ServiceChannelsCache[SongService] = allChannels;

                MainWindow.Dispatcher.InvokeAsync(() =>
                {
                    Channels.Clear();
                    Channels=new ObservableCollection<Channel>(allChannels);
                    var crtChannel = Channels.FirstOrDefault(s => s.Same(CurrentChannel));
                    if (crtChannel != null) CurrentChannel = crtChannel;
                });
            });
        }

        /// <summary>
        /// Get Song List
        /// </summary>
        /// <param name="callBack">Call Back Function(para is song list get from server)</param>
        /// <param name="actionType">OperationType (Played is default)</param>
        private void GetSongList(Action<List<Song>> callBack = null, ServiceModel.OperationType actionType = ServiceModel.OperationType.Played)
        {
            Task.Run(() =>
            {
                MediaManager.IsBuffering = true;
                //Get existed songs id list
                var exitingIds = SongList.Select(s => s.Sid);
                //Generate gain song parameter
                List<Song> songs;
                if (CurrentChannel != null)
                {
                    songs = GetSongListByChannel(CurrentChannel, actionType);
                }
                else if (CurrentArtist != null)
                    songs = SongService.GetSongList(CurrentArtist).Select(s => new Song(s)).ToList();
                else
                    songs = new List<Song>();
                songs = songs.Where(s => !exitingIds.Contains(s.Sid)).ToList();

                //Notify song list back to the main thread
                MainWindow.Dispatcher.InvokeAsync(() => { if (callBack != null) callBack(songs); });
                SettingHelper.SetSetting(SongListCacheName, songs.SerializeToString(), App.Name);
                //The url of song maybe expired in 2 hours
                SettingHelper.SetSetting(SongListExpireCacheName, DateTime.Now.AddHours(2).SerializeToString(), App.Name);
                MediaManager.IsBuffering = false;
            });
        }

        /// <summary>
        /// Get Song List By Channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public List<Song> GetSongListByChannel(Channel channel, ServiceModel.OperationType actionType = ServiceModel.OperationType.Played)
        {
            var history = HistorySongList.Cast<ServiceModel.Song>().ToList();
            history.Add(CurrentSong);
            var para = new ServiceModel.GainSongParameter(Account.AccountInfo)
                .HistoryString(history, actionType)
                .PositionSeconds((int)MediaManager.Position.TotalSeconds)
                .CurrentSongID(CurrentSong)
                .CurrentChennal(channel);
            return SongService.GetSongList((ServiceModel.GainSongParameter)para).Select(s => new Song(s)).ToList();
        }

        private void ComposeSongService()
        {
            var catalog = new AssemblyCatalog((typeof(ISongService).Assembly));
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);

            //Set Local Name
            foreach (var servics in AvalibleSongServices)
                servics.LocalizeName = LocalTextHelper.GetLocText(servics.Name);
        }
        #endregion
    }
}