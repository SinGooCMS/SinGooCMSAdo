using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SinGooCMS.Ado;
using SinGooCMS.Ado.DbMaintenance;
using SinGooCMS.Ado.Interface;

namespace NUnitTestProject
{
    public class MySqlAdoTest
    {
        IDbAccess dbMySqlAccess => DbProvider.Create(DbProviderType.MySql, "server=localhost;database=singoocmsv16;uid=root;pwd=123456;SslMode=None;");
        IMySql mysql = (IMySql)DbProvider.Create(DbProviderType.MySql, "server=localhost;database=singoocmsv16;uid=root;pwd=123456;SslMode=None;");
        IDbMaintenance dbMaintenance => new MySqlDbMaintenance().Set(dbMySqlAccess);

        public MySqlAdoTest()
        {
            //创建表
            if (!dbMaintenance.ExistsTable("DbMaintenanceTest"))
                dbMaintenance.CreateTable("DbMaintenanceTest", new List<DbColumnInfo>() { new DbColumnInfo() { ColumnName = "AutoID", DataType = "int", IsIdentity = true, IsPrimarykey = true }, new DbColumnInfo() { ColumnName = "UserName", Length = 50 } });
        }

        [Test]
        public void InsertTest()
        {
            //10万条记录 共运行时长：193929毫秒、197069毫秒、196735毫秒、184646毫秒,看起来比sqlserver花得时间长得多
            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (dbMaintenance.ExistsTable("DbMaintenanceTest"))
            {
                for (var i = 0; i < 100000; i++)
                {
                    dbMySqlAccess.InsertModel(new DbMaintenanceTestInfo() { UserName = "jsonlee" });
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
            if (!dbMaintenance.ExistsTable("Students"))
                dbMaintenance.CreateTable("Students", new List<DbColumnInfo>()
                {
                    new DbColumnInfo(){ ColumnName="Name",ColumnDescription="姓名",DataType="varchar",Length=50 ,IsNullable=false},
                    new DbColumnInfo(){ ColumnName="Age",ColumnDescription="年龄",DataType="int" }
                });

            
            if (dbMaintenance.ExistsTable("Students"))
                await dbMySqlAccess.InsertModelAsync(new Students()
                {
                    Name = "李四",
                    Age = 10
                });

            //var pId = dbMySqlAccess.InsertModel(new DbMaintenanceTestInfo() { UserName = "jsonlee" });
            //Console.WriteLine($"新增加的ID是：{pId}");
        }

        [Test]
        public void GetValueTest()
        {
            string val = dbMySqlAccess.GetValue<string>("select UserName from DbMaintenanceTest where 1=@param",
                new DbParameter[] { dbMySqlAccess.MakeParam("@param", 1) });
            Assert.AreEqual("jsonlee", val);
        }

        [Test]
        public void GetListValueTest()
        {
            var val = dbMySqlAccess.GetValueList<string>("select UserName from DbMaintenanceTest where 1=@param limit 10",
                new DbParameter[] { dbMySqlAccess.MakeParam("@param", 1) });
            Console.WriteLine(val.Count());
        }

        [Test]
        public void GetDataTableTest()
        {
            var dt = dbMySqlAccess.GetDataTable("select * from DbMaintenanceTest where 1=@param limit 10",
                new DbParameter[] { dbMySqlAccess.MakeParam("@param", 1) });
            Console.WriteLine($"dt行数；{dt.Rows.Count}");
            Assert.AreEqual("jsonlee", dt.Rows[0]["UserName"].ToString());
        }

        [Test]
        public void GetModelTest()
        {
            //GetList会返回多条记录。而GetModel只会取第一个记录并返回实体对象
            var val = dbMySqlAccess.GetModel<DbMaintenanceTestInfo>("select UserName from DbMaintenanceTest where 1=@param limit 100",
                new DbParameter[] { dbMySqlAccess.MakeParam("@param", 1) });
            Assert.AreEqual("jsonlee", val.UserName);
        }

        [Test]
        public void GetModelTest2()
        {
            var val = dbMySqlAccess.Find<DbMaintenanceTestInfo>(1); //读主键 1 的对象
            Assert.AreEqual(1, val.AutoID);
        }

        [Test]
        public void GetPagerTest()
        {
            //以下2个参数将被传出
            int totalCount = dbMySqlAccess.GetCount<DbMaintenanceTestInfo>();
            int totalPage = totalCount % 10 == 0 ? totalCount / 10 : (1 + totalCount / 10);
            //分页测试
            var pagerData = dbMySqlAccess.GetPagerList<DbMaintenanceTestInfo>("", "AutoID desc", 2000, 10);
            Console.WriteLine($"总记录数：{totalCount} 总页数：{totalPage}");
            Assert.AreEqual(10, pagerData.Count());
        }

        [Test]
        public void UpdateTest()
        {
            dbMySqlAccess.UpdateModel(new DbMaintenanceTestInfo() { AutoID = 10, UserName = "刘备" });
            Assert.AreEqual("刘备", dbMySqlAccess.Find<DbMaintenanceTestInfo>(10).UserName);
        }

        [Test]
        public void UpdateTest2()
        {
            dbMySqlAccess.UpdateModel(new DbMaintenanceTestInfo() { UserName = "张飞" }, "UserName='刘备'"); //指定更新条件
            Assert.AreEqual("张飞", dbMySqlAccess.Find<DbMaintenanceTestInfo>(10).UserName);
        }

        [Test]
        public void DeleteTest()
        {
            //15 被删除了，返回null
            dbMySqlAccess.DeleteModel(new DbMaintenanceTestInfo() { AutoID = 15 });
            Assert.AreEqual(null, dbMySqlAccess.Find<DbMaintenanceTestInfo>(15));
        }

        [Test]
        public void DeleteTest2()
        {
            dbMySqlAccess.Delete<Students>("李四");
            Assert.AreEqual(0, dbMySqlAccess.GetCount<Students>("Name='李四'"));
        }

        [Test]
        public void BulkInsert()
        {
            //5.9秒 效率还是很高的
            Stopwatch watch = new Stopwatch();
            watch.Start();

            var dt = new DataTable();
            dt.Columns.Add("AutoID", typeof(int));
            dt.Columns.Add("UserName", typeof(string));

            for (var i = 0; i < 1000000; i++)
            {
                var dr = dt.NewRow();
                dr["AutoID"] = 0;
                dr["UserName"] = "jsonlee" + i.ToString();
                dt.Rows.Add(dr);
            }

            mysql.BulkInsert<DbMaintenanceTestInfo>(dt);

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
            dbMySqlAccess.ExecTrans(lstSql);
            Assert.AreEqual("jsonlee", dbMySqlAccess.Find<DbMaintenanceTestInfo>(1).UserName);
        }

        //异步测试 并不一定是按从上往下顺序执行的，多线程执行
        [Test]
        public async Task GetAsyncTest()
        {
            //写原生sql要区分不同数据库的语法特性，用ef写linq就不会考虑这些
            var valTask = dbMySqlAccess.GetValueAsync<string>("select UserName from DbMaintenanceTest limit 1");
            var modelTask = dbMySqlAccess.GetModelAsync<DbMaintenanceTestInfo>("select * from DbMaintenanceTest limit 1");
            var lstTask = dbMySqlAccess.GetListAsync<DbMaintenanceTestInfo>("select * from DbMaintenanceTest limit 10");
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
