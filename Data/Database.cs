using System.Configuration;
using System.Data.SqlClient;

namespace GestionDocumentosMVC.Data
{
    public static class Database
    {
        private const string DbConnectionString = "GestionDocumentosCadenaDB";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings[DbConnectionString].ConnectionString);
        }
    }
}