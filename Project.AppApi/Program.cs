using Core;
using MvcCore.Extension.Swagger;
using MvcCore.Extension;
using IService;
using Service;
using MvcCore.Extension.Filter;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;
using Model.EnumModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MvcCore.Extension.Auth;
using IService.App;
using Service.App;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using System.Configuration;
using Newtonsoft.Json.Linq;

var ApiName = "Project.AppApi";

var builder = WebApplication.CreateBuilder(args);

//���ÿ���
//�O��Cors�����r��
builder.Services.AddCors(options =>
{
    //options.AddDefaultPolicy(builders =>
    //{
    //    builders
    //        .AllowAnyOrigin()  //������Դ
    //        .AllowAnyHeader()  //���б�ͷ
    //        .AllowAnyMethod();  //���� HTTP ����
    //});

    //options.AddPolicy("cors", builders =>
    //{
    //    builders
    //        .AllowAnyOrigin()  //������Դ
    //        .AllowAnyHeader()  //���б�ͷ
    //        .AllowAnyMethod();  //���� HTTP ����
    //        //.AllowCredentials();   //��һ����������CORS��ѡ�����ָʾ������Ƿ�����ͻ����ڿ��������а���ƾ����Ϣ���� Cookies��Authorization ��ͷ�ȡ�
    //});


    options.AddPolicy("cors", builders =>
    {
        builders
            .WithOrigins(builder.Configuration.GetValue<string>("ConfigSettings:Orgins").Split(','))  //������Դ�������Ҫ�������Ŀ����������
            .AllowAnyHeader()  //���б�ͷ
            .AllowAnyMethod()  //���� HTTP ����
            .AllowCredentials();   //��һ����������CORS��ѡ�����ָʾ������Ƿ�����ͻ����ڿ��������а���ƾ����Ϣ���� Cookies��Authorization ��ͷ�ȡ�
    });
});

//��ȡ�����ַ���
GlobalConfig.ConnectionString = builder.Configuration.GetValue<string>("ConnectionStrings:SqlServer");

// Add services to the container.
builder.Services.AddControllers();


//���ô���Ϊ,�����ûᵼ�£�������������ʾΪ��
builder.Services.AddControllers(
    options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

//�Ƿ���Swagger
var getconfig = builder.Configuration.GetValue<bool>("ConfigSettings:SwaggerEnable");
//Swagger
if (getconfig)
{
    builder.Services.AddSwaggerGens(ApiName, new string[] { "ViewModel.xml" });
}

//netocreĬ��ʹ��Newtonsoft.Json��ΪJson����������3.0+������Ĭ�ϣ�����ʹ��System.Text.Json�滻Newtonsoft.Json
//builder.Services.AddControllers().AddNewtonsoftJson();


// ���ӿ������������ʹ��������� ע��Ϊȫ�ֹ�����
builder.Services.AddMvc(options =>
{
    //����������
    options.Filters.Add(typeof(ErrorFilterAttribute));
    //�ӿ�����������
    options.Filters.Add(typeof(ApiFilterAttribute));
    //��Ȩ��֤������
    options.Filters.Add(typeof(AuthValidator));
});

#region SerilLog����

//SerilLog��Service�����ô�NuGet��
//ThreadId��Ҫ����ר�õ�NuGet��
//const string OUTPUT_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} <{ThreadId}> [{Level:u3}] {Message:lj}{NewLine}{Exception}";
const string OUTPUT_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";


var columnOpts = new ColumnOptions
{
    AdditionalColumns = new Collection<SqlColumn>
    {
        ////Ψһ���
        //new SqlColumn{ColumnName = "Guid", PropertyName = "Guid", DataType = SqlDbType.NVarChar, DataLength = 32, AllowNull = false},
        ////Ո��͑���� APP CMS
        //new SqlColumn{ColumnName = "ClientType", DataType = SqlDbType.NVarChar, DataLength = 10, AllowNull = false},
        ////API����
        //new SqlColumn{ColumnName = "APIName", DataType = SqlDbType.NVarChar, DataLength = 200, AllowNull = false},
        ////����ʽ POST GET��
        //new SqlColumn{ColumnName = "Request", DataType = SqlDbType.NVarChar, DataLength = 20, AllowNull = false},
        ////�û����
        //new SqlColumn{ColumnName = "UserId", DataType = SqlDbType.Int, AllowNull = false},
        ////�豸Ψһ���,����У�Ĭ��0
        //new SqlColumn{ColumnName = "DeviceId", DataType = SqlDbType.Int, AllowNull = true},
        ////����˵��
        //new SqlColumn{ColumnName = "Instructions", DataType = SqlDbType.NVarChar, DataLength = 200, AllowNull = false},
        ////�����������
        //new SqlColumn{ColumnName = "ReqParameter", DataType = SqlDbType.NVarChar, DataLength = -1, AllowNull = false},
        ////���ز�������
        //new SqlColumn{ColumnName = "ResParameter", DataType = SqlDbType.NVarChar, DataLength = -1, AllowNull = true},
        ////�ķ�ʱ��
        //new SqlColumn{ColumnName = "Time", DataType = SqlDbType.NVarChar, DataLength = 20, AllowNull = true},
        ////�����û�IP
        //new SqlColumn{ColumnName = "IP", DataType = SqlDbType.NVarChar, DataLength = 20, AllowNull = true},
        // //����������(���ؾ����¼)
        //new SqlColumn{ColumnName = "Server", DataType = SqlDbType.NVarChar, DataLength = 50, AllowNull = false}


         //Ψһ���
        new SqlColumn{ColumnName = "Guid", PropertyName = "Guid", DataType = SqlDbType.NVarChar, DataLength = 32, AllowNull = true},
        //Ո��͑���� APP CMS
        new SqlColumn{ColumnName = "ClientType", DataType = SqlDbType.NVarChar, DataLength = 10, AllowNull = true},
        //API����
        new SqlColumn{ColumnName = "APIName", DataType = SqlDbType.NVarChar, DataLength = 200, AllowNull = true},
        //����ʽ POST GET��
        new SqlColumn{ColumnName = "Request", DataType = SqlDbType.NVarChar, DataLength = 20, AllowNull = true},
        //�û����
        new SqlColumn{ColumnName = "UserId", DataType = SqlDbType.Int, AllowNull = true},
        //�豸Ψһ���,����У�Ĭ��0
        new SqlColumn{ColumnName = "DeviceId", DataType = SqlDbType.Int, AllowNull = true},
        //����˵��
        new SqlColumn{ColumnName = "Instructions", DataType = SqlDbType.NVarChar, DataLength = 200, AllowNull = true},
        //�����������
        new SqlColumn{ColumnName = "ReqParameter", DataType = SqlDbType.NVarChar, DataLength = -1, AllowNull = true},
        //���ز�������
        new SqlColumn{ColumnName = "ResParameter", DataType = SqlDbType.NVarChar, DataLength = -1, AllowNull = true},
        //�ķ�ʱ��
        new SqlColumn{ColumnName = "Time", DataType = SqlDbType.NVarChar, DataLength = 20, AllowNull = true},
        //�����û�IP
        new SqlColumn{ColumnName = "IP", DataType = SqlDbType.NVarChar, DataLength = 20, AllowNull = true},
         //����������(���ؾ����¼)
        new SqlColumn{ColumnName = "Server", DataType = SqlDbType.NVarChar, DataLength = 50, AllowNull = true}
    }
};


//columnOpts.Store.Remove(StandardColumn.Message);  //��־��Ϣ���ı����ݣ�������ɶ�����־��Ϣ��
columnOpts.Store.Remove(StandardColumn.Properties);//�ṹ����־�е����Լ��ϡ�����ʹ�� Serilog ��¼�ṹ����־ʱ����������ڱ�ʾ���ӵĽṹ�����ݡ�
columnOpts.Store.Remove(StandardColumn.MessageTemplate);//��־��Ϣ��ģ�壬����־��Ϣ�ĸ�ʽģ�塣���ģ����԰���ռλ����������Ⱦ��Ϣ�ı��ͽṹ�����ݡ�

//columnOpts.Store.Add(StandardColumn.LogEvent);
//columnOpts.LogEvent.DataLength = 2048;
//columnOpts.PrimaryKey = columnOpts.TimeStamp;
columnOpts.TimeStamp.NonClusteredIndex = true; //����Ϊ�Ǿ�������

//BatchPeriod
string interval = "00:00:05"; //��ʾ5��
TimeSpan ts;
TimeSpan.TryParse(interval, out ts);

//�����־�ȼ�, ���Խ�ֹ��� ASP.NET Core Ӧ�ó�������ʱ��¼�ģ�������ͨ��Ĭ�ϵ���־��¼������ģ�Information��
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() //������־��¼������С����Ϊ Debug����ֻ��¼ Debug��Information��Warning��Error �� Fatal �������־�¼���
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)//�� Microsoft �����ռ��µ�������־�¼�������д������С��������Ϊ Information����ֻ��¼ Information��Warning��Error �� Fatal �������־�¼���
                                                              //.ReadFrom.Configuration(new ConfigurationBuilder().AddJsonFile("appsettings.json").AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "PRODUCTION"}.json", optional: true).Build())
    .Enrich.FromLogContext() //������־�����Ĺ��ܣ��Զ���ȡ��ǰ�̺߳ͷ�����һЩ��Ϣ������ӵ�ÿ����־�¼��С�
    .WriteTo.Console(outputTemplate: OUTPUT_TEMPLATE)
    .WriteTo.File("logs/app.txt"
        , rollingInterval: RollingInterval.Day,
         rollOnFileSizeLimit: true, // ����־�ļ���С����ָ����Сʱ�Զ�������־�ļ�
         fileSizeLimitBytes: 1048576, // ��־�ļ�����СΪ 1MB
          retainedFileCountLimit: 7, // ��ౣ�� 7 �����־�ļ�
          outputTemplate: OUTPUT_TEMPLATE)
#region SerilLog�Ƿ���Ҫ����SqlServer
    //.AuditTo.MSSqlServer(
    //    connectionString: GlobalConfig.ConnectionString,
    //    sinkOptions: new MSSqlServerSinkOptions { TableName = "SystemLog", SchemaName = "dbo", AutoCreateSqlTable = true, BatchPeriod = ts, BatchPostingLimit = 50 },
    //    columnOptions: columnOpts)
#endregion
    .CreateLogger();

#region SerilLogд�����ݿ�Demo
//WriteTo��Ч AuditTo����Ч
//BatchPostingLimit: ����������������־�¼����������ơ�Ĭ��ֵΪ 50�������ۻ��� 50 ����־�¼�ʱ�ͻὫ������Ϊһ�����ν���д�����ݿ⡣���ѡ����԰����Ż����ܣ���Ϊһ���ύ��������־�¼���һ���ύ��������־�¼�Ч�ʸ��ߡ�
//BatchPeriod: ���������������ʱ������Ĭ��ֵΪ 2 �룬��ÿ�� 2 ��ͻὫ�����ѻ������־�¼���Ϊһ�����ν���д�����ݿ⡣���ѡ����Ա�֤��һ����ʱ������һ���������ݿ��ύ��־�¼����Ա�֤���ݵ�ʵʱ�Ժ������ԡ�

//Log.Logger = new LoggerConfiguration()
//    .WriteTo
//    .MSSqlServer(
//        connectionString: GlobalConfig.ConnectionString,
//        sinkOptions: new MSSqlServerSinkOptions { TableName = "testnew" ,SchemaName="dbo",AutoCreateSqlTable=true,BatchPostingLimit=1},
//        columnOptions: columnOpts)
//    .CreateLogger();


//������־���
//Log.Information("Hello {Name} from thread {ThreadId}", Environment.GetEnvironmentVariable("USERNAME"), Environment.CurrentManagedThreadId);
//Log.Warning("No coins remain at position {@Position}", new { Lat = 25, Long = 134 });
//Log.Error("{UserName}{UserId}{RequestUri}", 1, 2, 3);
#endregion

//ע�� �滻Ĭ����־
builder.Host.UseSerilog(Log.Logger, dispose: true);
#endregion


#region ����ע��
//ע�� MVC��Model-View-Controller������
builder.Services.AddControllersWithViews();

//GlobalConfig����ע��
//ע��������־
//GlobalConfig.SystemLogService()

// ����ע�����
builder.Services.Scan(scan => scan
          .FromAssemblyOf<BaseService>() // �� Startup �����ڵĳ��򼯿�ʼɨ��
          .AddClasses(classes => classes.AssignableTo<IBaseService>()) // ɨ��ʵ���� IService �ӿڵ���
              .AsImplementedInterfaces() // ע����Щ��ʵ�ֵ����нӿ�
              .WithScopedLifetime()); // ʹ��ָ�����������ڽ���ע�ᣬ������ Scoped ��������ʾ��

////ע��DB����
//builder.Services.AddScoped<IRepository, Repository>();
////ע��Log
//builder.Services.AddScoped<ISystemLogService, SystemLogService>();
////ע���û���
//builder.Services.AddScoped<IAppUserService, AppUserService>();
////ע��ֲ����
//builder.Services.AddScoped<IPlantService, PlantService>();
////ע���������
//builder.Services.AddScoped<IOtherSystemService, OtherSystemService>();
////ע��WebHooks��
//builder.Services.AddScoped<IWebhooksService, WebhooksService>();
////ע��EasyCard��
//builder.Services.AddScoped<IEasyCardServices, EasyCardServices>();
////ע��Config��
//builder.Services.AddScoped<IConfigService, ConfigService>();

//ע��httphelper
builder.Services.AddTransient(typeof(HttpHelper));

//��� HttpClientFactory ����
builder.Services.AddHttpClient();

#endregion

#region jwt�����֤
//����J�C
//builder.Services.AddAuthentications(builder.Configuration.GetValue<string>("ConfigSettings:Domain"), "/api/appuser/denied");
builder.Services.AddAuthentications(builder.Configuration);
#endregion


//����Ҫʹ�õ��ļ��У��ڴ˴��������ж��ļ����Ƿ����,�������򴴽�
string folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"other");
if (!Directory.Exists(folderPath))
{
    Directory.CreateDirectory(folderPath);
}

//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(folderPath, "keys")))
//    .ProtectKeysWithDpapi();// �� DPAPI ������Կ�����滻Ϊ�����ʺϵı�������,ֻ����windows�����Linux �� macOS �ȷ� Windows �����£�ʹ��.ProtectKeysWithCertificate("thumbprint");

var app = builder.Build();

// Configure the HTTP request pipeline. ���� http �D�� https��
//ʹ��http,���Խ�ֹʹ��
//app.UseHttpsRedirection();

//�����һ�N�汾��ՈҪConfigureService�����÷��� services.AddCors();
//app.UseCors();
app.UseCors("cors");

//�Ƿ���Swagger
if (getconfig)
{
    app.UseSwaggers(ApiName);
}

app.UseStaticFiles(new StaticFileOptions()
{
    //ָ��Ҫ�����ľ�̬�ļ����ڵ�Ŀ¼·�����ڴ�ʾ���У�������ǰ����Ŀ¼�µġ�Files����Ŀ¼ָ��Ϊ��̬�ļ��ĸ�Ŀ¼��
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"other")),

    //ָ�������ڷ��ʾ�̬�ļ��� URL ·�����ڴ�ʾ���У����� "/Files" ·��ӳ�䵽 "Files" Ŀ¼��
    RequestPath = new PathString("/other"),

    //һ������ֵ��ָ���Ƿ�Ӧ����������ļ�����δ֪ʱ�������̬�ļ���Ĭ������£������������Ϊ false����ʾֻ����֪ MIME ���͵��ļ��Żᱻ�����������Ϊ true����δ֪�ļ�����Ҳ���Ա�����
    ServeUnknownFileTypes = true,

    //����ָ���ض��ļ���չ���� MIME ���͵��ֵ䡣���������ļ���չ������б�ƥ�䣬�����������һ�� 404 ��Ӧ���ڴ�ʾ���У�ע�͵���������ԣ����������Ӧ���ļ����ͽ��������ļ���չ���Զ�ʶ��
    //ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
    //{
    //     { ".apk","application/vnd.android.package-archive"}
    //})

    //OnPrepareResponse = ctx =>
    //{
    //    // ����ļ����Ƿ���ڣ�����������򴴽�
    //    string filePath = Path.Combine(ctx.Context.Request.Path.Value, ctx.File.Name);
    //    string folderPath = Path.GetDirectoryName(filePath);
    //    if (!Directory.Exists(folderPath))
    //    {
    //        Directory.CreateDirectory(folderPath);
    //    }
    //}
});


app.Use(async (context, next) =>
{
    //��ʾ��API��ʲô��
    context.Request.Headers.Add("ClientType", "APP");

    //ע��Guidÿ������Ψһ����
    context.Request.Headers.Add("Guid", Guid.NewGuid().ToString("N"));

    //��ȡĬ������
    string language = context.QueryOrHeaders("language");
    if (string.IsNullOrEmpty(language))
    {
        language = ((int)LanguageEnum.CN).ToString();
    }
    context.SetHeaders("Language", language);


    //Token
    //context.SetHeaders("Token", context.QueryOrHeaders("Token"));
    string Token = context.QueryOrHeaders("Token");
    //�� Token ֵ���� Authorization ����ͷ
    if (!string.IsNullOrEmpty(Token))
    {
        context.Request.Headers["Authorization"] = "Bearer " + Token;
    }
    //context.SetHeaders("Authorization", Token);

    //string Authorization = context.QueryOrHeaders("Authorization");
    //if (!string.IsNullOrEmpty(Authorization))
    //{
    //    context.Request.Headers["Token"] = Authorization;
    //}
    //context.SetHeaders("Token", Token);
    await next();
});

app.UseRouting();

//ע��˳������֤����Ȩ����Ȼ�ӿڼ���Token��֤Ҳ����ͨ��
app.UseAuthentication();//������֤   

app.UseAuthorization();//������Ȩ

////��� JWT �쳣�����м��
//app.UseMiddleware<AuthValidator>();

app.MapControllers();

app.Run();