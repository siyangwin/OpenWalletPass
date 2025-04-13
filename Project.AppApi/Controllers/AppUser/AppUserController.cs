using IService.App;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.EnumModel;
using MvcCore.Extension.Auth;
using System.Security.Claims;
using ViewModel;
using ViewModel.App;

namespace Project.AppApi.Controllers
{
    /// <summary>
    /// ����-Jwt
    /// </summary>
    /// [Route("[controller]")]
    public class AppUserController : BaseController
    {
        private readonly IAppUserService appUserService;

        /// <summary>
        /// Jwt������
        /// </summary>
        private readonly GenerateJwt generateJwt;

        /// <summary>
        /// ע��
        /// </summary>
        /// <param name="appUserService">�û���</param>
        public AppUserController(IAppUserService appUserService, GenerateJwt generateJwt)
        {
            this.appUserService = appUserService;
            this.generateJwt = generateJwt;
        }


        /// <summary>
        /// ��¼
        /// </summary>
        /// <returns></returns>
        [Route("api/user/login")]
        [AllowAnonymous]
        [HttpPost]
        public ResultModel<string> Login([FromBody] AuthorizationReqDto AuthorizationInfo)
        {
            ResultModel<string> resultModel = new ResultModel<string>();
            if (AuthorizationInfo == null)
            {
                resultModel.success = false;
            }
            else
            {
                //string token = Guid.NewGuid().ToString();
                //���������Ϣ���J�C����
                var claims = new[]
                {
                    new Claim("UserId",AuthorizationInfo.account.ToString())
                };
                //��䛲��@ȡtoken
                //userResDto.Token = token;
                //HttpContext.SignInAsync(claims, token, userResDto.Id.ToString(), 240, true);
                resultModel.data = generateJwt.GenerateEncodedTokenAsync(claims);
            }
            return resultModel;
        }

        /// <summary>
        /// �ǳ�
        /// </summary>
        /// <returns></returns>
        [Route("/api/user/loginout")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultModel> LoginOut()
        {
            ResultModel resultModel = new ResultModel();
            resultModel.message = "�ǳ��ɹ�";
            HttpContext.SignOutAsync();
            return resultModel;
        }

        /// <summary>
        /// ��Ȩ ���¼һ��  account:123 pwd:admin
        /// </summary>
        /// <param name="AuthorizationInfo">��Ȩ��Ϣ</param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("/api/appuser/authorization")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultModel<AuthorizationResDto>> Authorization([FromBody] AuthorizationReqDto AuthorizationInfo)
        {
            return appUserService.Authorization(Language, AuthorizationInfo);
        }

        /// <summary>
        /// �鿴��Ȩ��Ϣ--��Ȩ
        /// </summary>
        /// <returns></returns>
        [Route("/api/appuser/checkauthorizationinfo")]
        [HttpGet]
        public async Task<ResultModel<string>> CheckAuthorizationInfo()
        {
            ResultModel<string> resultModel = new ResultModel<string>();


            resultModel.data = "��ǰ�û�Ϊ��" + UserId;
            return resultModel;
        }


        /// <summary>
        /// �鿴����-������Ȩ
        /// </summary>
        /// <returns></returns>
        [Route("/api/appuser/checknoAuthorizationinfo")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResultModel<string>> CheckNoAuthorizationInfo()
        {
            ResultModel<string> resultModel = new ResultModel<string>();
            resultModel.data = "��ǰ�û�Ϊ��" + UserId;
            return resultModel;
        }
    }
}