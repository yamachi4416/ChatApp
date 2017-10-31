(function angular_directives(chatapp) {
    angular.module('ChatApp')
        .value('chatMessageNoImage', '../images/noimage.jpg')
        .directive('chatRoom', ['$timeout', '$location', '$rootScope', '$q', chatapp.directives.ChatRoom])
        .directive('chatSidebar', [chatapp.directives.ChatSidebar])
        .directive('chatMessageImage', ['chatMessageNoImage', chatapp.directives.chatMessageImage])
        .directive('chatImageCliper', ['$timeout', '$window', chatapp.directives.ChatImageCliper]);
}(function chatapp_directives(chatapp) {
    'use strict';

    var directives = chatapp.directives = {};

    directives.ChatRoom = function ($timeout, $location, $rootScope, $q) {
        var containar,
            content,
            postform,
            saveScrollMap = {},
            isForceScrollBottom = true,
            _getOldMessages,
            _chatRoomChange;

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

        function getOldMessages(e) {
            if (!getOldMessages.enable) return;
            if (containar().scrollTop() === 0) {
                var def = _getOldMessages();
                if (!def) return;
                getOldMessages.enable = false;
                return def.finally(function () {
                    getOldMessages.enable = true;
                });
            }
        }

        getOldMessages.enable = true;

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
                angular.element(window).bind('resize', justMessageSize);
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
                chatContent: '@'
            },
            restrict: 'A',
            link: link
        };
    };

    directives.ChatSidebar = function () {
        return {
            scope: {
                chatPrefix: '@',
                chatSidebar: '<'
            },
            restrict: 'A',
            transclude: true,
            replace: true,
            controller: function () {
                this.selector = '';
                this.isShowSidebar = false;

                this.sidebarColClass = function () {
                    var cls = [];
                    angular.forEach(this.sizes, function (v, k) {
                        cls.push('col-' + k + '-' + v);
                    });
                    return cls.join(' ');
                };

                this.toggleSidebar = function () {
                    var ele = angular.element('#' + this.selector);
                    this.isShowSidebar = ele.is('.hidden-xs');
                    ele.toggleClass('hidden-xs');
                };
            },
            controllerAs: 'ctrl',
            templateUrl: '../templates/room/sidebar.html',
            link: function (scope, elem, attrs, ctrl) {
                ctrl.selector = scope.chatPrefix;
                ctrl.sizes = scope.chatSidebar;
            }
        };
    };

    directives.chatMessageImage = function (noImage) {
        var error_urls = {};

        function errorHandle(elem) {
            error_urls[elem.attr('src')] = true;
            var a = angular.element('<a>').attr({
                target: '_blank',
                title: elem.attr('alt'),
                href: elem.attr('src')
            });
            elem.attr('src', noImage).unbind('load').unwrap().wrap(a);
        }

        return {
            restrict: 'A',
            link: function (scope, elem, attrs, ctrl) {
                if (error_urls[elem.attr('src')]) {
                    errorHandle(elem);
                } else {
                    elem.on('load', function () {
                        elem.unwrap();
                    }).on('error', function () {
                        errorHandle(elem);
                    }).wrap(angular.element('<div>').addClass('chat-img-loading'));
                }
            }
        };
    };

    directives.ChatImageCliper = function ($timeout, $window) {
        return {
            scope: {
                cCtrl: '=',
                cAssign: '@',
                cOption: '<'
            },
            restrict: 'A',
            link: function (scope, elem, attrs) {
                var cliper = angular.element(elem).imageCliper(scope.cOption);
                var range = { min: 0, val: 50, max: 100 };

                scope.cCtrl[scope.cAssign] = {
                    cliper: cliper,
                    range: range
                };

                cliper.on('cliper.zoomed', function (e, info) {
                    $timeout(function () { range.val = info.width });
                });

                scope.$on('$destroy', function () {
                    cliper.stop($window);
                });

                cliper.start($window);
            }
        };
    };

    return chatapp;
}(window.chatapp = window.chatapp || {})));
