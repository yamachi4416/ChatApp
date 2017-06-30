/*
This file in the main entry point for defining grunt tasks and using grunt plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409
*/
module.exports = function (grunt) {
    function devPath(path) {
        return 'wwwdev/' + path;
    }

    function destPath(path) {
        return 'wwwroot/' + path;
    }

    grunt.loadNpmTasks('grunt-contrib-less');
    grunt.loadNpmTasks('grunt-contrib-watch');

    grunt.initConfig({
        uglify: {

        },
        less: {
            dev: {
                files: [{
                    expand: true,
                    cwd: devPath('less'),
                    src: ['**/*.less'],
                    dest: destPath('css'),
                    ext: '.css'
                }],
                options: {

                }
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
};
