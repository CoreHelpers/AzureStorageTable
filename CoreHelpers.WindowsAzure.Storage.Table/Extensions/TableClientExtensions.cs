using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Azure;
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

        public static async Task<Response<IReadOnlyList<Response>>> SubmitTransactionWithAutoCreateTableAsync(this TableClient tc, IEnumerable<TableTransactionAction> transactionActions, CancellationToken cancellationToken, bool allowAutoCreate)
        {
            try
            {
                return await tc.SubmitTransactionAsync(transactionActions, cancellationToken);
            }
            catch (TableTransactionFailedException ex)
            {
                // check the exception
                if (allowAutoCreate && ex.ErrorCode.Equals("TableNotFound"))
                {
                    // try to create the table
                    await tc.CreateAsync();

                    // retry 
                    return await tc.SubmitTransactionAsync(transactionActions, cancellationToken);
                }
                else
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return null;
                }
            }
        }        
    }
}

