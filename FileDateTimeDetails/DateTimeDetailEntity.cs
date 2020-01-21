using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileDateTimeDetails
{
    public class Data
    {
        public string UNCPath { get; set; }
        public string FolderName { get; set; }
        public string FileName { get; set; }
        [System.Xml.Serialization.XmlElement("LastUpdatedDate", IsNullable = true)]
        [XmlIgnore]
        public string LastUpdatedDate { get; set; }
        [System.Xml.Serialization.XmlElement("ErrorMessage", IsNullable = true)]
        public string ErrorMessage { get; set; }
        [XmlIgnore]
        public string LastModifiedDate { get; set; }

        [XmlIgnore]
        public List<DateTime> FileUpdatedDate { get; set; }
        
        public List<MinMaxDates> MinMaxData { get; set; }

        public string IgnorFiles { get; set; }
        //public DateTime LastUpdatedOnDate { get; set; }
        //public DateTime LastUpdatedOnTime { get; set; }
    }

    public class DateTimeDetails
    {
        [XmlElement("Data")]
        public List<Data> data = new List<Data>();
    }

    public class MinMaxDates
    {
        public DateTime MinDate { get; set; }
        public DateTime maxDate { get; set; }
    }
}
