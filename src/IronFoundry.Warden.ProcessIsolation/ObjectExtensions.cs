namespace IronFoundry.Warden.ProcessIsolation
{
    using System;

    public static class ObjectExtensions
    {
        public static void Try<T>(this object obj, Action<T> action, T arg1, Action<Exception> onError = null)
        {
            try
            {
                action(arg1);
            }
            catch (Exception ex)
            {
                if (onError != null)
                {
                    onError(ex);
                }
            }
        }

        public static void Try(this object obj, Action action, Action<Exception> onError = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (onError != null)
                {
                    onError(ex);
                }
            }
        }

        public static T Try<T>(this object obj, Func<T> function, Func<T> errorReturnValue, Action<Exception> onError = null)
        {
            try
            {
                return function();
            }
            catch(Exception ex)
            {
                if (onError != null)
                {
                    onError(ex);
                }

                return errorReturnValue();
            }
        }
    }
}
