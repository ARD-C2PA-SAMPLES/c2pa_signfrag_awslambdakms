using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace c2panalyze2
{

    public class procesFFmpeg
    {

        bool _useDASH = true;

        string _filetoFrag = "";

        public procesFFmpeg(string filetoFrag, bool useDASH)
        {
            _filetoFrag = filetoFrag;
            _useDASH = useDASH;
        }


        public void runFrag(string outputFolder)
        {

            Directory.CreateDirectory(outputFolder);

            Process ffmpegRunner = new Process();

            ffmpegRunner.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "ffmpeg");

            //ffmpeg -nostats -loglevel 0 -y -i test.mp4 -map 0:0 -map 0:0 -map 0:1 -s:v:0 1920x1080 -c:v:0 libx264 -profile:v:0 high -bf:v:0 4 -b:v:0 7000k -minrate:v:0 3000k -maxrate:v:0 10000k -bufsize:v:0 10000k -r:v:0 25 -g:v:0 50 -sc_threshold:v:0 0 -x264opts:v:0 keyint=50:min-keyint=50 -s:v:1 640x360 -c:v:1 libx264 -profile:v:1 high -bf:v:1 4 -b:v:1 1000k -minrate:v:1 700k -maxrate:v:1 1200k -bufsize:v:1 1000k -r:v:1 25 -g:v:1 50 -sc_threshold:v:1 0 -x264opts:v:1 keyint=50:min-keyint=50 -c:a:0 aac -b:a:0 128k -var_stream_map "v:0,agroup:aud v:1,agroup:aud a:0,agroup:aud" -master_pl_name master.m3u8 -f hls -hls_time 4 -hls_list_size 0 -hls_segment_type fmp4 -hls_playlist_type vod -hls_fmp4_init_filename "init.mp4" -hls_segment_filename  /Users/martingrohme/tmp/out/%v/fileSequence%d.m4s /Users/martingrohme/tmp/out/%v/prog_index.m3u8
            if (!_useDASH)
            {
                ffmpegRunner.StartInfo.Arguments = " -y -i " + _filetoFrag + " -map 0:0 -map 0:0 -map 0:1 -s:v:0 1920x1080 -c:v:0 libx264 -profile:v:0 high -bf:v:0 4 -b:v:0 7000k -minrate:v:0 3000k -maxrate:v:0 10000k -bufsize:v:0 10000k -r:v:0 25 -g:v:0 50 -sc_threshold:v:0 0 -x264opts:v:0 keyint=50:min-keyint=50 -s:v:1 640x360 -c:v:1 libx264 -profile:v:1 high -bf:v:1 4 -b:v:1 1000k -minrate:v:1 700k -maxrate:v:1 1200k -bufsize:v:1 1000k -r:v:1 25 -g:v:1 50 -sc_threshold:v:1 0 -x264opts:v:1 keyint=50:min-keyint=50 -c:a:0 aac -b:a:0 128k -var_stream_map \"v:0,agroup:aud v:1,agroup:aud a:0,agroup:aud\" -master_pl_name master.m3u8 -f hls -hls_time 4 -hls_list_size 0 -hls_segment_type fmp4 -hls_playlist_type vod -hls_segment_filename " + Path.Combine(outputFolder, "%v", "fileSequence%d.m4s") + " " + Path.Combine(outputFolder, "%v", "prog_index.m3u8");
            }
            else
            {
                ffmpegRunner.StartInfo.Arguments = " -y -i " + _filetoFrag + " -map 0:v -s:v:0 1920x1080 -c:v:0 libx264 -profile:v:0 high -bf:v:0 4 -b:v:0 7000k -minrate:v:0 3000k -maxrate:v:0 10000k -bufsize:v:0 10000k -r:v:0 25 -g:v:0 50 -sc_threshold:v:0 0 -x264opts:v:0 keyint=50:min-keyint=50 -map 0:v -s:v:1 640x360 -c:v:1 libx264 -profile:v:1 high -bf:v:1 4 -b:v:1 1000k -minrate:v:1 700k -maxrate:v:1 1200k -bufsize:v:1 1000k -r:v:1 25 -g:v:1 50 -sc_threshold:v:1 0 -x264opts:v:1 keyint=50:min-keyint=50 -map 0:a:0 -c:a aac -b:a 128k -f dash -use_timeline 1 -use_template 1 -seg_duration 4 -movflags cmaf -hls_playlist 1 -adaptation_sets \"id=0,streams=v id=1,streams=a\" -init_seg_name \"init_$RepresentationID$.mp4\" " + Path.Combine(outputFolder, "dash.mpd");
            }

            Console.WriteLine("runFrag 1 " + ffmpegRunner.StartInfo.Arguments);
            ffmpegRunner.StartInfo.CreateNoWindow = true;
            ffmpegRunner.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            ffmpegRunner.StartInfo.UseShellExecute = false;
            ffmpegRunner.StartInfo.RedirectStandardError = true;
            ffmpegRunner.StartInfo.RedirectStandardOutput = true;
            ffmpegRunner.Start();

            if (!ffmpegRunner.WaitForExit(600 * 1000))
            {
                try
                {
                    Console.WriteLine("runFrag 1: 'msg': 'ffmpeg process timed out'");
                    ffmpegRunner.Kill();

                }
                catch { }
            }

            string s_runffmpeg_out1 = "";
            string s_runffmpeg_err1 = "";

            try
            {
                s_runffmpeg_out1 = ffmpegRunner.StandardOutput.ReadToEnd().Trim();
                s_runffmpeg_err1 = ffmpegRunner.StandardError.ReadToEnd().Trim();
                ffmpegRunner.WaitForExit();
            }
            catch
            { }

            Console.WriteLine("runFrag 1: 'msg': 'c2patool process finished'");

            Console.WriteLine("runFrag 1: 'msg': 'c2patool process s_runffmpeg_out1'" + s_runffmpeg_out1);

            Console.WriteLine("runFrag 1: 'msg': 'c2patool process s_runffmpeg_err1'" + s_runffmpeg_err1);
        }
    }
}
