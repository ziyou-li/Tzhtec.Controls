using System;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace Tzhtec.Controls
{
    /// <summary>
    /// RegexTextBox
    /// </summary>
    [Description("RegexTextBox"), ToolboxBitmap(typeof(TextBox))]
    public class RegexTextBox : TextBox, ITzhtecControls
    {
        #region ITzhtecControls成员

        private ButtonBase btn;
        /// <summary>
        /// 获取或设置验证控件的按钮
        /// </summary>
        [Description("获取或设置验证控件的按钮"), Category("验证"), DefaultValue(true)]
        public ButtonBase Button
        {
            get { return btn; }
            set
            {
                if (!DesignMode && hasCreate)
                {
                    if (value == null) btn.RemoveControl(this);
                    else btn.AddControl(this);
                }
                btn = value;
            }
        }

        /// <summary>
        /// 获取或设置用于验证控件值的正则表达式
        /// </summary>
        [Description("获取或设置用于验证控件值的正则表达式"), Category("验证"), DefaultValue("")]
        public string RegexExpression { get; set; }

        private bool _allEmpty = true;
        /// <summary>
        /// 获取或设置是否允许空值
        /// </summary>
        [Description("获取或设置是否允许空值"), Category("验证"), DefaultValue(true)]
        public bool AllowEmpty
        {
            get { return _allEmpty; }
            set { _allEmpty = value; }
        }

        /// <summary>
        /// 获取或设置验证的时候是否除去头尾空格
        /// </summary>
        [Description("获取或设置验证的时候是否除去头尾空格"), Category("验证"), DefaultValue(false)]
        public bool RemoveSpace { get; set; }

        private string _messageTitle = "提示";
        /// <summary>
        /// 获取或设置显示消息框的标题
        /// </summary>
        [Description("获取或设置显示消息框的标题"), Category("验证"), DefaultValue("提示")]
        public string MessageTitle 
        {
            get { return _messageTitle; }
            set { _messageTitle = value; }
        }

        /// <summary>
        /// 获取或设置当控件的值为空的时候显示的信息
        /// </summary>
        [Description("获取或设置当控件的值为空的时候显示的信息"), Category("验证"), DefaultValue("")]
        public string EmptyMessage { get; set; }

        /// <summary>
        /// 获取或设置当不满足正则表达式结果的时候显示的错误信息
        /// </summary>
        [Description("获取或设置当不满足正则表达式结果的时候显示的错误信息"), Category("验证"), DefaultValue("")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 获取或设置未通过验证时文本框的背景色
        /// </summary>
        [Description("获取或设置未通过验证时文本框的背景色"), Category("验证"), DefaultValue(typeof(SystemColors), "Window")]
        public Color InvalidBackColor { get; set; }

        /// <summary>
        /// 获取或设置通过验证时文本框的背景色
        /// </summary>
        [Description("获取或设置通过验证时文本框的背景色"), Category("验证"), DefaultValue(typeof(SystemColors), "Window")]
        public Color NormalBackColor { get; set; }

        private Label _messageLabel;
        /// <summary>
        /// 获取或设置与之相关联的标签
        /// </summary>
        [Description("获取或设置与之相关联的标签"), Category("验证"), DefaultValue(null)]
        public Label MessageLabel 
        {
            get { return _messageLabel; }
            set 
            {
                _messageLabel = value;
                if (value != null)
                {
                    _errMethod = ErrorShowMethod.Label;
                    if (!DesignMode)
                    {
                        _messageLabel.Text = "";   //默认清空label内容
                        _messageLabel.Visible = false;
                    }
                }
                else _errMethod = ErrorShowMethod.ToolTip;
            }
        }

        private ErrorShowMethod _errMethod = ErrorShowMethod.ToolTip;
        /// <summary>
        /// 获取或设置提示信息显示的方式
        /// </summary>
        [Description("获取或设置提示信息显示的方式"), Category("验证"), DefaultValue(ErrorShowMethod.ToolTip)]
        public ErrorShowMethod ShowMethod
        {
            get { return _errMethod; }
            set
            {
                if (value == ErrorShowMethod.Label && _messageLabel == null)
                {
                    throw new Exception("必须先设置与之关联的Label控件");
                }
                if (value == ErrorShowMethod.Icon && _errPro == null)
                {
                    throw new Exception("必须先设置与之关联的ErrorProvider控件");
                }
                _errMethod = value;
            }
        }

        private ErrorProvider _errPro = null;
        /// <summary>
        /// 用于指示错误的用户界面
        /// </summary>
        [Description("用于指示错误的用户界面"), Category("验证"), DefaultValue(null)]
        public ErrorProvider ErrorProvider
        {
            get { return _errPro; }
            set
            {
                _errPro = value;
                if (value != null) _errMethod = ErrorShowMethod.Icon;
                else _errMethod = ErrorShowMethod.ToolTip;
            }
        }

        /// <summary>
        /// 自定义验证事件
        /// </summary>
        [Description("自定义验证事件"), Category("验证")]
        public event CustomerValidatedHandler CustomerValidated;

        public void SelectAll()
        {
            base.SelectAll();
        }
        #endregion

        private bool hasCreate = false;
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (btn != null) btn.AddControl(this);
            hasCreate = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (btn != null) btn.RemoveControl(this);
            base.Dispose(disposing);
        }
    }
}
