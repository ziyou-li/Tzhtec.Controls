using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace Tzhtec.Controls
{
    [Description("TreeHeadDataGridView"), ToolboxBitmap(typeof(DataGridView))]
    public partial class TreeHeadDataGridView : DataGridView
    {
        private TreeView tvHeads = new TreeView();
        public TreeHeadDataGridView()
        {
            InitializeComponent();
        }

        public TreeHeadDataGridView(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TreeNodeCollection HeadSource
        {
            get { return this.tvHeads.Nodes; }
        }


        private int _cellHeight = 18;
        private int _columnDeep = 1;
        [Description("设置或获得合并表头树的深度")]
        public int ColumnDeep
        {
            get
            {
                if (this.Columns.Count == 0)
                    _columnDeep = 1;

                this.ColumnHeadersHeight = _cellHeight * _columnDeep;
                return _columnDeep;
            }

            set
            {
                if (value < 1)
                    _columnDeep = 1;
                else
                    _columnDeep = value;
                this.ColumnHeadersHeight = _cellHeight * _columnDeep;
            }
        }

        ///<summary>
        ///绘制合并表头
        ///</summary>
        ///<param name="node">合并表头节点</param>
        ///<param name="e">绘图参数集</param>
        ///<param name="level">结点深度</param>
        ///<remarks></remarks>
        public void PaintUnitHeader(TreeNode node, DataGridViewCellPaintingEventArgs e, int level)
        {
            //根节点时退出递归调用
            if (level == 0)
                return;

            RectangleF uhRectangle;
            int uhWidth;
            SolidBrush gridBrush = new SolidBrush(this.GridColor);

            Pen gridLinePen = new Pen(gridBrush);
            StringFormat textFormat = new StringFormat();


            textFormat.Alignment = StringAlignment.Center;

            uhWidth = GetUnitHeaderWidth(node);

            //与原贴算法有所区别在这。
            if (node.Nodes.Count == 0)
            {
                uhRectangle = new Rectangle(e.CellBounds.Left,
                            e.CellBounds.Top + node.Level * _cellHeight,
                            uhWidth - 1,
                            _cellHeight * (_columnDeep - node.Level) - 1);
            }
            else
            {
                uhRectangle = new Rectangle(
                            e.CellBounds.Left,
                            e.CellBounds.Top + node.Level * _cellHeight,
                            uhWidth - 1,
                            _cellHeight - 1);
            }

            Color backColor = e.CellStyle.BackColor;
            if (node.BackColor != Color.Empty)
            {
                backColor = node.BackColor;
            }
            SolidBrush backColorBrush = new SolidBrush(backColor);
            //画矩形
            e.Graphics.FillRectangle(backColorBrush, uhRectangle);

            //划底线
            e.Graphics.DrawLine(gridLinePen
                                , uhRectangle.Left
                                , uhRectangle.Bottom
                                , uhRectangle.Right
                                , uhRectangle.Bottom);
            //划右端线
            e.Graphics.DrawLine(gridLinePen
                                , uhRectangle.Right
                                , uhRectangle.Top
                                , uhRectangle.Right
                                , uhRectangle.Bottom);

            ////写字段文本
            Color foreColor = Color.Black;
            if (node.ForeColor != Color.Empty)
            {
                foreColor = node.ForeColor;
            }
            e.Graphics.DrawString(node.Text, this.Font
                                    , new SolidBrush(foreColor)
                                    , uhRectangle.Left + uhRectangle.Width / 2 -
                                    e.Graphics.MeasureString(node.Text, this.Font).Width / 2 - 1
                                    , uhRectangle.Top +
                                    uhRectangle.Height / 2 - e.Graphics.MeasureString(node.Text, this.Font).Height / 2);

            ////递归调用()
            if (node.PrevNode == null)
                if (node.Parent != null)
                    PaintUnitHeader(node.Parent, e, level - 1);
        }

        /// <summary>
        /// 获得合并标题字段的宽度
        /// </summary>
        /// <param name="node">字段节点</param>
        /// <returns>字段宽度</returns>
        /// <remarks></remarks>
        private int GetUnitHeaderWidth(TreeNode node)
        {
            //获得非最底层字段的宽度

            int uhWidth = 0;
            //获得最底层字段的宽度
            if (node.Nodes == null)
                return this.Columns[GetColumnListNodeIndex(node)].Width;

            if (node.Nodes.Count == 0)
                return this.Columns[GetColumnListNodeIndex(node)].Width;

            for (int i = 0; i <= node.Nodes.Count - 1; i++)
            {
                uhWidth = uhWidth + GetUnitHeaderWidth(node.Nodes[i]);
            }
            return uhWidth;
        }


        /// <summary>
        /// 获得底层字段索引
        /// </summary>
        /// <param name="node">底层字段节点</param>
        /// <returns>索引</returns>
        /// <remarks></remarks>
        private int GetColumnListNodeIndex(TreeNode node)
        {
            for (int i = 0; i <= _columnList.Count - 1; i++)
            {
                if (((TreeNode)_columnList[i]).Equals(node))
                    return i;
            }
            return -1;
        }

        private List<TreeNode> _columnList = new List<TreeNode>();
        [Description("最底层节点集合")]
        public List<TreeNode> NadirColumnList
        {
            get
            {
                if (this.tvHeads == null)
                    return null;

                if (this.tvHeads.Nodes == null)
                    return null;

                if (this.tvHeads.Nodes.Count == 0)
                    return null;

                _columnList.Clear();
                foreach (TreeNode node in this.tvHeads.Nodes)
                {
                    GetNadirColumnNodes(_columnList, node);
                }
                return _columnList;
            }
        }

        private void GetNadirColumnNodes(List<TreeNode> alList, TreeNode node)
        {
            if (node.FirstNode == null)
            {
                alList.Add(node);
            }
            foreach (TreeNode n in node.Nodes)
            {
                GetNadirColumnNodes(alList, n);
            }
        }

        /// <summary>
        /// 获得底层字段集合
        /// </summary>
        /// <param name="alList">底层字段集合</param>
        /// <param name="node">字段节点</param>
        /// <param name="checked">向上搜索与否</param>
        /// <remarks></remarks>
        private void GetNadirColumnNodes(List<TreeNode> alList, TreeNode node, Boolean isChecked)
        {
            if (isChecked == false)
            {
                if (node.FirstNode == null)
                {
                    alList.Add(node);
                    if (node.NextNode != null)
                    {
                        GetNadirColumnNodes(alList, node.NextNode, false);
                        return;
                    }
                    if (node.Parent != null)
                    {
                        GetNadirColumnNodes(alList, node.Parent, true);
                        return;
                    }
                }
                else
                {
                    if (node.FirstNode != null)
                    {
                        GetNadirColumnNodes(alList, node.FirstNode, false);
                        return;
                    }
                }
            }
            else
            {
                if (node.FirstNode == null)
                {
                    return;
                }
                else
                {
                    if (node.NextNode != null)
                    {
                        GetNadirColumnNodes(alList, node.NextNode, false);
                        return;
                    }

                    if (node.Parent != null)
                    {
                        GetNadirColumnNodes(alList, node.Parent, true);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 单元格绘制(重写)
        /// </summary>
        /// <param name="e"></param>
        /// <remarks></remarks>
        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            //行标题不重写
            if (e.ColumnIndex < 0)
            {
                base.OnCellPainting(e);
                return;
            }

            if (_columnDeep == 1)
            {
                base.OnCellPainting(e);
                return;
            }

            //绘制表头
            if (e.RowIndex == -1)
            {
                PaintUnitHeader((TreeNode)NadirColumnList[e.ColumnIndex], e, _columnDeep);
                e.Handled = true;
            }
        }

        [Description("导出报表")]
        public void Export()
        {
            if (!Directory.Exists(Application.StartupPath + @"\Export\"))
            {
                Directory.CreateDirectory(Application.StartupPath + @"\Export\");
            }
            Export(Application.StartupPath + @"\Export\" + DateTime.Now.ToString("yyMMddHHmmss") + ".xls");
        }

        [Description("导出报表")]
        public void Export(string fileName)
        {
            try
            {
                if (File.Exists(fileName)) File.Delete(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8);
            try
            {
                #region 文件头信息
                sw.WriteLine("<?xml version=\"1.0\"?>");
                sw.WriteLine("<?mso-application progid=\"Excel.Sheet\"?>");
                sw.WriteLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                sw.WriteLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
                sw.WriteLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
                sw.WriteLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                sw.WriteLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
                sw.WriteLine(" <DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">");
                sw.WriteLine(" <Author>tzhtec</Author>");
                sw.WriteLine("  <LastAuthor>tzhtec</LastAuthor>");
                sw.WriteLine("  <Version>11.5606</Version>");
                sw.WriteLine(" </DocumentProperties>");
                sw.WriteLine(" <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">");
                sw.WriteLine("  <ProtectStructure>False</ProtectStructure>");
                sw.WriteLine("  <ProtectWindows>False</ProtectWindows>");
                sw.WriteLine(" </ExcelWorkbook>");
                #endregion

                #region 定义样式
                sw.WriteLine(" <Styles>");
                sw.WriteLine("  <Style ss:ID=\"Default\" ss:Name=\"Normal\">");
                sw.WriteLine("   <Alignment ss:Vertical=\"Center\"/>");
                sw.WriteLine("   <Borders/>");
                sw.WriteLine("   <Font ss:FontName=\"宋体\" x:CharSet=\"134\" ss:Size=\"12\"/>");
                sw.WriteLine("   <Interior/>");
                sw.WriteLine("   <NumberFormat/>");
                sw.WriteLine("   <Protection/>");
                sw.WriteLine("  </Style>");
                sw.WriteLine("  <Style ss:ID=\"cellStyle\">");
                sw.WriteLine("   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>");
                sw.WriteLine("   <Borders>");
                sw.WriteLine("    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
                sw.WriteLine("    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
                sw.WriteLine("    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
                sw.WriteLine("    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
                sw.WriteLine("   </Borders>");
                sw.WriteLine("  </Style>");
                sw.WriteLine(" </Styles>");
                #endregion

                #region 表头
                sw.WriteLine(" <Worksheet ss:Name=\"Sheet1\">");
                sw.WriteLine("  <Table ss:ExpandedColumnCount=\"" + this.Columns.Count.ToString() + "\" ss:ExpandedRowCount=\"" +
                                        (this.ColumnDeep + this.Rows.Count).ToString() + "\" x:FullColumns=\"1\" " +
                                        "x:FullRows=\"1\" ss:DefaultColumnWidth=\"54\" ss:DefaultRowHeight=\"14.25\">");
                // 设置列宽
                for (int i = 0; i < this.Columns.Count; ++i)
                {
                    sw.WriteLine("   <Column ss:AutoFitWidth=\"0\" ss:Width=\"" + (0.8 * this.Columns[i].Width).ToString("0.00") + "\"/>");
                }

                if (tvHeads != null && tvHeads.Nodes.Count > 0)
                {
                    // 递归显示表头
                    for (int i = 1; i <= this.ColumnDeep; ++i)
                    {
                        List<TreeNode> list = GetLevelNodes(i);
                        sw.WriteLine("   <Row>");
                        foreach (TreeNode tn in list)
                        {
                            WriteMultiHeader(sw, tn, i);
                        }
                        sw.WriteLine("   </Row>");
                    }
                }
                else
                {
                    // 未设置特殊表头情况
                    sw.WriteLine("   <Row>");
                    for (int i = 0; i < this.Columns.Count; ++i)
                    {
                        sw.WriteLine("    <Cell ss:StyleID=\"cellStyle\"><Data ss:Type=\"String\">" + this.Columns[i].HeaderText + "</Data></Cell>");
                    }
                    sw.WriteLine("   </Row>");
                }
                #endregion

                #region 数据部分
                string[] colType = new string[this.Columns.Count];  // 将类型转为Excel类型
                for (int i = 0; i < this.Columns.Count; ++i)
                {
                    if (this.Columns[i].ValueType == typeof(int) || this.Columns[i].ValueType == typeof(double) ||
                        this.Columns[i].ValueType == typeof(Int32) || this.Columns[i].ValueType == typeof(Int16) ||
                        this.Columns[i].ValueType == typeof(Int64) || this.Columns[i].ValueType == typeof(decimal) ||
                        this.Columns[i].ValueType == typeof(float))
                    {
                        colType[i] = "Number";
                    }
                    else if (this.Columns[i].ValueType == typeof(DateTime))
                    {
                        colType[i] = "DateTime";
                    }
                    else
                    {
                        colType[i] = "String";
                    }
                }
                // 循环写入数据
                for (int i = 0; i < this.Rows.Count; ++i)
                {
                    sw.WriteLine("   <Row>");
                    for (int j = 0; j < this.Columns.Count; ++j)
                    {
                        sw.WriteLine("    <Cell ss:StyleID=\"cellStyle\"><Data ss:Type=\"" + colType[j] + "\">" +
                                                this.Rows[i].Cells[j].Value + "</Data></Cell>");
                    }
                    sw.WriteLine("   </Row>");
                }
                #endregion

                #region 文件结尾
                sw.WriteLine("  </Table>");
                sw.WriteLine(" </Worksheet>");
                sw.WriteLine("</Workbook>");
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                sw.Flush();
                sw.Close();
            }
        }

        /// <summary>
        /// 写入表头
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="tn"></param>
        /// <param name="nowDeep">当前深度</param>
        private void WriteMultiHeader(StreamWriter sw, TreeNode tn, int nowDeep)
        {
            int index = GetIndex(tn);

            if (tn.Nodes.Count > 0) //如果有子节点则横向合并单元格
            {
                int count = GetColumnCount(tn);
                if (count == 1)
                {
                    if (index == 1)
                    {
                        sw.WriteLine("    <Cell ss:StyleID=\"cellStyle\"><Data ss:Type=\"String\">" + tn.Text + "</Data></Cell>");
                    }
                    else
                    {
                        sw.WriteLine("    <Cell ss:Index=\"" + index.ToString() + "\" ss:StyleID=\"cellStyle\"><Data ss:Type=\"String\">" + tn.Text + "</Data></Cell>");
                    }
                }
                else
                {
                    if (index == 1)
                    {
                        sw.WriteLine("    <Cell ss:MergeAcross=\"" + (count - 1).ToString() +
                                      "\" ss:StyleID=\"cellStyle\"><Data ss:Type=\"String\">" + tn.Text + "</Data></Cell>");
                    }
                    else
                    {
                        sw.WriteLine("    <Cell ss:Index=\"" + index.ToString() + "\" ss:MergeAcross=\"" + (count - 1).ToString() +
                                      "\" ss:StyleID=\"cellStyle\"><Data ss:Type=\"String\">" + tn.Text + "</Data></Cell>");
                    }
                }
            }
            else
            {
                if (this.ColumnDeep != nowDeep && this.ColumnDeep - nowDeep != 0)
                {
                    if (index == 1)
                    {
                        sw.WriteLine("    <Cell ss:MergeDown=\"" + (this.ColumnDeep - nowDeep).ToString() +
                                      "\" ss:StyleID=\"cellStyle\"><Data ss:Type=\"String\">" + tn.Text + "</Data></Cell>");
                    }
                    else
                    {
                        sw.WriteLine("    <Cell ss:Index=\"" + index.ToString() + "\" ss:MergeDown=\"" + (this.ColumnDeep - nowDeep).ToString() +
                                      "\" ss:StyleID=\"cellStyle\"><Data ss:Type=\"String\">" + tn.Text + "</Data></Cell>");
                    }
                }
                else
                {
                    if (index == 1)
                    {
                        sw.WriteLine("    <Cell ss:StyleID=\"cellStyle\"><Data ss:Type=\"String\">" + tn.Text + "</Data></Cell>");
                    }
                    else
                    {
                        sw.WriteLine("    <Cell ss:Index=\"" + index.ToString() + "\" ss:StyleID=\"cellStyle\"><Data ss:Type=\"String\">" + tn.Text + "</Data></Cell>");
                    }
                }
            }
        }

        #region 获得第n层节点集合
        /// <summary>
        /// 获得第n层节点集合
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private List<TreeNode> GetLevelNodes(int level)
        {
            List<TreeNode> list = new List<TreeNode>();
            foreach (TreeNode tn in this.tvHeads.Nodes)
            {
                list.Add(tn);
            }
            if (level == 1)
                return list;
            else
                return GetLevelNodes(list, level, 2);
        }
        /// <summary>
        /// 获得第n层节点集合
        /// </summary>
        /// <param name="parentNodes"></param>
        /// <param name="level"></param>
        /// <param name="nowlevel"></param>
        /// <returns></returns>
        private List<TreeNode> GetLevelNodes(List<TreeNode> parentNodes, int level, int nowlevel)
        {
            List<TreeNode> sublist = new List<TreeNode>();
            foreach (TreeNode tn in parentNodes)
            {
                if (tn.Nodes.Count > 0)
                {
                    foreach (TreeNode sub in tn.Nodes)
                    {
                        sublist.Add(sub);
                    }
                }
            }
            if (level == nowlevel) return sublist;
            else return GetLevelNodes(sublist, level, nowlevel + 1);
        }
        #endregion

        /// <summary>
        /// 获得当前节点下叶子节点个数
        /// </summary>
        /// <param name="tn"></param>
        /// <returns></returns>
        private int GetColumnCount(TreeNode tn)
        {
            if (tn.Nodes.Count == 0) return 0;
            int count = 0;
            foreach (TreeNode sub in tn.Nodes)
            {
                if (sub.Nodes.Count == 0) ++count;
                else
                    count += GetColumnCount(sub);
            }
            return count;
        }

        /// <summary>
        /// 获取缩进列数
        /// </summary>
        /// <param name="tn"></param>
        /// <returns></returns>
        private int GetIndex(TreeNode tn)
        {
            int count = 0;
            if (tn.Nodes.Count > 0)
            {
                tn = GetChildNodeOfFirst(tn);
            }
            List<TreeNode> list = NadirColumnList;
            foreach (TreeNode ltn in list)
            {
                ++count;
                if (ltn == tn) return count;
            }
            return 0;
        }
        /// <summary>
        /// 获取节点叶子节点的首个节点
        /// </summary>
        /// <param name="tn"></param>
        /// <returns></returns>
        private TreeNode GetChildNodeOfFirst(TreeNode tn)
        {
            if (tn.Nodes.Count == 0) return tn;
            return GetChildNodeOfFirst(tn.Nodes[0]);
        }
    }
}
