using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Tzhtec.Controls
{
    public delegate object WaitEvent(params object[] parameters);

    [DefaultEvent("Wait"), ToolboxBitmap(typeof(TzhtecWaitBar), "loader.bmp")]
    public partial class TzhtecWaitBar : UserControl
    {
        private Thread nowThread;   // 记录当前线程

        /// <summary>
        /// 需要执行的方法
        /// </summary>
        [Description("需要执行的方法"), Category("自定义")]
        public event WaitEvent Wait;

        /// <summary>
        /// 获取或设置控件显示的时候显示在控件上的提示信息
        /// </summary>
        [Description("获取或设置控件显示的时候显示在控件上的提示信息"), Category("外观"), DefaultValue("")]
        public string WaitText
        {
            get { return lblInfo.Text; }
            set
            {
                lblInfo.Text = value;
                this.Width = lblInfo.Left + lblInfo.Width + picWait.Left;
            }
        }

        private int _maxWaitSenconds = 0;
        /// <summary>
        /// 获取或设置执行方法最大等待时间，0表示无限制等待
        /// </summary>
        [Description("获取或设置执行方法最大等待时间，0表示无限制等待"), Category("外观"), DefaultValue(0)]
        public int MaxWaitSeconds
        {
            get { return _maxWaitSenconds; }
            set 
            {
                if (value < 0) throw new Exception("值必须为不小于0的整数");
                _maxWaitSenconds = value; 
            }
        }

        private bool _canStopWait = false;
        /// <summary>
        /// 获取或设置在执行方法过程中能否中断等待
        /// </summary>
        [Description("获取或设置在执行方法过程中能否中断等待"), Category("外观"), DefaultValue(false)]
        public bool CanStopWait
        {
            get { return _canStopWait; }
            set
            {
                _canStopWait = value;
                lblStop.Visible = _canStopWait;
            }
        }

        public TzhtecWaitBar()
        {
            InitializeComponent();
            this.Visible = false;
            this.lblStop.Visible = _canStopWait;
        }

        private void TzhtecWaitBar_Resize(object sender, EventArgs e)
        {
            this.Height = 92;
            picWait.Top = (this.Height - picWait.Height) / 2;
            lblInfo.Top = (this.Height - lblInfo.Height) / 2;
        }

        /// <summary>
        /// 执行自定义方法
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Description("执行自定义方法"), Browsable(false)]
        public object InvokeWait(params object[] parameters)
        {
            this.Location = new Point((this.Parent.Width - this.Width) / 2, (this.Parent.Height - this.Height) / 2);
            if (Wait != null)
            {
                this.Visible = true;
                SetEnabledControlToFlase(this.FindForm());
                object o = null;
                nowThread = new Thread(new ThreadStart(() =>
                {
                    o = Wait(parameters);
                }));
                nowThread.IsBackground = true;
                try
                {
                    nowThread.Start();
                    if (_maxWaitSenconds > 0) BeginTimer();
                    while (nowThread != null && nowThread.ThreadState != ThreadState.Stopped) 
                        Application.DoEvents();
                }
                catch
                {
                    if(nowThread != null) nowThread.Abort();
                    else throw;
                }
                finally
                {
                    ResetControlEnabled();
                }
                return o;
            }
            return default(object);
        }

        private IList<Control> list = new List<Control>();
        /// <summary>
        /// 设置控件不可用
        /// </summary>
        /// <param name="ctl"></param>
        private void SetEnabledControlToFlase(Control ctl)
        {
            if (ctl != null)
            {
                foreach (Control c in ctl.Controls)
                {
                    if (c is Button || c is MenuStrip || c is ToolStrip)
                    {
                        if (c.Enabled)
                        {
                            list.Add(c);
                            c.Enabled = false;
                        }
                    }
                    else if (c is Panel || c is SplitContainer || c is SplitterPanel || c is TabControl ||
                            c is GroupBox || c is FlowLayoutPanel || c is TableLayoutPanel)
                    {
                        SetEnabledControlToFlase(c);
                    }
                }
            }
        }

        /// <summary>
        /// 重置界面控件
        /// </summary>
        private void ResetControlEnabled()
        {
            try
            {
                foreach (Control c in list)
                {
                    c.Enabled = true;
                }
            }
            catch { }
            finally
            {
                this.Visible = false;
                list.Clear();
            }
        }

        /// <summary>
        /// 取消线程执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblCancel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (nowThread != null && nowThread.ThreadState != ThreadState.Stopped)
            {
                if (MessageBox.Show("确定要停止操作吗？", "询问", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    == DialogResult.Yes)
                {
                    try
                    {
                        nowThread.Abort();
                        nowThread = null;
                        MessageBox.Show("操作被终止！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    catch { }
                    finally
                    {
                        ResetControlEnabled();
                    }
                }
            }
        }

        /// <summary>
        /// 线程执行计时
        /// </summary>
        private void BeginTimer()
        {
            new Thread(new ThreadStart(() =>
            {
                int i = 0;
                while (nowThread != null && nowThread.ThreadState != ThreadState.Stopped)
                {
                    if (i >= _maxWaitSenconds)
                    {
                        try
                        {
                            if (nowThread.ThreadState != ThreadState.Stopped)
                            {
                                nowThread.Abort();
                                nowThread = null;
                                MessageBox.Show("操作超时，请重试！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        catch { }
                        finally
                        {
                            if (this.FindForm() != null)
                                this.FindForm().Invoke((EventHandler)delegate
                                {
                                    ResetControlEnabled();
                                });
                        }
                        break;
                    }
                    ++i;
                    Thread.Sleep(1000);
                }
            })) { IsBackground = true }.Start();
        }
    }
}
