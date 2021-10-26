using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using NAudio.Wave;

namespace MeAgent.global
{
    internal class Util
    {
        public static void ConvertMp3ToWav(string path)
        {
            //string path = Directory.GetCurrentDirectory() + @"\alarmSound\music.mp3";
            using (Mp3FileReader mp3 = new Mp3FileReader(path))
            {
                using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                {
                    WaveFileWriter.CreateWaveFile(Path.ChangeExtension(path, "wav"), pcm);
                }
            }
        }
    }
}