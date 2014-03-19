using System;
using System.Diagnostics;
using System.Windows.Input;

namespace CustomControlResources
{
    /* RoutedCommand vs RelayCommand
    * The key difference is that RoutedCommand is an ICommand implementation that uses a RoutedEvent
    * to route through the tree until a CommandBinding for the command is found, while RelayCommand
    * does no routing and instead directly executes some delegate. In a M-V-VM scenario a RelayCommand
    * is probably the better choice all around.
    * 
    * RoutedCommand is bubbling, WPF has routed commands build in
    * <AControl.CommandBindings>
         <CommandBinding Command="Help" CanExecute="CommandBinding_CanExecute" Executed="CommandBinding_Executed"></CommandBinding>
       </AControl.CommandBindings>
    * <Button Command="Help"/>
    * 
    */

    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other objects by invoking delegates. 
    /// The default return value for the CanExecute method is 'true'.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion 

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion
    }
}