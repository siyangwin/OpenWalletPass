using Core;
using IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PassKitHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ViewModel;

namespace Service.App
{
    /// <summary>
    /// Pass操作
    /// </summary>
    public class WalletPassService:IWalletPassService
    {

        private readonly IHttpClientFactory httpClientFactory;

        //读取配置文件
        private readonly IConfiguration configuration;


        /// <summary>
        /// 注入
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="configuration"></param>
        public WalletPassService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        #region Apple操作
        #region 创建/更新
        /// <summary>
        ///  AppleWallet生成/更新信息
        /// </summary>
        /// <param name="serialNumber">唯一编号</param>
        /// <param name="httpContext">请求信息,用于记录</param>
        /// <returns></returns>
        public async Task<string> PassCreateAsync(string serialNumber, HttpContext httpContext)
        {
            //判断文件夹是否存在,不存在则创建
            string Path = $"{AppContext.BaseDirectory}Files/Wallet/AppleWallet/";
            try
            {
                //获取配置数据
                //Apple名片团队ID
                string TeamIdentifier = configuration["Apple:TeamIdentifier"] ?? "";
                //Apple名片类型ID
                string PassTypeIdentifierConfig = configuration["Apple:PassTypeIdentifier"] ?? "";
                //Apple名片验证Token
                string AuthenticationTokenConfig = configuration["Apple:AuthenticationToken"] ?? "";
                //Apple回調Api地址
                string walletApi = configuration["Apple:WalletApi"] ?? "";
                //获取证书密钥
                string CerPwd= configuration["Apple:CerPwd"] ?? "";

                //读取本地证书
                var options = new PassKitOptions()
                {
                    PassCertificate = new X509Certificate2(File.ReadAllBytes("Cer/pass.p12"), CerPwd),
                    AppleCertificate = new X509Certificate2(File.ReadAllBytes("Cer/AppleWWDRCAG4.cer")),
                    ConfigureNewPass = p => p.Standard.PassTypeIdentifier(PassTypeIdentifierConfig).TeamIdentifier(TeamIdentifier)
                };

                //如果不为空,则为更新
                if (string.IsNullOrEmpty(serialNumber))
                {
                    //每次生成一个唯一的SN,实际使用要存起来。
                    serialNumber = Guid.NewGuid().ToString().Replace("-", "").ToLower();
                }

                //随机数，可以看出区别
                Random random = new Random();
                int randomNumber = random.Next(0, 1000);

                //创建Passkit
                IPassKitHelper passKitHelper = new PassKitHelper.PassKitHelper(options);
                var pass = passKitHelper.CreateNewPass()
                    .Standard
                    .SerialNumber(serialNumber)
                    .OrganizationName("PassKit")
                    .Description("Membership")
                    .VisualAppearance
                    .Barcodes("1234567890128", BarcodeFormat.QR, "CardNo")
                    .LogoText("CardNo")
                    .ForegroundColor("rgb(255, 255, 255)")
                    .BackgroundColor("rgb(197, 31, 31)")
                    .LabelColor("rgb(236, 240, 241)")
                    .Generic.PrimaryFields.Add("member").Value("SiYang Liu(XX)")
                    .SecondaryFields.Add("subtitle").Label("MEMBER SINCE").Value("2012.06")
                    .AuxiliaryFields.Add("level").Label("LEVEL").Value("Platinum").Add("favorite").Label("POINT").Value(randomNumber).TextAlignment(TextAlignment.Right)
                    .BackFields.Add("numberStyle").Label("spelled out").Value("200").NumberStyle(NumberStyle.SpellOut)
                    .WebService.AuthenticationToken(AuthenticationTokenConfig).WebServiceURL(walletApi);

                //加入图片
                var passPackage = passKitHelper.CreateNewPassPackage(pass)
                    .Icon(await File.ReadAllBytesAsync("images/icon.png"))
                    .Icon2X(await File.ReadAllBytesAsync("images/icon@2x.png"))
                    .Logo(await File.ReadAllBytesAsync("images/logo.png"))
                    .Logo2X(await File.ReadAllBytesAsync("images/logo@2x.png"))
                    .Thumbnail(await File.ReadAllBytesAsync("images/thumbnail.png"))
                    .Thumbnail2X(await File.ReadAllBytesAsync("images/thumbnail@2x.png"));

                MemoryStream packageFile = await passPackage.SignAndBuildAsync();

                //保存文件
                if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
                Path = Path + serialNumber + ".pkpass";

                // Now you have to "deliver" package file to user using any channel you have
                // (save as attachment in email, download from your webapp etc)
                await File.WriteAllBytesAsync(Path, packageFile.ToArray());

                //推送更新
                try
                {
                    //查询当前SN绑定的用户列表
                    // List<string> PushToken = connection.QuerySet<Model.ViewCms.vw_cms_ContactsList>()
                    //.Where(x => x.DeviceType == (int)DeviceTypeEnum.Apple && x.SerialNumber == serialNumber)
                    // .ToList(s => s.PushToken);

                    List<string> PushToken=new List<string>();
                    if (PushToken != null && PushToken.Count() > 0)
                    {
                        int PushTokenUpdate = 0;
                        foreach (var item in PushToken)
                        {
                            ResultModel resultModel1 = await PassUpdate(item);//更新
                            if (resultModel1.success)
                            {
                                PushTokenUpdate++;
                            }
                        }

                        Console.WriteLine("Apple Wallet待更新数据：" + PushToken.Count() + ",成功数据：" + PushTokenUpdate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Apple Wallet更新"+ JsonConvert.SerializeObject(ex));
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { error = new { code = 400, message = ex.Message } });
            }
            return Path;
        }

        #endregion

        #region 主动推送更新状态到Apple
        /// <summary>
        /// AppleWallet推送更新
        /// </summary>
        /// <returns></returns>
        public async Task<ResultModel> PassUpdate(string PushToken)
        {
            ResultModel resultModel = new ResultModel();

            //Apple名片团队ID
            string TeamIdentifier = configuration["Apple:TeamIdentifier"] ?? "";
            //Apple名片类型ID
            string PassTypeIdentifierConfig = configuration["Apple:PassTypeIdentifier"] ?? "";
            //获取证书密钥
            string CerPwd = configuration["Apple:CerPwd"] ?? "";

            var options = new PassKitOptions()
            {
                PassCertificate = new X509Certificate2(File.ReadAllBytes("Cer/pass.p12"), CerPwd),
                AppleCertificate = new X509Certificate2(File.ReadAllBytes("Cer/AppleWWDRCAG4.cer")),
                ConfigureNewPass =
               p => p.Standard
               .PassTypeIdentifier(PassTypeIdentifierConfig)
               .TeamIdentifier(TeamIdentifier)
            };

            IPassKitHelper passKitHelper = new PassKitHelper.PassKitHelper(options);

            bool resule = await passKitHelper.SendPushNotificationAsync(PushToken);

            resultModel.success = resule;

            resultModel.message = "推送成功";

            if (!resultModel.success)
            {
                resultModel.message = "推送失败";
            }

            return resultModel;
        }
        #endregion

        #region 下载
        /// <summary>
        /// 获取Apple Wallet文件
        /// </summary>
        /// <param name="SerialNumber">编号</param>
        /// <param name="P">验证密钥</param>
        /// <param name="httpContext">请求参数</param>
        /// <returns></returns>
        public async Task<IActionResult> GetWalletByAppleFile(string SerialNumber, string P, HttpContext httpContext)
        {
            ////验证密钥，不允许访问
            //if (!Check(P, httpContext))
            //{
            //    return null;
            //}

            //查询SN的准确性对应文件。
            //if (PassKit == null) return null;

            //获取服务器域名
            //string resourcesUrl = configService.Get("Resource:PassUrl", false).FirstOrDefault()?.Values ?? "";
            string resourcesUrl = configuration["Resource:PassUrl"] ??"";

            if (string.IsNullOrEmpty(resourcesUrl)) return null;

            //替换为你要获取文件的URL路径
            var url = resourcesUrl + "/"+ SerialNumber+ ".pkpass";
            
            //获取文件
            return await GetpkpassFiles(url);
        }
        #endregion

        #region Apple回调
        /// <summary>
        /// AppleWallet回调
        /// 注册设备发送PushToken
        /// </summary>
        /// <param name="deviceLibraryIdentifier">设备唯一编号</param>
        /// <param name="passTypeIdentifier">证书passTypeId</param>
        /// <param name="serialNumber">通行证创建时候的编号</param>
        /// <param name="PushToken">推送更新Token</param>
        /// <param name="Authorization">创建名片的时候加入的验证Token</param>
        /// <returns></returns>
        public string PassRegisterCallBackByApple(string deviceLibraryIdentifier, string passTypeIdentifier, string serialNumber, string PushToken, string Authorization)
        {
           
            //获取配置
            //Apple名片类型ID
            string PassTypeIdentifierConfig = configuration["Apple:PassTypeIdentifier"] ?? "";
            //Apple名片验证Token
            string AuthenticationTokenConfig = configuration["Apple:AuthenticationToken"] ?? "";

            //Check
            if (passTypeIdentifier != PassTypeIdentifierConfig || Authorization != "ApplePass " + AuthenticationTokenConfig)
            {
                return "Error";
            }

            //此处应该将PushToken、设备唯一编号和serialNumber的对应关系写入数据库，PushToken之后用于更新PassKit内容
            bool res = true;

            //存储Apple信息
            string message = "Register---" + deviceLibraryIdentifier + "---" + passTypeIdentifier + "---" + serialNumber + "---" + PushToken + "---" + res;
            return message;
        }


        /// <summary>
        /// AppleWallet删除回调
        /// 注销设备
        /// </summary>
        /// <param name="deviceLibraryIdentifier">设备唯一编号</param>
        /// <param name="passTypeIdentifier">证书passTypeId</param>
        /// <param name="serialNumber">通行证创建时候的编号</param>
        /// <param name="Authorization">创建名片的时候加入的验证Token</param>
        /// <returns></returns>
        public string PassDeleteCallbackByApple(string deviceLibraryIdentifier, string passTypeIdentifier, string serialNumber, string Authorization)
        {
            //获取配置
            //Apple名片类型ID
            string PassTypeIdentifierConfig = configuration["Apple:PassTypeIdentifier"] ?? "";
            //Apple名片验证Token
            string AuthenticationTokenConfig = configuration["Apple:AuthenticationToken"] ?? "";


            if (passTypeIdentifier != PassTypeIdentifierConfig || Authorization != "ApplePass " + AuthenticationTokenConfig)
            {
                return "Error";
            }

            //删除Apple信息
            //此处应该将PushToken、设备唯一编号和serialNumber的对应关系从数据库删除，更新不需要再发送给该设备。
            bool res = true;

            //存储Apple信息
            string message = "Delete---" + deviceLibraryIdentifier + "---" + passTypeIdentifier + "---" + serialNumber + "---" + res;
            return message;
        }


        /// <summary>
        /// AppleWallet回调
        /// 获取与设备关联的serialNumber
        /// </summary>
        /// <param name="deviceLibraryIdentifier">设备唯一编号</param>
        /// <param name="passTypeIdentifier">证书passTypeId</param>
        /// <param name="passesUpdatedSince"></param>
        /// <returns></returns>
        public object PassRegisterCallBackByApple(string deviceLibraryIdentifier, string passTypeIdentifier, string passesUpdatedSince)
        {
            List<string> SerialNumbers = new List<string>();
            try
            {
                //SerialNumbers = connection.QuerySet<vw_cms_ContactsList>()
                //.Where(x => x.DeviceType == (int)DeviceTypeEnum.Apple && x.DeviceLibraryIdentifier == deviceLibraryIdentifier)
                //.ToList(s => s.SerialNumber);

                //查询数据库,得到所有与此设备相关联的SerialNumbers，并传回Apple
                SerialNumbers.Add("SN1");
                SerialNumbers.Add("SN2");
                SerialNumbers.Add("SN3");
                SerialNumbers.Add("SN4");
            }
            catch
            {

            }

            var result = new { lastUpdated = DateTime.Now.ToString("yyyymmddHHmmssfffffff"), serialNumbers = SerialNumbers };
            return result;
        }


        /// <summary>
        ///  AppleWallet刷新
        ///  获取最新版本的通行证
        /// </summary>
        /// <param name="passTypeIdentifier">证书passTypeId</param>
        /// <param name="serialNumber">通行证创建时候的编号</param>
        /// <returns></returns>
        public async Task<IActionResult> PassReFresh(string passTypeIdentifier, string serialNumber, string Authorization)
        {

            //获取配置
            //Apple名片类型ID
            string PassTypeIdentifierConfig = configuration["Apple:PassTypeIdentifier"] ?? "";
            //Apple名片验证Token
            string AuthenticationTokenConfig = configuration["Apple:AuthenticationToken"] ?? "";

            if (passTypeIdentifier != PassTypeIdentifierConfig || Authorization != "ApplePass " + AuthenticationTokenConfig)
            {
                return null;
            }

            //查询数据库得到需要更新的文件
            //查询SN的准确性对应文件。
            //if (PassKit == null) return null;

            //获取服务器域名
            string resourcesUrl = configuration["Resource:AddressUrl"] ?? "";

            if (string.IsNullOrEmpty(resourcesUrl)) return null;

            //替换为你要获取文件的URL路径
            var url = resourcesUrl + "/Files/" + serialNumber + ".pkpass";

            return await GetpkpassFiles(url);
        }
        #endregion

        #region Apple下载PkPass公共方法

        /// <summary>
        /// 根据Url下载pkpass文件
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<IActionResult> GetpkpassFiles(string url)
        {
            try
            {
                //开始准备下载流程
                //特别注意,直接下载除Safari外其他浏览器可能不兼容。需要使用以下方法识别。
                var client = httpClientFactory.CreateClient();

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {

                    var fileBytes = await response.Content.ReadAsStreamAsync();

                    // 检查是否设置了正确的 MIME 类型和文件名
                    var contentType = "application/vnd.apple.pkpass";
                    var fileName = "pass.pkpass"; // 替换为正确的文件名

                    //return  File(fileBytes, contentType, fileName); // 返回文件流给前端

                    // 使用PhysicalFile方法
                    //return PhysicalFile(fileBytes, contentType, fileName);

                    //或者使用FileStreamResult方法
                    //var fileStream = new MemoryStream(fileBytes);
                    return new FileStreamResult(fileBytes, contentType)
                    {
                        FileDownloadName = fileName
                    };
                }
                else
                {
                    Console.WriteLine($"请求失败，状态码：{(int)response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("请求失败（网络问题、URL 错误等）：" + ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("请求超时：" + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("其他异常：" + ex.Message);
            }

            return null;
        }
        #endregion
        #endregion
    }
}
