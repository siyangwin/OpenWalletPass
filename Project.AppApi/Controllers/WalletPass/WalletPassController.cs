using IService.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcCore.Extension.Auth;
using Service.App;
using ViewModel.App;

namespace Project.AppApi.Controllers
{
    /// <summary>
    /// Pass操作
    /// </summary>
    public class WalletPassController : BaseController
    {
        IWalletPassService walletPassService { get; set; }

        /// <summary>
        /// 注入
        /// </summary>
        /// <param name="walletPassService">Pass操作</param>
        public WalletPassController(IWalletPassService walletPassService)
        {
            this.walletPassService = walletPassService;
        }

        #region Apple操作
        #region 创建


        #endregion

        #region 下载
        /// <summary>
        ///  获取Apple Wallet文件
        /// </summary>
        /// <param name="SerialNumber">编号</param>
        /// <param name="P">加密校验参数</param>
        /// <returns></returns>
        [Route("/api/wallet/apple-file")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetWalletByAppleFile(string SerialNumber, string P)
        {
            if (!string.IsNullOrEmpty(SerialNumber) && !string.IsNullOrEmpty(P))
            {
                IActionResult WalletFile = await walletPassService.GetWalletByAppleFile(SerialNumber, P, HttpContext);

                if (WalletFile == null)
                {
                    return NotFound();
                }

                return WalletFile;
            }
            else
            {
                return NotFound();
            }
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
        [Route("/v1/devices/{deviceLibraryIdentifier}/registrations/{passTypeIdentifier}/{serialNumber}/")]
        [AllowAnonymous]
        [HttpPost]
        public IActionResult PassRegisterCallBackByApple(string deviceLibraryIdentifier, string passTypeIdentifier, string serialNumber, [FromBody] ApplePushTokenReqDto PushToken, [FromHeader] string Authorization)
        {
            string result = walletPassService.PassRegisterCallBackByApple(deviceLibraryIdentifier, passTypeIdentifier, serialNumber, PushToken.pushToken, Authorization);

            if (result.ToLower().Contains("error"))
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// AppleWallet回调
        /// 注销设备-用户主动删除Pkpass
        /// </summary>
        /// <param name="deviceLibraryIdentifier">设备唯一编号</param>
        /// <param name="passTypeIdentifier">证书passTypeId</param>
        /// <param name="serialNumber">通行证创建时候的编号</param>
        /// <param name="Authorization">创建名片的时候加入的验证Token</param>
        /// <returns></returns>
        [Route("/v1/devices/{deviceLibraryIdentifier}/registrations/{passTypeIdentifier}/{serialNumber}/")]
        [AllowAnonymous]
        [HttpDelete]
        public IActionResult PassDeleteCallbackByApple(string deviceLibraryIdentifier, string passTypeIdentifier, string serialNumber, [FromHeader] string Authorization)
        {
            string result = walletPassService.PassDeleteCallbackByApple(deviceLibraryIdentifier, passTypeIdentifier, serialNumber, Authorization);

            if (result.ToLower().Contains("error"))
            {
                return NotFound();
            }
            return Ok(result);
        }


        /// <summary>
        /// AppleWallet回调
        /// 获取与设备关联的serialNumber
        /// </summary>
        /// <param name="deviceLibraryIdentifier">设备唯一编号</param>
        /// <param name="passTypeIdentifier">证书passTypeId</param>
        /// <param name="passesUpdatedSince"></param>
        /// <returns></returns>
        [Route("/v1/devices/{deviceLibraryIdentifier}/registrations/{passTypeIdentifier}/")]
        [AllowAnonymous]
        [HttpGet]
        public IActionResult PassRegisterCallBackByApple(string deviceLibraryIdentifier, string passTypeIdentifier, [FromQuery] string passesUpdatedSince)
        {
            object result = walletPassService.PassRegisterCallBackByApple(deviceLibraryIdentifier, passTypeIdentifier, passesUpdatedSince);
            return Ok(result);
        }


        /// <summary>
        /// AppleWallet回调
        /// AppleWallet记录错误日志
        /// </summary>
        /// <returns></returns>
        [Route("/v1/log/")]
        [AllowAnonymous]
        [HttpPost]
        public IActionResult PassLogByApple([FromBody] object RequestBody)
        {
            string message = "Log---" + RequestBody.ToString();
            return Ok(message);
        }



        /// <summary>
        ///  AppleWallet回调
        ///  刷新-获取最新版本的通行证
        /// </summary>
        /// <param name="passTypeIdentifier">证书passTypeId</param>
        /// <param name="serialNumber">通行证创建时候的编号</param>
        /// <returns></returns>
        [Route("/v1/passes/{passTypeIdentifier}/{serialNumber}")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> PassReFresh(string passTypeIdentifier, string serialNumber, [FromHeader] string Authorization)
        {
            //// 获取 .pass 文件的路径
            //string Path = $"{AppContext.BaseDirectory}Files/Wallet/AppleWallet/";

            //if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);

            //string fileName = serialNumber + ".pkpass";
            //// Now you have to "deliver" package file to user using any channel you have
            ////   (save as attachment in email, download from your webapp etc)

            //if (System.IO.File.Exists(Path + fileName))
            //{
            //    // 读取 .pass 文件的内容
            //    var passBytes = await System.IO.File.ReadAllBytesAsync(Path + fileName);

            //    // 设置响应的内容类型为 application/vnd.apple.pkpass
            //    return File(passBytes, "application/vnd.apple.pkpass", serialNumber + ".pkpass");
            //}
            //// 如果文件不存在，则返回 404 Not Found
            //return NotFound();

            Response.Headers.Add("last-modified'", DateTime.Now.ToString("yyyymmddHHmmssfffffff"));

            if (!string.IsNullOrEmpty(serialNumber) && !string.IsNullOrEmpty(passTypeIdentifier) && !string.IsNullOrEmpty(Authorization))
            {
                IActionResult WalletFile = await walletPassService.PassReFresh(passTypeIdentifier, serialNumber, Authorization);

                if (WalletFile == null)
                {
                    return NotFound();
                }

                return WalletFile;
            }
            else
            {
                return NotFound();
            }
        }
        #endregion
        #endregion

        #region Google操作

        #region 下载

        #endregion


        #region Apple回调

        #endregion

        #endregion

    }
}
