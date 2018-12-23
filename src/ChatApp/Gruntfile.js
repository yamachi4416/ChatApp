const grunt = require("grunt");

module.exports = function () {
    "use strict";

    let devPath = (path) => "./wwwroot/src/" + path;
    let destPath = (path) => "./wwwroot/" + path;

    grunt.loadNpmTasks("grunt-contrib-less");
    grunt.loadNpmTasks("grunt-contrib-cssmin");
    grunt.loadNpmTasks("grunt-contrib-watch");

    grunt.initConfig({
        less: {
            default: {
                files: [{
                    expand: true,
                    cwd: devPath("less"),
                    src: ["**/*.less", "!**/_*.less"],
                    dest: destPath("css"),
                    ext: ".css"
                }],
                options: {
                }
            }
        },
        cssmin: {
            target: {
                files: [{
                    expand: true,
                    cwd: destPath("css"),
                    src: ["**/*.css", "!*.min.css"],
                    dest: destPath("css"),
                    ext: ".min.css"
                }]
            }
        },
        watch: {
            less: {
                files: devPath("less/**/*.less"),
                tasks: ["less"],
                options: {
                    livereload: false
                }
            }
        }
    });

    grunt.registerTask("build", ["less", "cssmin"]);
};
