using DotNetEnv;
using log4net;
using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace MasterMind_Server.Data
{
    public partial class MasterMindEntities
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(MasterMindEntities));

        public MasterMindEntities(bool useCustomConnection) : base(BuildConnectionString())
        {
        }

        private static string BuildConnectionString()
        {
            try
            {
                Env.Load();

                var entityBuilder = new EntityConnectionStringBuilder
                {
                    Provider = "System.Data.SqlClient",
                    ProviderConnectionString = BuildProviderConnectionString(),
                    Metadata = "res://*/Data.MasterMindModel.csdl|res://*/Data.MasterMindModel.ssdl"
                        + "|res://*/Data.MasterMindModel.msl"
                };

                return entityBuilder.ConnectionString;
            }
            catch (ArgumentException ex)
            {
                logger.Error("Failed to build the MasterMind connection string. Verify that DB_SOURCE, "
                    + "DB_NAME, DB_USER and DB_PASSWORD are defined in the .env file.", ex);
                throw;
            }
        }

        private static string BuildProviderConnectionString()
        {
            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = Environment.GetEnvironmentVariable("DB_SOURCE"),
                InitialCatalog = Environment.GetEnvironmentVariable("DB_NAME"),
                UserID = Environment.GetEnvironmentVariable("DB_USER"),
                Password = Environment.GetEnvironmentVariable("DB_PASSWORD"),
                PersistSecurityInfo = true,
                TrustServerCertificate = true,
                Encrypt = false,
                MultipleActiveResultSets = true
            };

            return sqlBuilder.ConnectionString;
        }
    }
}
