using System.Collections.Generic;
using System.IO;

namespace CommonHelperLibrary.Audio
{
    /// <summary>
    /// Mp3 Header Info Helper
    /// </summary>
    public class Mp3Header
    {
        public bool IsValidated { get; private set; }

        // Public variables for storing the information about the MP3
        public int BitRate { get; private set; }
        public string FileName { get; private set; }
        public long FileSize { get; private set; }
        public int SampleRate { get; private set; }
        public string Mode { get; private set; }
        public int Length { get; private set; }
        public string LengthFormatted { get; private set; }

        // Private variables used in the process of reading in the MP3 files
        private ulong _bithdr;
        private bool _boolVBitRate;
        private int _intVFrames;

        public Mp3Header(string filePath)
        {
            IsValidated = ReadMp3Information(filePath);
        }

        private bool ReadMp3Information(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            // Set the filename not including the path information
            FileName = @fs.Name;
            var chrSeparators = new[] { '\\', '/' };
            var strSeparator = FileName.Split(chrSeparators);
            var intUpper = strSeparator.GetUpperBound(0);
            FileName = strSeparator[intUpper];

            // Replace ' with '' for the SQL INSERT statement
            FileName = FileName.Replace("'", "''");

            // Set the file size
            FileSize = fs.Length;

            var bytHeader = new byte[4];
            var bytVBitRate = new byte[12];
            var intPos = 0;

            // Keep reading 4 bytes from the header until we know for sure that in
            // fact it's an MP3
            do
            {
                fs.Position = intPos;
                fs.Read(bytHeader, 0, 4);
                intPos++;
                LoadMp3Header(bytHeader);
            }
            while (!IsValidHeader() && (fs.Position != fs.Length));

            // If the current file stream position is equal to the length,
            // that means that we've read the entire file and it's not a valid MP3 file
            if (fs.Position != fs.Length)
            {
                intPos += 3;

                if (GetVersionIndex() == 3)    // MPEG Version 1
                {
                    if (GetModeIndex() == 3) // Single Channel
                        intPos += 17;
                    else
                        intPos += 32;
                }
                else                        // MPEG Version 2.0 or 2.5
                {
                    if (GetModeIndex() == 3) // Single Channel
                        intPos += 9;
                    else
                        intPos += 17;
                }

                // Check to see if the MP3 has a variable bitrate
                fs.Position = intPos;
                fs.Read(bytVBitRate, 0, 12);
                _boolVBitRate = LoadVbrHeader(bytVBitRate);

                // Once the file's read in, then assign the properties of the file to the public variables
                BitRate = GetBitrate();
                SampleRate = GetFrequency();
                Mode = GetMode();
                Length = GetLengthInSeconds();
                LengthFormatted = GetFormattedLength();
                fs.Close();
                return true;
            }
            fs.Close();
            return false;
        }

        private void LoadMp3Header(IList<byte> c)
        {
            // this thing is quite interesting, it works like the following
            // c[0] = 00000011
            // c[1] = 00001100
            // c[2] = 00110000
            // c[3] = 11000000
            // the operator << means that we'll move the bits in that direction
            // 00000011 << 24 = 00000011000000000000000000000000
            // 00001100 << 16 =         000011000000000000000000
            // 00110000 << 24 =                 0011000000000000
            // 11000000       =                         11000000
            //                +_________________________________
            //                  00000011000011000011000011000000
            _bithdr = (ulong)(((c[0] & 255) << 24) | ((c[1] & 255) << 16) | ((c[2] & 255) << 8) | ((c[3] & 255)));
        }

        private bool LoadVbrHeader(IList<byte> inputheader)
        {
            // If it's a variable bitrate MP3, the first 4 bytes will read 'Xing'
            // since they're the ones who added variable bitrate-edness to MP3s
            if (inputheader[0] == 88 && inputheader[1] == 105 &&
                inputheader[2] == 110 && inputheader[3] == 103)
            {
                var flags = ((inputheader[4] & 255) << 24) | ((inputheader[5] & 255) << 16) | ((inputheader[6] & 255) << 8) | ((inputheader[7] & 255));
                if ((flags & 0x0001) == 1)
                {
                    _intVFrames = ((inputheader[8] & 255) << 24) | ((inputheader[9] & 255) << 16) | ((inputheader[10] & 255) << 8) | ((inputheader[11] & 255));
                    return true;
                }
                _intVFrames = -1;
                return true;
            }
            return false;
        }

        private bool IsValidHeader()
        {
            return (((GetFrameSync() & 2047) == 2047) &&
                    ((GetVersionIndex() & 3) != 1) &&
                    ((GetLayerIndex() & 3) != 0) &&
                    ((GetBitrateIndex() & 15) != 0) &&
                    ((GetBitrateIndex() & 15) != 15) &&
                    ((GetFrequencyIndex() & 3) != 3) &&
                    ((GetEmphasisIndex() & 3) != 2));
        }

        private int GetFrameSync()
        {
            return (int)((_bithdr >> 21) & 2047);
        }

        private int GetVersionIndex()
        {
            return (int)((_bithdr >> 19) & 3);
        }

        private int GetLayerIndex()
        {
            return (int)((_bithdr >> 17) & 3);
        }

        private int GetProtectionBit()
        {
            return (int)((_bithdr >> 16) & 1);
        }

        private int GetBitrateIndex()
        {
            return (int)((_bithdr >> 12) & 15);
        }

        private int GetFrequencyIndex()
        {
            return (int)((_bithdr >> 10) & 3);
        }

        private int GetPaddingBit()
        {
            return (int)((_bithdr >> 9) & 1);
        }

        private int GetPrivateBit()
        {
            return (int)((_bithdr >> 8) & 1);
        }

        private int GetModeIndex()
        {
            return (int)((_bithdr >> 6) & 3);
        }

        private int GetModeExtIndex()
        {
            return (int)((_bithdr >> 4) & 3);
        }

        private int GetCoprightBit()
        {
            return (int)((_bithdr >> 3) & 1);
        }

        private int GetOrginalBit()
        {
            return (int)((_bithdr >> 2) & 1);
        }

        private int GetEmphasisIndex()
        {
            return (int)(_bithdr & 3);
        }

        private double GetVersion()
        {
            double[] table = { 2.5, 0.0, 2.0, 1.0 };
            return table[GetVersionIndex()];
        }

        private int GetLayer()
        {
            return 4 - GetLayerIndex();
        }

        private int GetBitrate()
        {
            // If the file has a variable bitrate, then we return an integer average bitrate,
            // otherwise, we use a lookup table to return the bitrate
            if (_boolVBitRate)
            {
                var medFrameSize = FileSize / (double)GetNumberOfFrames();
                return (int)((medFrameSize * GetFrequency()) / (1000.0 * ((GetLayerIndex() == 3) ? 12.0 : 144.0)));
            }
            int[, ,] table ={
                { // MPEG 2 & 2.5
                    {0,  8, 16, 24, 32, 40, 48, 56, 64, 80, 96,112,128,144,160,0}, // Layer III
                    {0,  8, 16, 24, 32, 40, 48, 56, 64, 80, 96,112,128,144,160,0}, // Layer II
                    {0, 32, 48, 56, 64, 80, 96,112,128,144,160,176,192,224,256,0}  // Layer I
                },
                { // MPEG 1
                    {0, 32, 40, 48, 56, 64, 80, 96,112,128,160,192,224,256,320,0}, // Layer III
                    {0, 32, 48, 56, 64, 80, 96,112,128,160,192,224,256,320,384,0}, // Layer II
                    {0, 32, 64, 96,128,160,192,224,256,288,320,352,384,416,448,0}  // Layer I
                }
            };

            return table[GetVersionIndex() & 1, GetLayerIndex() - 1, GetBitrateIndex()];
        }

        private int GetFrequency()
        {
            int[,] table = {  
                            {32000, 16000,  8000}, // MPEG 2.5
                            {    0,     0,     0}, // reserved
                            {22050, 24000, 16000}, // MPEG 2
                            {44100, 48000, 32000}  // MPEG 1
                        };

            return table[GetVersionIndex(), GetFrequencyIndex()];
        }

        private string GetMode()
        {
            switch (GetModeIndex())
            {
                default:
                    return "Stereo";
                case 1:
                    return "Joint Stereo";
                case 2:
                    return "Dual Channel";
                case 3:
                    return "Single Channel";
            }
        }

        private int GetLengthInSeconds()
        {
            // "intKilBitFileSize" made by dividing by 1000 in order to match the "Kilobits/second"
            var intKiloBitFileSize = (int)((8 * FileSize) / 1000);
            return intKiloBitFileSize / GetBitrate();
        }

        private string GetFormattedLength()
        {
            // Complete number of seconds
            var s = GetLengthInSeconds();

            // Seconds to display
            var ss = s % 60;

            // Complete number of minutes
            var m = (s - ss) / 60;

            // Minutes to display
            var mm = m % 60;

            // Complete number of hours
            var h = (m - mm) / 60;

            // Make "hh:mm:ss"
            return h.ToString("D2") + ":" + mm.ToString("D2") + ":" + ss.ToString("D2");
        }

        private int GetNumberOfFrames()
        {
            // Again, the number of MPEG frames is dependant on whether it's a variable bitrate MP3 or not
            if (_boolVBitRate) return _intVFrames;
            var medFrameSize = ((GetLayerIndex() == 3) ? 12 : 144) * ((1000.0 * GetBitrate()) / GetFrequency());
            return (int)(FileSize / medFrameSize);
        }
    }
}