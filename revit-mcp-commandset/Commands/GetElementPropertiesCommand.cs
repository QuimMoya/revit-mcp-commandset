using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using RevitMCPSDK.API.Base;
using RevitMCPCommandSet.Services;

namespace RevitMCPCommandSet.Commands
{
    public class GetElementParametersCommand : ExternalEventCommandBase
    {
        private GetElementParametersEventHandler _handler => (GetElementParametersEventHandler)Handler;

        /// <summary>
        /// 命令名称
        /// </summary>
        public override string CommandName => "get_element_parameters";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="uiApp">Revit UIApplication</param>
        public GetElementParametersCommand(UIApplication uiApp)
            : base(new GetElementParametersEventHandler(), uiApp)
        {
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameters">输入参数（JObject格式）</param>
        /// <param name="requestId">请求ID</param>
        /// <returns>命令结果对象</returns>
        public override object Execute(JObject parameters, string requestId)
        {
            try
            {
                // 设置参数到 handler 中
                _handler.SetParameters(parameters);

                // 执行并等待完成
                if (RaiseAndWaitForCompletion(10000))
                {
                    return _handler.Result;
                }
                else
                {
                    throw new TimeoutException("获取构件参数操作超时");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取构件参数失败: {ex.Message}");
            }
        }
    }
}
