import { ChatSidebarDirective } from "./directives/ChatSidebarDirective";
import { ChatMessageImageDirective } from "./directives/ChatMessageImageDirective";
import { ChatAutoresizeDirective } from "./directives/ChatAutoresizeDirective";

(function angular_directives(chatapp) {
    angular.module('ChatApp')
        .value('chatMessageNoImage', '../images/noimage.jpg')
        .directive('chatRoom', ['$timeout', '$location', '$rootScope', '$q', chatapp.directives.ChatRoom])
        .directive('chatSidebar', ChatSidebarDirective.Factory)
        .directive('chatMessageImage', ChatMessageImageDirective.Factory)
        .directive('chatImageCliper', ['$timeout', '$document', chatapp.directives.ChatImageCliper])
        .directive('chatAutoresize', ChatAutoresizeDirective.Factory)
        .directive('mediaClass', ['$window', chatapp.directives.mediaClass]);
}(function chatapp_directives(chatapp) {
    'use strict';

    var directives = chatapp.directives = {};

    directives["ChatRoom"] = function ($timeout, $location, $rootScope, $q) {
        var containar,
            content,
            postform,
            saveScrollMap = {},
            isForceScrollBottom = true,
            _getOldMessages,
            _chatRoomChange,
            _loadingTemplate;

        function restoreScroll(roomid, defered) {
            if (!defered) return;
            var top = saveScrollMap[roomid];
            containar().css({ 'opacity': '0' });
            return defered.finally(function () {
                justMessageSize();
                $timeout(function () {
                    containar()
                        .scrollTop(top == null ? content().height() : top)
                        .animate({ 'opacity': '1' });
                });
            });
        }

        function keepScrollTop(defered) {
            return defered && defered.then(function () {
                var oldHeight = content().height();
                return $timeout(function () {
                    containar().scrollTop(content().height() - oldHeight);
                });
            });
        }

        var getOldMessagesEnable = true;

        function getOldMessages(e) {
            if (!getOldMessagesEnable) return;
            if (containar().scrollTop() === 0) {
                var def = _getOldMessages();
                if (!def || def.rejected === true) return;
                getOldMessagesEnable = false;
                var loading = angular.element(_loadingTemplate);
                containar().prepend(loading);
                return def.finally(function () {
                    loading.remove();
                    getOldMessagesEnable = true;
                });
            }
        }


        function justMessageSize() {
            $timeout(function () {
                var cm = containar();
                var cf = postform();
                cm.css({ 'overflow-y': 'scroll' });
                cm.height(angular.element(window).height() - cf.height() - cm.offset().top - 30);
            });
        }

        function registerHandles(scope) {
            $rootScope.$on('$locationChangeSuccess', function (e) {
                return restoreScroll($location.hash(), _chatRoomChange());
            });

            $rootScope.$on('chatRoomMessageReady', function () {
                angular.element(window).bind('resize', justMessageSize).trigger('resize');
                containar().bind('scroll', function (e) {
                    if (scope.chatRoom == null) return;
                    saveScrollMap[scope.chatRoom.id] = containar().scrollTop();
                    return keepScrollTop(getOldMessages(e));
                });
                restoreScroll($location.hash(), _chatRoomChange());
            });

            $rootScope.$on('chatRoomMessageScrollBottom', function (e, opts) {
                var opt = angular.extend({}, opts);
                var oldTop = containar().scrollTop();
                var oldHeight = content().height();

                if (opt.ifShowBottom && !isForceScrollBottom) {
                    if (oldHeight - oldTop - 10 > containar().height()) return;
                }

                isForceScrollBottom = false;

                $timeout(function () {
                    var height = content().height();
                    containar().scrollTop(height - containar().height());
                });
            });
        }

        function link(scope, elem, attrs, ctrl) {
            var _containar = angular.element('#' + scope.chatPrefix);

            _getOldMessages = scope.oldMessages;
            _chatRoomChange = scope.chatRoomChange;
            _loadingTemplate = scope.loadingTemplate;

            containar = function () {
                return _containar;
            };

            content = function () {
                return containar().find(scope.chatContent);
            };

            postform = function () {
                return angular.element('#' + scope.chatPrefix + '-form');
            };

            postform().on('click', 'button', function () {
                isForceScrollBottom = true;
            });

            containar().css({ 'opacity': '0' });

            registerHandles(scope);
        }

        return {
            scope: {
                chatPrefix: '@',
                chatRoom: '<',
                chatRoomChange: '&',
                oldMessages: '&',
                chatContent: '@',
                loadingTemplate: '<'
            },
            restrict: 'A',
            link: link
        };
    };

    directives["ChatImageCliper"] = function ($timeout, $document) {
        return {
            scope: {
                cCtrl: '=',
                cAssign: '@',
                cOption: '<',
                cSrc: '@'
            },
            restrict: 'A',
            link: function (scope, elem, attrs) {
                var cliper = angular.element(elem).append(
                    angular.element('<img>').attr('src', scope.cSrc)
                )["imageCliper"](scope.cOption);
                var range = { min: 0, val: 50, max: 100 };

                scope.cCtrl[scope.cAssign] = {
                    cliper: cliper,
                    range: range
                };

                cliper.on('cliper.srcChanged', updateRange);
                cliper.on('cliper.zoomed', function (e, info) {
                    $timeout(function () { range.val = info.width });
                });

                function updateRange(e, info) {
                    $timeout(function () {
                        range.max = info.maxWidth;
                        range.min = info.minWidth;
                        $timeout(function () { range.val = info.width; });
                    });
                }

                cliper.start();
            }
        };
    };

    directives["mediaClass"] = function ($window) {
        var xs = 480;
        var sm = 768;
        var md = 992;

        return {
            restrict: "A",
            scope: {
                media: '<mediaClass'
            },
            link: function (scope, element, attrs) {
                var xsClass = scope.media.xs,
                    smClass = scope.media.sm,
                    mdClass = scope.media.md;

                var watcher = function () { return $window.innerWidth; };

                var handler = function (newVal, oldVal?) {
                    xsClass && element.removeClass(xsClass);
                    smClass && element.removeClass(smClass);
                    mdClass && element.removeClass(mdClass);
                    if (newVal <= xs) {
                        xsClass && element.addClass(xsClass);
                    } else if (newVal <= sm) {
                        smClass && element.addClass(smClass);
                    } else {
                        mdClass && element.addClass(mdClass);
                    }
                };

                scope.$watch(watcher, handler);

                handler(watcher());
            }
        };
    };

    return chatapp;
}(window["chatapp"] = window["chatapp"] || {})));
