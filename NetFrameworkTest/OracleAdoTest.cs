using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using SinGooCMS.Ado;
using SinGooCMS.Ado.DbMaintenance;
using SinGooCMS.Ado.Interface;
using Assert = NUnit.Framework.Assert;

namespace NetFrameworkTest
{
    [TestClass]
    public class OracleAdoTest
    {
        IDbAccess dbOracleAccess => DbProvider.Create(DbProviderType.Oracle, "data source=localhost:1521;user id=jsonlee;password=123;min pool size=5;max pool size=512;");
        IOracle oracle => (IOracle)DbProvider.Create(DbProviderType.Oracle, "data source=localhost:1521;user id=jsonlee;password=123;min pool size=5;max pool size=512;");
        IDbMaintenance dbMaintenance => new OracleDbMaintenance().Set(dbOracleAccess);

        public OracleAdoTest()
        {
            //创建表
            if (!dbMaintenance.ExistsTable("DbMaintenanceTest"))
                dbMaintenance.CreateTable("DbMaintenanceTest", new List<DbColumnInfo>() { new DbColumnInfo() { ColumnName = "AutoID", DataType = "int", IsIdentity = true, IsPrimarykey = true }, new DbColumnInfo() { ColumnName = "UserName", Length = 50 } });
        }

        /// <summary>
        /// 在插入数据的时候不要启用分析，否则会很慢
        /// </summary>
        [Test]
        public void InsertTest()
        {
            //10万条记录 共运行时长：103741毫秒
            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (dbMaintenance.ExistsTable("DbMaintenanceTest"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    dbOracleAccess.InsertModel(new DbMaintenanceTestInfo() { AutoID = i + 1, UserName = "jsonlee" });
                }
            }
            watch.Stop();
            Console.WriteLine("共运行时长：" + watch.ElapsedMilliseconds + "毫秒");
        }

        [Test]
        public void ExistsSEQTest()
        {
            //删除序列
            //oracle.DelSequence("SEQ_" + "DbMaintenanceTest".ToUpper());

            Assert.AreEqual(true, oracle.ExisSequence("SEQ_" + "DbMaintenanceTest".ToUpper())); //是否存在序列
        }

        [Test]
        public void GetValueTest()
        {
            string val = dbOracleAccess.GetValue<string>("select UserName from DbMaintenanceTest where 1=:param and rownum<=1",
                new DbParameter[] { dbOracleAccess.MakeParam(":param", 1) });
            Assert.AreEqual("jsonlee", val);
        }

        [Test]
        public void GetDataTableTest()
        {
            var dt = dbOracleAccess.GetDataTable("select * from DbMaintenanceTest where 1=:param and rownum<=10",
                new DbParameter[] { dbOracleAccess.MakeParam(":param", 1) });
            Console.WriteLine($"dt行数；{dt.Rows.Count}");
            Assert.AreEqual("jsonlee", dt.Rows[0]["UserName"].ToString());
        }

        [Test]
        public void GetListValueTest()
        {
            var val = dbOracleAccess.GetValueList<string>("select UserName from DbMaintenanceTest where 1=:param and rownum<=10",
                new DbParameter[] { dbOracleAccess.MakeParam(":param", 1) });
            Console.WriteLine(val.Count());
        }

        [Test]
        public void GetModelTest()
        {
            //GetList会返回多条记录。而GetModel只会取第一个记录并返回实体对象
            var val = dbOracleAccess.GetModel<DbMaintenanceTestInfo>("select UserName from DbMaintenanceTest where 1=:param and rownum<=1",
                new DbParameter[] { dbOracleAccess.MakeParam(":param", 1) });
            Assert.AreEqual("jsonlee", val.UserName);
        }

        [Test]
        public void GetModelTest2()
        {
            var val = dbOracleAccess.Find<DbMaintenanceTestInfo>(1); //读主键 1 的对象
            Assert.AreEqual(1, val.AutoID);
        }

        [Test]
        public void GetPagerTest()
        {
            //以下2个参数将被传出
            int totalCount = dbOracleAccess.GetCount<DbMaintenanceTestInfo>();
            int totalPage = totalCount % 25 == 0 ? totalCount / 25 : (1 + totalCount / 25);
            //分页测试
            var pagerData = dbOracleAccess.GetPagerList<DbMaintenanceTestInfo>("", "AutoID desc", 200, 25);
            Console.WriteLine($"总记录数：{totalCount} 总页数：{totalPage}");
            Assert.AreEqual(25, pagerData.Count());
        }

        [Test]
        public void UpdateTest()
        {
            dbOracleAccess.UpdateModel(new DbMaintenanceTestInfo() { AutoID = 10, UserName = "刘备" });
            Assert.AreEqual("刘备", dbOracleAccess.Find<DbMaintenanceTestInfo>(10).UserName);
        }

        [Test]
        public async Task UpdateColumnTest()
        {
            await dbOracleAccess.UpdateColumnAsync<DbMaintenanceTestInfo>((p) => new DbMaintenanceTestInfo() { UserName = "赵云" }, "AutoID=@PKey", new DbParameter[] { dbOracleAccess.MakeParam("@PKey", 1) }); //指定更新条件
            Assert.AreEqual("赵云", (await dbOracleAccess.FindAsync<DbMaintenanceTestInfo>(1)).UserName);
        }

        [Test]
        public void DeleteTest()
        {
            //15 被删除了，返回null
            dbOracleAccess.DeleteModel(new DbMaintenanceTestInfo() { AutoID = 15 });
            Assert.AreEqual(null, dbOracleAccess.Find<DbMaintenanceTestInfo>(15));
        }

        [Test]
        public void DeleteTest2()
        {
            //16 被删除了，返回null
            dbOracleAccess.Delete<DbMaintenanceTestInfo>(16);
            Assert.AreEqual(null, dbOracleAccess.Find<DbMaintenanceTestInfo>(16));
        }

        [Test]
        public void TransTest()
        {
            //事务具有原子性，所有要么同时成功，要么同时失败
            var lstSql = new List<string>();
            lstSql.Add("update DbMaintenanceTest set UserName='张飞' where AutoID =1");
            //这句会报错，所以第一句也不会执行
            lstSql.Add("update DbMaintenanceTest set UserName1='关羽' where AutoID =15");
            dbOracleAccess.ExecTrans(lstSql);
            Assert.AreEqual("jsonlee", dbOracleAccess.Find<DbMaintenanceTestInfo>(1).UserName);
        }

        //异步测试 并不一定是按从上往下顺序执行的，多线程执行
        [Test]
        public async Task GetAsyncTest()
        {
            var valTask = dbOracleAccess.GetValueAsync<string>("select UserName from DbMaintenanceTest where rownum<=1");
            var modelTask = dbOracleAccess.GetModelAsync<DbMaintenanceTestInfo>("select * from DbMaintenanceTest where rownum<=1");
            var lstTask = dbOracleAccess.GetListAsync<DbMaintenanceTestInfo>("select * from DbMaintenanceTest where rownum<=10");
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
