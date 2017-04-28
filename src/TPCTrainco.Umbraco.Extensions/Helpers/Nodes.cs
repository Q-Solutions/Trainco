using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace TPCTrainco.Umbraco.Extensions.Helpers
{
    public sealed class Nodes
    {
        private static volatile Nodes instance;
        private static object _padLock = new Object();

        private Nodes() { }

        public IPublishedContent Home;
        public IPublishedContent SiteSettings;
        public IPublishedContent News;
        public IPublishedContent CourseCatalog;
        public IPublishedContent SeminarSearch;
        public IEnumerable<IPublishedContent> Redirects;
        public IPublishedContent SocialLinkFolder;
        public IEnumerable<IPublishedContent> SocialLinks;
        public IEnumerable<IPublishedContent> SeminarItems;
        public IPublishedContent SeminarMainOverview;
        public IEnumerable<IPublishedContent> SeminarCategories;

        public UmbracoHelper UmbracoHelper;

        public static Nodes Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_padLock)
                    {
                        if (instance == null)
                        {
                            var umbHelper = new UmbracoHelper(UmbracoContext.Current);

                            instance = new Nodes();
                            instance.UmbracoHelper = umbHelper;

                            instance.Home = umbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("HomePage"));
                            instance.SiteSettings = umbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("SiteSettings"));

                            instance.SocialLinkFolder = instance.SiteSettings.Children.FirstOrDefault(n => n.IsDocumentType("SocialLinks"));
                            instance.SocialLinks = instance.SocialLinkFolder.Children;

                            instance.SeminarItems = instance.Home.Descendants("SeminarItem");
                            instance.CourseCatalog = instance.Home.Descendants("SeminarCatalog").FirstOrDefault();
                            instance.SeminarSearch = instance.Home.Descendants("SearchSeminars").FirstOrDefault();
                            instance.SeminarMainOverview = instance.Home.Children.FirstOrDefault(n => n.IsDocumentType("SeminarMainOverview"));
                            instance.SeminarCategories = instance.SeminarMainOverview.Children.Where(n => n.IsDocumentType("SeminarCategory"));
                        }
                    }
                }
                return instance;
            }
            set
            {
                instance = value;
            }
        }


        public static IPublishedContent SiteSettingsDirect()
        {
            IPublishedContent siteSettings = null;

            var umbHelper = new UmbracoHelper(UmbracoContext.Current);

            siteSettings = umbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("SiteSettings"));

            return siteSettings;
        }


        public static IPublishedContent OnSiteTraining()
        {
            IPublishedContent onsiteTrainingNode = null;

            var umbHelper = new UmbracoHelper(UmbracoContext.Current);

            IPublishedContent homepageNode = umbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("HomePage"));

            if (homepageNode != null)
            {
                onsiteTrainingNode = homepageNode.Children.FirstOrDefault(n => n.IsDocumentType("OnSiteTraining"));
            }

            return onsiteTrainingNode;
        }

        public static IEnumerable<IPublishedContent> SeminarCategoryList()
        {
            IEnumerable<IPublishedContent> seminarCategoryList = null;

            var umbHelper = new UmbracoHelper(UmbracoContext.Current);

            IPublishedContent homepageNode = umbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("HomePage"));

            if (homepageNode != null)
            {
                IPublishedContent seminarMain = homepageNode.Children.FirstOrDefault(n => n.IsDocumentType("SeminarMainOverview"));

                if (seminarMain != null)
                {
                    seminarCategoryList = seminarMain.Children.Where(n => n.IsDocumentType("SeminarCategory"));
                }
            }

            return seminarCategoryList;
        }


        public static IEnumerable<IPublishedContent> SeminarItemList()
        {
            IEnumerable<IPublishedContent> seminarItemList = null;

            var umbHelper = new UmbracoHelper(UmbracoContext.Current);

            IPublishedContent homepageNode = umbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("HomePage"));

            if (homepageNode != null)
            {
                seminarItemList = homepageNode.Descendants().Where(n => n.IsDocumentType("SeminarItem"));
            }

            return seminarItemList;
        }


        public static IPublishedContent RedirectFolder()
        {
            IPublishedContent redirectFolder = null;

            var umbHelper = new UmbracoHelper(UmbracoContext.Current);

            IPublishedContent siteSettings = umbHelper.TypedContentAtRoot().FirstOrDefault(n => n.IsDocumentType("SiteSettings"));

            if (siteSettings != null)
            {
                redirectFolder = siteSettings.Children.FirstOrDefault(n => n.IsDocumentType("RedirectsFolder"));
            }

            return redirectFolder;
        }


        public static IEnumerable<IPublishedContent> RedirectList()
        {
            IEnumerable<IPublishedContent> redirectLIst = null;

            var umbHelper = new UmbracoHelper(UmbracoContext.Current);

            IPublishedContent redirectFolder = RedirectFolder();

            if (redirectFolder != null)
            {
                redirectLIst = redirectFolder.Children;
            }

            return redirectLIst;
        }

        public static string OnSiteSeminarLinks()
        {
            string sLinks = "";
            try {
                IPublishedContent node = Nodes.Instance.SeminarSearch;
                if (node == null)
                    throw new Exception("");
                if (!node.HasProperty("onSiteCourses") || !node.GetProperty("onSiteCourses").HasValue)
                    throw new Exception("");
                sLinks = node.GetProperty("onSiteCourses").Value.ToString();
            }
            catch(Exception ex) {
                sLinks = "";
            }
            return sLinks;
        }
    }
}