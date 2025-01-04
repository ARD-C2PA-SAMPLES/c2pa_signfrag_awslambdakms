using System.Diagnostics;

namespace c2panalyze
{

    public class processC2PA
    {

        bool _useDASH = true;

        string _pathToSign = "";

        public processC2PA(string pathToSign, bool useDASH)
        {
            _pathToSign = pathToSign;
            _useDASH = useDASH;
        }

        public void runSign(string outputPath)
        {
            if (_useDASH)
            {
                foreach (string file in Directory.GetFiles(_pathToSign, "*.mp4"))
                {
                    string currID = Path.GetFileName(file).Replace(Path.GetExtension(file), "").Replace("init_", "").Trim('/').Trim('\\');
                    Console.WriteLine("runC2PA DASH currID " + currID);
                    runSignDASH(outputPath, currID);
                }

                string temp_dir = "";
                //bugfix issue nested folder
                foreach (string file in Directory.GetFiles(outputPath, "*.*", SearchOption.AllDirectories))
                {
                    Console.WriteLine("runC2PA fix " + file);
                    DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetFullPath(file));
                    Console.WriteLine("runC2PA fix " + directoryInfo.Parent.FullName);

                    // Copy a file up one directory level
                    string destinationPath = Path.Combine(directoryInfo.Parent.Parent.FullName, Path.GetFileName(file));
                    temp_dir = directoryInfo.Parent.Parent.FullName;
                    File.Move(file, destinationPath, true);
                }
                try
                {
                    Directory.Delete(temp_dir);
                }
                catch { }

                //copying master playlist and variant playlists to outputPath from pathToSign
                foreach (string file in Directory.GetFiles(_pathToSign, "*.m3u8"))
                {
                    string tempcurrpath = file.Replace(Path.GetFileName(file), "").Replace(_pathToSign, "").Trim('/');
                    Console.WriteLine("runC2PA DASH copy m3u8 " + file);
                    Console.WriteLine("runC2PA DASH copy m3u8 " + Path.Combine(outputPath, tempcurrpath, Path.GetFileName(file)));
                    File.Copy(file, Path.Combine(outputPath, tempcurrpath, Path.GetFileName(file)), true);
                }

                //copying master playlist and variant playlists to outputPath from pathToSign
                foreach (string file in Directory.GetFiles(_pathToSign, "*.mpd"))
                {
                    string tempcurrpath = file.Replace(Path.GetFileName(file), "").Replace(_pathToSign, "").Trim('/');
                    Console.WriteLine("runC2PA DASH copy mpd " + file);
                    Console.WriteLine("runC2PA DASH copy mpd " + Path.Combine(outputPath, tempcurrpath, Path.GetFileName(file)));
                    File.Copy(file, Path.Combine(outputPath, tempcurrpath, Path.GetFileName(file)), true);
                }
            }
            else
            {
                runSignHLS(outputPath);
            }
        }

        private void runSignHLS(string outputPath)
        {
            if (_pathToSign != "")
            {
                Process c2parunner = new Process();

                c2parunner.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "c2pa", "c2patool");
                c2parunner.StartInfo.Arguments =  " -m " + Path.Combine(Directory.GetCurrentDirectory(), "certs/manifest.json") + " --signer-path " + Path.Combine(Directory.GetCurrentDirectory(), "c2pa", "kms_signer") + " -o " + outputPath + "/ \"" + _pathToSign + "/**/init_*[0-9].mp4\"  fragment --fragments_glob \"fileSequence*[0-9].m4s\"";

                Console.WriteLine("runC2PA " + c2parunner.StartInfo.Arguments);

                c2parunner.StartInfo.CreateNoWindow = true;
                c2parunner.StartInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "c2pa");
                c2parunner.StartInfo.UseShellExecute = false;
                c2parunner.StartInfo.RedirectStandardError = true;
                c2parunner.StartInfo.RedirectStandardOutput = true;
                c2parunner.Start();

                if (!c2parunner.WaitForExit(60 * 1000))
                {
                    try
                    {
                        Console.WriteLine("runC2PA: 'msg': 'c2patool process timed out'");
                        c2parunner.Kill();

                    }
                    catch { }
                }

                string s_runc2pa_out1 = "";
                string s_runc2pa_err1 = "";

                try
                {
                    s_runc2pa_out1 = c2parunner.StandardOutput.ReadToEnd().Trim();
                    s_runc2pa_err1 = c2parunner.StandardError.ReadToEnd().Trim();
                    c2parunner.WaitForExit();
                }
                catch
                { }

                Console.WriteLine("runC2PA: 'msg': 'c2patool process finished s_runc2pa_out1'" + s_runc2pa_out1);
                Console.WriteLine("runC2PA: 'msg': 'c2patool process finished s_runc2pa_err1'" + s_runc2pa_err1);
                try 
                {
                    Console.WriteLine("runC2PA: 'msg': 'KMS Err " + File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "c2pa", "error_kms.err")));
                }
                catch { }

                //copying master playlist and variant playlists to outputPath from pathToSign
                foreach(string file in Directory.GetFiles(_pathToSign,"*.m3u8", SearchOption.AllDirectories))
                {
                    string tempcurrpath = file.Replace(Path.GetFileName(file), "").Replace(_pathToSign, "").Trim('/');
                    File.Copy(file, Path.Combine(outputPath, tempcurrpath, Path.GetFileName(file)));

                }
            }
        }

        private void runSignDASH(string outputPath, string rendition)
        {
            if (_pathToSign != "")
            {
                Process c2parunner = new Process();

                c2parunner.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "c2pa", "c2patool");
                c2parunner.StartInfo.Arguments = " -m " + Path.Combine(Directory.GetCurrentDirectory(), "certs/manifest.json") + " --signer-path " + Path.Combine(Directory.GetCurrentDirectory(), "c2pa", "kms_signer") + " -o " + outputPath + "/ \"" + _pathToSign + "/init_" + rendition + ".mp4\"  fragment --fragments_glob \"chunk-stream" + rendition + "-*[0-9].m4s\"";

                Console.WriteLine("runC2PA " + c2parunner.StartInfo.Arguments);

                c2parunner.StartInfo.CreateNoWindow = true;
                c2parunner.StartInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "c2pa");
                c2parunner.StartInfo.UseShellExecute = false;
                c2parunner.StartInfo.RedirectStandardError = true;
                c2parunner.StartInfo.RedirectStandardOutput = true;
                c2parunner.Start();

                if (!c2parunner.WaitForExit(60 * 1000))
                {
                    try
                    {
                        Console.WriteLine("runC2PA: 'msg': 'c2patool process timed out'");
                        c2parunner.Kill();

                    }
                    catch { }
                }

                string s_runc2pa_out1 = "";
                string s_runc2pa_err1 = "";

                try
                {
                    s_runc2pa_out1 = c2parunner.StandardOutput.ReadToEnd().Trim();
                    s_runc2pa_err1 = c2parunner.StandardError.ReadToEnd().Trim();
                    c2parunner.WaitForExit();
                }
                catch
                { }

                Console.WriteLine("runC2PA: 'msg': 'c2patool process finished s_runc2pa_out1'" + s_runc2pa_out1);
                Console.WriteLine("runC2PA: 'msg': 'c2patool process finished s_runc2pa_err1'" + s_runc2pa_err1);
                try
                {
                    Console.WriteLine("runC2PA: 'msg': 'KMS Err " + File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "c2pa", "error_kms.err")));
                }
                catch { }
            }
        }
    }
}
