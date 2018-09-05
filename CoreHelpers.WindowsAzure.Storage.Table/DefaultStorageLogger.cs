using System;
namespace CoreHelpers.WindowsAzure.Storage.Table
{
    public abstract class DefaultStorageLogger : IStorageLogger
    {
        public void LogInformation(string text, params object[] args)
        {
            LogInformation(String.Format(text, args));
        }

        public abstract void LogInformation(string text);
    }
}
