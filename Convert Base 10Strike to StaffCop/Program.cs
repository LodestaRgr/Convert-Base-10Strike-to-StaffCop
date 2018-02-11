//extern alias ReadINI;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;

namespace Convert_Base_10Strike_to_StaffCop.Internals
{
    internal struct Program
    {
        //public static object MessageBox { get; private set; }

        static void Main(string[] args)
        {
            
            Console.Clear();
            Console.WriteLine("\n=[ Convert Base 10Strike-Inventoryzation to StaffCop 3.6 ]=====================\n" + 
                                "==============================[ 2018 Zemlyakov Jurij (LodestaRgr@yandex.ru) ]==\n");
            //Получаем имя запускного файла (без расширения)
            string exe_name = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location);

// -------- Проверяем наличие INI файла

            if(!File.Exists(exe_name + ".ini"))
            {
                Console.WriteLine("Not found INI file (" + exe_name + ".ini) !");
                goto iniexample;
            }

            var rini = new ReadINI(exe_name + ".ini");
            string SCu_body = rini.Read("SCu_body","StaffCop_user.xml");
            string SCu_group = rini.Read("SCu_group", "StaffCop_user.xml");
            string SCu_agent = rini.Read("SCu_agent", "StaffCop_user.xml");
 
            if (String.IsNullOrEmpty(SCu_body) |
                String.IsNullOrEmpty(SCu_group) |
                String.IsNullOrEmpty(SCu_agent)) goto iniexample;

            goto iniend;

            iniexample: //информация о синтаксите INI файла

                Console.WriteLine("\nExample ini file:\n\n" +
                                        "[StaffCop_user.xml]\n" +
                                        "SCu_body =<StaffcopAgents>$SCu_group$</StaffcopAgents>\n" +
                                        "SCu_group=<Group Name = \"$NAME$\">$SCu_agent$</Group>\n" +
                                        "SCu_agent=<Agent Name = \"$NAME$\" ComputerName=\"$IP$\" UserName=\"$IP$\"/>\n");
                return;

            iniend:

            // -------- Проверка аргументов
            string path10strike = "",
                   pathStaffCop = "";
            try
            {
                string in_name = args[0];

                if (Directory.Exists(in_name))
                {
                    path10strike = Path.GetFullPath(in_name);   //Путь к папке с архивами от 10Strike-Inventoryzation
                }
                else goto argsexample;
            }
            catch { goto argsexample; }

            try
            {

                string out_name = args[1];

                if (Directory.Exists(out_name))
                {
                    pathStaffCop = Path.GetFullPath(out_name);   //Путь к папке для создания базы StaffCop
                }
                else goto argsexample;
            }
            catch { goto argsexample; }

            goto argsend;

            argsexample: //информация о синтаксите аргументов

            Console.WriteLine("  Usege:\t.exe [Base_10Strike] [out_Base_StaffCop]\n\n" +
                                "\tBase_10Strike     - path to directory database file (*.zip)\n" +
                                "\tout_Base_StaffCop - path to directory for create StaffCop file (*.xml)\n\n" +
                                "  Example:\n\n\t" + exe_name + ".exe .\\Base_10Strike\\ .\\Base_StaffCop\\\n\n");
            return;

            argsend:

//            return;

            //string path10strike = @".\Base_10Strike\";   //Путь к папке с архивами от 10Strike-Inventoryzation

            FileInfo LastZipConfigFile; //Поиск последнего созданного файла с настройками 10Strike-Inventoryzation
            try 
            {
                LastZipConfigFile = new DirectoryInfo(path10strike).GetFiles("*inventoryconfig*.zip").OrderByDescending(f => f.LastWriteTime).First();
            }
            catch (Exception)
            {
                Console.WriteLine("No found '10Strike' config base file (inventoryconfig*.zip)");
                return;
            }

            FileInfo LastZipBaseFile;   //Поиск последнего созданного файла с базой 10Strike-Inventoryzation
            try    
            {
                LastZipBaseFile = new DirectoryInfo(path10strike).GetFiles("*inventorybase*.zip").OrderByDescending(f => f.LastWriteTime).First();
            }
            catch (Exception)
            {
                Console.WriteLine("No found '10Strike' base file (inventorybase*.zip)");
                return;
            }

            //Console.WriteLine(LastZipConfigFile.ToString());
            //Console.WriteLine(LastZipBaseFile.ToString());

            // Создает TEMP папку
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);

            //Console.WriteLine(tempFolder);

            //Извлекает файл "GroupsTree.dat" из "inventoryconfig*.zip" в TEMP
            using (var unzip = new Unzip(Path.GetFullPath(path10strike + "\\" + LastZipConfigFile.ToString())))
            {
                unzip.Extract("GroupsTree.dat", tempFolder + @"\GroupsTree.dat");
            }

            //строки файла в массив "GroupsTree.dat"
            List<string> ListBaseConfig = new List<string>(System.IO.File.ReadAllLines(tempFolder + @"\GroupsTree.dat", Encoding.GetEncoding("windows-1251")));

            //список файлов из архива "inventorybase*.zip" в массив
            List<string> ListBaseComp = new List<string>();
            using (var unzip = new Unzip(Path.GetFullPath(path10strike + "\\" + LastZipBaseFile.ToString())))
            {
                foreach (var fileName in unzip.FileNames)
                {
                    //Конвертация из Cyrilic-866 в Windows-1251
                    ListBaseComp.Add(Encoding.GetEncoding(866).GetString(Encoding.GetEncoding(1251).GetBytes(fileName)));
                }

            }

            string thisGroup = "";       //текущая группа
            string thisGroupOld = "";    //предыдущая группа
            string thisComp = "";        //текущий компьютор

            string outfile = "";         //буффер для формирования финального файла
            string outagent = "";       //временный буфер
            string outbuf = "";       //временный буфер

            int ci = 1;                     //Номер компьютора
            int cc = ListBaseConfig.Count;  //Всего компьютеров
            int ccl = cc.ToString().Length; //количество знаков

            //открывает файл для записи
            //FileStream out_file = new FileStream(@".\temp", FileMode.Create);
            //StreamWriter writer = new StreamWriter(out_file, Encoding.Default);

            Console.Write("Processed ");
            
            //последняя позиция курсора
            int cx = Console.CursorLeft;
            int cy = Console.CursorTop;

            Console.WriteLine(String.Format("{0," + ccl + "}", "") + " of " + cc);

            //Читает каждую чтроку массива "GroupsTree.dat"
            foreach (string r in ListBaseConfig)
            {
                //Если строка соответствует "Группе"
                if (Regex.IsMatch(r, @"^(\d+)\\(.+)"))
                {
                    string pattern = @"^(\d+)\\|(.+)";
                    Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                    MatchCollection matches = rgx.Matches(r);

                    //Console.WriteLine("{1} - {0}", r, matches.Count);
                    //Console.WriteLine("    {0}", matches[1].Value);

                    //writer.Write(String.Format(matches[1].Value + "\n"));

                    thisGroup = matches[1].Value;

                    //foreach (Match match in matches)
                    //    Console.WriteLine("   " + match.Value);
                }
                //Если строка является именем компьютора
                else
                { 

                    if (thisGroup != thisGroupOld && !String.IsNullOrEmpty(thisGroupOld))
                    {
                        if (!String.IsNullOrEmpty(outbuf))
                        {
                            //Формирует строку xml <Group .../>
                            outbuf = Regex.Replace(SCu_group, @"\$SCu_agent\$", outbuf);
                            outbuf = Regex.Replace(outbuf, @"\$NAME\$", Regex.Replace(thisGroupOld, "\"", "&quot;"));
                            outfile += outbuf + "\n";

                            //writer.Write(String.Format(outbuf + "\n"));
                            outbuf = "";
                        }
                        //writer.Write(String.Format("--( " + thisGroup + " )------------------------------\n"));
                    }
                    thisGroupOld = thisGroup;

                    //получить полное имя файла текущего компьютора
                    thisComp = ListBaseComp.Find(x => x.Contains(r + ".fld"));
                    if (!String.IsNullOrEmpty(thisComp))
                    {
                          //Console.WriteLine(thisComp);

                        Console.SetCursorPosition(cx, cy);                      //ставит курсор на позицию
                        Console.Write(String.Format("{0," + ccl + "}", ci));    //пишет номер обрабатываемой строки

                        //Извлекает файл компьютора из "inventorybase*.zip" в TEMP
                        using (var unzip = new Unzip(Path.GetFullPath(path10strike + "\\" + LastZipBaseFile.ToString())))
                        {
                            //Конвертация из Windows-1251 в Cyrilic-866
                            unzip.Extract(Encoding.GetEncoding(1251).GetString(Encoding.GetEncoding(866).GetBytes(thisComp)), tempFolder + @"\" + thisComp);
                        }

                        //Ищет значение IPAddress и файле компьютора
                        //открывает файл для чтения
                        FileStream in_file = new FileStream(tempFolder + @"\" + thisComp, FileMode.Open, FileAccess.Read);
                        StreamReader reader = new StreamReader(in_file, Encoding.Default);

                        string buf;                 

                        while (!reader.EndOfStream)                     //Чтение файла
                        {
                            buf = reader.ReadLine();                    //считывает строку
                            if (!String.IsNullOrEmpty(buf))
                            {
                                //Ищем IP-адрес
                                if (Regex.IsMatch(buf, @".IPAddress=(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}"))
                                {
                                    string pattern = @".IPAddress=|(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}";
                                    Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                                    MatchCollection matches = rgx.Matches(buf);
                                    if (matches.Count > 1)
                                    {
                                        Console.WriteLine("\n\n" + matches[1].Value + "\t" + r + "\t\t");

                                        //writer.Write(String.Format(matches[1].Value + "\t" + r + "\t" + thisGroup + "\n"));

                                        //Формирует строку xml <Agent .../>
                                        outagent = Regex.Replace(SCu_agent, @"\$NAME\$", 
                                            Regex.Replace(
                                                //к имени компьютора добавляется октеты из IP-адреса (С.D)
                                                r + Regex.Replace(matches[1].Value, @"[0-9]+.[0-9]+.([0-9]+)(\.[0-9]+)", " ($1$2)"),
                                            //Заменяет двойные кавычки на &quot;
                                            "\"", "&quot;")
                                            );
                                        outagent = Regex.Replace(outagent, @"\$IP\$", matches[1].Value);
                                        outbuf += outagent;

                                        //Console.WriteLine("\n\n" + Regex.Replace(matches[1].Value, @"[0-9]+.[0-9]+.([0-9]+)(\.[0-9]+)", " ($1$2)"));
                                    }
                                    break;
                                }
                            }
                        }

                        //Закрыть считываемый фалй
                        reader.Close();

                        //writer.Write(String.Format(thisGroup + "        " + r + "\n"));
                    }
                }

                ci = ci + 1;
            }

            Console.WriteLine("\n\n\t\t\t\t\t\t\t\t\t");
            Console.SetCursorPosition(cx, cy);
            Console.WriteLine("\n");

            //Формирует строку xml <StaffcopAgents> (body)
            outfile = Regex.Replace(SCu_body, @"\$SCu_group\$", "\n" + outfile);

            //открывает файл для записи
            string outxml = System.IO.Path.GetFileNameWithoutExtension(LastZipBaseFile.ToString()) + ".xml";
            string outxmltmp = tempFolder + @"\" + outxml;

            FileStream out_file = new FileStream(outxmltmp, FileMode.Create);
            StreamWriter writer = new StreamWriter(out_file, Encoding.Default);


            //pathStaffCop + "\\" +
            //Конвертация из Windows-1251 в UTF-8
            outfile = Encoding.GetEncoding(1251).GetString(Encoding.UTF8.GetBytes(outfile));

            //Запись в файл 
            writer.Write(String.Format(outfile));

            //Закрыть файл
            writer.Close();

            //Сортировка по алфавиту XML файла
            var sx = new SortXml();
                 bool sort_node = true,
                      sort_attr = false,
                      overwriteSelf = true;

                string primary_attr = "Name";
                string inf = outxmltmp,
                outf = outxml;

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(File.ReadAllText(inf));
                //doc.PreserveWhitespace = !pretty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("**** Could not load input file");
                Console.WriteLine(ex.Message);
                return;
            }


            if (sort_attr)
            {
                if (primary_attr == null || primary_attr.Length == 0)
                {
                    primary_attr = "Name";
                }
                sx.SortNodeAttrs(doc.DocumentElement);
            }
            if (sort_node)
            {
                sx.SortNodes(doc.DocumentElement);
            }

            if (outf.Length == 0 && overwriteSelf)
            {
                outf = inf;
            }

            if (outf.Length > 0)
            {
                try
                {
                    doc.Save(pathStaffCop + "\\" + outf); //Записть файла xml StaffCop
                }
                catch (Exception ex)
                {
                    Console.WriteLine("**** Could not save output file");
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
            else
            {
                doc.Save(Console.Out);
            }
 

            Console.WriteLine("Complete...");

            /*
                        if (args.Length == 2)
                        {
                            //Console.WriteLine("Arguments: " + string.Join(",", args));
                            string in_name = args[0];
                            string out_name = args[1];

                            if (File.Exists(in_name))
                            {
                                long in_count = System.IO.File.ReadAllLines(in_name).Length;
                                long i = 1;
                                bool j = false;

                                //определяет имя файла
                                FileInfo fi = new FileInfo(in_name);
                                string fname = fi.Name;

                                //определяет версию файла
                                string fver = "???";							//если первые символы другие
                                if(fname.Length >= 3)							//если имя файла >= 3-м символам
                                {
                                    if (fname.Substring(0,3) == "012")			//если первые символы 012
                                    {
                                        fver = "012";
                                    }
                                    else if (fname.Substring(0,3) == "120")		//если первые символы 120
                                    {
                                        fver = "120";
                                    }
                                }

                                //открывает файл для чтения
                                FileStream in_file = new FileStream(in_name, FileMode.Open, FileAccess.Read);
                                StreamReader reader = new StreamReader(in_file, Encoding.Default);


                                //открывает файл для записи
                                FileStream out_file = new FileStream(out_name, FileMode.Create);
                                StreamWriter writer = new StreamWriter(out_file, Encoding.Default);

                                Console.Write("  PFR base : \t" + fname + " (ver. " + fver + ")\n\n" +
                                              "  Total lines: \t" + in_count + "\n" +
                                              "  Convert: \t");

                                //последняя позиция курсора
                                int cx = Console.CursorLeft;
                                int cy = Console.CursorTop;

                                Console.Write("\n\n==============================================================================");

                                string buf = reader.ReadLine();                 //Считывает первую строку
                                string k = ((char) reader.Peek()).ToString();   //Запоминает первый символ следующей строки

                                while (!reader.EndOfStream)                     //Запуск процесса конвертации
                                {
                                    Console.SetCursorPosition(cx, cy);          //ставит курсор на позицию
                                    Console.Write(i);                           //пишет номер обрабатываемой строки

                                    buf = reader.ReadLine();                    //считывает строку

                                    if (buf[0].ToString() == k)                 //если строка начинается с символа к
                                    {
                                        if (buf.Length == 978 || buf.Length == 1022)
                                        {
                                            writer.Write(buf);
                                        }
                                        else									//если строка не стандартной длины
                                        {
                                            if (fver == "012")					//если версия файла 012
                                            {
                                                writer.Write(String.Format("{0,-978}", buf));	//добавить пробелы до 978
                                            }
                                            else if (fver == "120")				//если версия файла 120
                                            {
                                                writer.Write(String.Format("{0,-1022}", buf));	//добавить пробелы до 1022
                                            }
                                            else								//если  версия файла другая
                                            {
                                                if (buf.Length < 978)			//если строка меньше формата (MS0 - 012xxx.ms0)
                                                {
                                                    writer.Write(String.Format("{0,-978}", buf));	//добавить пробелы до 978
                                                }
                                                else							//если строка меньше формата (000 - 120xxx.ms0)
                                                {
                                                    writer.Write(String.Format("{0,-1022}", buf));	//добавить пробелы до 1022
                                                }
                                            }
                                        }
                                        j = false;
                                    }
                                    else
                                    {
                                        if (!j)
                                        {
                                            writer.Write(buf + "\n");
            //								writer.Write(String.Format("{0,-222}", buf)); 	//добавить пробелы до 222
            //								writer.Write("\n");
                                            j = true;
                                        }

                                    }
                                    //System.Threading.Thread.Sleep(10);        //пауза 10мс
                                    i++;
                                }

                                Console.WriteLine("\n\n=[ Process is finished! ]");

                                writer.Close();
                                reader.Close();
                            }
                            else
                                Console.WriteLine("  File PFR base (MS0) - not exists.");

                        }
                        else
                            Console.WriteLine("  Usege:\t10strk_to_staffcop.exe [MS0 file] [out txt file]\n\n" +
                                              "\t\tMS0 file - this PFR base file (format MS0)\n" +
                                              "\t\tTXT file - this output file after convert\n\n" +
                                              "  Example:\t10strk_to_staffcop.exe PFR12_O.MS0 PFR12_O.txt");
            */
            //Console.ReadLine();
        Directory.Delete(tempFolder, true);

        }

    }
}
