#define NORMAL
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

            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("C:/Users/Julia/source/repos/Serialization/Binary_Serialization/example.txt", FileMode.Open, FileAccess.Read);

            //Deserialize- first method---------------
            //int length = (int)formatter.Deserialize(stream);
            //List<Gra.Ruch> tab = new List<Gra.Ruch>(length);
            //for (int i = 0; i < length; i++)
            //{
            //    tab.Add((Gra.Ruch)formatter.Deserialize(stream));
            //}
            //-------------------------------------------

            //Deserialize- second method--------------
            IReadOnlyList<Gra.Ruch> tab = formatter.Deserialize(stream) as IReadOnlyList<Gra.Ruch>;
            //--------------------------------------

            Console.ReadKey();
#endif
        }

    }
}
