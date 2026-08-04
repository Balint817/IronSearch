//using System;
//using System.IO;
//using UnityEngine;
//using System.Collections.Generic;
//using System.Threading;

//namespace ChartExporter;
//public static class SavWav
//{

//    const int HEADER_SIZE = 44;
//    struct ClipData
//    {

//        public int samples;
//        public int channels;
//        public float[] samplesData;

//    }

//    public static byte[] Save(AudioClip clip)
//    {
//        ClipData clipdata = new ClipData();
//        clipdata.samples = clip.samples;
//        clipdata.channels = clip.channels;
//        float[] dataFloat = new float[clip.samples * clip.channels];
//        clip.GetData(dataFloat, 0);
//        clipdata.samplesData = dataFloat;


//        using var memoryStream = new MemoryStream();
//        ConvertAndWrite(memoryStream, clipdata);
//        WriteHeader(memoryStream, clip);
//        memoryStream.Position = 0;
//        return memoryStream.ReadFully();

//        //return true; // TODO: return false if there's a failure saving the file
//    }

//    public static AudioClip TrimSilence(AudioClip clip, float min)
//    {
//        var samples = new float[clip.samples * clip.channels];

//        clip.GetData(samples, 0);

//        return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
//    }

//    public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz)
//    {
//        return TrimSilence(samples, min, channels, hz, false, false);
//    }

//    public static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool _3D, bool stream)
//    {
//        int i;

//        for (i = 0; i < samples.Count; i++)
//        {
//            if (Mathf.Abs(samples[i]) > min)
//            {
//                break;
//            }
//        }

//        samples.RemoveRange(0, i);

//        for (i = samples.Count - 1; i > 0; i--)
//        {
//            if (Mathf.Abs(samples[i]) > min)
//            {
//                break;
//            }
//        }

//        samples.RemoveRange(i, samples.Count - i);

//        var clip = AudioClip.Create("TempClip", samples.Count, channels, hz, _3D, stream);

//        clip.SetData(samples.ToArray(), 0);

//        return clip;
//    }

//    static void ConvertAndWrite(MemoryStream memStream, ClipData clipData)
//    {
//        float[] samples = clipData.samplesData;

//        Int16[] intData = new Int16[samples.Length];

//        Byte[] bytesData = new Byte[samples.Length * 2];

//        const float rescaleFactor = 32767; //to convert float to Int16

//        for (int i = 0; i < samples.Length; i++)
//        {
//            intData[i] = (short)(samples[i] * rescaleFactor);
//            //Debug.Log (samples [i]);
//        }
//        Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
//        memStream.Write(bytesData, 0, bytesData.Length);
//    }

//    static void WriteHeader(MemoryStream memoryStream, AudioClip clip)
//    {

//        var hz = clip.frequency;
//        var channels = clip.channels;
//        var samples = clip.samples;

//        memoryStream.Seek(0, SeekOrigin.Begin);

//        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
//        memoryStream.Write(riff, 0, 4);

//        Byte[] chunkSize = BitConverter.GetBytes(memoryStream.Length - 8);
//        memoryStream.Write(chunkSize, 0, 4);

//        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
//        memoryStream.Write(wave, 0, 4);

//        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
//        memoryStream.Write(fmt, 0, 4);

//        Byte[] subChunk1 = BitConverter.GetBytes(16);
//        memoryStream.Write(subChunk1, 0, 4);

//        UInt16 two = 2;
//        UInt16 one = 1;

//        Byte[] audioFormat = BitConverter.GetBytes(one);
//        memoryStream.Write(audioFormat, 0, 2);

//        Byte[] numChannels = BitConverter.GetBytes(channels);
//        memoryStream.Write(numChannels, 0, 2);

//        Byte[] sampleRate = BitConverter.GetBytes(hz);
//        memoryStream.Write(sampleRate, 0, 4);

//        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
//        memoryStream.Write(byteRate, 0, 4);

//        UInt16 blockAlign = (ushort)(channels * 2);
//        memoryStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

//        UInt16 bps = 16;
//        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
//        memoryStream.Write(bitsPerSample, 0, 2);

//        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
//        memoryStream.Write(datastring, 0, 4);

//        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
//        memoryStream.Write(subChunk2, 0, 4);

//        //		fileStream.Close();
//    }
//}