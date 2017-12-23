import { ImageCliper } from "./ImageCliper"

$.fn.imageCliper = function (options: ImageCliperOptions) {
    const cliper = new ImageCliper(options, this);
    const dnoop = function (args?) {
        const df = $.Deferred();
        return df.resolve.call(df, arguments).promise();
    };

    function applyMethod(method) {
        var f = cliper[method];
        return function () {
            f.apply(cliper, arguments);
            return this;
        };
    }

    return $.extend(this, {
        getSrc: function () {
            return cliper.src;
        },
        setSrc: function(src, def?) {
            return cliper.setSrc(src, def);
        },
        start: applyMethod('start'),
        normalizeRect: function() {
            return cliper.normalizeRect(this.getSrc(), $.Deferred());
        },
        zoom: applyMethod('zoom'),
        zoomOut: applyMethod('zoomOut'),
        zoomIn: applyMethod('zoomIn'),
        toBlob: function () {
            var d = $.Deferred();
            cliper.toBlob(function (blob) {
                d.resolve(blob, cliper);
            });
            return d.promise();
        },
        toBlobUrl: function () {
            return this.toBlob().then(function (blob) {
                return URL.createObjectURL(blob);
            });
        },
        loadFile: function (file) {
            return cliper.loadFile(file);
        },
        postImage: function (url, name, options, blob) {
            var toBlob = blob ? dnoop(blob) : this.toBlob();

            return toBlob.then(function (blob) {
                var form = new FormData();
                form.append(name, new File(blob, 'avatar.png', { type: cliper.options.imageType }));

                return $.ajax($.extend({
                    type: 'POST',
                    url: url,
                    data: form,
                    processData: false,
                    contentType: false
                }, options || {}));
            }.bind(this));
        },
        openFileDialog: function () {
            var df = $.Deferred();

            $('<input type="file" accept="image/*">')
                .on('change', function () {
                    if (this.files) {
                        df.resolve(this.files);
                    }
                }).click();

            return df.promise();
        },
        imageInfo: function () {
            return cliper.imageInfo();
        }
    });
};