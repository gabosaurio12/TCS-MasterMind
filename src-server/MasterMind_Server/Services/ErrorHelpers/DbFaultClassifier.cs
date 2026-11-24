using MasterMind_Server.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace MasterMind_Server.Services.ErrorHelpers
{
    internal static class DbFaultClassifier
    {
        private const int TimeoutErrorNumber = -2;
        private const int NetworkPathNotFoundErrorNumber = 53;
        private const int ServerNotFoundErrorNumber = 2;
        private const int LoginFailedErrorNumber = 18456;
        private const int DatabaseUnavailableErrorNumber = 4060;
        private const int ServerShuttingDownErrorNumber = 6005;
        private const int ConnectionBrokenErrorNumber = 10054;

        private static readonly HashSet<int> connectionErrorNumbers = new HashSet<int>
        {
            TimeoutErrorNumber,
            NetworkPathNotFoundErrorNumber,
            ServerNotFoundErrorNumber,
            LoginFailedErrorNumber,
            DatabaseUnavailableErrorNumber,
            ServerShuttingDownErrorNumber,
            ConnectionBrokenErrorNumber
        };

        public static ErrorMessages Classify(Exception exception)
        {
            var current = exception;

            while (current != null)
            {
                if (current is SqlException sqlException && IsConnectionError(sqlException))
                {
                    return ErrorMessages.DbServerError;
                }

                current = current.InnerException;
            }

            return ErrorMessages.DataBaseError;
        }

        private static bool IsConnectionError(SqlException sqlException)
        {
            foreach (SqlError error in sqlException.Errors)
            {
                if (connectionErrorNumbers.Contains(error.Number))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
