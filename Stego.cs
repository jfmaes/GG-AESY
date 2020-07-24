using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AESAllTheThings
{
    class Stego
    {



        /*
        * Uses the buffer returned from ReadFile to look for potential injection points. 
        * has a submethod to Find the SoS (Start of Scan) marker, as injection needs to happen before this marker. 
        * Injectionpoints are possible right before the FF bytes, which indicates the markers.
        */
        private static List<int> FindInjectionPoints(List<byte> buffer, bool verbose = false, bool veryVerbose = false)
        {
            List<int> injectionPoints = new List<int>();
            byte[] SoS = { 0xff, 0xda };
            byte markerStart = 0xff;
            int SoSPos = 0;
            //hunt for SoS
            int FindSoS()
            {
                for (int i = 0; i < buffer.Count; i++)
                {
                    byte[] hunter = { buffer[i], buffer[i + 1] };
                    //Console.WriteLine("HUNTER {0} = {1}",i,BitConverter.ToString(hunter));
                    if (hunter.SequenceEqual(SoS))
                    {
                        SoSPos = i;
                        break;
                    }
                }
                return SoSPos;
            }
            // position of the SoS in the bytestream
            SoSPos = FindSoS();
            //starting from 1 as we are skipping FFD8 as you can't inject before it.
            for (int i = 2; i < SoSPos; i++)
            {
                if (buffer[i] == markerStart)
                {
                    if (verbose)
                    { Console.WriteLine("Injection point at byte: " + i); }

                    injectionPoints.Add(i);
                }
            }

            return injectionPoints;

        }



        //puts the payload(payload,key or keyandiv) in a marker.
        private static byte [] GenerateMarker(byte[] payload,bool verbose = false,bool veryVerbose = false)
        {
            Encoding ascii = Encoding.ASCII;
            Random random = new Random();
            byte markerHeader = 0xff;
            byte[] marker;
            byte[] size;
            byte[] markers = { 0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe9, 0xea, 0xeb, 0xec, 0xed, 0xef, 0xfe };
 
            ushort iSize = (ushort)((ushort)payload.Length + 2) ; // +2 because marker size bytes count in total size.
            size = BitConverter.GetBytes(iSize);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(size);
            }
            List<byte> lMarker= new List<byte>(payload);
            lMarker.Insert(0, markerHeader);
            byte randomMarker = markers[random.Next(markers.Length)];
            lMarker.Insert(1, randomMarker);
            lMarker.InsertRange(2, size);
            marker = lMarker.ToArray();
            if(verbose)
            {
                Console.WriteLine("Marker successfully created!");
            }

            if(veryVerbose)
            {
                Console.WriteLine("Marker created! Marker is: {0}", BitConverter.ToString(marker));
            }
            return marker;
        }

        private static byte [] RandomInjection(List<int> injectionPoints,List<byte> imageBytes, byte[] marker,String mode,String outFile="")
        {
            Random random = new Random();
            int randomInjectionPoint = injectionPoints[random.Next(injectionPoints.Count)];
            Console.WriteLine("Injecting {0} marker at byte {1}, actual {0} starts at byte {2} ", mode, randomInjectionPoint,randomInjectionPoint+3);
            Console.WriteLine("Hex offset for actual start of {0} is: {1} ", mode, Convert.ToString(randomInjectionPoint +4, 16));
            if(!String.IsNullOrEmpty(outFile))
            {
                StreamWriter sw = new StreamWriter(outFile,true);
                sw.WriteLine("===================STEGO=============================");
                sw.WriteLine("Injecting {0} marker at byte {1}, actual {0} starts at byte {2} ", mode, randomInjectionPoint, randomInjectionPoint + 3);
                sw.WriteLine("Hex offset for actual start of {0} is: {1} ", mode, Convert.ToString(randomInjectionPoint + 4, 16));
                sw.WriteLine("=====================================================");
                sw.Close();
            }
            imageBytes.InsertRange(randomInjectionPoint, marker);
            return imageBytes.ToArray();

        }

 
        //all error handling is done in this method as well. except keysize error, this gets checked at main method.
        public static void HideMe(String imagePath, String mode,byte[] keyBytes=null, byte[] IVBytes = null, byte[] payloadBytes = null, bool verbose = false, bool veryVerbose = false,String outFile = "")
        {
            
            try
            {
                if (mode == "rk")
                {
                    byte [] marker = GenerateMarker(keyBytes, verbose, veryVerbose);
                    List<byte> imageBytes =  HelperFunctions.ReadFile(imagePath);
                    List<int> injectionPoints = FindInjectionPoints(imageBytes, verbose);
                    byte[] injectedImage = RandomInjection(injectionPoints, imageBytes, marker,"Key",outFile);
                    File.WriteAllBytes(imagePath,injectedImage);
                }

                else if (mode == "ra")
                {
                    byte[] marker = GenerateMarker(keyBytes, verbose, veryVerbose);
                    List<byte> imageBytes = HelperFunctions.ReadFile(imagePath);
                    List<int> injectionPoints = FindInjectionPoints(imageBytes, verbose);
                    byte[] injectedImage = RandomInjection(injectionPoints, imageBytes, marker,"Key",outFile);
                    List<byte>buffer = new List<byte>(injectedImage);
                    //second cycle for IV this time.
                    marker = GenerateMarker(IVBytes, verbose, veryVerbose);
                    imageBytes = buffer;
                    injectionPoints = FindInjectionPoints(imageBytes, verbose);
                    injectedImage = RandomInjection(injectionPoints, imageBytes, marker,"IV",outFile);
                    File.WriteAllBytes(imagePath, injectedImage);
                }

                else if (mode == "rp")
                {
                    HelperFunctions.CheckPayloadSize(payloadBytes);
                    byte[]  payload = GenerateMarker(payloadBytes, verbose, veryVerbose);
                    List<byte> imageBytes = HelperFunctions.ReadFile(imagePath);
                    List<int> injectionPoints = FindInjectionPoints(imageBytes, verbose);
                    byte[] injectedImage = RandomInjection(injectionPoints, imageBytes, payload,"payload",outFile);
                    File.WriteAllBytes(imagePath, injectedImage);
                }

                else if (mode == "ak")
                {
                    FileStream fs = new FileStream(imagePath, FileMode.Append);
                    //append Key
                    byte[] marker = GenerateMarker(keyBytes, verbose, veryVerbose);
                    long position = fs.Position;
                    Console.WriteLine("Injecting key marker at byte {0},actual key starts at byte {1}", position,position +4 );
                    Console.WriteLine("hex offset of actual key:{0}", Convert.ToString(position + 4, 16));
                    if (!String.IsNullOrEmpty(outFile))
                    {
                        StreamWriter sw = new StreamWriter(outFile,true);
                        sw.WriteLine("===================STEGO=============================");
                        sw.WriteLine("Injecting key marker at byte {0},actual key starts at byte {1}", position, position + 4);
                        sw.WriteLine("hex offset of actual key:{0}", Convert.ToString(position + 4, 16));
                        sw.WriteLine("=====================================================");
                        sw.Close();

                    }
                    fs.Write(marker, 0, marker.Length);
                }

                else if (mode == "aa")
                {
                    FileStream fs = new FileStream(imagePath, FileMode.Append);
                    //append Key
                    byte[] marker = GenerateMarker(keyBytes, verbose, veryVerbose);
                    long position = fs.Position ; 
                    Console.WriteLine("Injecting key marker at byte {0}, actual key starts at byte {1}", position, position + 4);
                    Console.WriteLine("hex offset of actual key:{0}", Convert.ToString(position + 4, 16));
                    fs.Write(marker, 0, marker.Length);

                    if (!String.IsNullOrEmpty(outFile))
                    {
                        StreamWriter sw = new StreamWriter(outFile, true);
                        sw.WriteLine("===================STEGO=============================");
                        sw.WriteLine("Injecting key marker at byte {0}, actual key starts at byte {1}", position, position + 4);
                        sw.WriteLine("hex offset of actual key:{0}", Convert.ToString(position + 4, 16));
                        sw.Close();
                    }

                    //append IV
                    marker = GenerateMarker(IVBytes, verbose, veryVerbose);
                    fs.Position = fs.Seek(0,SeekOrigin.End);
                    position = fs.Position;
                    Console.WriteLine("Injecting IV marker at byte {0}, actual IV starts at byte {1}", position, position + 4);
                    Console.WriteLine("hex offset of actual IV:{0}", Convert.ToString(position + 4, 16));
                    fs.Write(marker, 0, marker.Length);

                    if (!String.IsNullOrEmpty(outFile))
                    {
                        StreamWriter sw = new StreamWriter(outFile, true);
                        sw.WriteLine("Injecting IV at byte {0}, actual IV starts at byte {1}", position, position + 4);
                        sw.WriteLine("hex offset of actual IV:{0}", Convert.ToString(position + 4, 16));
                        sw.WriteLine("=====================================================");
                        sw.Close();
                    }
                }

                else if (mode == "ap" || mode == "apu")
                {
                    FileStream fs = new FileStream(imagePath, FileMode.Append);
                    
                    long position = fs.Position;
                    position = fs.Position;
                    Console.WriteLine("Injecting payload at byte {0}", position);
                    if (mode == "apu")
                    { Console.WriteLine("payload size is {0} bytes", payloadBytes.Length); }
                    fs.Write(payloadBytes, 0, payloadBytes.Length);
                    if (!String.IsNullOrEmpty(outFile))
                    {
                        StreamWriter sw = new StreamWriter(outFile,true);
                        sw.WriteLine("===================STEGO=============================");
                        sw.WriteLine("Injecting payload at byte {0}", position);
                        sw.WriteLine("=====================================================");
                        sw.Close();
                    }

                }

                else { throw new ArgumentException("I don't know how to do " + mode +" sorry!"); }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }
    }
}
