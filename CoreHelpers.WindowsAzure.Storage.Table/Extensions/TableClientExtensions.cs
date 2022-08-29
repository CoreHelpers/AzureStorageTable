using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace CoreHelpers.WindowsAzure.Storage.Table.Extensions
{
    public static class TableClientExtensions
    {
        public static async Task DeleteIfExistsAsync(this TableClient tc)
        {
            try
            {
                await tc.DeleteAsync();
            } catch (Exception)
            {}
        }

        public static async Task<bool> ExistsAsync(this TableClient tc)
        {
            try
            {
                await tc.GetAccessPoliciesAsync();
                return true;
            } catch(Azure.RequestFailedException ex)
            {
                if (ex.Status == 404)
                    return false;
                else
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return false;
                }
            }            
        }
    }
}

