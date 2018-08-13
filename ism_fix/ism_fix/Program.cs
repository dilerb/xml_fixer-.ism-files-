using System;
using System.IO;
using System.Xml;

namespace ism_fix
{
    class Program
    {
        public static string output_log = "";
        static void Main(string[] args)
        {
   
            //File.AppendAllText("ism_log.csv", DateTime.Now + ";Fixer is started" + Environment.NewLine);
            if (args.Length != 1)
            {
                //File.AppendAllText("ism_log.csv", "Only 1 argument can be used" + Environment.NewLine + Environment.NewLine);
                return;
            }
            string path = args[0];
            DirectoryInfo dpath = new DirectoryInfo(path);
            FileInfo[] fileList = null;

            try
            {
                fileList = dpath.GetFiles("*.ism", SearchOption.AllDirectories);
            }
            catch (Exception e)
            {
                //File.AppendAllText("ism_log.csv", e.Message + " (" + DateTime.Now + ")" + Environment.NewLine + Environment.NewLine);
                return;
            }

            foreach (var f in fileList)
            {
                if (f.Extension != ".ism")
                    continue;
                output_log = f.FullName+";";
                if (!backup(f)) /* BACKUP */
                    continue; 

                bool flag = false; // Does include systemLanguage?

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(f.FullName);
                XmlNodeList audio = xmlDoc.GetElementsByTagName("audio");

                for (int i = 0; i < audio.Count; i++)
                {
                    flag = false;
                    var audio_att = audio[i].Attributes;
                    XmlAttribute[] child_att = new XmlAttribute[audio_att.Count];
                    audio_att.CopyTo(child_att, 0);

                    for (int l = 0; l < child_att.Length; l++)
                    {
                        if (child_att[l].Name == "systemLanguage")
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag == false)
                    {
                        if (GetTrackName(audio, i) == "Orijinal")
                        {
                            XmlAttribute newnode = xmlDoc.CreateAttribute("systemLanguage");
                            newnode.Value = "eng";
                            xmlDoc.GetElementsByTagName("audio")[i].Attributes.Append(newnode);
                        }
                        else if (GetTrackName(audio, i) == "Turkce")
                        {
                            XmlAttribute newnode = xmlDoc.CreateAttribute("systemLanguage");
                            newnode.Value = "tur";
                            xmlDoc.GetElementsByTagName("audio")[i].Attributes.Append(newnode);
                        }
                    }
                }
                //File.AppendAllText("ism_log.csv", DateTime.Now + ";" + f.FullName + ";" + "true" + Environment.NewLine);
                xmlDoc.Save(f.FullName);
            }
            //File.AppendAllText("ism_log.csv", DateTime.Now + ";Fixer is done" + Environment.NewLine + Environment.NewLine);
        }

        private static bool backup(FileInfo f)
        {
            int count = 1;
            string backup_path = "";
            try
            {                
                if (File.Exists(f.FullName + ".backup"))
                {
                    while (File.Exists(f.FullName + ".backup." + count.ToString()))
                    {
                        count++;
                    }
                    backup_path = f.FullName + ".backup." + count.ToString();                    
                    File.Copy(f.FullName, backup_path, false);
                    File.AppendAllText("ism_log.csv", DateTime.Now + ";" + output_log + backup_path + ";" + "true" + Environment.NewLine);
                }
                else
                {
                    backup_path = f.FullName + ".backup";
                    //throw new Exception();
                    File.Copy(f.FullName, backup_path, false);
                    File.AppendAllText("ism_log.csv", DateTime.Now + ";" + output_log + backup_path + ";" + "true" + Environment.NewLine);
                }
                return true;
            }
            catch (Exception e)
            {
                File.AppendAllText("ism_log.csv", DateTime.Now + ";" + output_log + backup_path + ";" + "false" + Environment.NewLine);
                return false;
            }
        }

        private static string GetTrackName(XmlNodeList audio, int i)
        {
            for (int j = 0; j < audio[i].ChildNodes.Count; j++)
            {
                var att = audio[i].ChildNodes[j].Attributes;
                for (int k = 0; k < att.Count; k++)
                {
                    if (att[k].Value == "trackName")
                    {
                        for (int l = 0; l < att.Count; l++)
                        {
                            if (att[l].Name == "value")
                                return att[l].Value;
                        }

                    }
                }
            }
            //File.AppendAllText("ism_log.csv", "trackName could not find!" + Environment.NewLine);
            return "trackName could not find!";
        }
    }
}
