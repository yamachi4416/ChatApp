import "jquery.cookie";
import "../lib/imageCliper/ImageCliper.jquery"

jQuery(function ($) {
    'use strict';

    const $modal = $('#imageClipModal');
    const $img = $modal.find('img');
    const $avatar = $('#user-avater');
    const cliper = $('#image-clip').imageCliper({
        zoomStep: 10,
        imageType: 'image/png',
        parentSelector: '.modal'
    });

    $('#fileSelectButton').on('click', function () {
        cliper.openFileDialog()
            .then(function (files) {
                cliper.loadFile(files[0]);
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
                    'X-XSRF-TOKEN': $["cookie"]('XSRF-TOKEN') || ''
                }
            }, blob).done(function (data) {
                console.log(arguments);
                var url = $avatar.attr('src');
                $avatar.attr('src', url.replace(/[gG]et\/?[^\/]*$/, 'Get/' + data.id));
            }).fail(function () {
                console.log(arguments);
            }).always(function () {
                $img.attr({ src: '' });
                $modal["modal"]('hide');
            });
        });
    });

    $('#zoomRange').on('input', function (e) {
        var w = parseFloat($img.css('width'));
        var r = parseInt(this.value);
        cliper.zoom(r - w);
    });

    $modal.on('show.bs.modal', function() {
        cliper.setSrc($avatar.attr('src'));
    });

    cliper.on('cliper.srcChanged', function (e, info) {
        $('#zoomRange').attr({
            max: info.maxWidth,
            min: info.minWidth
        }).val(info.naturalWidth);
    }).on('cliper.zoomed', function (e, info) {
        $('#zoomRange').val(info.width);
    });

    cliper.start();
});
