using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SinGooCMS.Ado;
using SinGooCMS.Ado.DbMaintenance;
using SinGooCMS.Ado.Interface;

namespace NetFrameworkTest
{
    public class SqliteAdoTest
    {
        IDbAccess dbSqliteAccess => DbProvider.Create(DbProviderType.Sqlite, @"Data Source=F:\jsonlee\SinGooCMS-v1.6-20201010\SinGooCMS.UI\SinGooCMS.WebUI\db\singootop.db");
        IDbMaintenance dbMaintenance => new SqliteDbMaintenance().Set(dbSqliteAccess);

        public SqliteAdoTest()
        {
            //创建表
            if (!dbMaintenance.ExistsTable("DbMaintenanceTest"))
                dbMaintenance.CreateTable("DbMaintenanceTest", new List<DbColumnInfo>() { new DbColumnInfo() { ColumnName = "AutoID", DataType = "int", IsIdentity = true, IsPrimarykey = true }, new DbColumnInfo() { ColumnName = "UserName", Length = 50 } });
        }

        [TestMethod]
        public void InsertTest()
        {
            //10万条记录 sqlite插入数据的效率实在是太慢了，我不得不停止，用在单机上是可以的
            //1000条数据：48361毫秒、40336毫秒
            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (dbMaintenance.ExistsTable("DbMaintenanceTest"))
            {
                for (var i = 0; i < 1000; i++)
                {
                    dbSqliteAccess.InsertModel(new DbMaintenanceTestInfo() { UserName = "jsonlee" });
                }
            }
            watch.Stop();
            Console.WriteLine("共运行时长：" + watch.ElapsedMilliseconds + "毫秒");
        }

        [TestMethod]
        public void GetValueTest()
        {
            string val = dbSqliteAccess.GetValue<string>("select UserName from DbMaintenanceTest where 1=@param",
                new DbParameter[] { dbSqliteAccess.MakeParam("@param", 1) });
            Assert.AreEqual("jsonlee", val);
        }

        [TestMethod]
        public void GetListValueTest()
        {
            var val = dbSqliteAccess.GetValueList<string>("select UserName from DbMaintenanceTest where 1=@param limit 10",
                new DbParameter[] { dbSqliteAccess.MakeParam("@param", 1) });
            Console.WriteLine(val.Count());
        }

        [TestMethod]
        public void GetDataTableTest()
        {
            var dt = dbSqliteAccess.GetDataTable("select * from DbMaintenanceTest where 1=@param limit 10",
                new DbParameter[] { dbSqliteAccess.MakeParam("@param", 1) });
            Console.WriteLine($"dt行数；{dt.Rows.Count}");
            Assert.AreEqual("jsonlee", dt.Rows[0]["UserName"].ToString());
        }

        [TestMethod]
        public void GetModelTest()
        {
            //GetList会返回多条记录。而GetModel只会取第一个记录并返回实体对象
            var val = dbSqliteAccess.GetModel<DbMaintenanceTestInfo>("select UserName from DbMaintenanceTest where 1=@param limit 100",
                new DbParameter[] { dbSqliteAccess.MakeParam("@param", 1) });
            Assert.AreEqual("jsonlee", val.UserName);
        }

        [TestMethod]
        public void GetModelTest2()
        {
            var val = dbSqliteAccess.Find<DbMaintenanceTestInfo>(1); //读主键 1 的对象
            Assert.AreEqual(1, val.AutoID);
        }

        [TestMethod]
        public void GetPagerTest()
        {
            //以下2个参数将被传出
            int totalCount = dbSqliteAccess.GetCount<DbMaintenanceTestInfo>();
            int totalPage = totalCount % 10 == 0 ? totalCount / 10 : (1 + totalCount / 10);
            //分页测试
            var pagerData = dbSqliteAccess.GetPagerList<DbMaintenanceTestInfo>("", "AutoID desc", 25, 10);
            Console.WriteLine($"总记录数：{totalCount} 总页数：{totalPage}");
            Assert.AreEqual(10, pagerData.Count());
        }

        [TestMethod]
        public void UpdateTest()
        {
            dbSqliteAccess.UpdateModel(new DbMaintenanceTestInfo() { AutoID = 10, UserName = "刘备" });
            Assert.AreEqual("刘备", dbSqliteAccess.Find<DbMaintenanceTestInfo>(10).UserName);
        }

        [TestMethod]
        public async Task UpdateColumnTest()
        {
            await dbSqliteAccess.UpdateColumnAsync<DbMaintenanceTestInfo>((p) => new DbMaintenanceTestInfo() { UserName = "赵云" }, "AutoID=@PKey", new DbParameter[] { dbSqliteAccess.MakeParam("@PKey", 1) }); //指定更新条件
            Assert.AreEqual("赵云", (await dbSqliteAccess.FindAsync<DbMaintenanceTestInfo>(1)).UserName);
        }

        [TestMethod]
        public void DeleteTest()
        {
            //15 被删除了，返回null
            dbSqliteAccess.DeleteModel(new DbMaintenanceTestInfo() { AutoID = 15 });
            Assert.AreEqual(null, dbSqliteAccess.Find<DbMaintenanceTestInfo>(15));
        }

        [TestMethod]
        public void DeleteTest2()
        {
            //15 被删除了，返回null
            dbSqliteAccess.Delete<DbMaintenanceTestInfo>(16);
            Assert.AreEqual(0, dbSqliteAccess.GetCount<DbMaintenanceTestInfo>("AutoID=16"));
        }

        [TestMethod]
        public void TransTest()
        {
            //事务具有原子性，所有要么同时成功，要么同时失败
            var lstSql = new List<string>();
            lstSql.Add("update DbMaintenanceTest set UserName='张飞' where AutoID =1");
            //这句会报错，所以第一句也不会执行
            lstSql.Add("update DbMaintenanceTest set UserName1='关羽' where AutoID =15");
            dbSqliteAccess.ExecTrans(lstSql);
            Assert.AreEqual("jsonlee", dbSqliteAccess.Find<DbMaintenanceTestInfo>(1).UserName);
        }

        //异步测试 并不一定是按从上往下顺序执行的，多线程执行
        [TestMethod]
        public async Task GetAsyncTest()
        {
            var valTask = dbSqliteAccess.GetValueAsync<string>("select UserName from DbMaintenanceTest limit 1");
            var modelTask = dbSqliteAccess.GetModelAsync<DbMaintenanceTestInfo>("select * from DbMaintenanceTest limit 1");
            var lstTask = dbSqliteAccess.GetListAsync<DbMaintenanceTestInfo>("select * from DbMaintenanceTest limit 10");
            var tasks = new List<Task> { valTask, modelTask, lstTask };
            while (tasks.Count > 0)
            {
                Task finishedTask = await Task.WhenAny(tasks);
                if (finishedTask == valTask)
                {
                    Console.WriteLine($"1、取值：{valTask.Result}");
                }
                else if (finishedTask == modelTask)
                {
                    Console.WriteLine($"2、model值：{modelTask.Result.UserName}");
                }
                else if (finishedTask == lstTask)
                {
                    Console.WriteLine($"3、lst记录数：{lstTask.Result.Count()}");
                }
                tasks.Remove(finishedTask);
            }

            Assert.IsNotNull(valTask);
        }
    }
}
