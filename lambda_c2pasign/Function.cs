using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3.Model;
using Amazon.S3;
using c2panalyze2;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace c2panalyze;

public class Function
{
    private readonly IAmazonS3 _s3Client;

    bool useDASH = true;

    public Function()

    {
        _s3Client = new AmazonS3Client();
    }

    public async Task<string> FunctionHandlerSign(S3Event evnt, ILambdaContext context)
    {

        var s3Event = evnt.Records?.FirstOrDefault();
        if (s3Event == null)
        {
            return "No S3 event detected.";
        }

        string s3BucketPathSigned = "data_sign";
        try
        {
            s3BucketPathSigned = Environment.GetEnvironmentVariable("s3BucketPathSigned").TrimStart('/');
        }
        catch
        {
        }

        string s3BucketPath = "data";
        try
        {
            s3BucketPath = Environment.GetEnvironmentVariable("s3BucketPath").TrimStart('/');
        }
        catch
        {
        }

        string s3Region = "eu-central-1";
        try
        {
            s3Region = Environment.GetEnvironmentVariable("s3Region").TrimStart('/');
        }
        catch
        {
        }     

        string bucketName = s3Event.S3.Bucket.Name;
        string fileName = Path.GetFileName(s3Event.S3.Object.Key);

        Console.WriteLine("s3BucketPath " + s3BucketPath);
        Console.WriteLine("s3BucketPathSigned " + s3BucketPathSigned);
        Console.WriteLine("bucketName " + bucketName);
        Console.WriteLine("fileName " + fileName);

        string extension = Path.GetExtension(fileName);
        string _outputDirectory = "/tmp/" + fileName.Replace(extension, "");
        string _tmpFilename = "/tmp/" + fileName;
        string _outputDirectorySigned = "/tmp/" + fileName.Replace(extension, "") + "_signed";

        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }

        if (!Directory.Exists(_outputDirectorySigned))
        {
            Directory.CreateDirectory(_outputDirectorySigned);
        }   

        Console.WriteLine("_tmpFilename " + _tmpFilename);
        Console.WriteLine("_outputDirectorySigned " + _outputDirectorySigned);
        Console.WriteLine("_outputDirectory " + _outputDirectory);


        //1. get file from bucket
        try
        {
            Console.WriteLine("get file");
            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = s3Event.S3.Object.Key
            };
            var response = _s3Client.GetObjectAsync(getRequest).GetAwaiter().GetResult();
            response.WriteResponseStreamToFileAsync(_tmpFilename, false, new CancellationTokenSource().Token).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Console.WriteLine("get File failed " + e.Message + "@" + e.StackTrace);
        }

        //2. transcode to HLS
        try
        {
            procesFFmpeg runFFmpeg = new procesFFmpeg(_tmpFilename,useDASH);
            runFFmpeg.runFrag(_outputDirectory);
        }
        catch (Exception e)
        {
            Console.WriteLine("ffmpeg failed Error " + e.Message + "@" + e.StackTrace);
        }

        //3. Sign HLS and upload to S3
        try
        {
            processC2PA run = new processC2PA(_outputDirectory,useDASH);
            run.runSign(_outputDirectorySigned);

            s3Load s3Loader = new s3Load("", "", s3Region);
            List<string> _ingredientFiles1 = new List<string>();
            foreach (string file in Directory.GetFiles(_outputDirectorySigned, "*.*", SearchOption.AllDirectories))
            {
                _ingredientFiles1.Add(file);
            }
            
            Console.WriteLine("Upload files Signed from " + _outputDirectorySigned);
            string s3result1 = s3Loader.putS3Files(_ingredientFiles1, bucketName, s3BucketPathSigned, "/tmp/").GetAwaiter().GetResult();
            Console.WriteLine("putS3Files Result " + s3result1);
            Directory.Delete(_outputDirectory,true);
            Directory.Delete(_outputDirectorySigned,true);
            File.Delete(_tmpFilename);
        }
        catch (Exception e)
        {
            Console.WriteLine("RunSign or Upload failed Error " + e.Message + "@" + e.StackTrace);
        }

        return "ok";
    }


}