using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup.Localizer;
using CommonHelperLibrary;
using CustomControlResources;
using MusicFm.Helper;
using Service;
using Service.Model;

namespace MusicFm.ViewModel
{
    public class AccountManager :ViewModelBase
    {
        #region NotifyProperties
        #region IsShowLoginBox (INotifyPropertyChanged Property)

        private bool _isShowLoginBox;

        public bool IsShowLoginBox
        {
            get { return _isShowLoginBox; }
            set
            {
                if (_isShowLoginBox.Equals(value)) return;
                _isShowLoginBox = value;
                RaisePropertyChanged("IsShowLoginBox");
            }
        }

        #endregion

        #region AccountType (INotifyPropertyChanged Property)

        private AccountType _accountType;

        public AccountType AccountType
        {
            get { return _accountType; }
            set
            {
                if (_accountType.Equals(value)) return;
                _accountType = value;
                RaisePropertyChanged("AccountType");
            }
        }

        #endregion

        #region UserName (INotifyPropertyChanged Property)

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != null && _userName.Equals(value)) return;
                _userName = value;
                RaisePropertyChanged("UserName");
            }
        }

        #endregion

        #region Passwrod (INotifyPropertyChanged Property)

        private string _passwrod;

        public string Passwrod
        {
            get { return _passwrod; }
            set
            {
                if (_passwrod != null && _passwrod.Equals(value)) return;
                _passwrod = value;
                RaisePropertyChanged("Passwrod");
            }
        }

        #endregion

        #region AccountInfo (INotifyPropertyChanged Property)

        private Account _accountInfo;

        public Account AccountInfo
        {
            get { return _accountInfo; }
            set
            {
                if (_accountInfo != null && _accountInfo.Equals(value)) return;
                _accountInfo = value;
                RaisePropertyChanged("AccountInfo");
            }
        }
        #endregion
        
        #region Feedback (INotifyPropertyChanged Property)

        private string _feedback;

        public string Feedback
        {
            get { return _feedback; }
            set
            {
                if (_feedback != null && _feedback.Equals(value)) return;
                _feedback = value;
                RaisePropertyChanged("Feedback");
            }
        }

        #endregion 
        #endregion

        #region DelegateCommands

        #region RelayCommand ShowLoginBoxCmd

        private RelayCommand _showLoginBoxCmd;

        public ICommand ShowLoginBoxCmd
        {
            get { return _showLoginBoxCmd ?? (_showLoginBoxCmd = new RelayCommand(param => ShowLoginBoxExecute())); }
        }

        public void ShowLoginBoxExecute()
        {
            IsShowLoginBox = true;
        }

        #endregion

        #region RelayCommand HideLoginBoxCmd

        private RelayCommand _hideLoginBoxCmd;

        public ICommand HideLoginBoxCmd
        {
            get { return _hideLoginBoxCmd ?? (_hideLoginBoxCmd = new RelayCommand(s => HideLoginBoxExecute())); }
        }

        private void HideLoginBoxExecute()
        {
            IsShowLoginBox = false;
            Feedback = string.Empty;

            if (AccountInfo != null && UserName != AccountInfo.UserName)
                UserName = AccountInfo.UserName;
        }

        #endregion

        #region RelayCommand LoginCmd

        private RelayCommand _loginCmd;

        public ICommand LoginCmd
        {
            get { return _loginCmd ?? (_loginCmd = new RelayCommand(LoginExcute)); }
        }

        public async void LoginExcute(object para)
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Passwrod))
            {
                Feedback = LocalTextHelper.GetLocText("UnamePwdCantEmpty");
                return;
            }

            var login = Task.Run(() => ViewModel.SongService.Login(UserName, Passwrod, AccountType));
            await login;
            if (login.Result == null)
            {
                Feedback = LocalTextHelper.GetLocText("UnamePwdMayWrong");
                return;
            }
            Feedback = string.Empty;

            AccountInfo = login.Result;
            UserName = login.Result.UserName;
            IsShowLoginBox = false;
            UpdateAccountDic();
        }

        #endregion

        #endregion

        #region Fields
        protected MainViewModel ViewModel;
        protected const string CacheName = "AccountDic";
        //<Name of song service, Account info>
        protected Dictionary<string, Account> AccountDic; 
        #endregion

        public AccountManager(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            IsShowLoginBox = false;

            viewModel.SongServiceChanged += OnSongServiceChanged;
        }

        /// <summary>
        /// Try to get account info from config file
        /// </summary>
        public async void TryGetAccount()
        {
            AccountDic = SettingHelper.GetSetting(CacheName, App.Name).Deserialize<Dictionary<string, Account>>();
            if (AccountDic == null)
            {
                AccountDic = new Dictionary<string, Account>();
                return;
            }
            foreach (var pair in new Dictionary<string, Account>(AccountDic))
            {
                var name = pair.Key;
                var service = ViewModel.AvalibleSongServices.FirstOrDefault(s => s.Name == name);
                var account = pair.Value;
                if (service == null || account == null) continue;
                var needRefresh = account.Expire.HasValue &&
                                  (account.Expire.GetValueOrDefault() - DateTime.Now).Days < 3;
                if (service == ViewModel.SongService)
                {
                    if (needRefresh)
                    {
                        var login = Task.Run(() => service.Login(account.Email, account.Password, account.AccountType));
                        await login;
                        AccountInfo = login.Result;
                        UpdateAccountDic();
                    }
                    else
                    {
                        AccountInfo = account;
                        UserName = account.UserName;
                    }

                }
                else if (needRefresh)
                {
                    Task.Run(() => service.Login(account.Email, account.Password, account.AccountType))
                        .ContinueWith(t =>
                        {
                            AccountDic[name] = t.Result;
                            UpdateAccountDic();
                        }, new CancellationToken(), TaskContinuationOptions.None, ViewModel.ContextTaskScheduler);
                }
            }
        }

        #region Processors
        private void UpdateAccountDic()
        {
            var name = ViewModel.SongService.Name;

            if (AccountDic.ContainsKey(name)) AccountDic[name] = AccountInfo;
            else AccountDic.Add(name, AccountInfo);
            Task.Run(() => SettingHelper.SetSetting(CacheName, AccountDic.SerializeToString(), App.Name));
        }

        private void OnSongServiceChanged()
        {
            if (ViewModel.SongService == null || AccountDic == null) return;
            var name = ViewModel.SongService.Name;
            AccountInfo = AccountDic.ContainsKey(name) ? AccountDic[name] : null;

            if (AccountInfo == null)
            {
                AccountType = ViewModel.SongService.AvaliableAccountTypes[0];
                UserName = string.Empty;
            }
            else
            {
                AccountType = AccountInfo.AccountType;
                UserName = AccountInfo.Email;
            }
            Passwrod = string.Empty;
        } 
        #endregion

    }
}