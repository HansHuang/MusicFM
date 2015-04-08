using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CustomControlResources
{
    /// <summary>
    /// Source From：http://blogs.msdn.com/b/pfxteam/archive/2011/01/15/10116210.aspx
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncProperty<T> : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged RaisePropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region AsyncValue (INotifyPropertyChanged Property)

        private T _asyncValue;

        public T AsyncValue
        {
            get
            {
                if (HasError) return _asyncValue;
                else if (lazy.Value.IsCompleted)
                    return lazy.Value.Result;
                else
                    lock (locker)
                        return _asyncValue;
            }
            set
            {
                if (_asyncValue != null && _asyncValue.Equals(value)) return;
                _asyncValue = value;
                RaisePropertyChanged("AsyncValue");
            }
        }
        #endregion

        #region Error (INotifyPropertyChanged Property)

        private string _error;

        public string Error
        {
            get { return _error; }
            set
            {
                if (_error != null && _error.Equals(value)) return;
                _error = value;
                RaisePropertyChanged("Error");
            }
        }
        #endregion

        #region HasError (INotifyPropertyChanged Property)

        private bool _hasError;

        public bool HasError
        {
            get { return _hasError; }
            set
            {
                if (_hasError.Equals(value)) return;
                _hasError = value;
                RaisePropertyChanged("HasError");
            }
        }
        #endregion

        #region HasValue (INotifyPropertyChanged Property)

        private bool _hasValue;

        public bool HasValue
        {
            get { return _hasValue; }
            set
            {
                if (_hasValue.Equals(value)) return;
                _hasValue = value;
                RaisePropertyChanged("HasValue");
            }
        }
        #endregion

        #region Fields

        readonly object locker = new object();
        Lazy<Task<T>> lazy;

        #endregion Fields


        #region Constructors

        public AsyncProperty(Func<T> valueFactory, T defVal = default(T))
        {
            if (valueFactory == null)
                throw new ArgumentNullException("valueFactory");

            _asyncValue = defVal;
            lazy = new Lazy<Task<T>>(() => AppendTask(Task.Run(valueFactory)));
        }

        public AsyncProperty(Func<Task<T>> taskFactory, T defVal = default(T))
        {
            if (taskFactory == null)
                throw new ArgumentNullException("taskFactory");

            _asyncValue = defVal;
            lazy = new Lazy<Task<T>>(() => AppendTask(Task.Run(taskFactory)));
        }

        #endregion Constructors

        public TaskAwaiter<T> GetAwaiter()
        {
            return lazy.Value.GetAwaiter();
        }

        private Task<T> AppendTask(Task<T> task)
        {
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    if (t.Exception is AggregateException)
                        Error = t.Exception.InnerExceptions[0].Message;
                    else
                        Error = t.Exception.Message;
                    HasValue = HasError = true;
                }
                else
                {
                    lock (locker)
                        AsyncValue = t.Result;
                    HasValue = true;
                }


            }, TaskScheduler.FromCurrentSynchronizationContext());
            return task;
        }

    }
}
