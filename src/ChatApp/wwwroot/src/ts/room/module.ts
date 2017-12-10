import { Renderer } from "marked";

angular.module('ChatApp', ['hc.marked', 'ui.bootstrap'])
    .config(['markedProvider', function (markedProvider) {
        var options = {
            gfm: true,
            tables: true,
            sanitize: true
        };

        var reciever = {options: options};
        var renderer = Renderer.prototype;

        markedProvider.setOptions(options);
        markedProvider.setRenderer({
            link: function() {
                var link = renderer.link.apply(reciever, arguments);
                return '<a tabindex="-1" target="_blank"' + link.substring('<a'.length);
            },
            image: function() {
                var img = renderer.image.apply(reciever, arguments);
                return '<img chat-message-image ' + img.substring('<img'.length);
            }
        });
     }]);