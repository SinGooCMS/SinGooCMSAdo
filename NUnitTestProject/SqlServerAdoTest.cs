using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SinGooCMS.Ado;
using SinGooCMS.Ado.DbMaintenance;
using SinGooCMS.Ado.Interface;

namespace NUnitTestProject
{
    public class SqlServerAdoTest
    {
        IDbAccess dbSqlServerAccess => DbProvider.Create(DbProviderType.SqlServer, "server=(local);database=TestDB;uid=sa;pwd=123");
        ISqlServer sqlserver=(ISqlServer)DbProvider.Create(DbProviderType.SqlServer, "server=(local);database=TestDB;uid=sa;pwd=123");
        IDbMaintenance dbMaintenance => new SqlServerDbMaintenance().Set(dbSqlServerAccess);

        public SqlServerAdoTest()
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
            //10万条记录 共运行时长：27332毫秒 、27963毫秒 、25918毫秒、27539毫秒
            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (dbMaintenance.ExistsTable("DbMaintenanceTest"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    dbSqlServerAccess.InsertModel(new DbMaintenanceTestInfo() { UserName = "jsonlee" });
                }
            }
            watch.Stop();
            Console.WriteLine("共运行时长：" + watch.ElapsedMilliseconds + "毫秒");
        }

        /// <summary>
        /// 添加到无主键表
        /// </summary>
        [Test]
        public async Task InsertTest2()
        {
            //动态类型
            //dynamic student = new ExpandoObject();
            //student.Name = "李四";
            //student.Age = 10;

            if (!dbMaintenance.ExistsTable("Students"))
                dbMaintenance.CreateTable("Students", new List<DbColumnInfo>()
                {
                    new DbColumnInfo(){ ColumnName="Name",ColumnDescription="姓名",DataType="varchar",Length=50 ,IsNullable=false},
                    new DbColumnInfo(){ ColumnName="Age",ColumnDescription="年龄",DataType="int" }
                });

            if (dbMaintenance.ExistsTable("Students"))
                await dbSqlServerAccess.InsertModelAsync(new Students()
                {
                    Name = "李四",
                    Age = 10
                });
        }

        [Test]
        public void GetValueTest()
        {
            string val = dbSqlServerAccess.GetValue<string>("select  top 1 UserName from DbMaintenanceTest where 1=@param",
                new DbParameter[] { dbSqlServerAccess.MakeParam("@param", 1) });
            Assert.AreEqual("jsonlee", val);
        }

        [Test]
        public void GetDataTableTest()
        {
            var dt = dbSqlServerAccess.GetDataTable("select  top 10 * from DbMaintenanceTest where 1=@param",
                new DbParameter[] { dbSqlServerAccess.MakeParam("@param", 1) });
            Console.WriteLine($"dt行数；{dt.Rows.Count}");
            Assert.AreEqual("jsonlee", dt.Rows[0]["UserName"].ToString());
        }

        [Test]
        public void GetListValueTest()
        {
            var val = dbSqlServerAccess.GetValueList<string>("select  top 10 UserName from DbMaintenanceTest where 1=@param",
                new DbParameter[] { dbSqlServerAccess.MakeParam("@param", 1) });
            Console.WriteLine(val.Count());
        }

        [Test]
        public void GetModelTest()
        {
            //GetList会返回多条记录。而GetModel只会取第一个记录并返回实体对象
            var val = dbSqlServerAccess.GetModel<DbMaintenanceTestInfo>("select top 100 UserName from DbMaintenanceTest where 1=@param",
                new DbParameter[] { dbSqlServerAccess.MakeParam("@param", 1) });
            Assert.AreEqual("jsonlee", val.UserName);
        }

        [Test]
        public void GetModelTest2()
        {
            var model1 = dbSqlServerAccess.Find<DbMaintenanceTestInfo>(1); //读主键 1 的对象
            var model2 = dbSqlServerAccess.Find<Students>("李四");

            Assert.AreEqual(1, model1.AutoID);
            Assert.AreEqual("李四", model2.Name);
        }

        [Test]
        public void GetPagerTest()
        {
            //以下2个参数将被传出
            int totalCount = dbSqlServerAccess.GetCount<DbMaintenanceTestInfo>();
            int totalPage = totalCount % 10 == 0 ? totalCount / 10 : (1 + totalCount / 10);
            //分页测试
            var pagerData = dbSqlServerAccess.GetPagerList<DbMaintenanceTestInfo>("", "AutoID desc", 6000, 10, "UserName");
            Console.WriteLine($"总记录数：{totalCount} 总页数：{totalPage}");
            Assert.AreEqual(10, pagerData.Count());
        }

        [Test]
        public void UpdateTest()
        {
            dbSqlServerAccess.UpdateModel(new DbMaintenanceTestInfo() { AutoID = 10, UserName = "刘备" });
            Assert.AreEqual("刘备", dbSqlServerAccess.Find<DbMaintenanceTestInfo>(10).UserName);
        }

        [Test]
        public void UpdateTest2()
        {
            dbSqlServerAccess.UpdateModel(new DbMaintenanceTestInfo() { UserName = "张飞" }, "UserName='刘备'"); //指定更新条件
            Assert.AreEqual("张飞", dbSqlServerAccess.Find<DbMaintenanceTestInfo>(10).UserName);
        }

        [Test]
        public void DeleteTest()
        {
            //15 被删除了，返回null
            dbSqlServerAccess.DeleteModel(new DbMaintenanceTestInfo() { AutoID = 15 });
            Assert.AreEqual(null, dbSqlServerAccess.Find<DbMaintenanceTestInfo>(15));
        }

        [Test]
        public void DeleteTest2()
        {
            //16 被删除了，返回null
            dbSqlServerAccess.Delete<DbMaintenanceTestInfo>(16);
            Assert.AreEqual(null, dbSqlServerAccess.Find<DbMaintenanceTestInfo>(16));
        }

        [Test]
        public void BulkInsert()
        {
            //7.9秒
            Stopwatch watch = new Stopwatch();
            watch.Start();

            var dt = new DataTable();
            dt.Columns.Add("AutoID",typeof(int));
            dt.Columns.Add("UserName", typeof(string));

            for(var i=0;i<1000000;i++)
            {
                var dr = dt.NewRow();
                dr["AutoID"] = 0;
                dr["UserName"] = "jsonlee" + i.ToString();
                dt.Rows.Add(dr);
            }

            sqlserver.BulkInsert<DbMaintenanceTestInfo>(dt);

            watch.Stop();
            Console.WriteLine("共运行时长：" + watch.ElapsedMilliseconds + "毫秒");
        }

        [Test]
        public void TransTest()
        {
            //事务具有原子性，所有要么同时成功，要么同时失败
            var lstSql = new List<string>();
            lstSql.Add("update DbMaintenanceTest set UserName='张飞' where AutoID =1");
            //这句会报错，所以第一句也不会执行
            lstSql.Add("update DbMaintenanceTest set UserName1='关羽' where AutoID =15");
            dbSqlServerAccess.ExecTrans(lstSql);
            Assert.AreEqual("jsonlee", dbSqlServerAccess.Find<DbMaintenanceTestInfo>(1).UserName);
        }

        //异步测试 并不一定是按从上往下顺序执行的，多线程执行
        [Test]
        public async Task GetAsyncTest()
        {
            var valTask = dbSqlServerAccess.GetValueAsync<string>("select top 1 UserName from DbMaintenanceTest");
            var modelTask = dbSqlServerAccess.GetModelAsync<DbMaintenanceTestInfo>("select top 1 * from DbMaintenanceTest");
            var lstTask = dbSqlServerAccess.GetListAsync<DbMaintenanceTestInfo>("select top 10 * from DbMaintenanceTest");
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

    [Table("DbMaintenanceTest")]
    public class DbMaintenanceTestInfo
    {
        [Key]
        [NotMapped]
        public int AutoID { get; set; }
        public string UserName { get; set; }
    }

    public class Students
    {
        [Key]
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
