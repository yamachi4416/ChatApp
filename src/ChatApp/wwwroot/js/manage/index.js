$(function () {
    'use strict';

    var $modal = $('#imageClipModal');
    var $img = $modal.find('img');

    var cliper = $('#image-clip').imageCliper({
        zoomStep: 10,
        imageType: 'image/png',
        parentSelector: '.modal'
    }).start();

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
            console.log(blob.size);

            cliper.postImage($modal.data('upload-url'), 'ImageFile', {
                headers: {
                    'X-XSRF-TOKEN': $.cookie('XSRF-TOKEN') || ''
                }
            }, blob).done(function (data) {
                console.log(arguments);
                var avatar = $('#user-avater');
                var url = avatar.attr('src');
                avatar.attr('src', url.replace(/[gG]et\/?[^\/]*$/, 'Get/' + data.id));
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
});
