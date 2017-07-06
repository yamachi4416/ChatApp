/*
This file in the main entry point for defining grunt tasks and using grunt plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409
*/
module.exports = function (grunt) {
    "use strict";
    const paths = require('path');
    const fs = require('fs');

    let devPath = (path) => 'wwwdev/' + path;
    let destPath = (path) => 'wwwroot/' + path;
    let doOnChdir = (path, f) => {
        process.chdir(path);
        f();
        process.chdir(__dirname);
    };

    let loadNpmTasks = (name) => {
        let root = paths.resolve('/');
        let cdir = '.';
        while (root != paths.resolve(cdir)) {
            let p = paths.resolve(cdir, 'node_modules', name);
            if (fs.existsSync(p)) {
                doOnChdir(cdir, () => grunt.loadNpmTasks(name));
                return;
            }
            cdir += '/../'
        }
    };

    loadNpmTasks('grunt-contrib-uglify');
    loadNpmTasks('grunt-contrib-less');
    loadNpmTasks('grunt-contrib-watch');
    loadNpmTasks('grunt-contrib-cssmin');

    let uglify_wwwroot_files = () => {
        let setting = {};
        let _ = (p) => `${destPath('js/' + p )}`;

        let angular_scripts = (mod) => {
            let ax = ['app', 'services', 'directives', 'controllers']
                .map((name) => _(`${mod}/${name}.js`));
            ax.push('!*.min.js');
            setting[_(`${mod}/app.min.js`)] = ax;
        };
        
        setting[_('site.min.js')] = _('site.js');
        angular_scripts('room');

        return setting;
    };

    grunt.initConfig({
        uglify: {
            wwwroot: {
                files: uglify_wwwroot_files()
            }
        },
        less: {
            wwwdev: {
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

    grunt.registerTask('build', ['less', 'cssmin', 'uglify']);
};
