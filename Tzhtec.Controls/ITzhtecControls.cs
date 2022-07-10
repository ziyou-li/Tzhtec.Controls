using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace Tzhtec.Controls
{
    #region 枚举定义
    /// <summary>
    /// 错误显示方式
    /// </summary>
    public enum ErrorShowMethod
    {
        /// <summary>
        /// 对话框模式
        /// </summary>
        MessageBox,
        /// <summary>
        /// 标签模式
        /// </summary>
        Label,
        /// <summary>
        /// 气泡通知模式
        /// </summary>
        ToolTip,
        /// <summary>
        /// ErrorProvider形式显示
        /// </summary>
        Icon,
        /// <summary>
        /// 不以任何形式显示
        /// </summary>
        None
    }
    #endregion

    /// <summary>
    /// 为自定义验证事件提供参数
    /// </summary>
    [Description("为自定义验证事件提供参数"), Browsable(false)]
    public class CustomerEventArgs : EventArgs
    {
        /// <summary>
        /// 是否通过验证
        /// </summary>
        [Description("是否通过验证"), DefaultValue(true)]
        public bool Validated { get; set; }

        /// <summary>
        /// 获取或设置被验证的值
        /// </summary>
        [Description("获取或设置被验证的值")]
        public string Value { get; set; }

        /// <summary>
        /// 获取或设置错误信息
        /// </summary>
        [Description("获取或设置错误信息")]
        public string ErrorMessage { get; set; }
    }

    public delegate void CustomerValidatedHandler(object sender, CustomerEventArgs e);

    interface ITzhtecControls : System.ComponentModel.IComponent
    {
        ButtonBase Button { get; set; }

        string RegexExpression { get; set; }

        bool AllowEmpty { get; set; }

        bool RemoveSpace { get; set; }

        string MessageTitle { get; set; }

        string EmptyMessage { get; set; }

        string ErrorMessage { get; set; }

        Color InvalidBackColor { get; set; }

        Color NormalBackColor { get; set; }

        Label MessageLabel { get; set; }

        ErrorShowMethod ShowMethod { get; set; }

        ErrorProvider ErrorProvider { get; set; }

        void SelectAll();

        event CustomerValidatedHandler CustomerValidated;

    }
}
