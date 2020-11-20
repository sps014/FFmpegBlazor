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
        public static IJSRuntime Runtime { get; private set; }

        private static IJSUnmarshalledObjectReference reference;
        private static IJSInProcessObjectReference processReference;

        private static DotNetObjectReference<FFmpegFactory> dotNetObjectReference;

        public static async Task Init([NotNull] IJSRuntime runtime, string cdnURL = null)
        {

            Runtime = runtime;

            if (Runtime == null)
                throw new ArgumentNullException(paramName: nameof(runtime), message: "runtime parameter can't be null");

            cdnURL ??= "https://unpkg.com/@ffmpeg/ffmpeg@0.9.5/dist/ffmpeg.min.js";

            await Runtime.InvokeVoidAsync("import", cdnURL);
            await Runtime.InvokeVoidAsync("import", "./_content/FFmpegBlazor/blazorFfmpeg.js");


            dotNetObjectReference = DotNetObjectReference.Create(new FFmpegFactory());

            reference = ((IJSUnmarshalledRuntime)Runtime)
                .InvokeUnmarshalled<IJSUnmarshalledObjectReference>("FfmpegBlazorReference");


            processReference = await ((IJSInProcessRuntime)Runtime)
                .InvokeAsync<IJSInProcessObjectReference>("FfmpegBlazorReference");
        }

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
        public static string CreateURLFromBuffer(byte[] buffer, string name, string type)
        {
            return reference.InvokeUnmarshalled<byte[], string, string, string>("createObjectURL", buffer, name, type);
        }
        public static void DownloadBufferAsFile(byte[] buffer, string name, string type)
        {
            reference.InvokeUnmarshalled<byte[], string, string, object>("downloadFile", buffer, name, type);
        }
        public static void RevokeObjectURL(string blobURL)
        {
            reference.InvokeVoid("revokeObjectURLCleanUp", blobURL);
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
    public struct FFmpegConfig
    {
        public string CorePath { get; init; }
        public bool Log { get; init; }
    }
    public struct Progress
    {
        [JsonPropertyName("ratio")]
        public double Ratio { get; init; }
    }
    public struct Logs
    {
        [JsonPropertyName("type")]
        public string Type { get; init; }

        [JsonPropertyName("message")]
        public string Message { get; init; }
    }
}
