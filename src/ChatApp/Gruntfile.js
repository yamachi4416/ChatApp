/*
This file in the main entry point for defining grunt tasks and using grunt plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409
*/
module.exports = function (grunt) {
    "use strict";
    const paths = require('path');
    const fs = require('fs');

    let devPath = (path) => './wwwdev/' + path;
    let destPath = (path) => './wwwroot/' + path;

    grunt.loadNpmTasks('grunt-contrib-less');
    grunt.loadNpmTasks('grunt-contrib-cssmin');
    grunt.loadNpmTasks('grunt-contrib-watch');

    grunt.initConfig({
        less: {
            wwwdev: {
                files: [{
                    expand: true,
                    cwd: devPath('less'),
                    src: ['**/*.less', '!**/_*.less'],
                    dest: destPath('css'),
                    ext: '.css'
                }],
                options: {
                }
            }
        },
        cssmin: {
            target: {
                files: [{
                    expand: true,
                    cwd: destPath('css'),
                    src: ['**/*.css', '!*.min.css'],
                    dest: destPath('css'),
                    ext: '.min.css'
                }]
            }
        },
        watch: {
            less: {
                files: devPath('less/**/*.less'),
                tasks: ['less'],
                options: {
                    livereload: false
                }
            }
        }
    });

    grunt.registerTask('build', ['less', 'cssmin']);
};
