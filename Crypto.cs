using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace AESAllTheThings
{
    class Crypto
    {

        public static byte[] EncryptPayload(String payloadPath, String outFile, String encryptionFile = "", byte[] IV=null, byte[] key = null,bool verbose = false,bool veryVerbose=false)
        {
            byte[] payload = File.ReadAllBytes(payloadPath);
            byte[] encryptedPayload = new byte[payload.Length];

            using (AesManaged aesAlg = new AesManaged())
            {
                if(IV == null)
                {
                   IV = aesAlg.IV ;
                }

                if(key == null)
                {
                   key = aesAlg.Key;
                }
               
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(key, IV);
                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    {
                        //Write all data to the stream.
                        csEncrypt.Write(payload, 0, payload.Length);
                        csEncrypt.FlushFinalBlock();
                        encryptedPayload = msEncrypt.ToArray();
                    }
                }

            }

            if (!String.IsNullOrEmpty(outFile))
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(outFile))
                {
                    file.WriteLine("===================CRYPTO=============================");
                    file.WriteLine("IMPORTANT INFO, DO NOT LOSE!");
                    file.WriteLine("If you wanna test decryption, you can copy paste the hex (payload,iv,key) straight in cyberchef :)");
                    file.WriteLine("The KEY in ASCII is:{0}", Encoding.Default.GetString(key));
                    file.WriteLine("Key in HEX: {0}", BitConverter.ToString(key));
                    file.WriteLine("The IV in ASCII is: {0}", Encoding.Default.GetString(IV));
                    file.WriteLine("The IV (hex) is: {0}", BitConverter.ToString(IV));
                    file.WriteLine("We are using PKCS7");
                    file.WriteLine("Good luck operator!");
                    file.WriteLine("The encrypted Payload size is: {0} bytes long.", encryptedPayload.Length);
                    //file.WriteLine("\n\n\nEncypted payload (in hex): \n");
                    //file.Write(BitConverter.ToString(encryptedPayload) + "\n");
                    file.Write("=======================================================\n\n\n");
                    file.Close();

                }
            }
            
            if(verbose || veryVerbose)
            {
                Console.WriteLine("================================================");
                Console.WriteLine("IMPORTANT INFO, DO NOT LOSE!");
                //Console.WriteLine("The KEY in ASCII is:{0}", Encoding.Default.GetString(key));
                Console.WriteLine("Key in HEX: {0}", BitConverter.ToString(key));
                Console.WriteLine("We are using PKCS7");
                // Console.WriteLine("The IV in ASCII is: {0}", Encoding.Default.GetString(IV));
                Console.WriteLine("The IV (hex) is: {0}", BitConverter.ToString(IV));
                Console.WriteLine("The encrypted Payload size is: {0} bytes long.", encryptedPayload.Length);
                if (veryVerbose)
                {
                    Console.WriteLine("Payload in HEX:\n{0}", BitConverter.ToString(encryptedPayload));
                }
                Console.WriteLine("Good luck operator!");
                if (!String.IsNullOrEmpty(outFile))
                {
                    Console.WriteLine("This data is written to the following path: " + outFile);
                }
                Console.WriteLine("================================================");
            }

            if (!String.IsNullOrEmpty(encryptionFile))
            {
                using (StreamWriter file = new StreamWriter(encryptionFile))
                {
                    file.WriteLine(BitConverter.ToString(encryptedPayload));
                }
                Console.WriteLine("Encrypted payload written to: {0}", encryptionFile);

            }
            return encryptedPayload;

        }



        public static byte[] DecryptPayLoad(String key = "", String IV = "", String offsetKey = "", String offsetIV = "", String offsetPayload = "", String offsetKeyHex = "", String offsetIVHex = "", String offsetPayloadHex = "", String outFile = "", String encryptedFile = "", String encryptedImage ="",int payloadSize=0)
        {
            byte[] encryptedPayload =null;
            byte[] decryptedPayload = null;

            if (String.IsNullOrEmpty(encryptedFile) && String.IsNullOrEmpty(encryptedImage))
            {
                throw new ArgumentException("you'll need a file that contains encrypted material...");
            }

            if(!String.IsNullOrEmpty(encryptedImage) && ((String.IsNullOrEmpty(offsetIV)&&String.IsNullOrEmpty(offsetIVHex)) && (String.IsNullOrEmpty(offsetKey) && String.IsNullOrEmpty(offsetKeyHex)) && String.IsNullOrEmpty(offsetPayload)))
                {
                throw new ArgumentException("you'll need to specify at least one offset (key or key and IV) or payload.");
            }

            if(String.IsNullOrEmpty(outFile))
            {
                throw new ArgumentException("you'll need an outfile for the decrypted data. dynamic execution is NOT supported in this software.");
            }

            if((!String.IsNullOrEmpty(offsetKey) || !String.IsNullOrEmpty(offsetIV) || !String.IsNullOrEmpty(offsetKeyHex) || !String.IsNullOrEmpty(offsetIVHex)) && String.IsNullOrEmpty(encryptedImage))
            {
                throw new ArgumentException("You'll need to specify an image file!");
            }


            if(!String.IsNullOrEmpty(encryptedFile))
            {

                encryptedPayload = HelperFunctions.GetByteArrayFromFile(encryptedFile);
            }

            if (!String.IsNullOrEmpty(offsetPayload) || !String.IsNullOrEmpty(offsetPayloadHex))
            { 
               
                encryptedPayload = HelperFunctions.ReadUntilNextMarkerOrUntilEndOfSize(encryptedImage, offsetPayload, offsetPayloadHex,payloadSize); 
            }
            

            if((!String.IsNullOrEmpty(offsetKey) || !String.IsNullOrEmpty(offsetIV) || !String.IsNullOrEmpty(offsetPayload)) && String.IsNullOrEmpty(encryptedImage))
            {
                throw new ArgumentException("I need your image file to extract the data from.");

            }

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.KeySize = 256;

                if (!String.IsNullOrEmpty(offsetKey) || !String.IsNullOrEmpty(offsetKeyHex))
                {
                    byte [] bKey = HelperFunctions.ReadUntilNextMarkerOrUntilEndOfSize(encryptedImage,offsetKey, offsetKeyHex, 32);
                    aesAlg.Key = bKey;
                }
                else
                {
                    key = HelperFunctions.FormatString(key);
                    aesAlg.Key = HelperFunctions.GetBytesFromString(key);
                }

                if (!String.IsNullOrEmpty(offsetIV) || !String.IsNullOrEmpty(offsetIVHex))
                {
                    aesAlg.IV = HelperFunctions.ReadUntilNextMarkerOrUntilEndOfSize(encryptedImage,offsetIV, offsetIVHex, 16);
                }
                else
                {
                    IV = HelperFunctions.FormatString(IV);
                    aesAlg.IV = HelperFunctions.GetBytesFromString(IV);
                }
       
                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream())
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                {
                    csDecrypt.Write(encryptedPayload, 0, encryptedPayload.Length);
                    csDecrypt.FlushFinalBlock();
                    decryptedPayload = msDecrypt.ToArray();
                }
            }
            //Console.WriteLine(BitConverter.ToString(decryptedPayload));
            File.WriteAllBytes(outFile,decryptedPayload);
            Console.WriteLine("decrypted data written to: {0}", outFile);
            return decryptedPayload;
        }
    }
}
