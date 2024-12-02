﻿using Microsoft.JSInterop;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
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
        internal IJSInProcessObjectReference processReference;
        internal int Hash;
        internal static int HashCount = 0;
        internal DotNetObjectReference<FFMPEG> dotnetReference;
        internal static int ErrorHashCount = 0;
        internal ConcurrentDictionary<int, string> ErrorMessageMap = new();
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
        /// This is the major function in ffmpeg.wasm, you can just imagine it as ffmpeg native cli and what you need to pass args in the same way.
        /// </summary>
        /// <param name="Parameters">variables number of params you can pass around</param>
        /// <returns>a Task</returns>
        public async Task Run(params string[] Parameters)
        {
            await Run(default, Parameters);
        }
        public async Task Run(CancellationToken token,params string[] Parameters)
        {
            var errorHash = ErrorHashCount++;
            await processReference.InvokeVoidAsync("runFFmpeg",token, Hash, Parameters, errorHash, dotnetReference);
            string errorMessage = null;
            while(!ErrorMessageMap.TryGetValue(errorHash,out errorMessage))
            {
                await Task.Delay(30);
            }
            ErrorMessageMap.TryRemove(errorHash,out _);
            if (!string.IsNullOrWhiteSpace(errorMessage))
                throw new System.Exception(errorMessage);
        }
        /// <summary>
        /// Read In-Memory Wasm File (Ideal Method)
        /// </summary>
        /// <param name="path">path of in-mem file</param>
        /// <returns>byte[] reference</returns>
        public async Task<byte[]> ReadFile(string path)
        {
            processReference.InvokeVoid("readFileFFmpeg",Hash,path);
          
            await Task.Delay(5);
           
            var array =processReference.Invoke<byte[]>("readFileProcess",Hash);
            return array;
        }
        /// <summary>
        /// Write buffer of C# to WASM in-memory File so that FFmpeg can interact 
        /// </summary>
        /// <param name="path">write path of file</param>
        /// <param name="buffer">File bytes</param>
        public void WriteFile(string path, byte[] buffer)
        {
            processReference.InvokeVoid("writeFileFFmpeg", Hash,path, buffer);
        }
        /// <summary>
        /// Delete In Memory Wasm file 
        /// </summary>
        /// <param name="path">path to file to delete</param>
        public void UnlinkFile(string path)
        {
            processReference.InvokeVoid("unlinkFileFFmpeg", Hash, path);
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
            return await processReference.InvokeAsync<T>("fsFFmpeg", method, args);
        }
        public void Exit()
        {
            processReference.InvokeVoid("exitfs",Hash);
        }
        internal bool IsLoadedFFmpeg()
        {
            return processReference.Invoke<bool>("isLoadedFFmpeg", Hash);
        }
        ~FFMPEG()
        {
            processReference.InvokeVoid("disposeFFmpeg", Hash);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JSInvokable("runCompleted")]
        public void ZRunCompleted(string message, int errorHash)
        {
            ErrorMessageMap.TryAdd(errorHash, message);
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

        internal class FileConf
        {
            public string Path;
            public int Hash;
        }

    }

}
