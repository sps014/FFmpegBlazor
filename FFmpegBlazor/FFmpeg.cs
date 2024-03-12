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
        internal int Hash { get; }

        private static int HashCount = 0;

        private IJSInProcessObjectReference FFmpegObjectReference;
        private DotNetObjectReference<FFmpeg> dotnetObjectReference;

        public bool Loaded
        {
            get
            {
                return FFmpegObjectReference.Invoke<bool>("loaded", Hash);
            }
        }

        private FFmpeg(int hash,IJSInProcessObjectReference jsObjectReference)
        {
            Hash = hash;
            FFmpegObjectReference = jsObjectReference;
            dotnetObjectReference = DotNetObjectReference.Create(this);
        }

        ~FFmpeg()
        {
            FFmpegObjectReference.InvokeVoid("destroyFFmpeg", Hash);
            FFmpegObjectReference.Dispose();
        }

        public static async ValueTask<FFmpeg> CreateAsync(IJSRuntime runtime)
        {
            var jsObjectReference = await runtime.InvokeAsync<IJSInProcessObjectReference>("import", "./_content/FFmpegBlazor/js/index.js");
            var hash = HashCount++;
            await jsObjectReference.InvokeVoidAsync("createFFmpeg", hash);
            return new FFmpeg(hash,jsObjectReference);
        }


        private SemaphoreSlim loaderSemaphore=new SemaphoreSlim(0,1);
        private bool loadResult;
        private string loadError;

        public async ValueTask<bool> Load()
        {
            await FFmpegObjectReference.InvokeVoidAsync("load", Hash,dotnetObjectReference);
            await loaderSemaphore.WaitAsync();

            if(!string.IsNullOrWhiteSpace(loadError))
                throw new Exception(loadError);

            return loadResult;
        }

        [JSInvokable("OnFFmpegLoaded")]
        public void OnFFmpegLoaded(bool result,string error)
        {
            loadResult = result;
            loadError = error;
            //loaderSemaphore.Release();
        }
      
    }
}
