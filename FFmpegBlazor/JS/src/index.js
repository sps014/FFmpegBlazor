import { FFmpeg } from "@ffmpeg/ffmpeg";

let ffmpegs = {}; ///unique id to ffmpeg object map

export function createFFmpeg(hash) {
  ffmpegs[hash] = new FFmpeg();
}

export function destroyFFmpeg(hash) {
  delete ffmpegs[hash];
}

export function loaded(hash) {
  return ffmpegs[hash].loaded;
}

export async function exec(hash, args, timeout = -1, _namedParameters = {}) {
  await ffmpegs[hash].exec(args, timeout, _namedParameters);
}

export async function load(hash, dotnet) {
    try {
        console.log(hash);

        let result = await ffmpegs[hash].load();
        await dotnet.invokeMethodAsync("OnFFmpegLoaded", result, null);
    }
    catch (e) {
        await dotnet.invokeMethodAsync("OnFFmpegLoaded", false, e.message);
  }
}
