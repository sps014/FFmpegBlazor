using Microsoft.JSInterop;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FFmpegBlazor
{
    public class FFMPEG
    {
        /// <summary>
        /// Whether FFMpeg core wasm loaded or not
        /// </summary>
        public bool IsLoaded => IsLoadedFFmpeg();

        internal IJSRuntime Runtime;
        internal IJSUnmarshalledObjectReference reference;
        internal IJSInProcessObjectReference processReference;
        internal int Hash;
        internal static int HashCount = 0;
        internal DotNetObjectReference<FFMPEG> dotnetReference;
        internal FFMPEG(int hash)
        {
            Hash = hash;
            dotnetReference = DotNetObjectReference.Create(this);
        }
        
        /// <summary>
        /// Download pure Wasm On CLient side around ~25 mb
        /// </summary>
        /// <param name="triggerEvents">whether to trigger events like progress at object level or not, if false FFmpegFactory.Progress can give progress ratio</param>
        /// <returns></returns>
        public async Task Load(bool triggerEvents=false)
        {
            await processReference.InvokeVoidAsync("loadFFmpeg", Hash);
            if(triggerEvents)
                await processReference.InvokeVoidAsync("setFFmpegEvent", Hash,dotnetReference);

        }

        /// <summary>
        /// This is the major function in ffmpeg.wasm, you can just imagine it as ffmpeg native cli and what you need to pass is the same.
        /// </summary>
        /// <param name="Parameters">variables number of params you can pass around</param>
        /// <returns>a Task</returns>
        public async Task Run(params string[] Parameters)
        {
            await processReference.InvokeVoidAsync("runFFmpeg", Hash, Parameters);
        }
        /// <summary>
        /// Read In-Memory Wasm File (Ideal Method)
        /// </summary>
        /// <param name="path">path of in-mem file</param>
        /// <returns>byte[] reference</returns>
        public async Task<byte[]> ReadFile(string path)
        {
            _ = reference.InvokeUnmarshalled<FileConf, bool>("readFileFFmpeg", new FileConf()
            {
                Hash = Hash,
                Path = path
            });

            await Task.Delay(1);

            var length = reference.InvokeUnmarshalled<FileConf, int>("readFileLength", new() { Hash = Hash });
            var array = new byte[length];

            reference.InvokeUnmarshalled<FileConf, byte[], object>("readFileProcess", new() { Hash = Hash }, array);
            
            return array;
        }
        /// <summary>
        /// Write buffer of C# to WASM in-memory File so that FFmpeg can interact 
        /// </summary>
        /// <param name="path">write path of file</param>
        /// <param name="buffer">array will all bytes</param>
        public void WriteFile(string path, byte[] buffer)
        {
            reference.InvokeUnmarshalled<FileConf, byte[], object>("writeFileFFmpeg", new FileConf()
            {
                Hash = Hash,
                Path = path
            }, buffer);
        }
        /// <summary>
        /// Delete In Memory Wasm file 
        /// </summary>
        /// <param name="path">path to file to delete</param>
        public void UnlinkFile(string path)
        {
            reference.InvokeVoid("unlinkFileFFmpeg", Hash, path);
        }
        /// <summary>
        /// Use fast ReadFile(), WriteFile() and UnlinkFile() , for other command use use this FS method directly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<T> FS<T>(string method, params object[] args)
        {
            return await reference.InvokeAsync<T>("fsFFmpeg", method, args);
        }
        internal bool IsLoadedFFmpeg()
        {
            return reference.Invoke<bool>("isLoadedFFmpeg", Hash);
        }
        ~FFMPEG()
        {
            processReference.InvokeVoid("disposeFFmpeg", Hash);
        }

        /// <summary>
        /// This Method is not intented for External Use
        /// </summary>
        /// <param name="message"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JSInvokable("logger")]
        public void ZLoggerCallback(Logs message)
        {
            Logger?.Invoke(message);
        }

        /// <summary>
        /// This Method is not intented for External Use
        /// </summary>
        /// <param name="p"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JSInvokable("progress")]
        public void ZProgressCallback(Progress p)
        {
            Progress?.Invoke(p);
        }

        public delegate void LoggerHandler(Logs log);
        /// <summary>
        /// Subsribe to this event and obtain each logs [Object Level]
        /// </summary>
        public event LoggerHandler Logger;
        public delegate void ProgressHandler(Progress p);
        /// <summary>
        /// Subsribe to this event and obtain conversion progress [Object Level]
        /// </summary>
        public event ProgressHandler Progress;

        [StructLayout(LayoutKind.Explicit)]
        internal struct FileConf
        {
            [FieldOffset(0)]
            public string Path;
            [FieldOffset(8)]
            public int Hash;
        }

    }

}
