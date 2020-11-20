// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

window.ffmpegObjectInstances = new Object();
window.readFileBuffers = new Object();

window.FfmpegBlazorReference = () => {
    return {
        createFFmpeg: (hash, config, Dotnet) => {
            if (config.corePath == null)
            ffmpegObjectInstances[hash] = FFmpeg.createFFmpeg({
                log: config.log,
                logger: (message) => Dotnet.invokeMethodAsync("logger",message),
                progress: (p) => Dotnet.invokeMethodAsync("progress",p)
            });
            else
                ffmpegObjectInstances[hash] = FFmpeg.createFFmpeg({
                    corePath: config.corePath,
                    log: config.log,
                    logger: ({ message }) => Dotnet.invokeMethodAsync("logger", message),
                    progress: (p) => Dotnet.invokeMethodAsync("progress", p)
                });

        },
        loadFFmpeg: async (hash) =>
        {
            await ffmpegObjectInstances[hash].load();
        },
        runFFmpeg: async (hash, params) =>
        {
            await ffmpegObjectInstances[hash].run(...params);
        },
        readFileFFmpeg: async (obj) => {
            const h = Blazor.platform.readInt32Field(obj, 8);
            const p = Blazor.platform.readStringField(obj, 0);
            readFileBuffers[h] = await ffmpegObjectInstances[h].FS('readFile', p);
            return true; //Uint8Array array
        },
        readFileLength: (obj) => {
            const h = Blazor.platform.readInt32Field(obj, 8);
            return readFileBuffers[h].byteLength;
        },
        readFileProcess: (obj,buffer)=>
        {
            const h = Blazor.platform.readInt32Field(obj, 8);
            let buff = Blazor.platform.toUint8Array(buffer);
            buff.set(readFileBuffers[h]);
            delete readFileBuffers[h];
        },
        writeFileFFmpeg: async (obj, buffer)=>{
            const contentArray = Blazor.platform.toUint8Array(buffer);
            const h = Blazor.platform.readInt32Field(obj, 8);
            const p = Blazor.platform.readStringField(obj, 0);
            await ffmpegObjectInstances[h].FS("writeFile", p, contentArray);

        },
        unlinkFileFFmpeg: async (h, p) => {
            await ffmpegObjectInstances[h].FS('unlink', p);
        },
        fsFFmpeg: async (h, method, args) => {
            return await ffmpegObjectInstances[h].FS(method, ...args);
        },
        dispose: function () {
            DotNet.disposeJSObjectReference(this);
        },
        disposeFFmpeg: (hash) => {
            delete ffmpegObjectInstances[hash];
        },
        createObjectURL: (data,name,type) => {
            const contentArray = Blazor.platform.toUint8Array(data);
            const nameStr = BINDING.conv_string(name);
            const contentTypeStr = BINDING.conv_string(type);

            const file = new File([contentArray], nameStr, { type: contentTypeStr });
            return BINDING.js_to_mono_obj(URL.createObjectURL(file));

        }
    };
}
