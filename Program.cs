using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using NDesk.Options;

namespace AESAllTheThings
{
    class Program
    {

        public static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("\tEncryptor and (optional) stegano\n");
            Console.WriteLine(" Usage:");
            p.WriteOptionDescriptions(Console.Out);
        }
        static void Main(string[] args)
        {

            try
            {
                #region ArgParsing

                //args
                bool help = false;
                bool encryptOnlyMode = false;
                bool decryptMode = false;
                bool unpackMode = false;
                bool verbose = false;
                bool veryVerbose = false;
                bool randomKeyMode = false;
                bool randomAllMode = false;
                bool randomPayLoadMode = false;
                bool appendKeyMode = false;
                bool appendAllMode = false;
                bool appendPayloadMode = false;
                bool appendPayloadUnencryptedMode = false;
                bool hasKey = false;
                bool hasIV = false;
                String payloadPath = "";
                String outFile = "";
                String imageFile = "";
                String key = "";
                String iv = "";
                //vars for key,IV and Payload
                byte[] bKey = null;
                byte[] bIV = null;
                byte[] payload = null;

                //vars for offsets for decryption.
                String offsetKey = "";
                String offsetKeyHex = "";
                String offsetIV = "";
                String offsetIVHex = "";
                String offsetPayload = "";
                String offsetPayloadHex = "";
                String encryptedFile = "";
                int iPayloadSize = 0;



                var options = new OptionSet()
                {
                    {"h|?|help","Show Help\n\n", o => help = true},
                    {"e|encrypt-only","Only encrypts given payload\n", o => encryptOnlyMode = true },
                    {"d|decrypt","decryption mode\n",o => decryptMode = true },
                    {"u|unpack","unpacks unencrypted payloads appended to jpeg",o => unpackMode =true },
                    {"ps|payload-size=","only needed if extracting payload from image for decryption\n", o => iPayloadSize = Int32.Parse(o) },
                    {"ef|encrypted-file=","ENCRYPTION: The outfile for encrypted data\n\nDECRYPTION:The inputfile needed to decrypt the payload.\n\n\n\n",o=> encryptedFile = o },
                    {"p|payload=","The path to the payload you want to encrypt\n", o => payloadPath = o},
                    {"o|outfile=","The path to the outfile where all important data will be written to (key,iv and encrypted payload)\n", o => outFile = o },
                    {"i|image=","The image file to hide the key and/or IV in, currently only supports JPEG (JPG) format!\n", o => imageFile = o },
                    {"ok|offset-key=","The offset to search for the key in image (in decimal)\n", o => offsetKey = o },
                    {"okh|offset-key-hex=","The offset to search for the key in image (in hex)\n",o => offsetKeyHex = o },
                    {"oIV|offset-IV=","The offset to search for the IV in image (in decimal)\n", o => offsetIV = o },
                    {"oIVh|offset-IV-hex=","The offset to search for the IV in image (in hex)\n",o => offsetIVHex = o },
                    {"op|offset-payload=","The offset to search for the payload in image (in decimal)\n", o => offsetPayload = o },
                    {"oph|offset-payload-hex=","The offset to search for the payload in image (in hex)\n",o => offsetPayloadHex = o },
                    {"v|verbose","write all the good stuff to console,recommended you actually always use this.\n", o => verbose = true },
                    {"vv|very-verbose","prints encrypted payload array to console",o=> veryVerbose = true },
                    {"k|key=","in case you want to use your own key value!\n", o => key = o },
                    {"IV|initialization-vector=","in case you want to use your own IV\n", o=> iv = o },
                    {"rk|random-key-mode","will hide your key in a random insertion point in the provided image, without breaking said image. will print the offset to console\n",o => randomKeyMode = true },
                    {"ra|random-all-mode","will hide both Key and IV in a random insertion point of the image.\n", o => randomAllMode = true },
                    {"ak|append-key-mode","will hide the key at the end of the image file\n",o => appendKeyMode = true },
                    {"aa|append-all-mode","will hide the key and the IV at the end of the image file.\n", o => appendAllMode = true },
                    {"ap|append-payload-mode","will hide the payload at the end of the image file\n", o => appendPayloadMode = true},
                    {"rp|random-payload-mode","will hide the payload at a random insertion point. \n", o => randomPayLoadMode = true },
                    {"apu|append-payload-unencrypted","appends your payload without crypto, useful for very quick and dirty data exfil.", o => appendPayloadUnencryptedMode = true }

                };
                #endregion

                Info.PrintBanner();
                try
                {
                    options.Parse(args);
                    IEnumerable<bool> modes = new List<bool> { encryptOnlyMode, unpackMode, decryptMode, randomKeyMode, randomAllMode, appendAllMode, appendKeyMode, randomPayLoadMode, appendPayloadMode, appendPayloadUnencryptedMode };
                    hasKey = HelperFunctions.HasKey(key);
                    hasIV = HelperFunctions.HasIV(iv);
                    #region pre-Flight-Checks
                    if (help)
                    {
                        ShowHelp(options);
                        return;
                    }




                    if ((modes.Count(b => b) == 0))
                    {
                        throw new ArgumentException("Please choose a mode.");
                    }

                    bool isSingleMode = HelperFunctions.CheckModes(1, modes);
                    if (!isSingleMode)
                    {
                        throw new ArgumentException("ERROR: only one mode can be active at the same time.");

                    }

                    if (!appendPayloadUnencryptedMode && !decryptMode && ((!verbose && !veryVerbose) && String.IsNullOrEmpty(outFile)))
                    {
                        throw new ArgumentException("You'll need an output file for your encrypted payload and key/iv data as backup or need to enable verbose or very verbose mode.");
                    }


                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    return;
                }
                #endregion


                using (AesManaged myAes = new AesManaged())
                {
                    if (hasKey)
                    {
                        bKey = HelperFunctions.GetBytesFromString(key);
                        if (bKey.Length != 32)
                        {
                            throw new ArgumentException("Your key is not 32 bytes long.");
                        }
                    }
                    else
                        bKey = myAes.Key;

                    if (hasIV)
                    {
                        bIV = HelperFunctions.GetBytesFromString(iv);
                        if (bIV.Length != 16)
                        {
                            throw new ArgumentException("Your IV is not 16 bytes long.");
                        }
                    }
                    else
                        bIV = myAes.IV;

                    myAes.KeySize = 256;

                    if (encryptOnlyMode)
                    {
                        if(String.IsNullOrEmpty(encryptedFile))
                        { throw new ArgumentException("you'll need to specify an encryptedfile -ef to save your encrypted payload!"); }
                        Crypto.EncryptPayload(payloadPath, outFile, encryptedFile, bIV, bKey, verbose, veryVerbose);
                    }

                    if (randomKeyMode)
                    {
                        if (String.IsNullOrEmpty(encryptedFile))
                        { throw new ArgumentException("you'll need to specify an encryptedfile -ef to save your encrypted payload!"); }
                        Crypto.EncryptPayload(payloadPath, outFile, encryptedFile, bIV, bKey, verbose, veryVerbose);
                        Stego.HideMe(imageFile, "rk", bKey, bIV, null, verbose, veryVerbose, outFile);
                    }

                    if (randomAllMode)
                    {
                        if (String.IsNullOrEmpty(encryptedFile))
                        { throw new ArgumentException("you'll need to specify an encryptedfile -ef to save your encrypted payload!"); }
                        Crypto.EncryptPayload(payloadPath, outFile, encryptedFile, bIV, bKey, verbose, veryVerbose);
                        Stego.HideMe(imageFile, "ra", bKey, bIV, null, verbose, veryVerbose, outFile);

                    }

                    if (randomPayLoadMode)
                    {
                        payload = Crypto.EncryptPayload(payloadPath, outFile, encryptedFile, bIV, bKey, verbose, veryVerbose);
                        Stego.HideMe(imageFile, "rp", bKey, bIV, payload, verbose, veryVerbose, outFile);
                    }

                    if (appendKeyMode)
                    {
                        if (String.IsNullOrEmpty(encryptedFile))
                        { throw new ArgumentException("you'll need to specify an encryptedfile -ef to save your encrypted payload!"); }
                        Crypto.EncryptPayload(payloadPath, outFile, encryptedFile, bIV, bKey, verbose, veryVerbose);
                        Stego.HideMe(imageFile, "ak", bKey, bIV, null, verbose, veryVerbose, outFile);
                    }

                    if (appendAllMode)
                    {
                        if (String.IsNullOrEmpty(encryptedFile))
                        { throw new ArgumentException("you'll need to specify an encryptedfile (-ef) to save your encrypted payload!"); }
                        Crypto.EncryptPayload(payloadPath, outFile, encryptedFile, bIV, bKey, verbose, veryVerbose);
                        Stego.HideMe(imageFile, "aa", bKey, bIV, null, verbose, veryVerbose, outFile);
                    }
                    if (appendPayloadMode)
                    {
                        payload = Crypto.EncryptPayload(payloadPath, outFile, encryptedFile, bIV, bKey, verbose, veryVerbose);
                        Stego.HideMe(imageFile, "ap", bKey, bIV, payload, verbose, veryVerbose, outFile);
                    }
                    if (appendPayloadUnencryptedMode)
                    {
                        payload = File.ReadAllBytes(payloadPath);
                        Stego.HideMe(imageFile, "apu", bKey, bIV, payload, verbose, veryVerbose, outFile);
                    }

                    if (decryptMode)
                    {
                        Crypto.DecryptPayLoad(key, iv, offsetKey, offsetIV, offsetPayload, offsetKeyHex, offsetIVHex, offsetPayloadHex, outFile, encryptedFile, imageFile, iPayloadSize);
                    }

                    if (unpackMode)
                    {
                        HelperFunctions.Unpack(imageFile, outFile, iPayloadSize, offsetPayload, offsetPayloadHex);
                    }

                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }


        }

    }
}



