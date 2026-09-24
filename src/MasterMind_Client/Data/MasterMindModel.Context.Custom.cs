using DotNetEnv;
using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace MasterMind_Client.Data
{
    public partial class MasterMindEntities
    {
        public MasterMindEntities(bool useCustomConnection) : base(BuildConnectionString())
        {
        }

        private static string BuildConnectionString()
        {
            Env.Load();

            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = @"(localdb)\MSSQLLocalDB",
                InitialCatalog = Environment.GetEnvironmentVariable("DB_NAME"),
                UserID = Environment.GetEnvironmentVariable("DB_USER"),
                Password = Environment.GetEnvironmentVariable("DB_PASSWORD"),
                PersistSecurityInfo = true,
                TrustServerCertificate = true,
                MultipleActiveResultSets = true
            };

            var entityBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = sqlBuilder.ConnectionString,
                Metadata = "res://*/Data.MasterMindModel.csdl|res://*/Data.MasterMindModel.ssdl|res://*/Data.MasterMindModel.msl"
            };

            return entityBuilder.ConnectionString;
        }
    }
}