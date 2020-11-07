# SinGooCMSAdo
SinGooCMS.Ado 支持netstandard2.1和net framework 4.6.1，原生sql语句操作，支持SqlServer、MySql、Sqlite

# 1、引用 SinGooCMS.Ado
```c#
using SinGooCMS.Ado;
using SinGooCMS.Ado.Interface;
```
项目是开源的，所有你可以改个命名空间！

# 2、配置 appsetting.json
```c#
//.net core配置
"ConnectionStrings": {
  "ProviderName": "Sqlite",
  //"SQLConnSTR": "server=(local);database=SinGooCMS-v1.6;uid=sa;pwd=123;Application Name=SinGooCMS;pooling=true;min pool size=5;max pool size=512;connect timeout = 20;",
  //"SQLConnSTR": "server=localhost;database=SinGooCMSv16;uid=root;pwd=123456;SslMode=None;",
  "SQLConnSTR": "Data Source=db/singootop.db",
  //"DistributedCacheConnStr": "server=(local);database=SinGooCMS-v1.6;uid=sa;pwd=123;Application Name=SinGooCMS;pooling=true;min pool size=5;max pool size=512;connect timeout = 20;",
  "RedisExchangeHosts": "127.0.0.1:6379,allowadmin=true"
}

//.net framework配置
<connectionStrings>
  <add name="SQLConnSTR" providerName="SqlServer" connectionString="server=(local);database=SinGooCMS-v1.6;uid=sa;pwd=123"/>
  <!--<add name="SQLConnSTR" providerName="MySql" connectionString="server=localhost;database=SinGooCMSv16;uid=root;pwd=123456;SslMode=None;"/>
  <add name="SQLConnSTR" providerName="Sqlite" connectionString="Data Source=db/singootop.db"/>-->
</connectionStrings> 

```
ProviderName 支持三种数据库：SqlServer、MySql、Sqlite
SQLConnSTR 数据库连接字符串

# 3、使用
```c#
//使用默认的字符串
IDbAccess dbSqlServerAccess => DbProvider.DbAccess;
//也可以使用指定的连接字符串
IDbAccess dbSqlServerAccess => DbProvider.Create("SqlServer", "server=(local);database=SinGooCMS-v1.6;uid=sa;pwd=123");
//使用其它数据库
IDbAccess dbMySqlAccess => DbProvider.Create("MySql", "server=localhost;database=singoocmsv16;uid=root;pwd=123456;SslMode=None;");

//CRUD
//查询
var model=dbSqlServerAccess.GetModel<DbMaintenanceTestInfo>(10);
var model=dbSqlServerAccess.GetModelAsync<DbMaintenanceTestInfo>(10);

var pagerData=dbSqlServerAccess.GetPagerList<DbMaintenanceTestInfo>("", "AutoID desc", 2, 10, ref totalCount, ref totalPage);
var pagerData=dbSqlServerAccess.GetPagerListAsync<DbMaintenanceTestInfo>("", "AutoID desc", 2, 10, ref totalCount, ref totalPage);

//增加
dbSqlServerAccess.InsertModel(new DbMaintenanceTestInfo() { UserName = "jsonlee" });
dbSqlServerAccess.InsertModelAsync(new DbMaintenanceTestInfo() { UserName = "jsonlee" });

//修改
dbSqlServerAccess.UpdateModel(new DbMaintenanceTestInfo() { AutoID=10,UserName="刘备" });
dbSqlServerAccess.UpdateModelAsync(new DbMaintenanceTestInfo() { AutoID=10,UserName="刘备" });

//删除
dbSqlServerAccess.DeleteModel(new DbMaintenanceTestInfo() { AutoID=15 });
dbSqlServerAccess.DeleteModelAsync(new DbMaintenanceTestInfo() { AutoID=15 });

关于实体类特性
[Table("DbMaintenanceTest")]
public class DbMaintenanceTestInfo
{
    [Key]
    public int AutoID { get; set; }
    public string UserName { get; set; }
}

Table("数据库表名") 表示实体类映射的数据库的表名
Key 表示当前列是主键列，一般主键列都是自增列
NotMapped 表示不需要映射到数据库字段，一般是自定义的属性

关于性能分析
从2.3版本开始，添加了MiniProfiler性能分析工具，在对系统优化时可以根据性能报告找出需要优化的地方。

//使用方法，第一个参数是指定数据库类型，第二个参数是自定义的连接字符串，第三个参数是标示“是否开启分析”
IDbAccess dbSqlServerAccess => DbProvider.Create("SqlServer", "server=(local);database=SinGooCMS-v1.6;uid=sa;pwd=123", true);

如此在程序运行时，会记录下程序运行性能，并把文件保存到如：/bin/Debug/netcoreapp3.1/singoocms.ado.profiler-2020-08-27.txt，内容如下

JSONLEE at 2020-08-27 00:40:34
SinGooCMS-Ado-GetObject 738.8ms (sql = 442.8ms in 2 cmds)
JSONLEE at 2020-08-27 00:40:34
SinGooCMS-Ado-GetObject 47.6ms (sql = 38ms in 2 cmds)

注意：在正式生产环境中，请不要使用性能分析，因为写日志需要IO操作，会影响运行效率！

```
如果大家觉得可以用，麻烦点个星！
