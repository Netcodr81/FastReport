using FastReport.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastReport.Web.Application
{
    public static partial class Extensions
    {
        public static void FindClickedObject<T>(
                this Report Report,
                string objectName,
                int pageN,
                float left,
                float top,
                Action<T, ReportPage, int> action
            )
            where T : ComponentBase
        {
            if (Report.PreparedPages == null)
                return;

            bool found = false;
            while (pageN < Report.PreparedPages.Count && !found)
            {
                ReportPage page = Report.PreparedPages.GetPage(pageN);
                if (page != null)
                {
                    ObjectCollection allObjects = page.AllObjects;
                    float pointX = left + 1;
                    float pointY = top + 1;
                    foreach (Base obj in allObjects)
                    {
                        if (obj is ReportComponentBase)
                        {
                            ReportComponentBase c = obj as ReportComponentBase;
                            if (c is TableBase)
                            {
                                TableBase table = c as TableBase;
                                for (int i = 0; i < table.RowCount; i++)
                                {
                                    for (int j = 0; j < table.ColumnCount; j++)
                                    {
                                        TableCell textcell = table[j, i];
                                        if (textcell.Name == objectName)
                                        {
                                            float rectLeft = table.Columns[j].AbsLeft;
                                            float rectTop = table.Rows[i].AbsTop;
                                            float rectRight = rectLeft + textcell.Width;
                                            float rectBottom = rectTop + textcell.Height;
                                            if (pointX >= rectLeft && pointX <= rectRight && pointY >= rectTop && pointY <= rectBottom)
                                            {
                                                action(textcell as T, page, pageN);
                                                found = true;
                                                break;
                                            }
                                        }
                                        else if (textcell.FindObject(objectName) is ReportComponentBase innerObj && innerObj is T)
                                        {
                                            float rectLeft = table.Columns[j].AbsLeft + innerObj.Left;
                                            float rectTop = table.Rows[i].AbsTop + innerObj.Top;
                                            float rectRight = rectLeft + innerObj.Width;
                                            float rectBottom = rectTop + innerObj.Height;
                                            if (pointX >= rectLeft && pointX <= rectRight && pointY >= rectTop && pointY <= rectBottom)
                                            {
                                                action(innerObj as T, page, pageN);
                                                found = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (found)
                                        break;
                                }
                            }
                            else if (c is T)
                            {
                                if (c.Name == objectName &&
                                    pointX >= c.AbsBounds.Left && pointX <= c.AbsBounds.Right &&
                                    pointY >= c.AbsBounds.Top && pointY <= c.AbsBounds.Bottom)
                                {
                                    action(c as T, page, pageN);
                                    found = true;
                                    break;
                                }
                            }
                            if (found)
                                break;
                        }
                    }
                    page.Dispose();
                    pageN++;
                }
            }
        }
    }
}
