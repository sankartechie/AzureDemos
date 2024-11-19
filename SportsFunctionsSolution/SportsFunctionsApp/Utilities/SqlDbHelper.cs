using Microsoft.Data.SqlClient;

namespace SportsFunctionsApp.Utilities
{
    public class SqlDbHelper
    {
        private readonly string _connectionString;

        public SqlDbHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
