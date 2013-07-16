namespace IronFoundry.Warden.ProcessIsolation
{
    using System;

    public class EventArgs<T> : EventArgs
    {
        public T Value { get; set; }

        public EventArgs(T value)
        {
            Value = value;
        }
    }

    public static class EventArgsExtensions
    {
        public static void Raise<T>(this EventHandler<EventArgs<T>> eventHandler, T value)
        {
            if (eventHandler != null)
            {
                eventHandler(null, new EventArgs<T>(value));
            }
        }
    }
}
