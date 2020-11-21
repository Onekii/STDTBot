using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace STDTBot.Database
{
    class STDTContextFactory : IDesignTimeDbContextFactory<STDTContext>
    {
        public STDTContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<STDTContext>();
            optionsBuilder.UseMySQL(BuildConnectionString());

            return new STDTContext(optionsBuilder.Options);
        }

        private string BuildConnectionString()
        {
            return new MySqlConnectionStringBuilder()
            {
                Server = "localhost",
                Password = "",
                Database = "",
                UserID = "",
                Port = 3306
            }
            .ConnectionString;
        }
    }
}
