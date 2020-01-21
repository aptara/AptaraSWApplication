using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Specialized;
using FileDateTimeDetails;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Globalization;

namespace FileDateTimeDetails
{
    public partial class FileDateTimeDetails : Form
    {
        public FileDateTimeDetails()
        {
            InitializeComponent();
        }

        private void FileDateTimeDetails_Load(object sender, EventArgs e)
        {
            ReadLogFilesData();
            ReadFileDetails();
        }
        public void ReadFileDetails()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string folderPath = (ConfigurationSettings.AppSettings["XMLFileLocation"]).ToString();
            string xmlString = string.Empty;
            List<Data> listOfFileDetailsToSave = new List<Data>();
            List<Data> OrderByAscending = new List<Data>();
            List<Data> OrderByDescending = new List<Data>();


            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = "DateTimeDetails";
            xRoot.IsNullable = true;
            XmlSerializer deserializer = new XmlSerializer(typeof(DateTimeDetails), xRoot);
            //TextReader reader = new StreamReader(@"..\..\FileData.xml");
            TextReader reader = new StreamReader(ConfigurationSettings.AppSettings["XMLFileDataPath"]);

            object obj = deserializer.Deserialize(reader);
            DateTimeDetails XmlData = (DateTimeDetails)obj;
            for (int i = 0; i < XmlData.data.Count; i++)
            {

                DirectoryInfo d = new DirectoryInfo(XmlData.data[i].FolderName);
                string path = XmlData.data[i].UNCPath + XmlData.data[i].FolderName;       


                foreach (string file in Directory.EnumerateFiles(path, "*.log"))
                {
                    string IgnorFiles = XmlData.data[i].IgnorFiles;
                    string[] ArrayIgnorFiles = IgnorFiles.Split(',');

                    if (!ArrayIgnorFiles.Contains(Path.GetFileName(file)))
                    {
                        string FileName = Path.GetFileName(file);
                        string[] lines = File.ReadAllLines(file);
                        Data objData = new Data();
                        objData.FolderName = "";
                        objData.FileName = FileName;
                        objData.UNCPath = folderPath;
                        objData.FileUpdatedDate = new List<DateTime>();
                        objData.MinMaxData = new List<MinMaxDates>();
                        foreach (string line in lines)
                        {

                            DateTime FindDate;
                            string CurretLogFileLine = line;
                            Match MatchRegulerExpression = Regex.Match(CurretLogFileLine, @"20\d{2}(-|\/)((0[1-9])|(1[0-2]))(-|\/)((0[1-9])|([1-2][0-9])|(3[0-1]))(T|\s)(([0-1][0-9])|(2[0-3])):([0-5][0-9]):([0-5][0-9])");
                            if (MatchRegulerExpression == null)
                            {
                                MatchRegulerExpression = Regex.Match(CurretLogFileLine, @"^((((([0-1]?\d)|(2[0-8]))\/((0?\d)|(1[0-2])))|(29\/((0?[1,3-9])|(1[0-2])))|(30\/((0?[1,3-9])|(1[0-2])))|(31\/((0?[13578])|(1[0-2]))))\/((19\d{2})|([2-9]\d{3}))|(29\/0?2\/(((([2468][048])|([3579][26]))00)|(((19)|([2-9]\d))(([2468]0)|([02468][48])|([13579][26]))))))\s(([01]?\d)|(2[0-3]))(:[0-5]?\d){2}$");
                            }
                            if (MatchRegulerExpression == null)
                            {
                                MatchRegulerExpression = Regex.Match(CurretLogFileLine, @"^(?=\d)(?:(?:31(?!.(?:0?[2469]|11))|(?:30|29)(?!.0?2)|29(?=.0?2.(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00)))(?:\x20|$))|(?:2[0-8]|1\d|0?[1-9]))([-./])(?:1[012]|0?[1-9])\1(?:1[6-9]|[2-9]\d)?\d\d(?:(?=\x20\d)\x20|$))?(((0?[1-9]|1[012])(:[0-5]\d){0,2}(\x20[AP]M))|([01]\d|2[0-3])(:[0-5]\d){1,2})?$");
                            }

                            if (!string.IsNullOrEmpty(MatchRegulerExpression.Value))
                            {
                                FindDate = Convert.ToDateTime(MatchRegulerExpression.Value);
                                if (!objData.FileUpdatedDate.Contains(FindDate))
                                {
                                    objData.FileUpdatedDate.Add(FindDate);
                                }
                            }
                        }

                        var groupByDates = objData.FileUpdatedDate.GroupBy(o => o.Date);
                        foreach (IGrouping<DateTime, DateTime> singleDates in groupByDates)
                        {
                            objData.MinMaxData.Add(new MinMaxDates { MinDate = singleDates.Min(), maxDate = singleDates.Max() });
                        }

                        listOfFileDetailsToSave.Add(objData);
                    }
                }
            }
            xmlString = Serialize(listOfFileDetailsToSave);
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("USP_DateTimeDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@InputXMLString", SqlDbType.VarChar).Value = xmlString;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Data Uploaded Successfully");
        }



        public void ReadLogFilesData()
        {

            // string xmlString = System.IO.File.ReadAllText("../../FileData.xml");
            List<Data> listOfSystemDetails = new List<Data>();
            List<Data> listOfFileDetailsToSave = new List<Data>();
            XmlRootAttribute xRoot = new XmlRootAttribute();
            string xmlString = string.Empty;
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            xRoot.ElementName = "DateTimeDetails";
            xRoot.IsNullable = true;
            XmlSerializer deserializer = new XmlSerializer(typeof(DateTimeDetails), xRoot);
            //TextReader reader = new StreamReader(@"..\..\FileData.xml");
            TextReader reader = new StreamReader(ConfigurationSettings.AppSettings["XMLFileDataPath"]);

            object obj = deserializer.Deserialize(reader);
            DateTimeDetails XmlData = (DateTimeDetails)obj;
            reader.Close();

            //Loop through each record
            for (int i = 0; i < XmlData.data.Count; i++)
            {
                try
                {
                    DirectoryInfo d = new DirectoryInfo(XmlData.data[i].FolderName);
                    string path = XmlData.data[i].UNCPath + XmlData.data[i].FolderName + XmlData.data[i].FileName;
                    //"\\\\PNF1-ENT-C-009\\Users\\rahul.wani\\AppData\\Local\\Articulate\\360\\Logs\\test.txt";


                    if (File.Exists(path))
                    {
                        //FileInfo[] Files = d.GetFiles();
                        //foreach (FileInfo file in Files)
                        //{
                        Data obj1 = new Data();
                        String modification = File.GetLastWriteTime(path).ToString();
                        obj1.FolderName = XmlData.data[i].FolderName;
                        obj1.FileName = XmlData.data[i].FileName;
                        obj1.LastUpdatedDate = modification;
                        obj1.UNCPath = XmlData.data[i].UNCPath;
                        obj1.ErrorMessage = "";
                        listOfFileDetailsToSave.Add(obj1);
                        // }
                    }
                    else
                    {
                        Data objData = new Data();
                        objData.FolderName = XmlData.data[i].FolderName;
                        objData.FileName = XmlData.data[i].FileName;
                        objData.LastUpdatedDate = "";
                        objData.UNCPath = XmlData.data[i].UNCPath;
                        objData.ErrorMessage = "File Not Exists in Given Path";
                        listOfFileDetailsToSave.Add(objData);
                    }
                }
                catch (Exception ex)
                {
                    Data objData = new Data();
                    objData.FolderName = XmlData.data[i].FolderName;
                    objData.FileName = XmlData.data[i].FileName;
                    objData.LastUpdatedDate = "";
                    objData.UNCPath = XmlData.data[i].UNCPath;
                    objData.ErrorMessage = "Error while reading file details in given path. /n" + ex.Message;
                    listOfFileDetailsToSave.Add(objData);
                }
            }
            xmlString = Serialize(listOfFileDetailsToSave);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("USP_DateTimeDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@InputXMLString", SqlDbType.VarChar).Value = xmlString;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static string Serialize<T>(T dataToSerialize)
        {
            try
            {

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringBuilder builder = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                using (XmlWriter stringWriter = XmlWriter.Create(builder, settings))
                {
                    serializer.Serialize(stringWriter, dataToSerialize);
                    return builder.ToString();
                }

            }
            catch (Exception e)
            {
                throw;
            }
        }


    }
}
