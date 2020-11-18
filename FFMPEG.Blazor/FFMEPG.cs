using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ffmpeg
{
    public static class FFMEPG
    {
        public static IJSRuntime Runtime { get; private set; }
        private static IJSUnmarshalledObjectReference reference;

        public static void CreateFFmpeg(IJSRuntime runtime)
        {
            Runtime = runtime;
            reference = ((IJSUnmarshalledRuntime)Runtime)
            .InvokeUnmarshalled<IJSUnmarshalledObjectReference>(
            "FfmpegBlazorReference");
            reference.InvokeUnmarshalled<object>("alert");
        }
    }

}
