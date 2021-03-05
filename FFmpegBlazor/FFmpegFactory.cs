using Microsoft.JSInterop;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FFmpegBlazor
{
    public class FFmpegFactory
    {
        /// <summary>
        /// Read Only JS Runtime Reference from Blazor Page, must not be null, use Init() to assign it
        /// </summary>
        public static IJSRuntime Runtime { get; private set; }

        private static IJSUnmarshalledObjectReference reference;
        private static IJSInProcessObjectReference processReference;
        private static DotNetObjectReference<FFmpegFactory> dotNetObjectReference;

        /// <summary>
        /// This Initializes the IJSRuntime ,dependencies automatically , fetches required Scripts, It should be Called First
        /// </summary>
        /// <param name="runtime">Reference to IJSRuntime , use @inject IJSRuntime Runtime to obtain one from Razor Page</param>
        /// <param name="cdnURL">An updated CDN Version URL , can be null</param>
        /// <returns>a Task that must be awaited..</returns>
        public static async Task Init([NotNull] IJSRuntime runtime, string cdnURL = null)
        {
            Runtime = runtime;

            if (Runtime == null)
                throw new ArgumentNullException(paramName: nameof(runtime), message: "runtime parameter can't be null");

            cdnURL ??= "https://unpkg.com/@ffmpeg/ffmpeg@0.9.7/dist/ffmpeg.min.js";

            await Runtime.InvokeVoidAsync("import", cdnURL);
            await Runtime.InvokeVoidAsync("import", "./_content/FFmpegBlazor/blazorFfmpeg.js");


            dotNetObjectReference = DotNetObjectReference.Create(new FFmpegFactory());

            reference = ((IJSUnmarshalledRuntime)Runtime)
                .InvokeUnmarshalled<IJSUnmarshalledObjectReference>("FfmpegBlazorReference");


            processReference = await ((IJSInProcessRuntime)Runtime)
                .InvokeAsync<IJSInProcessObjectReference>("FfmpegBlazorReference");
        }

        /// <summary>
        /// Creates An Instance Of FFmpeg, which performs core operations
        /// </summary>
        /// <param name="config">pass configuration options such as module path, log boolean etc.</param>
        /// <returns>FFmpeg Object</returns>
        public static FFMPEG CreateFFmpeg(FFmpegConfig config = new FFmpegConfig())
        {

            processReference.InvokeVoid("createFFmpeg", FFMPEG.HashCount, config, dotNetObjectReference);

            var ffObject = new FFMPEG(FFMPEG.HashCount)
            {
                processReference = processReference,
                reference = reference,
                Runtime = Runtime
            };

            FFMPEG.HashCount++;

            return ffObject;
        }
        /// <summary>
        /// Creates a URL for a given Buffer so that it can be referenced by Html component as source
        /// </summary>
        /// <param name="buffer">bytes of files to be initalized to url object</param>
        /// <param name="name">name of the file like "myFile.mp4"</param>
        /// <param name="type">Content Type eg. "video/mp4"</param>
        /// <returns>ra URL Object</returns>
        public static string CreateURLFromBuffer(byte[] buffer, string name, string type)
        {
            return reference.InvokeUnmarshalled<byte[], string, string, string>("createObjectURL", buffer, name, type);
        }
        /// <summary>
        ///  Download  Buffer as File on client machine, instantly , without wait
        /// </summary>
        /// <param name="buffer">bytes of files to be downloaded</param>
        /// <param name="name">name of the file like "myFile.mp4"</param>
        /// <param name="type">Content Type eg. "video/mp4"</param>
        public static void DownloadBufferAsFile(byte[] buffer, string name, string type)
        {
            reference.InvokeUnmarshalled<byte[], string, string, object>("downloadFile", buffer, name, type);
        }
        /// <summary>
        /// Free URL object and buffer , caused by CreateURLFromBuffer()
        /// </summary>
        /// <param name="blobURL">url to clean</param>
        public static void RevokeObjectURL(string blobURL)
        {
            reference.InvokeVoid("revokeObjectURLCleanUp", blobURL);
        }
        /// <summary>
        /// This Method is not intented for External Use
        /// </summary>
        /// <param name="message"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JSInvokable("logger")]
        public void LoggerCallback(Logs message)
        {
            Logger?.Invoke(message);
        }
        /// <summary>
        /// This Method is not intented for External Use
        /// </summary>
        /// <param name="p"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JSInvokable("progress")]
        public void ProgressCallback(Progress p)
        {
            Progress?.Invoke(p);
        }
        public delegate void LoggerHandler(Logs log);
        /// <summary>
        /// Subsribe to this event and obtain each logs [Globally]
        /// </summary>
        public static event LoggerHandler Logger;
        public delegate void ProgressHandler(Progress p);
        /// <summary>
        /// Subsribe to this event and obtain conversion progress [Globally]
        /// </summary>
        public static event ProgressHandler Progress;
    }
    public struct FFmpegConfig
    {
        /// <summary>
        /// Path to custom Modules
        /// </summary>
        public string CorePath { get; init; }
        /// <summary>
        /// Whether to show logs in console or not
        /// </summary>
        public bool Log { get; init; }
    }
    public struct Progress
    {
        /// <summary>
        /// Progress Value between 0-1
        /// </summary>
        [JsonPropertyName("ratio")]
        public double Ratio { get; init; }
    }
    public struct Logs
    {
        /// <summary>
        /// Type of log like fferr , stderr etc
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; init; }
        /// <summary>
        /// Log Message 
        /// </summary>

        [JsonPropertyName("message")]
        public string Message { get; init; }
    }
}
