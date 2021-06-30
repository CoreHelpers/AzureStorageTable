using System;
namespace CoreHelpers.WindowsAzure.Storage.Table.Abstractions
{
    public interface IStorageLogger
    {
        void LogInformation(string text, params object[] args);

        void LogInformation(string text);
    }
}
