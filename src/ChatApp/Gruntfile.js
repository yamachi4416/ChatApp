const grunt = require("grunt");

module.exports = function () {
    "use strict";
    const fs = require("fs");
    const path = require("path");
    const glob = require("glob");
    const child_process = require("child_process");

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
    grunt.registerTask("typings", "typings", function () {
        const done = this.async();
        if (!fs.existsSync("./typings")) {
            child_process.execSync("typings init");
        }

        glob.glob(devPath("/ts/**/*.d.ts"), function (err, files) {
            files.forEach((file) => {
                let command = `typings install file:${file} --global`;
                let result = child_process.execSync(command);
                grunt.log.write(result);
            });
            done();
        });
    });
};
