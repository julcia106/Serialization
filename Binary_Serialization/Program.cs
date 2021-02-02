//#define NORMAL
using GraZaDuzoZaMalo.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AppGraZaDuzoZaMaloCLI
{
    class Program
    {
        static void Main(string[] args)
        {
#if NORMAL
            (new KontrolerCLI()).Uruchom();
#else

            const string FileName = "C:/Users/Julia/source/repos/Serialization/Binary_Serialization/example.txt";

            if (File.Exists(FileName))
            {
                Console.WriteLine("Reading saved file");
                Stream openFileStream = File.OpenRead(FileName);
                BinaryFormatter deserializer = new BinaryFormatter();

                //Deserialize- first method---------------
                int length = (int)deserializer.Deserialize(openFileStream);
                List<Gra.Ruch> tab = new List<Gra.Ruch>(length);
                for (int i = 0; i < length; i++)
                {
                    tab.Add((Gra.Ruch)deserializer.Deserialize(openFileStream));
                }
                //-------------------------------------------
                //Deserialize- second method--------------
                //IReadOnlyList<Gra.Ruch> tab = deserializer.Deserialize(openFileStream) as IReadOnlyList<Gra.Ruch>;
                //--------------------------------------

                openFileStream.Close();
            }
            else
                throw new FileNotFoundException();
#endif
        }

    }
}
