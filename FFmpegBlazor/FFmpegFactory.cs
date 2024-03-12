using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpegBlazor
{
    public static class FFmpegFactory
    {
        public static async void Init(IJSRuntime Runtime)
        {
            var obj = await Runtime.InvokeAsync<IJSObjectReference>("import", "./_content/FFmpegBlazor/js/index.js");
        }
    }
}
