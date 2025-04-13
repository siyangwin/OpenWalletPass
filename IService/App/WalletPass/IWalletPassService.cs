using IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Service.App
{
    /// <summary>
    /// Pass操作
    /// </summary>
    public interface IWalletPassService : IBaseService
    {

        #region Apple操作

        #region 下载
        /// <summary>
        /// 获取Apple Wallet文件
        /// </summary>
        /// <param name="SerialNumber">编号</param>
        /// <param name="P">验证密钥</param>
        /// <param name="httpContext">请求参数</param>
        /// <returns></returns>
        Task<IActionResult> GetWalletByAppleFile(string SerialNumber, string P, HttpContext httpContext);
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
        /// <returns></returns>
        string PassRegisterCallBackByApple(string deviceLibraryIdentifier, string passTypeIdentifier, string serialNumber, string PushToken, string Authorization);

        /// <summary>
        /// AppleWallet删除回调
        /// 注销设备
        /// </summary>
        /// <param name="deviceLibraryIdentifier">设备唯一编号</param>
        /// <param name="passTypeIdentifier">证书passTypeId</param>
        /// <param name="serialNumber">通行证创建时候的编号</param>
        /// <returns></returns>
        string PassDeleteCallbackByApple(string deviceLibraryIdentifier, string passTypeIdentifier, string serialNumber, string Authorization);


        /// <summary>
        /// AppleWallet回调
        /// 获取与设备关联的serialNumber
        /// </summary>
        /// <param name="deviceLibraryIdentifier">设备唯一编号</param>
        /// <param name="passTypeIdentifier">证书passTypeId</param>
        /// <param name="passesUpdatedSince"></param>
        /// <returns></returns>
        object PassRegisterCallBackByApple(string deviceLibraryIdentifier, string passTypeIdentifier, string passesUpdatedSince);


        /// <summary>
        ///  AppleWallet刷新
        ///  获取最新版本的通行证
        /// </summary>
        /// <param name="passTypeIdentifier">证书passTypeId</param>
        /// <param name="serialNumber">通行证创建时候的编号</param>
        /// <returns></returns>
        Task<IActionResult> PassReFresh(string passTypeIdentifier, string serialNumber, string Authorization);
        #endregion

        #endregion
    }
}
