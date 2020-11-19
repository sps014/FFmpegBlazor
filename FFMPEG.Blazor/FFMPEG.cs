using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ffmpeg
{
    public class FFMPEG
    {
        internal IJSRuntime Runtime;
        internal IJSUnmarshalledObjectReference reference;
        internal IJSInProcessObjectReference processReference;
        internal int Hash;
        internal static int HashCount = 0;
        internal FFMPEG(int hash)
        {
            Hash = hash;
        }
        public async Task Load()
        {
            await processReference.InvokeVoidAsync("loadFFmpeg", Hash);
        }
        public async Task Run(params string[] Parameters)
        {
            await processReference.InvokeVoidAsync("runFFmpeg", Parameters);
        }
        public byte[] ReadFile(string path)
        {
            return reference.InvokeUnmarshalled<FileConf,byte[]>("readFileFFmpeg",new FileConf() 
            {   
                Hash=Hash,
                Path=path
            });
        }
        public void WriteFile(string path, byte[] buffer)
        {
            reference.InvokeUnmarshalled<FileConf, byte[], object>("writeFileFFmpeg", new FileConf()
            {
                Hash = Hash,
                Path=path
            },buffer);
        }
        ~FFMPEG()
        {
            processReference.InvokeVoid("disposeFFmpeg", Hash);
        }
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
