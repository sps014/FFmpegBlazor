module.exports = {
  // plugins: [["@snowpack/plugin-optimize"]],

  buildOptions: {
    out: "../wwwroot/js/",
    clean: true,
  },

  mount: {
    src: "/",
  },
};
