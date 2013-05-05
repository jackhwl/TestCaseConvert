using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace Convert
{
    class Program
    {
        static void Main(string[] args)
        {
            var testFolder = @"C:\Users\Jack\Documents\GitHub\WebPerformanceTest\EcmWeb\EcmWeb";
            var testFile = @"C:\Users\Jack\Documents\GitHub\WebPerformanceTest\EcmWebBilingual\EcmWebBilingual\WebAdminActionConfig\WT_DT_ActionAddAll.webtest";
            var testFile2 = @"C:\Users\Jack\Documents\GitHub\WebPerformanceTest\EcmWebBilingual\EcmWebBilingual\WebAdminActionConfig\Data.xml";
            var logDuplicateFile = @"C:\Users\Jack\Documents\GitHub\WebPerformanceTest\logDuplicate.txt";
            var logSuccessFile = @"C:\Users\Jack\Documents\GitHub\WebPerformanceTest\logSuccess.txt";
            var logFailFile = @"C:\Users\Jack\Documents\GitHub\WebPerformanceTest\logFail.txt";
            int counterDup = 0, counterSuccess = 0, counterFail = 0;

            List<string> test = getResourceID("Completed Successfully");


            TextWriter writer = File.CreateText(logSuccessFile);
            TextWriter writerFail = File.CreateText(logFailFile);
            TextWriter writerDuplicate = File.CreateText(logDuplicateFile);
            

            DirectoryInfo directoryInfo = new DirectoryInfo(testFolder);
            IEnumerable<FileInfo> fileList = directoryInfo.GetFiles("*.webtest", SearchOption.AllDirectories);

            XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

            var previousDiretoryName = fileList.First().DirectoryName;
            writer.WriteLine("********************  " + previousDiretoryName + "  ********************");
            writerFail.WriteLine("********************  " + previousDiretoryName + "  ********************");
            writerDuplicate.WriteLine("********************  " + previousDiretoryName + "  ********************");
            foreach (FileInfo fileinfo in fileList)
            {
                var directoryName = fileinfo.DirectoryName;
                if (directoryName != previousDiretoryName)
                {
                    previousDiretoryName = directoryName;
                    writer.WriteLine("\n");
                    writerFail.WriteLine("\n");
                    writerDuplicate.WriteLine("\n");
                    writer.WriteLine("********************  " + previousDiretoryName + "  ********************");
                }
                //if (fileinfo.Name == "Action16_Open_Config+List_WindowOption.webtest")
                //{
                writer.WriteLine("===========     " + fileinfo.Name + "     ===========");
                writerFail.WriteLine("===========     " + fileinfo.Name + "     ===========");
                writerDuplicate.WriteLine("===========     " + fileinfo.Name + "     ===========");
                    XDocument xdoc = XDocument.Load(fileinfo.FullName);
                    var findTextList = from item in xdoc.Descendants(ns + "ValidationRule")
                                       where item.Attribute("Classname").Value.IndexOf("Microsoft.VisualStudio.TestTools.WebTesting.Rules.ValidationRuleFindText") >= 0
                                       select item;

                    foreach (var findTextRule in findTextList)
                    {
                        foreach (XElement itemElement in findTextRule.Element(ns + "RuleParameters").Elements(ns + "RuleParameter"))
                        {
                            if (itemElement.Attribute("Name").Value == "FindText") //&& itemElement.Attribute("Value").Value == "Completed Successfully"
                            {
                                var residList = getResourceID(itemElement.Attribute("Value").Value);
                                if (residList.Count == 1)
                                {
                                    writer.WriteLine(string.Format("\"{0}\" replaced by id: \"{1}\"", itemElement.Attribute("Value").Value, residList.FirstOrDefault()));
                                    itemElement.SetAttributeValue("Value", residList.FirstOrDefault());
                                    findTextRule.SetAttributeValue("Classname", findTextRule.Attribute("Classname").Value.Replace("Microsoft.VisualStudio.TestTools.WebTesting.Rules.ValidationRuleFindText", "Doxim.TestTools.WebTesting.Rules.CustomValidationRule.FindResourceText"));
                                    XElement para = new XElement(ns + "RuleParameter");
                                    para.SetAttributeValue("Name", "Parameter0");
                                    para.SetAttributeValue("Value", "");
                                    itemElement.Parent.Add(para);
                                    para = new XElement(ns + "RuleParameter");
                                    para.SetAttributeValue("Name", "Parameter1");
                                    para.SetAttributeValue("Value", "");
                                    itemElement.Parent.Add(para);
                                    para = new XElement(ns + "RuleParameter");
                                    para.SetAttributeValue("Name", "Parameter2");
                                    para.SetAttributeValue("Value", "");
                                    itemElement.Parent.Add(para);
                                    para = new XElement(ns + "RuleParameter");
                                    para.SetAttributeValue("Name", "Parameter3");
                                    para.SetAttributeValue("Value", "");
                                    itemElement.Parent.Add(para);
                                    counterSuccess += 1;
                                }
                                else if (residList.Count == 0)
                                {
                                    writerFail.WriteLine(string.Format("##### can't find value: \"{0}\" ", itemElement.Attribute("Value").Value));
                                    counterFail += 1;
                                }
                                else 
                                {
                                    foreach (string resid in residList)
                                    {
                                        writerDuplicate.WriteLine(string.Format("......\"{0}\" find in {1} ", itemElement.Attribute("Value").Value, resid));
                                    }
                                    writerDuplicate.WriteLine();
                                    counterDup += 1; 
                                }
                                break;
                            }
                        }
                        //writer.WriteLine(findTextRule.ToString());
                    }
                    xdoc.Save(fileinfo.FullName);
                //}

            }

            writer.WriteLine("Total #: " + counterSuccess);
            writerFail.WriteLine("Total #: " + counterFail);
            writerDuplicate.WriteLine("Total #: " + counterDup);

            writer.Close();
            writerFail.Close();
            writerDuplicate.Close();

            Console.WriteLine(fileList.Count());
        }

        private static List<string> getResourceID(string findTextValue)
        {
            var testFolder = @"C:\Users\Jack\Documents\GitHub\WebPerformanceTest\EcmWebBilingual\DoximLocalization\Resources";

            XNamespace ns = "";
            DirectoryInfo directoryInfo = new DirectoryInfo(testFolder);
            IEnumerable<FileInfo> fileList = directoryInfo.GetFiles("*Res.resx", SearchOption.AllDirectories);
            /*/counter: 12
                            Js
                            LibBusinessLogic
                            LibCommon
                            LibDataAccess
                            LibDefs
                            LibExceptions
                            LibMisc
                            LibModel
                            Admin
                            Common
                            Misc
                            Web*/
            List<string> resIDList = new List<string>();
            foreach (FileInfo fileinfo in fileList)
            {
                XDocument xdoc = XDocument.Load(fileinfo.FullName);
                var fullname = fileinfo.FullName;
                var folder = fullname.Substring(fileinfo.DirectoryName.LastIndexOf("\\") + 1, fullname.LastIndexOf("\\") - fileinfo.DirectoryName.LastIndexOf("\\") - 1);
                var resName = (folder == "Lib" ? "Lib" : "")+fileinfo.Name.Substring(0, fileinfo.Name.Length-"Res.Resx".Length);

                var findTextList = from item in xdoc.Descendants(ns + "data")
                                   where item.Element("value").Value==findTextValue
                                   select item;

                foreach (var findText in findTextList)
                {
                    resIDList.Add(resName + "." + findText.Attribute("name").Value);
                }
            }
            return resIDList; 
        }

        

    }
}
