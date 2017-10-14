using System;
using System.Threading.Tasks;

namespace CoreHelpers.WindowsAzure.Storage.Table.Demo.Contracts
{
	public interface IDemoCase
	{
		Task Execute(string storageKey, string storageSecret, string endpointSuffix = null);
	}
}
