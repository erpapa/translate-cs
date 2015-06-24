using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using NAudio;
using NAudio.Wave; 


namespace WindowsFormsApplication3
{
    class PlayMp3Url
    {
        public PlayMp3Url(String url)
        {
            this.PlayMp3(url);
        }
        private void PlayMp3(string url)
        {
            try
            {
                Stream ms = new MemoryStream();
                // 设置参数
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //发送请求并获取相应回应数据
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                Stream responseStream = response.GetResponseStream();
                byte[] buffer = new byte[1024];
                int size = 0;
                //size是读入缓冲区中的总字节数。 如果当前可用的字节数没有请求的字节数那么多，则总字节数可能小于请求的字节数，或者如果已到达流的末尾，则为零 (0)。
                while ((size = responseStream.Read(buffer, 0, buffer.Length)) != 0)//将responseStream的内容读入buffer数组
                {
                    ms.Write(buffer, 0, size);//将buffer数组的内容写入内存流ms
                }
                responseStream.Close();//关闭流并释放连接以供重用
                ms.Position = 0;//将内存流数据读取位置归零
                WaveStream blockAlignedStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(ms)));
                WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
                waveOut.Init(blockAlignedStream);
                //waveOut.PlaybackStopped += (sender, e) => { waveOut.Stop(); };
                waveOut.Play();
                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }

            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }
    }
}
