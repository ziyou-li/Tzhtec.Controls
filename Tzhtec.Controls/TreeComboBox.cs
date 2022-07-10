using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data;

namespace Tzhtec.Controls
{
    [DefaultEvent("AfterSelect"), ToolboxBitmap(typeof(ComboBox))]
    public partial class TreeComboBox : ComboBox, ITzhtecControls
    {
        /// <summary>
        /// API常量
        /// </summary>
        private const int WM_LBUTTONDOWN = 0x201, WM_LBUTTONDBLCLK = 0x203;

        ToolStripControlHost treeViewHost;  
        ToolStripDropDown dropDown;
        TreeView tv = new TreeView();

        /// <summary>
        /// 在选定树节点后发生
        /// </summary>
        [Description("在选定树节点后发生")]
        public event TreeViewEventHandler AfterSelect;

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

        public TreeComboBox()
        {
            InitializeComponent();

            tv.AfterSelect += new TreeViewEventHandler(treeView_AfterSelect);
            tv.BorderStyle = BorderStyle.None;

            treeViewHost = new ToolStripControlHost(tv);
            dropDown = new ToolStripDropDown();
            dropDown.Width = this.Width;
            dropDown.Items.Add(treeViewHost);
        }

        public TreeComboBox(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            tv.AfterSelect += new TreeViewEventHandler(treeView_AfterSelect);
            tv.BorderStyle = BorderStyle.None;

            treeViewHost = new ToolStripControlHost(tv);
            dropDown = new ToolStripDropDown();
            dropDown.Width = this.Width;
            dropDown.Items.Add(treeViewHost);
        }

        private bool hasCreate = false;
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (!DesignMode && this.DropDownStyle == ComboBoxStyle.DropDownList)
            {
                ToDataSource();
            }
            if (btn != null) btn.AddControl(this);
            hasCreate = true;
        }

        /// <summary>
        /// 展开所有树节点
        /// </summary>
        [Description("展开所有树节点")]
        public void ExpandAll()
        {
            tv.ExpandAll();
        }

        /// <summary>
        /// 将属性结构转为数据源对象，满足ComboBoxStyle.DropDownList情况
        /// </summary>
        private void ToDataSource()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("text");
            dt.Columns.Add("value");
            foreach (TreeNode node in tv.Nodes)
            {
                DataRow dr = dt.NewRow();
                dr["text"] = node.Text;
                dr["value"] = node;
                dt.Rows.Add(dr);
                AddToDataSource(node, dt);
            }
            base.DataSource = dt;
            base.DisplayMember = "text";
            base.ValueMember = "value";
        }
        /// <summary>
        /// 递归添加所有节点
        /// </summary>
        /// <param name="tn"></param>
        /// <param name="dt"></param>
        private void AddToDataSource(TreeNode tn, DataTable dt)
        {
            foreach (TreeNode node in tn.Nodes)
            {
                DataRow dr = dt.NewRow();
                dr["text"] = node.Text;
                dr["value"] = node;
                dt.Rows.Add(dr);
                AddToDataSource(node, dt);
            }
        }

        /// <summary>
        /// 获取或设置选定的树节点
        /// </summary>
        [Description("获取或设置选定的树节点"), Browsable(false)]
        public new TreeNode SelectedValue
        {
            get { return this.tv.SelectedNode; }
            set 
            {
                tv.SelectedNode = value;
                base.SelectedValue = value;
            }
        }
        /// <summary>
        /// 获取或设置选定树的Tag属性的节点
        /// </summary>
        [Description("获取或设置选定树的Tag属性的节点"), Browsable(false)]
        public object SelectedTag
        {
            get 
            {
                if (this.tv.SelectedNode == null) return null;
                return this.tv.SelectedNode.Tag; 
            }
            set
            {
                foreach (TreeNode tn in tv.Nodes)
                {
                    if (tn.Tag == value)
                    {
                        tv.SelectedNode = tn;
                        break;
                    }
                    SearchTag(tn, value);
                }
            }
        }
        /// <summary>
        /// 查找相应Tag属性值的节点
        /// </summary>
        /// <param name="tn"></param>
        /// <param name="value"></param>
        private void SearchTag(TreeNode tn, object value)
        {
            foreach (TreeNode sub in tv.Nodes)
            {
                if (sub.Tag == value)
                {
                    tv.SelectedNode = sub;
                    break;
                }
                SearchTag(sub, value);
            }
        }

        /// <summary>
        /// 显示树
        /// </summary>
        private void ShowDropDown()
        {
            if (dropDown != null)
            {
                treeViewHost.Size = new Size(DropDownWidth - 2, DropDownHeight);
                if (dropDown.Visible)
                {
                    dropDown.Close();
                }
                else
                {
                    dropDown.Show(this, 0, this.Height);
                }
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.Text = tv.SelectedNode.Text;
            dropDown.Close();
            if (AfterSelect != null) AfterSelect(this, e);
        }

        /// <summary>
        /// 获取一个对象，该对象表示当前树的所有节点的集合
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Description("获取一个对象，该对象表示当前树的所有节点的集合")]
        public new TreeNodeCollection Items
        {
            get { return tv.Nodes; }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDBLCLK || m.Msg == WM_LBUTTONDOWN)
            {
                ShowDropDown();
                return;
            }
            base.WndProc(ref m);
        }
    }
}
