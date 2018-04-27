using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using umbraco.NodeFactory;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace TPCTrainco.Umbraco.Extensions.Helpers
{
    public class ImportRedirects
    {
        public static string Import(string filePath)
        {
            FileStream stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read);
            ContentService contentSerivce = new ContentService();
            StringBuilder sb = new StringBuilder();
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

            excelReader.IsFirstRowAsColumnNames = true;

            DataSet result = excelReader.AsDataSet();

            if (result.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in result.Tables[0].Rows)
                {
                    string oldUrl = row[0].ToString();
                    string newUrl = row[1].ToString();

                    if (false == string.IsNullOrWhiteSpace(oldUrl) && false == string.IsNullOrWhiteSpace(newUrl))
                    {
                        if (oldUrl.IndexOf("tpctrainco.com") >= 0)
                        {
                            oldUrl = oldUrl.Replace("http://www.americantrainco.com", "");
                            oldUrl = oldUrl.Replace("https://www.americantrainco.com", "");
                            newUrl = newUrl.Replace("tpctrainco.com", "");

                            Node nodeFind = umbraco.uQuery.GetNodeByUrl(newUrl);

                            if (nodeFind != null)
                            {
                                StringBuilder urlPicker = new StringBuilder();

                                urlPicker.AppendLine("{");
                                urlPicker.AppendLine("  \"type\": \"content\",");
                                urlPicker.AppendLine("  \"meta\": {");
                                urlPicker.AppendLine("    \"title\": \"\",");
                                urlPicker.AppendLine("    \"newWindow\": false");
                                urlPicker.AppendLine("},");
                                urlPicker.AppendLine("  \"typeData\": {");
                                urlPicker.AppendLine("    \"url\": \"\",");
                                urlPicker.AppendLine("    \"contentId\": "+ nodeFind.Id +",");
                                urlPicker.AppendLine("    \"mediaId\": null");
                                urlPicker.AppendLine("}");
                                urlPicker.AppendLine("}");

                                if (false == IsExistingUrl(oldUrl))
                                {
                                    if (nodeFind.Id > 0)
                                    {
                                        IContent node = contentSerivce.CreateContent(oldUrl, 1092, "Redirect", 0);

                                        node.SetValue("hideInXmlSitemap", true);
                                        node.SetValue("urlToRedirect", oldUrl);
                                        node.SetValue("redirectToUrl", urlPicker.ToString());
                                        node.SetValue("statusCode", "301");

                                        contentSerivce.Publish(node, 0);

                                        //sb.AppendLine("REDIRECT FOUND (" + nodeFind.Id + "): " + nodeFind.Url);
                                    }
                                    else
                                    {
                                        sb.AppendLine("BAD NODE (" + nodeFind.Id + "): " + nodeFind.Url + " | " + oldUrl);
                                    }
                                }
                                else
                                {
                                    sb.AppendLine("REDIRECT FOUND: " + oldUrl);
                                }
                            }
                            else
                            {
                                sb.AppendLine("CANNOT FIND NODE: " + newUrl);
                            }
                        }
                        else
                        {
                            sb.AppendLine("NOT ATI URL: " + oldUrl);
                        }
                    }
                }
            }

            string output = sb.ToString();

            excelReader.Close();

            return output;
        }

        public static string ImportExternalRedirects(string filePath)
        {
            FileStream stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read);
            ContentService contentSerivce = new ContentService();
            StringBuilder sb = new StringBuilder();
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            excelReader.IsFirstRowAsColumnNames = true;
            DataSet result = excelReader.AsDataSet();
            if (result.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in result.Tables[0].Rows)
                {
                    string oldUrl = row[0].ToString();
                    string newUrl = row[1].ToString();

                    if (string.IsNullOrWhiteSpace(oldUrl) || string.IsNullOrWhiteSpace(newUrl) || oldUrl.IndexOf("tpctrainco.com") == -1)
                        continue;
                    oldUrl = oldUrl.Replace("tpctrainco.com", "");
                    StringBuilder urlPicker = new StringBuilder();
                    urlPicker.AppendLine("{");
                    urlPicker.AppendLine("  \"type\": \"url\",");
                    urlPicker.AppendLine("  \"meta\": {");
                    urlPicker.AppendLine("    \"title\": \"\",");
                    urlPicker.AppendLine("    \"newWindow\": false");
                    urlPicker.AppendLine("},");
                    urlPicker.AppendLine("  \"typeData\": {");
                    urlPicker.AppendLine("    \"url\": \"" + newUrl + "\",");
                    urlPicker.AppendLine("    \"contentId\": \"\",");
                    urlPicker.AppendLine("    \"mediaId\": null");
                    urlPicker.AppendLine("}");
                    urlPicker.AppendLine("}");
                    deleteExistingRedirects(oldUrl);
                    IContent node = contentSerivce.CreateContent(oldUrl, 1092, "Redirect", 0);
                    node.SetValue("hideInXmlSitemap", true);
                    node.SetValue("urlToRedirect", oldUrl);
                    node.SetValue("redirectToUrl", urlPicker.ToString());
                    node.SetValue("statusCode", "301");
                    contentSerivce.Publish(node, 0);
                }
            }
            string output = sb.ToString();
            contentSerivce.RePublishAll();
            excelReader.Close();
            return output;
        }
        public static bool IsExistingUrl(string url)
        {
            bool found = false;

            var umbHelper = new UmbracoHelper(UmbracoContext.Current);

            IPublishedContent siteSettings = umbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("SiteSettings")); ;
            IPublishedContent redirectsFolder = siteSettings.Children.FirstOrDefault(n => n.IsDocumentType("RedirectsFolder"));

            IEnumerable<IPublishedContent> redirects = redirectsFolder.Children;

            IPublishedContent findRedirect = redirects.Where(p => p.Name.ToLower() == url.ToLower()).FirstOrDefault();

            if (findRedirect != null)
            {
                found = true;
            }
            return found;
        }

        public static void deleteExistingRedirects(string url)
        {
            var umbHelper = new UmbracoHelper(UmbracoContext.Current);
            var cs = ApplicationContext.Current.Services.ContentService;
            IPublishedContent siteSettings = umbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("SiteSettings")); ;
            IPublishedContent redirectsFolder = siteSettings.Children.FirstOrDefault(n => n.IsDocumentType("RedirectsFolder"));
            IEnumerable<IPublishedContent> redirects = redirectsFolder.Children;
            List<IPublishedContent> nodes = redirects.Where(p => p.Name.ToLower() == url.ToLower()).ToList();
            foreach (IPublishedContent node in nodes)
            {
                IContent contentNode = cs.GetById(node.Id);
                if (contentNode == null)
                    continue;
                cs.Delete(contentNode);
            }
        }

    }
}
