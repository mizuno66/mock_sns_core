using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace mock_sns_core2.Services
{
    public class HLSConvertService
    {
        public HLSConvertService()
        {
        }

        public static void convertHLS(string ContentPath)
        {
            var dirname = Path.GetDirectoryName(ContentPath);
            System.Diagnostics.Process process = new System.Diagnostics.Process();

            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = " -i " + ContentPath + " -c:v copy -c:a copy " +
                "-f hls -hls_time 9 " +
                "-hls_playlist_type vod -hls_segment_filename " +
                "\"" + dirname + "/video%3d.ts\" " +
                dirname + "/video.m3u8";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();

            process.WaitForExit();
            process.Close();
        }

        public static void createThumbnail(string ContentPath)
        {
            var dirname = Path.GetDirectoryName(ContentPath);
            System.Diagnostics.Process process = new System.Diagnostics.Process();

            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = " -i " + ContentPath +
                " -ss 0 -t 1 -r 5 " +
                dirname + "/thumbnail.mp4";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();
            process.WaitForExit();
            process.Close();
        }
    }
}
