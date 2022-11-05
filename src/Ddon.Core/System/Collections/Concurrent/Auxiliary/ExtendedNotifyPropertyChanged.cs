﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Collections.Concurrent.Auxiliary
{

    /// <summary>
    /// Provides a INotifyPropertyChanged implementation where the event args contains new and old values
    /// </summary>
    public class ExtendedNotifyPropertyChanged : INotifyPropertyChanged
    {

        /// <summary>
        /// Checks if the value has changed and uses the call stack to determine
        /// which property this was called from, then calls OnPropertyChanged
        /// with the property name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="current"></param>
        /// <param name="newValue"></param>
        protected bool SetProperty<T>(ref T current, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(current, newValue))
            {
                T oldValue = current;
                current = newValue;
                RaisePropertyChangedWithValues(propertyName, oldValue!, newValue!);
                return true;
            }
            else
            {
                return false;
            }
        }

        //-------------------------------------------------------------------------
        protected bool SetProperty<T>(ref T current, T newValue, params string[] propertyNames)
        {
            if (!EqualityComparer<T>.Default.Equals(current, newValue))
            {
                current = newValue;
                RaisePropertyChanged(propertyNames);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Triggers PropertyChanged event in a way that it can be handled
        /// by controls on a different thread.
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            if (propertyNames.Length == 0)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
        }

        protected virtual void RaisePropertyChangedWithValues(string propertyName, object oldValue, object newValue)
        {
            PropertyChanged?.Invoke(this, new ExtendedPropertyChangedEventArgs(propertyName, oldValue, newValue));
        }



        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }
}
