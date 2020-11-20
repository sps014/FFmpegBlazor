using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ffmpeg
{
    public class FFmpegFactory
    {
        public static IJSRuntime Runtime { get; private set; }

        private static IJSUnmarshalledObjectReference reference;
        private static IJSInProcessObjectReference processReference;

        private static DotNetObjectReference<FFmpegFactory> dotNetObjectReference;

        public static async Task Init(IJSRuntime runtime)
        {

            Runtime = runtime;

            if (Runtime == null)
                throw new ArgumentNullException("runtime parameter can't be null");

            dotNetObjectReference = DotNetObjectReference.Create(new FFmpegFactory());

            reference = ((IJSUnmarshalledRuntime)Runtime)
                .InvokeUnmarshalled<IJSUnmarshalledObjectReference>("FfmpegBlazorReference");


            processReference = await ((IJSInProcessRuntime)Runtime)
                .InvokeAsync<IJSInProcessObjectReference>("FfmpegBlazorReference");
        }

        public static FFMPEG CreateFFmpeg(FFmpegConfig config=null)
        {
            if (config == null)
                config = new FFmpegConfig();
            
            processReference.InvokeVoid("createFFmpeg",FFMPEG.HashCount,config,dotNetObjectReference);

            var ffObject=new FFMPEG(FFMPEG.HashCount) 
            {
                processReference = processReference,
                reference = reference,
                Runtime = Runtime
            };

            FFMPEG.HashCount++;

            return ffObject;
        }
        public static string CreateURLFromBuffer(byte[] buffer,string name,string type)
        {
            return reference.InvokeUnmarshalled<byte[], string, string, string>("createObjectURL", buffer, name, type);
        }
        public static void DownloadBufferAsFile(byte[] buffer, string name, string type)
        {
            reference.InvokeUnmarshalled<byte[], string, string, object>("downloadFile", buffer, name, type);
        }
        public static void RevokeObjectURL(string blobURL)
        {
            reference.InvokeVoid("revokeObjectURLCleanUp",blobURL);
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JSInvokable("logger")]
        public void LoggerCallback(Logs message)
        {
            Logger?.Invoke(message);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JSInvokable("progress")]
        public void ProgressCallback(Progress p)
        {
            Progress?.Invoke(p);
        }
        public delegate void LoggerHandler(Logs log);
        public static event LoggerHandler Logger;
        public delegate void ProgressHandler(Progress p);
        public static event ProgressHandler Progress;

    }
    public class FFmpegConfig
    {
        public string CorePath { get; init; }
        public bool Log { get; init; } = true;
    }
    public class Progress
    {
        public double ratio { get; init; }
    }
    public class Logs
    {
        public string type { get; init; }
        public string message { get; init; }
    }
}
