using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AESAllTheThings
{
    class HelperFunctions
    {
        public static String FormatString(String stringToFormat)
        {
            stringToFormat = stringToFormat.Replace("-", "");
            var list = Enumerable
            .Range(0, stringToFormat.Length / 2)
            .Select(i => stringToFormat.Substring(i * 2, 2));
            String res = String.Join("-", list.ToArray());
            return res;
        }

        public static byte[] GetBytesFromString(String stringToConvert)
        {
            stringToConvert = stringToConvert.Replace("-", "");
            byte[] byteArray;
            try
            {
                String helperString = FormatString(stringToConvert);
                byteArray = helperString.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
            }
            catch (Exception) //String was not a valid byte array
            {
                byteArray = Encoding.ASCII.GetBytes(stringToConvert);
            }
            return byteArray;
        }


        public static bool CheckModes(int threshold, IEnumerable<bool> modes)
        { return modes.Count(b => b) == threshold; }

        public static bool HasKey(string key)
        {
            bool hasKey = (!String.IsNullOrEmpty(key)) ? true : false;
            return hasKey;
        }

        public static bool HasIV(string iv)
        {
            bool hasIV = (!String.IsNullOrEmpty(iv)) ? true : false;
            return hasIV;
        }


        public static List<byte> ReadFile(string imagePath)
        {
            List<byte> buffer;
            buffer = new List<byte>(File.ReadAllBytes(imagePath));
            Console.WriteLine("Buffer contains {0} bytes", buffer.Count);
            return buffer;
        }

        // checks if payload doesn't exceed FFFF
        public static void CheckPayloadSize(byte[] payload)
        {
            int iSize = payload.Length;
            if (iSize > 65535) //FFFF (max size of marker)
            {
                throw new ArgumentException("your payload is above 65535 bytes. use append mode as your payload can't fit in a marker.");
            }
        }

        /*
         * file can have ffffff format or ff-ff-ff format... or even ascii..
         * reads file, joins all lines in one string, removes eventual "-" and gets the byte values even in ascii.
         */
        public static byte[] GetByteArrayFromFile(String file)
        {
            try
            {
                byte[] byteArray = null;
                String[] unsanitized = File.ReadAllLines(file);
                String helperString = String.Join(",", unsanitized.Select(s => s.ToString()).ToArray());
                byteArray = HelperFunctions.GetBytesFromString(helperString);
                return byteArray;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }


        public static byte [] ReadUntilNextMarkerOrUntilEndOfSize(String imageFile,String offsetDec = "",String offsetHex = "",int size = 0)
        {

            FileStream fs = new FileStream(imageFile, FileMode.Open);
            List<int> helperList = new List<int>();
            byte[] buffer = new byte[fs.Length];
            int counter = 0;
            int helper = 0;

            try
            {
                if (!String.IsNullOrEmpty(offsetDec))
                { fs.Position = Int32.Parse(offsetDec); }

                if (!String.IsNullOrEmpty(offsetHex))
                {
                    int offset = Int32.Parse(offsetHex, System.Globalization.NumberStyles.HexNumber);
                    fs.Position = offset; 
                }
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (size >0)
            {
                while(counter < size)
                {
                    helper = fs.ReadByte();
                    helperList.Add(helper);
                    counter += 1;
                }

                buffer = helperList.Select(i => (byte)i).ToArray();
            }
            fs.Close();
            return buffer;
          
            
        }

        public static void Unpack(String image, String outFile, int payloadSize, String offsetDec = "", String offsetHex = "")
        {
            try
            {
                if (String.IsNullOrEmpty(offsetHex) && String.IsNullOrEmpty(offsetDec))
                {
                    throw new ArgumentException("you'll need to specify the offset in order to unpack");
                }

                byte[] buffer = new byte[payloadSize];
                buffer = ReadUntilNextMarkerOrUntilEndOfSize(image, offsetDec, offsetHex, payloadSize);
                File.WriteAllBytes(outFile,buffer);
                Console.WriteLine("payload unpacked in : {0}", outFile);
            }
            catch (Exception e)
            { Console.WriteLine(e.Message); }
        }
        
    }
}

