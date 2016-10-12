/*
 * HS Replicator - Gustavo Augusto Hennig
 * http://code.google.com/p/hsreplicator/
 *  
 * "The contents of this file are subject to the Mozilla Public License
 * Version 1.1 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS"
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
 * License for the specific language governing rights and limitations
 * under the License.
 * The Initial Developer of the Original Code is Gustavo Augusto Hennig.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;


namespace FileSearching
{
    public class Serializer
    {


        public void Grava(object obj, string FileName)
        {

            //Opens a file and serializes the object into it in binary format.
            Stream stream = File.Open(FileName, FileMode.Create);
            //BinaryFormatter formatter = new BinaryFormatter();
            XmlSerializer formatter = new XmlSerializer(obj.GetType());
            


            //BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            stream.Close();

        }

        public object  Busca(string FileName, Type type)
        {

            //BinaryFormatter formatter = new BinaryFormatter();
            //Opens file "data.xml" and deserializes the object from it.
            Stream stream = File.Open(FileName, FileMode.Open);

            XmlSerializer formatter = new XmlSerializer(type);
            //formatter = new BinaryFormatter();

            object obj = formatter.Deserialize(stream);
            stream.Close();
            return obj;


        }
    }
}
