using Microsoft.JSInterop;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FFmpegBlazor
{
    public class FFMPEG
    {
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
        
        public async Task Load(bool triggerEvents=false)
        {
            await processReference.InvokeVoidAsync("loadFFmpeg", Hash);
            if(triggerEvents)
                await processReference.InvokeVoidAsync("setFFmpegEvent", Hash,dotnetReference);

        }
        public async Task Run(params string[] Parameters)
        {
            await processReference.InvokeVoidAsync("runFFmpeg", Hash, Parameters);
        }
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
        public void WriteFile(string path, byte[] buffer)
        {
            reference.InvokeUnmarshalled<FileConf, byte[], object>("writeFileFFmpeg", new FileConf()
            {
                Hash = Hash,
                Path = path
            }, buffer);
        }
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
        public event LoggerHandler Logger;
        public delegate void ProgressHandler(Progress p);
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
