/*
 * FileSearcher.java
 *
 * Created on 9 de Março de 2007, 14:21
 *
 * To change this template, choose Tools | Template Manager
 * and open the template in the editor.
 */

using System.IO;
using System;
using System.Collections.Generic;


namespace SimpleFileSearch
{



    /**
     *
     * @author gustavoh
     */
    public class FileSearcher
    {

        public event Estado estado;

        /** Creates a new instance of FileSearcher */
        public FileSearcher()
        {
        }

        /**
         *
         * @param path
         * @param filename
         * @param infile
         * @return
         */
        public List<FileInfo> SearchFiles(String path, String filename, String infile, bool CaseSens)
        {

            List<FileInfo> al = new List<FileInfo>();

            bool bSearchIn = (infile != "");


            try
            {

                string[] files = Directory.GetFiles(path, filename, SearchOption.AllDirectories);

                estado.Invoke("Verifying files...");

                int Max = files.Length;
                int cnt =0;
                int show = 0;
                

                foreach (string sf in files)
                {
                    cnt++;
                    if (bSearchIn)
                    {
                        if (SearchInFile(sf, infile, CaseSens))
                        {
                            al.Add(new FileInfo(sf));
                        }
                    }
                    else
                    {
                        al.Add(new FileInfo(sf));
                        
                    }
                    show++;
                    if(show > 50){
                        show = 0;
                        estado.Invoke("Verifying files, "+ cnt + " de " + Max + " ...");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return al;

        }



        private bool SearchInFile(string file, String searchWord, bool CaseSens)
        {

            try
            {
                

                using (StreamReader sr = File.OpenText(file))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {

                        if (CaseSens)
                        {
                            if (line.Contains(searchWord))
                            {
                                return true;
                            }
                        }
                        else {
                            if (line.IndexOf(searchWord,StringComparison.InvariantCultureIgnoreCase) > 0)
                            {
                                return true;
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;

            }

            return false;

        }

    }

    public delegate void Estado(string value);

}