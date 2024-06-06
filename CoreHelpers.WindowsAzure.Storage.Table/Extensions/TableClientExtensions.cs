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
                    
                    // This is a double check pattern to ensure that two independent processes 
                    // who are trying to create the table in parallel do not end up in an unhandled
                    // situation.   
                    try
                    {
                        // try to create the table
                        await tc.CreateAsync();
                    }
                    catch (TableTransactionFailedException doubleCheckEx)
                    {
                        // check if we have an errorCode if not the system throws the exception 
                        // to the caller 
                        if (String.IsNullOrEmpty(doubleCheckEx.ErrorCode))
                        {
                            ExceptionDispatchInfo.Capture(ex).Throw();
                            return null;
                        }
                        
                        // Every error except the TableAlreadyExists is thrown to the caller but 
                        // in the case the system is trying to create the table in parallel we
                        // ignore the error and execute the transaction!
                        if (!doubleCheckEx.ErrorCode.Equals("TableAlreadyExists"))
                        {
                            ExceptionDispatchInfo.Capture(ex).Throw();
                            return null;
                        }
                    }

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

