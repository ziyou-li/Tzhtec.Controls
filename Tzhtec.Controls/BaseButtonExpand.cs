using System;
using System.Linq;
using System.Collections;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Tzhtec.Controls
{
    public static class BaseButtonExpand
    {
        private static Hashtable ht = new Hashtable();  // 存储相关控件与对应的按钮关系
        private static Hashtable eventHt = new Hashtable(); // 存储按钮原有事件
        private static ToolTip _tooltip;   // 控件的tooltip

        /// <summary>
        /// 将控件添加到控制列表
        /// </summary>
        /// <param name="baseButton"></param>
        /// <param name="control"></param>
        public static void AddControl(this ButtonBase baseButton, Control control)
        {
            if (control as ITzhtecControls == null) return;
            if (!ht.ContainsKey(control))
            {
                ht.Add(control, baseButton);
                HookButtonClickEvent(baseButton);
                control.TextChanged += new EventHandler(control_TextChanged);
            }
        }

        /// <summary>
        /// 将控件从控制列表移除
        /// </summary>
        /// <param name="baseButton"></param>
        /// <param name="control"></param>
        public static void RemoveControl(this ButtonBase baseButton, Control control)
        {
            if (control as ITzhtecControls == null) return;
            if (ht.ContainsKey(control))
            {
                ht.Remove(control);
                RemoveHookEvent(baseButton);
                control.TextChanged -= new EventHandler(control_TextChanged);
            }
        }

        /// <summary>
        /// 文本改变消除提示信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void control_TextChanged(object sender, EventArgs e)
        {
            ITzhtecControls c = sender as ITzhtecControls;
            if (_tooltip != null) _tooltip.Dispose();
            if (c.MessageLabel != null) c.MessageLabel.Text = string.Empty;
            if (c.ErrorProvider != null) c.ErrorProvider.Clear();
            (sender as Control).BackColor = c.NormalBackColor;
        }

        /// <summary>
        /// 捕获按钮click事件
        /// </summary>
        /// <param name="baseButton"></param>
        private static void HookButtonClickEvent(ButtonBase baseButton)
        {
            if (baseButton == null) return;
            if (eventHt.Contains(baseButton)) return;
            PropertyInfo pi = baseButton.GetType().GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic);
            if (pi != null)
            {
                //获得事件列表
                EventHandlerList eventList = (EventHandlerList)pi.GetValue(baseButton, null);
                if (eventList != null && eventList is EventHandlerList)
                {
                    //查找按钮点击事件
                    FieldInfo fi = (typeof(Control)).GetField("EventClick", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                    if (fi != null)
                    {
                        EventHandler eh = eventList[fi.GetValue(baseButton)] as EventHandler;   //记录原有按钮事件
                        if (eh != null) baseButton.Click -= eh; //移除原有的按钮事件
                        baseButton.Click += new EventHandler(button_Click); //替换按钮事件
                        eventHt.Add(baseButton, eh);
                    }
                }
            }
        }

        /// <summary>
        /// 还原按钮click事件
        /// </summary>
        /// <param name="baseButton"></param>
        private static void RemoveHookEvent(ButtonBase baseButton)
        {
            if (!ht.ContainsValue(baseButton))
            {
                foreach (DictionaryEntry de in eventHt)  //遍历hashtable得到与之相关的按钮
                {
                    if (de.Key == baseButton)
                    {
                        EventHandler eh = de.Value as EventHandler;
                        baseButton.Click -= button_Click;   //移除替换事件
                        baseButton.Click += eh; // 还原事件
                        break;
                    }
                }
                eventHt.Remove(baseButton);
            }
        }

        /// <summary>
        /// 替换按钮的click事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void button_Click(object sender, EventArgs e)
        {
            if (sender == null) return;
            // 获取当前按钮相关的所有控件，并根据tabindex升序排序
            var ctls = (from item in ht.Cast<DictionaryEntry>()
                        where item.Value == sender as ButtonBase
                        select item.Key as Control).Distinct().OrderBy(c => c.TabIndex);
            foreach (var ctl in ctls)
            {
                // 未激活或者已隐藏控件不验证
                if (!EnabledControl(ctl)) continue;
                ITzhtecControls c = ctl as ITzhtecControls;
                if (c != null)
                {
                    if (!EmptyValidate(ctl)) return;
                    if (!RegexExpressionValidate(ctl)) return;
                    if (!InvokeCustomerEvent(ctl)) return;
                }
                ctl.BackColor = c.NormalBackColor;
            }

            foreach (DictionaryEntry de in eventHt)  //遍历hashtable得到与之相关的按钮
            {
                if (de.Key == sender as ButtonBase)
                {
                    EventHandler eh = de.Value as EventHandler;
                    if (eh != null) eh(sender, e);
                    break;
                }
            }
        }

        /// <summary>
        /// 检测控件是否处于激活和可视状态
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        private static bool EnabledControl(Control control)
        {
            if (control != null)
                return control.Visible && control.Enabled && EnabledControl(control.Parent);
            return true;
        }

        /// <summary>
        /// 非空验证
        /// </summary>
        /// <param name="ctl"></param>
        /// <returns></returns>
        private static bool EmptyValidate(Control ctl)
        {
            ITzhtecControls c = ctl as ITzhtecControls;

            if (!c.AllowEmpty)
            {
                if ((c.RemoveSpace && ctl.Text.Trim() == "") || ctl.Text == "")
                {
                    ShowErrorMessage(ctl as ITzhtecControls, c.EmptyMessage);
                    c.SelectAll();
                    ctl.Focus();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 正则验证
        /// </summary>
        /// <param name="ctl"></param>
        /// <returns></returns>
        private static bool RegexExpressionValidate(Control ctl)
        {
            ITzhtecControls c = ctl as ITzhtecControls;

            if (!((c.RemoveSpace && ctl.Text.Trim() == "") || ctl.Text == ""))
            {
                if (!string.IsNullOrEmpty(c.RegexExpression) && !Regex.IsMatch((c.RemoveSpace ? ctl.Text.Trim() : ctl.Text),
                    c.RegexExpression))
                {
                    ShowErrorMessage(ctl as ITzhtecControls, c.ErrorMessage);
                    c.SelectAll();
                    ctl.Focus();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 自定义验证
        /// </summary>
        /// <param name="ctl"></param>
        /// <returns></returns>
        private static bool InvokeCustomerEvent(Control ctl)
        {
            PropertyInfo pi = ctl.GetType().GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic);
            if (pi != null)
            {
                EventHandlerList ehl = (EventHandlerList)pi.GetValue(ctl, null);
                if (ehl != null)
                {   // 得到自定义验证事件
                    FieldInfo fi = ctl.GetType().GetField("CustomerValidated", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        Delegate d = fi.GetValue(ctl) as Delegate;
                        if (d != null)
                        {
                            CustomerEventArgs ce = new CustomerEventArgs();
                            ce.Value = ctl.Text;
                            ce.Validated = true;
                            d.DynamicInvoke(ctl, ce);   // 执行自定义验证方法
                            if (!ce.Validated)
                            {
                                ITzhtecControls c = ctl as ITzhtecControls;
                                ShowErrorMessage(c, ce.ErrorMessage);
                                c.SelectAll();
                                ctl.Focus();
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 显示提示信息
        /// </summary>
        /// <param name="ctl"></param>
        /// <param name="err"></param>
        private static void ShowErrorMessage(ITzhtecControls control, string err)
        {
            Control ctl = control as Control;
            ctl.BackColor = control.InvalidBackColor;
            switch (control.ShowMethod)
            {
                case ErrorShowMethod.MessageBox:
                    MessageBox.Show(err, control.MessageTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;
                case ErrorShowMethod.ToolTip:
                    if (_tooltip != null) _tooltip.Dispose(); // 如果tooltip已经存在则销毁
                    _tooltip = new ToolTip();
                    _tooltip.ToolTipIcon = ToolTipIcon.Warning;
                    _tooltip.IsBalloon = true;
                    _tooltip.ToolTipTitle = control.MessageTitle;
                    _tooltip.AutoPopDelay = 5000;
                    //得到信息显示的行数
                    if (err == null) err = " ";
                    int l = err.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length;
                    _tooltip.Show(err, ctl, new Point(10, -47 - l * 18 + (ctl.Height - 21) / 2));
                    break;
                case ErrorShowMethod.Icon:
                    if (control.ErrorProvider != null)
                        control.ErrorProvider.SetError(ctl, err);
                    break;
                case ErrorShowMethod.Label:
                    if (control.MessageLabel != null)
                    {
                        control.MessageLabel.Text = err;
                        control.MessageLabel.Visible = true;
                    }
                    break;
                default: break;
            }
        }
    }
}
