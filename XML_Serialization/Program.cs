//#define NORMAL
using GraZaDuzoZaMalo.Model;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace AppGraZaDuzoZaMaloCLI
{
    class Program
    {
        static void Main(string[] args)
        {
#if NORMAL
            (new KontrolerCLI()).Uruchom();
#else
            const string fileName = "C:/Users/Julia/source/repos/Serialization/XML_Serialization/example.xml";
            FileStream fs = new FileStream(fileName, FileMode.Open);

            DataContractSerializer dcs = new DataContractSerializer(typeof(Gra.Ruch));

            XmlDictionaryReader xdr = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());

            Gra.Ruch p = (Gra.Ruch)dcs.ReadObject(xdr);

            Console.WriteLine(String.Format("{0} {1} {2}, {3}",
                p.Czas, p.Liczba, p.StatusGry, p.Wynik ));


            xdr.Close();
            fs.Close();
#endif
        }

    }
}
