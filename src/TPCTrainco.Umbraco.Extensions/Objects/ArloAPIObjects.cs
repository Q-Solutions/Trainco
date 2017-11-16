using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TPCTrainco.Umbraco.Extensions.Objects
{

    #region EventTemplates

    [XmlRoot(ElementName = "EventTemplates")]
    public class EventTemplates
    {
        [XmlElement("Link")]
        public Link[] Links { get; set; }
    }

    public class Link
    {
        [XmlElement("EventTemplate")]
        public EventTemplate EventTemplate { get; set; }

        [XmlAttribute(AttributeName = "href")]
        public string href { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string title { get; set; }
    }

    public class EventTemplate
    {
        [XmlElement("TemplateID")]
        public int TemplateID { get; set; }

        [XmlElement("UniqueIdentifier")]
        public string UniqueIdentifier { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Code")]
        public string Code { get; set; }

        [XmlElement("AdvertisedDuration")]
        public string AdvertisedDuration { get; set; }

        [XmlElement("TemplateHosting")]
        public string TemplateHosting { get; set; }

        [XmlElement("IsPrivate")]
        public bool IsPrivate { get; set; }

        [XmlElement("DefaultEventSessionType")]
        public string DefaultEventSessionType { get; set; }

        [XmlElement("Status")]
        public string Status { get; set; }

        [XmlElement("PublishOnWebsite")]
        public bool PublishOnWebsite { get; set; }

        [XmlElement("CreatedDateTime")]
        public DateTime CreatedDateTime { get; set; }

        [XmlElement("LastModifiedDateTime")]
        public DateTime LastModifiedDateTime { get; set; }

        [XmlElement("Link")]
        public EventTempleteLink[] Link { get; set; }
    }

    public class EventTempleteLink
    {
        [XmlAttribute(AttributeName = "href")]
        public string href { get; set; }
        [XmlAttribute(AttributeName = "title")]
        public string title { get; set; }
    }

    #endregion

    #region Events

    [XmlRoot(ElementName = "Events")]
    public class Events
    {
        [XmlElement("Link")]
        public EventBaseLink[] Links { get; set; }
    }

    public class EventBaseLink
    {
        [XmlElement("Event")]
        public Event Event { get; set; }

        [XmlAttribute(AttributeName = "href")]
        public string href { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string title { get; set; }
    }

    public class Event
    {
        [XmlElement("EventID")]
        public int EventID { get; set; }

        [XmlElement("UniqueIdentifier")]
        public string UniqueIdentifier { get; set; }

        [XmlElement("Code")]
        public string Code { get; set; }

        [XmlElement("StartDateTime")]
        public DateTime StartDateTime { get; set; }

        [XmlElement("FinishDateTime")]
        public DateTime FinishDateTime { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("LocationName")]
        public string LocationName { get; set; }

        [XmlElement("IsPrivate")]
        public bool IsPrivate { get; set; }

        [XmlElement("Status")]
        public string Status { get; set; }

        [XmlElement("CreatedDateTime")]
        public DateTime CreatedDateTime { get; set; }

        [XmlElement("LastModifiedDateTime")]
        public DateTime LastModifiedDateTime { get; set; }

        [XmlElement("Link")]
        public EventTempleteLink[] Link { get; set; }
    }

    #endregion
}
