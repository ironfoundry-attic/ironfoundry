namespace System
{
    public static class FloatExtensionMethods
    {
        public static float Truncate(this float argThis, int digits)
        {
            double mult = Math.Pow(10.0, digits);
            double result = Math.Truncate(mult * argThis) / mult;
            return (float)result;
        }
    }
}