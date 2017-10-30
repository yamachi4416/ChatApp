(function ($) {
    'use strict';
    var disableHandle = function (e) { e.preventDefault(); };

    var ImageCliper = function (options, ele) {

        this.opts = $.extend({
            zoomStep: 0.5,
            imageType: 'image/png',
            imageQuality: 1,
            minRatio: 0.5,
            maxRatio: 5,
        }, options);

        this.state = {
            px: 0,
            py: 0,
            drag: false,
            dtagStart: false
        };

        this.initElements(ele);
    };

    ImageCliper.prototype.initElements = function (ele) {
        this._clip = $(ele);
        this._img = this._clip.find('img');

        this._clip.css({
            position: 'relative',
            width: this._clip.width(),
            height: this._clip.height(),
        }).on('contextmenu', disableHandle);

        this._img.css({
            position: 'absolute'
        }).on('dragstart', disableHandle);
    };

    ImageCliper.prototype.isForcus = function () {
        return !this.opts.focusOnly || this._clip.is(':focus');
    };

    ImageCliper.prototype.isDragEnable = function () {
        return this.state.drag && this.isForcus();
    };

    ImageCliper.prototype.dragStart = function (e) {
        this.state.drag = true;
        this.state.dragStart = true;
    };

    ImageCliper.prototype.dragStop = function (e) {
        this.state.dragStart = false;
        if (this.isDragEnable()) {
            this.state.drag = false;
            this.touchStart = null;
        }
    };

    ImageCliper.prototype.move = function (x, y) {
        var pos = this._img.offset();
        var newpos = {
            top: Math.floor(pos.top + y),
            left: Math.floor(pos.left + x)
        };
        this._img.offset(newpos);
    };

    ImageCliper.prototype.drag = function (e) {
        if (!this.isDragEnable())
            return;

        e.preventDefault();
        if (this.state.dragStart) {
            this.state.dragStart = false;
        } else {
            this.move(e.pageX - this.state.px, e.pageY - this.state.py);
        }

        this.state.px = e.pageX;
        this.state.py = e.pageY;
    };

    ImageCliper.prototype.rectInfo = function ($ele) {
        return {
            width: parseFloat($ele.css('width')),
            height: parseFloat($ele.css('height')),
            top: parseFloat($ele.css('top')),
            left: parseFloat($ele.css('left'))
        };
    };

    ImageCliper.prototype.imageInfo = function () {
        var info = this.rectInfo(this._img);
        var cinfo = this.rectInfo(this._clip);
        info.minWidth = cinfo.width * this.opts.minRatio;
        info.naturalWidth = this._img[0].naturalWidth;
        info.maxWidth = info.naturalWidth * this.opts.maxRatio;
        return info;
    };

    ImageCliper.prototype.zoom = function (step) {
        var $img = this._img;
        var c = this.rectInfo(this._clip);
        var p = this.rectInfo($img);
        var cw = c.width;
        var ch = c.height;
        var ow = $img[0].naturalWidth;

        if (p.width + step < cw * this.opts.minRatio && step < 0)
            return;

        if (p.width + step > ow * this.opts.maxRatio && step > 0)
            return;

        $img.css('width', p.width + step);

        var n = this.rectInfo($img);

        $img.css({
            left: p.left - (n.width - p.width) * (cw / 2 - p.left) / p.width,
            top: p.top - (n.height - p.height) * (ch / 2 - p.top) / p.height
        });

        this._clip.trigger('cliper.zoomed', this.imageInfo());
    };

    ImageCliper.prototype.zoomIn = function (step) {
        this.zoom(step || this.opts.zoomStep)
    };

    ImageCliper.prototype.zoomOut = function (step) {
        this.zoom(step || -this.opts.zoomStep)
    };

    ImageCliper.prototype.zoomInHandle = function (e) {
        if (!this.isForcus())
            return;

        e.preventDefault();
        requestAnimationFrame(function () {
            this.zoomIn();
        }.bind(this));
    };

    ImageCliper.prototype.zoomOutHandle = function (e) {
        if (!this.isForcus())
            return;

        e.preventDefault();
        requestAnimationFrame(function () {
            this.zoomOut();
        }.bind(this));
    };

    ImageCliper.prototype.drawImageContext = function () {
        var canvas = document.createElement('canvas');
        var context = canvas.getContext('2d');
        var $img = this._img;
        var sw = $img[0].naturalWidth;
        var sh = $img[0].naturalHeight;
        var i = this.rectInfo($img);
        var c = this.rectInfo(this._clip);
        canvas.width = c.width;
        canvas.height = c.height;
        context.drawImage($img[0], 0, 0, sw, sh, i.left, i.top, i.width, i.height);
        return context;
    };

    ImageCliper.prototype._dataUrlToBlob = function(dataUrl) {
        var b64 = dataUrl.replace(/^data:image\/[^;]+;base64,/, '');
        var binary = atob(b64);
        var array = new Uint8Array(binary.length);
        for (var i = 0, l = binary.length; i < l; i++) {
            array[i] = binary.charCodeAt(i);
        }
        return new Blob([array], { type: this.opts.imageType });
    };

    ImageCliper.prototype.toBlob = function(callback) {
        var context = this.drawImageContext();
        var canvas = context.canvas;
        if (typeof canvas.toBlob === 'function') {
            canvas.toBlob(callback, this.opts.imageType, this.opts.imageQuality);
        } else {
            var dataUrl = canvas.toDataURL(this.opts.imageType, this.opts.imageQuality);
            callback(this._dataUrlToBlob(dataUrl));
        }
    };

    ImageCliper.prototype.touchRect = function (ts) {
        if (!(ts[0] && ts[1])) return;

        var x1 = ts[0].clientX;
        var y1 = ts[0].clientY;
        var x2 = ts[1].clientX;
        var y2 = ts[1].clientY;

        return Math.abs(x1 - x2) * Math.abs(y1 - y2);
    };

    ImageCliper.prototype.touchStart = function (e) {
        e.stopImmediatePropagation();
        this.touchStart = e.originalEvent.touches;
        this.dragStart(e);
    };

    ImageCliper.prototype.touchMove = function (e) {
        if (!this.isDragEnable())
            return;

        var ts = e.originalEvent.touches;
        if (this.touchStart.length == 1) {
            e.pageX = ts[0].clientX;
            e.pageY = ts[0].clientY;
            this.drag(e);
        } else {
            e.preventDefault();
            var rs = this.touchStart;
            var r1 = this.touchRect(rs);
            var r2 = this.touchRect(ts);
            this.touchStart = ts;
            r1 == r2 || (r1 < r2 ? this.zoomInHandle(e) : this.zoomOutHandle(e));
        }
    };

    ImageCliper.prototype.loadFile = function (file) {
        var df = new $.Deferred();

        if (!file)
            return df.reject();

        var reader = new FileReader();
        reader.onload = function (e) {
            var buff = e.target.result;
            var blob = new Blob([buff], { type: file.type });
            var image = new Image();

            image.onload = function (e) {
                var $img = this._img
                    .attr('src', image.src)
                    .css('width', image.width);
                var c = this.rectInfo(this._clip);
                var i = this.rectInfo($img);
                $img.css({
                    left: (c.width - i.width) / 2,
                    top: (c.height - i.height) / 2
                });
                df.resolve(this.imageInfo());
            }.bind(this);

            image.onerror = function (e) {
                df.reject(e);
            };

            image.src = URL.createObjectURL(blob);
        }.bind(this);

        reader.onerror = function (e) {
            df.rejecte(e);
        }.bind(this);

        reader.readAsArrayBuffer(file);

        return df.promise();
    };

    ImageCliper.prototype.wheelHandle = function (e) {
        var oe = e.originalEvent;
        if (oe.deltaY < 0) {
            this.zoomOutHandle(e);
        } else if (oe.deltaY > 0) {
            this.zoomInHandle(e);
        }
    };

    ImageCliper.prototype.start = function (ele) {
        $(ele)
            .on('mousemove', this.drag.bind(this))
            .on('mouseup', this.dragStop.bind(this));

        this._clip
            .on('mousedown', this.dragStart.bind(this))
            .on('touchstart', this.touchStart.bind(this))
            .on('touchmove', this.touchMove.bind(this))
            .on('touchend touchcancel', this.dragStop.bind(this))
            .on('wheel', this.wheelHandle.bind(this));
    };

    $.fn.imageCliper = function (options) {
        var cliper = new ImageCliper(options, this);
        var dnoop = function() {
            var df = new $.Deferred();
            return df.resolve.call(df, arguments).promise();
        };

        return $.extend(this, {
            start: function () {
                cliper.start.apply(cliper, arguments);
                return this;
            },
            zoom: function () {
                cliper.zoom.apply(cliper, arguments);
                return this;
            },
            zoomOut: function () {
                cliper.zoomOut.apply(cliper, arguments);
                return this;
            },
            zoomIn: function () {
                cliper.zoomIn.apply(cliper, arguments);
                return this;
            },
            toBlob: function () {
                var d = new $.Deferred();
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
                    form.append(name, new File(blob, 'avatar.png', { type: cliper.opts.imageType }));

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
                var df = new $.Deferred();

                $('<input type="file" accept="image/*">')
                    .on('change', function () {
                        if (this.files) {
                            df.resolve(this.files);
                        }
                    }).click();

                return df.promise();
            }
        });
    };

})(jQuery);
