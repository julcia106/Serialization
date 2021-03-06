﻿using GraZaDuzoZaMalo.Model;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using static System.Console;


namespace AppGraZaDuzoZaMaloCLI
{
    class WidokCLI
    {
        public const char ZNAK_ZAKONCZENIA_GRY = 'X';

        private KontrolerCLI kontroler;

        public WidokCLI(KontrolerCLI kontroler) => this.kontroler = kontroler;

        public void CzyscEkran() => Clear();

        public void KomunikatPowitalny() => WriteLine("Wylosowałem liczbę z zakresu ");

        public int WczytajPropozycje()
        {
            int wynik = 0;
            bool sukces = false;
            while (!sukces)
            {
                Write("Podaj swoją propozycję (lub " + KontrolerCLI.ZNAK_ZAKONCZENIA_GRY + " aby przerwać): ");
                try
                {
                    string value = ReadLine().TrimStart().ToUpper();
                    if (value.Length > 0 && value[0].Equals(ZNAK_ZAKONCZENIA_GRY))
                        throw new KoniecGryException();

                    //UWAGA: poniżej może zostać zgłoszony wyjątek 
                    wynik = Int32.Parse(value);
                    sukces = true;
                }
                catch (FormatException)
                {
                    WriteLine("Podana przez Ciebie wartość nie przypomina liczby! Spróbuj raz jeszcze.");
                    continue;
                }
                catch (OverflowException)
                {
                    WriteLine("Przesadziłeś. Podana przez Ciebie wartość jest zła! Spróbuj raz jeszcze.");
                    continue;
                }
                catch (KoniecGryException)
                {
                    WriteLine("Koniec gry");
                    SerializeControler();
                    //zapis do pliku
                    Environment.Exit(0);
                }
                catch (Exception)
                {
                    WriteLine("Nieznany błąd! Spróbuj raz jeszcze.");
                    continue;
                }
            }
            return wynik;
        }

        public void OpisGry()
        {
            WriteLine("Gra w \"Za dużo za mało\"." + Environment.NewLine
                + "Twoimm zadaniem jest odgadnąć liczbę, którą wylosował komputer." + Environment.NewLine + "Na twoje propozycje komputer odpowiada: za dużo, za mało albo trafiłeś");
        }

        public bool ChceszKontynuowac(string prompt)
        {
            Write(prompt);
            char odp = ReadKey().KeyChar;
            WriteLine();
            return (odp == 't' || odp == 'T');
        }

        public void HistoriaGry()
        {
            if (kontroler.ListaRuchow.Count == 0)
            {
                WriteLine("--- pusto ---");
                return;
            }
            
            WriteLine("Nr    Propozycja     Odpowiedź     Czas    Status");
            WriteLine("=================================================");
            int i = 1;
            foreach (var ruch in kontroler.ListaRuchow)
            {
                WriteLine($"{i}     {ruch.Liczba}      {ruch.Wynik}  {ruch.Czas.Second}   {ruch.StatusGry}");
                i++;
            }
        }

        public void SerializeControler()
        {
            const string fileName = "C:/Users/Julia/source/repos/Serialization/XML_Serialization/example.xml";
            FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                NewLineOnAttributes = true,
                ConformanceLevel= ConformanceLevel.Fragment,
            };

            XmlWriter xdw = XmlWriter.Create(stream, settings);

            DataContractSerializer dcs = new DataContractSerializer(typeof(Gra.Ruch));

            var startRoot = "<root>";
            dcs.WriteStartObject(xdw, startRoot);

            foreach (var ruch in kontroler.ListaRuchow)
            {
                dcs.WriteObject(xdw, ruch);
            }

            dcs.WriteEndObject(xdw);

            xdw.Flush();
            stream.Flush();
            stream.Close();

            Aes key = null;

            try
            {
                // Create a new AES key.
                key = Aes.Create();

                // Load an XML document.
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.Load(fileName);

                // Encrypt the Gra.Ruch elements.
                Encrypt(xmlDoc, "Gra.Ruch", key);

                Console.WriteLine();
                Console.WriteLine("The element was encrypted in example.xml\n");

                Console.WriteLine(xmlDoc.InnerXml);

                xmlDoc.Save(fileName);

                Decrypt(xmlDoc, key);

                Console.WriteLine();
                Console.WriteLine("The element was decrypted\n");

                Console.WriteLine(xmlDoc.InnerXml);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // Clear the key.
                if (key != null)
                {
                    key.Clear();
                }
            }
        }

        public static void Encrypt(XmlDocument Doc, string ElementName, SymmetricAlgorithm Key)
        {
            if (Doc == null)
                throw new ArgumentNullException("Doc");
            if (ElementName == null)
                throw new ArgumentNullException("ElementToEncrypt");
            if (Key == null)
                throw new ArgumentNullException("Alg");

            XmlElement elementToEncrypt = Doc.GetElementsByTagName(ElementName)[0] as XmlElement;

            if (elementToEncrypt == null)
            {
                throw new XmlException("The specified element was not found");
            }

            EncryptedXml eXml = new EncryptedXml();

            byte[] encryptedElement = eXml.EncryptData(elementToEncrypt, Key, false);

            EncryptedData edElement = new EncryptedData();
            edElement.Type = EncryptedXml.XmlEncElementUrl;

            string encryptionMethod = null;

            if (Key is Aes)
            {
                encryptionMethod = EncryptedXml.XmlEncAES256Url;
            }
            else
            {
                throw new CryptographicException("The specified algorithm is not supported or not recommended for XML Encryption.");
            }

            edElement.EncryptionMethod = new EncryptionMethod(encryptionMethod);
            edElement.CipherData.CipherValue = encryptedElement;

            EncryptedXml.ReplaceElement(elementToEncrypt, edElement, false);
        }

        public static void Decrypt(XmlDocument Doc, SymmetricAlgorithm Alg)
        {
            if (Doc == null)
                throw new ArgumentNullException("Doc");
            if (Alg == null)
                throw new ArgumentNullException("Alg");

            XmlElement encryptedElement = Doc.GetElementsByTagName("EncryptedData")[0] as XmlElement;

            if (encryptedElement == null)
            {
                throw new XmlException("The EncryptedData element was not found.");
            }

            EncryptedData edElement = new EncryptedData();
            edElement.LoadXml(encryptedElement);

            EncryptedXml exml = new EncryptedXml();

            byte[] rgbOutput = exml.DecryptData(edElement, Alg);

            exml.ReplaceData(encryptedElement, rgbOutput);
        }

        public void KomunikatZaDuzo()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine("Za dużo!");
            Console.ResetColor();
        }

        public void KomunikatZaMalo()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine("Za mało!");
            Console.ResetColor();
        }

        public void KomunikatTrafiono()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLine("Trafiono!");
            Console.ResetColor();
        }
    }

}
