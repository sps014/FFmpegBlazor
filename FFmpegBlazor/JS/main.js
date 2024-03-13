
import { FFmpeg } from "@ffmpeg/ffmpeg";
import { toBlobURL } from "@ffmpeg/util";


let ffmpegs = {}; ///unique id to ffmpeg object map

window.createFFmpeg = function () {
  return new FFmpeg();
};

window.load = async function (ffmpeg, param) {
  console.log(ffmpeg);
  return await ffmpeg.load(param);
};

window.toBlobURL = async function (
  url,
  mimetype,
  progress = false,
  callback = undefined
) {
  return await toBlobURL(url, mimetype, progress, callback);
};
