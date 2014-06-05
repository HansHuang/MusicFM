using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonHelperLibrary;
using CustomControlResources;
using MusicFmApplication.Helper;
using Service.Model;

namespace MusicFmApplication.ViewModel
{
    public class AccountManager : INotifyPropertyChanged
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
            Task.Run(() =>
                {
                    Thread.Sleep(300);
                    UserName = AccountInfo == null ? string.Empty : AccountInfo.Email;
                    Passwrod = string.Empty;
                });
        }

        #endregion

        #region RelayCommand LoginCmd

        private RelayCommand _loginCmd;

        public ICommand LoginCmd
        {
            get { return _loginCmd ?? (_loginCmd = new RelayCommand(LoginExcute)); }
        }

        public void LoginExcute(object type)
        {
            var accType = type is AccountType ? (AccountType)type : AccountType.DoubanFm;
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Passwrod))
            {
                Feedback = LocalTextHelper.GetLocText("UnamePwdCantEmpty");
                return;
            }
            Feedback = string.Empty;

            var loginTask = Task.Run(() => ViewModel.SongService.Login(UserName, Passwrod, accType));

            loginTask.GetAwaiter().OnCompleted(() =>
                {
                    var account = loginTask.Result;
                    ViewModel.MainWindow.Dispatcher.InvokeAsync(() =>
                        {
                            if (account == null)
                            {
                                Feedback = LocalTextHelper.GetLocText("UnamePwdMayWrong");
                                return;
                            }
                            AccountInfo = account;
                            UserName = account.UserName;
                            IsShowLoginBox = false;
                        });
                    //Write account info to local file
                    SettingHelper.SetSetting(CacheName, account.SerializeToString(), App.Name);
                });
        }

        #endregion

        #endregion

        protected MainViewModel ViewModel;
        protected const string CacheName = "Account";

        public AccountManager(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            IsShowLoginBox = false;

            TryGetAccount();
        }

        /// <summary>
        /// Try to get account info from config file
        /// </summary>
        private void TryGetAccount()
        {
            Task.Run(() =>
                {
                    var account = SettingHelper.GetSetting(CacheName, App.Name).Deserialize<Account>();
                    if (account == null) return;
                    ViewModel.MainWindow.Dispatcher.InvokeAsync(() =>
                    {
                        if ((account.Expire - DateTime.Now).Days < 3)
                        {
                            UserName = account.Email;
                            Passwrod = account.Password;
                            LoginCmd.Execute(account.AccountType);
                        }
                        else
                        {
                            AccountInfo = account;
                            UserName = AccountInfo.UserName;
                        }
                    });
                });
        }
    }
}