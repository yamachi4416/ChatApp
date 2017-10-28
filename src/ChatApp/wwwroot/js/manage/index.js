$(function () {
    var $modal = $('#imageClipModal');
    var $img = $modal.find('img');

    var cliper = $('#image-clip').imageCliper({
        zoomStep: 10,
        imageType: 'image/png'
    }).start(window);

    $('#fileSelectButton').on('click', function () {
        cliper.openFileDialog()
            .then(function (files) {
                cliper.loadFile(files[0]).then(function (info) {
                    $('#zoomRange').attr({
                        max: info.maxWidth,
                        min: info.minWidth
                    }).val(info.naturalWidth);
                });
            });
    });

    $('#drawImageButton').on('click', function () {
        if (!$img.attr('src')) {
            return;
        }

        cliper.toBlob().then(function (blob) {
            var cookie = getCookies();
            
            cliper.postImage($modal.data('upload-url'), 'ImageFile', {
                headers: {
                    'X-XSRF-TOKEN': cookie['XSRF-TOKEN']
                }
            }, blob).done(function () {
                var url = URL.createObjectURL(blob);
                $('#user-avater').attr('src', url);
            }).fail(function () {
                console.log(arguments);
            }).always(function () {
                $img.attr({ src: '' });
                $modal.modal('hide');
            });
        });
    });

    $('#zoomRange').on('input', function () {
        var w = parseFloat($img.css('width'));
        var r = parseInt(this.value);
        cliper.zoom(r - w);
    });

    $('#image-clip').on('cliper.zoomed', function (e, info) {
        $('#zoomRange').val(info.width);
    });

    function getCookies() {
        var cookie = {};
        document.cookie.split(';').forEach(function(val) {
            var kv = val.trim().split('=');
            cookie[kv[0]] = kv[1] || '';
        });
        return cookie;
    }
});
