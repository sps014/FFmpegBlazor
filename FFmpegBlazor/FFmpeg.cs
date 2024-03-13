using BlazorBindGen;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpegBlazor
{
    public class FFmpeg
    {
        public JObjPtr FFmpegPtr { get; }

        const string baseURL = "https://unpkg.com/@ffmpeg/core@0.12.6/dist/umd"; //manually control which version to ship with razor lib


        public bool Loaded
        {
            get
            {
                return FFmpegPtr.PropVal<bool>("loaded");
            }
        }

        private FFmpeg(JObjPtr ffmpeg)
        {
            FFmpegPtr = ffmpeg;
        }

        public static async ValueTask<FFmpeg> CreateAsync(IJSRuntime runtime)
        {
            await BindGen.InitAsync(runtime);
            Util.Runtime = runtime;

            await runtime.InvokeVoidAsync("import","./_content/FFmpegBlazor/js/index.js");
            var ffmpeg = BindGen.Window.CallRef("createFFmpeg");
            return new FFmpeg(ffmpeg);
        }
        public async ValueTask<bool> LoadAsync(object? _nameParameters=null)
        {
            if(_nameParameters == null)
            {
                _nameParameters = new
                {
                    coreURL = await Util.ToBlobURL($"{baseURL}/ffmpeg-core.js", "text/javascript"),
                    wasmURL = await Util.ToBlobURL($"{baseURL}/ffmpeg-core.wasm", "application/wasm"),
                    workerURL= await Util.ToBlobURL($"{baseURL}/ffmpeg-core.worker.js","text/javascript"),
                };
            }
            return await BindGen.Window.CallAwaitedAsync<bool>("load",FFmpegPtr, _nameParameters);
        }
        public async ValueTask<int> ExecAsync(string[] args,int timeout=-1,object? _namedParameters=null)
        {
            if(_namedParameters == null)
                return await FFmpegPtr.CallAwaitedAsync<int>("exec", args, timeout);

            return await FFmpegPtr.CallAwaitedAsync<int>("exec",args,timeout,_namedParameters);
        }

        public ValueTask TerminateAsync()
        {
            return FFmpegPtr.CallVoidAsync("terminate");
        }

    }
}
