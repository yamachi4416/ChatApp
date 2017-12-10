(function angular_directives(chatapp) {
    angular.module('ChatApp')
        .value('chatMessageNoImage', '../images/noimage.jpg')
        .directive('chatRoom', ['$timeout', '$location', '$rootScope', '$q', chatapp.directives.ChatRoom])
        .directive('chatSidebar', [chatapp.directives.ChatSidebar])
        .directive('chatMessageImage', ['chatMessageNoImage', chatapp.directives.chatMessageImage])
        .directive('chatImageCliper', ['$timeout', '$document', chatapp.directives.ChatImageCliper])
        .directive('chatAutoresize', ['$window', '$timeout', chatapp.directives.chatAutoresize])
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

    directives["ChatSidebar"] = function () {
        return {
            scope: {
            },
            restrict: 'A',
            transclude: true,
            replace: true,
            controller: function () {
                this.isShowSidebar = false;

                this.toggleSidebar = function () {
                    var ele = angular.element('#chat-sidebar');
                    this.isShowSidebar = ele.is('.hidden-xs');
                    ele.toggleClass('hidden-xs');
                };
            },
            controllerAs: 'ctrl',
            templateUrl: '/sidebar/main.tmpl.html',
            link: function (scope, elem, attrs, ctrl) {
            }
        };
    };

    directives["chatMessageImage"] = function (noImage) {
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

    directives["ChatImageCliper"] = function ($timeout, $document) {
        return {
            scope: {
                cCtrl: '=',
                cAssign: '@',
                cOption: '<'
            },
            restrict: 'A',
            link: function (scope, elem, attrs) {
                var cliper = angular.element(elem)["imageCliper"](scope.cOption);
                var range = { min: 0, val: 50, max: 100 };

                scope.cCtrl[scope.cAssign] = {
                    cliper: cliper,
                    range: range
                };

                cliper.on('cliper.zoomed', function (e, info) {
                    $timeout(function () { range.val = info.width });
                });

                cliper.start();
            }
        };
    };

    directives["chatAutoresize"] = function ($window, $timeout) {
        function getMaxHeight(element, parent) {
            var maxHeight = element.css('max-height');
            if (maxHeight == 'none' || !maxHeight) {
                return null;
            }

            if (/^[0-9.]+%$/.test(maxHeight)) {
                return parent.height() * parseFloat(maxHeight) / 100;
            } else if (/^[0-9.]+$/.test(maxHeight)) {
                return parseFloat(maxHeight);
            }

            return null;
        }

        function resize(element, parent, diff) {
            var oldHeight = element.height();
            element.height(0);
            var newHeight = element[0].scrollHeight - diff;
            var maxHeight = getMaxHeight(element, parent);

            if (maxHeight && newHeight > maxHeight) {
                element.height(maxHeight);
            } else {
                element.height(newHeight);
            }

            parent.scrollTop(parent.height());
        }

        function ScopeWatcher(scope, watcher, handler) {
            var watch;
            return {
                start: function (isCall) {
                    this.stop();
                    isCall && handler();
                    watch = scope.$watch(watcher, handler);
                },
                stop: function () {
                    if (watch) {
                        watch();
                        watch = null;
                    }
                }
            };
        }

        return {
            restrict: "A",
            require: "ngModel",
            scope: {},
            link: function (scope, element, attrs, ngModel) {
                var heightDiff = parseInt(element.css('padding-bottom')) + parseInt(element.css('padding-top'));
                var resizeHandler = resize.bind(null, element, angular.element($window), heightDiff);
                var modelWatcher = ScopeWatcher(scope, function () { return ngModel.$viewValue; }, resizeHandler);
                var initHeight = element.height();
                var restoreHeight = function () {
                    element.height(initHeight);
                    angular.element($window).trigger('resize');
                };

                element.bind('focus', function () {
                    modelWatcher.start(true);
                }).bind('blur', function () {
                    $timeout(function () {
                        var focus = element.closest('form').find(':focus');
                        if (!focus.length) {
                            modelWatcher.stop();
                            restoreHeight();
                        } else {
                            focus.one('blur', restoreHeight);
                        }
                    });
                }).height(element.scrollHeight - heightDiff).css({
                    opacity: 0
                });

                scope.$on('chatRoomMessageReady', function () {
                    element.css({ opacity: 1 });
                });
                scope.$on('$destroy', modelWatcher.stop.bind(modelWatcher));
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
