using System;
using System.Collections;
using System.Data;
using ScrapingSpider.Models;

namespace ScrapingSpider.DataAccess
{
    public class WebPageDao
    {
        public static int SaveOrUpdateWebPage(string hashId, string url)
        {
            return SaveOrUpdateWebPage(new WebPage
                                     {
                                         Id = hashId,
                                         Url = url,
                                         InsertDate = DateTime.Now
                                     });
        }


        /// <summary>
        /// 保存或更新WebPage
        /// </summary>
        /// <param name="webPage"></param>
        /// <returns></returns>
        public static int SaveOrUpdateWebPage(WebPage webPage)
        {
            string connStr = SqlHelper.ConnectionString();
            string tableName = webPage.GetType().Name;

            Hashtable hashtable = new Hashtable();
            foreach (var type in webPage.GetType().GetProperties())
            {
                var value = type.GetValue(webPage, null);
                if (value != null)
                    hashtable.Add(type.Name, value);
            }

            if (IsIdExisted(webPage.Id))
            {
                return SqlHelper.Update(connStr, tableName, hashtable, "where Id='" + webPage.Id + "'");
            }

            return SqlHelper.Insert(connStr, tableName, hashtable);
        }

        public static bool IsIdExisted(string id)
        {
            return IsExisted("Id='" + id + "'");
        }

        public static bool IsUrlExisted(string Url)
        {
            return IsExisted("Url='" + Url + "'");
        }

        /// <summary>
        /// 判断记录是否存在
        /// </summary>
        /// <param name="where">查询条件，如：Url='http://www.baidu.com' and Title='百度'</param>
        /// <returns></returns>
        public static bool IsExisted(string where)
        {
            int count = (int)SqlHelper.ExecuteScalar(
                SqlHelper.ConnectionString(), 
                CommandType.Text,
                "select count(*) from WebPage where " + where);
            return count != 0;
        }
    }
}
