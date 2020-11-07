# SinGooCMSAdo
SinGooCMS.Ado 支持netstandard2.1和net framework 4.6.1，原生sql语句操作，支持SqlServer、MySql、Sqlite、Oracle

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
  //"SQLConnSTR": "data source=localhost:1521;user id=jsonlee;password=123;;min pool size=5;max pool size=512;"
  //"DistributedCacheConnStr": "server=(local);database=SinGooCMS-v1.6;uid=sa;pwd=123;Application Name=SinGooCMS;pooling=true;min pool size=5;max pool size=512;connect timeout = 20;",
  "RedisExchangeHosts": "127.0.0.1:6379,allowadmin=true"
}

//.net framework配置
<connectionStrings>
  <add name="SQLConnSTR" providerName="SqlServer" connectionString="server=(local);database=SinGooCMS-v1.6;uid=sa;pwd=123"/>
  <!--<add name="SQLConnSTR" providerName="MySql" connectionString="server=localhost;database=SinGooCMSv16;uid=root;pwd=123456;SslMode=None;"/>
  <add name="SQLConnSTR" providerName="Oracle" connectionString="data source=localhost:1521;user id=jsonlee;password=123;min pool size=5;max pool size=512;" />
  <add name="SQLConnSTR" providerName="Sqlite" connectionString="Data Source=db/singootop.db"/>-->
</connectionStrings> 

```
ProviderName 支持四种数据库：SqlServer、MySql、Sqlite、Oracle
SQLConnSTR 数据库连接字符串

# 3、使用
```c#
//使用默认的字符串
IDbAccess dbSqlServerAccess => DbProvider.DbAccess;
//也可以使用指定的连接字符串
IDbAccess dbSqlServerAccess => DbProvider.Create(DbProviderType.SqlServer, "server=(local);database=SinGooCMS-v1.6;uid=sa;pwd=123");
//使用其它数据库
IDbAccess dbMySqlAccess => DbProvider.Create(DbProviderType.MySql, "server=localhost;database=singoocmsv16;uid=root;pwd=123456;SslMode=None;");

//CRUD
//查询
var model=dbSqlServerAccess.Find<DbMaintenanceTestInfo>(10);
var model=dbSqlServerAccess.FindAsync<DbMaintenanceTestInfo>(10);

var model=dbSqlServerAccess.GetModel<DbMaintenanceTestInfo>("select * from cms_User where UserName='jsonlee'");
var model=dbSqlServerAccess.GetModelAsync<DbMaintenanceTestInfo>("select * from cms_User where UserName='jsonlee'");

DataTable dt=dbSqlServerAccess.GetDataTable("select * from cms_User");

//分页
var pagerData=dbSqlServerAccess.GetPagerList<DbMaintenanceTestInfo>("", "AutoID desc", 2, 10);
var pagerData=dbSqlServerAccess.GetPagerListAsync<DbMaintenanceTestInfo>("", "AutoID desc", 2, 10);

//增加
dbSqlServerAccess.InsertModel(new DbMaintenanceTestInfo() { UserName = "jsonlee" });
dbSqlServerAccess.InsertModelAsync(new DbMaintenanceTestInfo() { UserName = "jsonlee" });

//修改
dbSqlServerAccess.UpdateModel(new DbMaintenanceTestInfo() { AutoID=10,UserName="刘备" });
dbSqlServerAccess.UpdateModelAsync(new DbMaintenanceTestInfo() { AutoID=10,UserName="刘备" });

//删除
dbSqlServerAccess.DeleteModel(new DbMaintenanceTestInfo() { AutoID=15 });
dbSqlServerAccess.DeleteModelAsync(new DbMaintenanceTestInfo() { AutoID=15 });

dbSqlServerAccess.DeleteModel<DbMaintenanceTestInfo>(15);
dbSqlServerAccess.DeleteModelAsync<DbMaintenanceTestInfo>(15);

关于实体类特性
[Table("DbMaintenanceTest")]
public class DbMaintenanceTestInfo
{
    [Key]
    public int AutoID { get; set; }
    public string UserName { get; set; }
}

Table("数据库表名") 表示实体类映射的数据库的表名
Key 表示当前列是主键列
NotMapped 表示不需要映射到数据库字段，一般是自定义的属性

关于主键
1）如果主键是自增整型，应该加上 NotMapped 特性，这样插入数据是会忽略些列
2）非自增需要插入数据的主键，不要有 NotMapped 特性。

移除性能分析

```
2020-11-07
