using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using SinGooCMS;
using SinGooCMS.Ado;
using SinGooCMS.Ado.DbMaintenance;
using SinGooCMS.Ado.Interface;

namespace NUnitTestProject
{
    public class DbMaintenanceTest
    {
        readonly IDbAccess dbSqlServerAccess = DbProvider.Create(DbProviderType.SqlServer, "server=(local);database=SinGooCMS-v1.6;uid=sa;pwd=123");
        readonly IDbMaintenance dbSqlServerMaintenance = new SqlServerDbMaintenance();

        readonly IDbAccess dbMySqlAccess = DbProvider.Create(DbProviderType.MySql, "server=localhost;database=singoocmsv16;uid=root;pwd=123456;SslMode=None;");
        readonly IDbMaintenance dbMySqlMaintenance = new MySqlDbMaintenance();

        readonly IDbAccess dbSqliteAccess = DbProvider.Create(DbProviderType.Sqlite, @"Data Source=F:\jsonlee\netcore\SinGooCMS-v1.6\SinGooCMS.UI\singoocms\db\singootop.db");
        readonly IDbMaintenance dbSqliteMaintenance = new SqliteDbMaintenance();

        readonly IDbAccess dbOracleAccess = DbProvider.Create(DbProviderType.Oracle, "data source=localhost:1521;user id=jsonlee;password=123;min pool size=5;max pool size=512;");
        readonly IDbMaintenance dbOracleMaintenance = new OracleDbMaintenance();

        #region sqlserver    

        [Test]
        public void ShowTables()
        {
            dbSqlServerMaintenance.Set(dbSqlServerAccess);
            Console.WriteLine(dbSqlServerMaintenance.ShowTables().Count());
        }

        [Test]
        public void ShowColumns()
        {
            dbSqlServerMaintenance.Set(dbSqlServerAccess);
            Console.WriteLine(dbSqlServerMaintenance.ShowColumns("sys_Folder").Count());
        }

        [Test]
        public void CreateTableTest()
        {
            dbSqlServerMaintenance.Set(dbSqlServerAccess);
            //创建表
            dbSqlServerMaintenance.CreateTable("DbMaintenanceTest", new List<DbColumnInfo>() { new DbColumnInfo() { ColumnName = "UserName", Length = 50 } });
            Assert.AreEqual(true, dbSqlServerMaintenance.ExistsTable("DbMaintenanceTest"));
            //更改列名
            dbSqlServerMaintenance.RenameColumn("DbMaintenanceTest", "UserName", "TestName");
            Assert.AreEqual(true, dbSqlServerMaintenance.ExistsColumn("DbMaintenanceTest", "TestName"));
            //更新列
            dbSqlServerMaintenance.UpdateColumn("DbMaintenanceTest", new DbColumnInfo() { ColumnName = "TestName", DataType = "int" });
            Assert.AreEqual(true, dbSqlServerMaintenance.ExistsColumn("DbMaintenanceTest", "TestName"));

            //删除表
            dbSqlServerMaintenance.DropTable("DbMaintenanceTest");
            Assert.AreEqual(false, dbSqlServerMaintenance.ExistsTable("DbMaintenanceTest"));
        }

        #endregion

        #region mysql     

        [Test]
        public void ShowMySqlTables()
        {
            dbMySqlMaintenance.Set(dbMySqlAccess);
            Console.WriteLine(dbMySqlMaintenance.ShowTables().Count());
        }

        [Test]
        public void ShowMySqlColumns()
        {
            dbMySqlMaintenance.Set(dbMySqlAccess);
            Console.WriteLine(dbMySqlMaintenance.ShowColumns("sys_Folder").Count());
        }

        [Test]
        public void CreateMySqlTableTest()
        {
            dbMySqlMaintenance.Set(dbMySqlAccess);

            //创建表
            dbMySqlMaintenance.CreateTable("DbMaintenanceTest", new List<DbColumnInfo>() { new DbColumnInfo() { ColumnName = "UserName", Length = 50 } });
            Assert.AreEqual(true, dbMySqlMaintenance.ExistsTable("DbMaintenanceTest"));
            //更改列名
            dbMySqlMaintenance.RenameColumn("DbMaintenanceTest", "UserName", "TestName");
            Assert.AreEqual(true, dbMySqlMaintenance.ExistsColumn("DbMaintenanceTest", "TestName"));
            //更新列
            dbMySqlMaintenance.UpdateColumn("DbMaintenanceTest", new DbColumnInfo() { ColumnName = "TestName", DataType = "int" });
            Assert.AreEqual(true, dbMySqlMaintenance.ExistsColumn("DbMaintenanceTest", "TestName"));

            //删除表
            dbMySqlMaintenance.DropTable("DbMaintenanceTest");
            Assert.AreEqual(false, dbMySqlMaintenance.ExistsTable("DbMaintenanceTest"));
        }

        #endregion

        #region sqlite

        [Test]
        public void ShowSqliteTables()
        {
            dbSqliteMaintenance.Set(dbSqliteAccess);
            Console.WriteLine(dbSqliteMaintenance.ShowTables().Count());
        }

        [Test]
        public void ShowSqliteColumns()
        {
            dbSqliteMaintenance.Set(dbSqliteAccess);
            Console.WriteLine(dbSqliteMaintenance.ShowColumns("sys_Folder").Count());
        }

        [Test]
        public void CreateSqliteTableTest()
        {
            dbSqliteMaintenance.Set(dbSqliteAccess);

            //创建表
            dbSqliteMaintenance.CreateTable("DbMaintenanceTest", new List<DbColumnInfo>() { new DbColumnInfo() { ColumnName = "UserName", Length = 50 } });
            Assert.AreEqual(true, dbSqliteMaintenance.ExistsTable("DbMaintenanceTest"));
            //删除表
            dbSqliteMaintenance.DropTable("DbMaintenanceTest");
            Assert.AreEqual(false, dbSqliteMaintenance.ExistsTable("DbMaintenanceTest"));
        }

        #endregion

        #region oracle

        [Test]
        public void ShowOracleTables()
        {
            dbOracleMaintenance.Set(dbOracleAccess);
            Console.WriteLine(dbOracleMaintenance.ShowTables().Count());
        }

        [Test]
        public void ShowOracleColumns()
        {
            dbOracleMaintenance.Set(dbOracleAccess);
            Console.WriteLine(dbOracleMaintenance.ShowColumns("sys_Folder").Count());
        }

        [Test]
        public void CreateOracleTableTest()
        {
            dbOracleMaintenance.Set(dbOracleAccess);

            //创建表
            dbOracleMaintenance.CreateTable("DbMaintenanceTest", new List<DbColumnInfo>() { new DbColumnInfo() { ColumnName = "UserName", Length = 50 } });
            Assert.AreEqual(true, dbOracleMaintenance.ExistsTable("DbMaintenanceTest"));
            //更改列名
            dbOracleMaintenance.RenameColumn("DbMaintenanceTest", "UserName", "TestName");
            Assert.AreEqual(true, dbOracleMaintenance.ExistsColumn("DbMaintenanceTest", "TestName"));
            //更新列
            dbOracleMaintenance.UpdateColumn("DbMaintenanceTest", new DbColumnInfo() { ColumnName = "TestName", DataType = "int" });
            Assert.AreEqual(true, dbOracleMaintenance.ExistsColumn("DbMaintenanceTest", "TestName"));

            //删除表
            dbOracleMaintenance.DropTable("DbMaintenanceTest");
            Assert.AreEqual(false, dbOracleMaintenance.ExistsTable("DbMaintenanceTest"));
        }

        #endregion
    }
}
