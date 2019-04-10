using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace copybot
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();         
        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "sln files (*.sln)|*.sln|All files (*.*)|*.*";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string file = openFileDialog1.FileName;
                var sol = new Solution(file);
                label5.Text = sol.Projects.Count.ToString();

                var mojxml = new XmlDocument();
                try
                {
                    int project_nr = 0;
                    int files_count=0;

                    foreach (SolutionProject temp in sol.Projects)
                    {
                        string directoryPath = Path.GetDirectoryName(openFileDialog1.FileName);
                        //Console.WriteLine(directoryPath+"\\" + sol.Projects[project_nr].ProjectName + "\\" +sol.Projects[project_nr].ProjectName+".csproj");
            
                        mojxml.Load(directoryPath + "\\" + sol.Projects[project_nr].ProjectName + "\\" + sol.Projects[project_nr].ProjectName + ".csproj");

                        XmlNode currNode = mojxml.DocumentElement.FirstChild;
                        do
                        {
                            currNode = currNode.NextSibling;
                        } while (currNode.Name != "ItemGroup");
                        currNode = currNode.NextSibling;
                        currNode = currNode.FirstChild;

                        var lista = new List<string>();
                        XmlElement jeden = (XmlElement)currNode;
                        XmlAttribute attr = jeden.GetAttributeNode("Include");
                        lista.Add(attr.InnerXml);
                        //Console.WriteLine(attr.InnerXml);

                        bool warunek = true;
                        while(warunek)          //STRASZNIE ŁOPATOLOGICZNIE
                        {                      
                            if (currNode.NextSibling != null)
                            {
                                currNode = currNode.NextSibling;
                                jeden = (XmlElement)currNode;
                                attr = jeden.GetAttributeNode("Include");
                                //Console.WriteLine(attr.InnerXml);
                                lista.Add(attr.InnerXml);
                            }
                            else { warunek = false; }  
                        }
                        

                        string sourcePath = Path.GetFullPath(@"..\..\..\");
                        string destinationPath = Path.GetFullPath(@"..\..\..\KOPIA "+ sol.Projects[project_nr].ProjectName);

                        
                        string sourceFileName = openFileDialog1.SafeFileName;                      
                        string destinationFileName = "COPY " + sourceFileName;
                        string sourceFile = Path.Combine(sourcePath, sourceFileName);
                        string destinationFile = Path.Combine(destinationPath, destinationFileName);

      
                        if (!Directory.Exists(destinationPath))
                        {
                            Directory.CreateDirectory(destinationPath);
                        }
                        File.Copy(sourceFile, destinationFile, true);       //kopiuje SLN RECZNIE
                                                                            //przy wielu projektach bedzie sie kopiował wielokrotnie
                                                                            //do każdego folderu z plikami projektu
                        sourcePath = directoryPath+ "\\" + sol.Projects[project_nr].ProjectName;
                        Console.WriteLine("PATH: " + sourcePath);
                        sourceFileName = sol.Projects[project_nr].ProjectName + ".csproj";
                        Console.WriteLine("CSPROJ: " + sourceFileName);
                        destinationFileName = "COPY " + sourceFileName;
                        sourceFile = Path.Combine(sourcePath, sourceFileName);
                        Console.WriteLine("sourceFile: "+sourceFile);
                        destinationFile = Path.Combine(destinationPath, destinationFileName);
                        File.Copy(sourceFile, destinationFile, true);       //kopiuje CSPROJ

                        foreach (string plik in lista)
                        {
                            string[] name_order;
                            if (plik.Contains("\\"))
                            {
                                name_order = plik.Split('\\');
                                sourceFileName = name_order[1];                                                               
                                sourceFile = Path.Combine(sourcePath+"\\"+name_order[0], sourceFileName);
                                Console.WriteLine("SOURCE FILE: " + sourceFile);
                                destinationFileName = "COPY " + sourceFileName;
                                destinationPath = Path.GetFullPath(@"..\..\..\KOPIA " + sol.Projects[project_nr].ProjectName+"\\"+name_order[0]);
                                destinationFile = Path.Combine(destinationPath, destinationFileName);
                            }
                            else
                            {
                                sourceFileName = plik;
                                sourceFile = Path.Combine(sourcePath, sourceFileName);
                                destinationFileName = "COPY " + sourceFileName;
                                destinationPath = Path.GetFullPath(@"..\..\..\KOPIA " + sol.Projects[project_nr].ProjectName);
                                destinationFile = Path.Combine(destinationPath, destinationFileName);
                            }
                            
                            if (!Directory.Exists(destinationPath))
                            {
                                Directory.CreateDirectory(destinationPath);
                            }
                            File.Copy(sourceFile, destinationFile, true); 
                        }                                                   
                        project_nr++;
                        files_count += lista.Count+2; // liczba plików projeku + projekt + solucja
                        label6.Text = files_count.ToString();
                        label8.Text = "SKOPIOWANO PLIKI";
                    }
                }
                catch (FileNotFoundException)
                {
                    label6.Text = "BRAK PLIKU";
                }
                try
                {
                    label3.Text = openFileDialog1.SafeFileName;
                }
                catch (IOException)
                {
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = "KOPIOINATOR";
        }
    }
}
