using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonHelperLibrary.WEB;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Service.Model;

namespace MusicFmApplication
{
    public class AccountManager : NotificationObject
    {
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


        #region DelegateCommands
        public DelegateCommand ShowLoginBoxCommand { get; private set; }
        public void ShowLoginBoxExecute()
        {
            IsShowLoginBox = true;

            if (AccountInfo == null)
                Task.Run(() =>
                    {
                        Thread.Sleep(300);
                        UserName = string.Empty;
                        Passwrod = string.Empty;
                    });
        }

        public DelegateCommand LoginCommand { get; private set; }
        public void LoginExcute()
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Passwrod))
            {
                Feedback = LocalTextHelper.GetLocText("UnamePwdCantEmpty");
                return;
            }
            Feedback = string.Empty;
            Task.Run(() =>
                {
                    var json = HttpWebDealer.GetJsonObject("https://www.douban.com/j/app/login?email=" + UserName +
                                                           "&password=" + Passwrod +
                                                           "&app_name=radio_desktop_win&version=100");
                    if (json == null || json["err"] == null) return;
                    ViewModel.MainWindow.Dispatcher.BeginInvoke((Action) (() =>
                        {
                            if (!json["err"].Equals("ok"))
                            {
                                Feedback = LocalTextHelper.GetLocText("UnamePwdMayWrong");
                                return;
                            }
                            var expire = DateTime.Now.AddMilliseconds(Convert.ToInt64(json["expire"]));
                            AccountInfo = new Account
                                {
                                    Email = json["email"],
                                    Expire = expire,
                                    ExpireString = json["expire"],
                                    LoginTime = DateTime.Now,
                                    Password = Passwrod,
                                    R = json["r"].ToString(),
                                    Token = json["token"],
                                    UserId = json["user_id"].ToString(),
                                    UserName = json["user_name"]
                                };
                            UserName = AccountInfo.UserName;
                            IsShowLoginBox = false;
                        }));
                });
        }
        #endregion

        protected MainViewModel ViewModel;

        public AccountManager(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            IsShowLoginBox = false;

            ShowLoginBoxCommand = new DelegateCommand(ShowLoginBoxExecute);
            LoginCommand = new DelegateCommand(LoginExcute);
        }
    }
}
