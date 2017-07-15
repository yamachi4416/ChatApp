(function angular_directives(chatapp) {
    angular.module('ChatApp')
        .directive('chatRoom', ['$timeout', '$location', '$rootScope', '$q', chatapp.directives.ChatRoom])
        .directive('chatSidebar', [chatapp.directives.ChatSidebar])
        .directive('chatMessageImage', [chatapp.directives.chatMessageImage]);
}
(function chatapp_directives(chatapp) {
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
            containar().css({ 'opacity': '0' });
            return defered.finally(function() {
                justMessageSize();
                $timeout(function () {
                    var top = saveScrollMap[roomid];
                    containar()
                        .scrollTop(top == null ? content().height() : top)
                        .animate({ 'opacity': '1' });
                });
            });
        }

        function keepScrollTop(defered) {
            return defered && defered.then(function() {
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
                return def.finally(function() {
                    getOldMessages.enable = true;
                });
            }
        }

        getOldMessages.enable = true;

        function justMessageSize() {
            $timeout(function() {
                var cm = containar();
                var cf = postform();
                cm.css({ 'overflow-y': 'scroll' });
                cm.height(angular.element(window).height() - cf.height() - cm.offset().top - 30);
            });
        }

        function registerHandles(scope) {
            containar().bind('scroll', function(e) {
                if (scope.chatRoom == null) return;
                saveScrollMap[scope.chatRoom.id] = containar().scrollTop();
                return keepScrollTop(getOldMessages(e));
            });

            $rootScope.$on('$locationChangeSuccess', function(e) {
                return restoreScroll($location.hash(), _chatRoomChange());
            });

            $rootScope.$on('chatRoomMessageReady', function() {
                angular.element(window).bind('resize', justMessageSize);
                justMessageSize();
                restoreScroll($location.hash(), _chatRoomChange());
            });

            $rootScope.$on('chatRoomMessageScrollBottom', function(e, opts) {
                var opt = angular.extend({}, opts);
                var oldTop = containar().scrollTop();
                var oldHeight = content().height();
                
                if (opt.ifShowBottom && !isForceScrollBottom) {
                    if (oldHeight - oldTop -10 > containar().height()) return;
                }

                isForceScrollBottom = false;

                $timeout(function() {
                    var height = content().height();
                    containar().scrollTop(height - containar().height());
                });
            });
        }

        function link(scope, elem, attrs, ctrl) {
            var _containar = angular.element('#' + scope.chatPrefix); 
            
            _getOldMessages = scope.oldMessages;
            _chatRoomChange = scope.chatRoomChange;

            containar = function() {
                return _containar;
            };

            content = function() {
                return containar().find(scope.chatContent);
            };

            postform = function() {
                return angular.element('#' + scope.chatPrefix + '-form');
            };

            postform().on('click', 'button', function() {
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

    directives.ChatSidebar = function() {
        return {
            scope: {
                chatPrefix: '@',
                chatSidebar: '<'
            },
            restrict: 'A',
            transclude: true,
            replace: true,
            controller: function() {
                this.selector = '';
                this.isShowSidebar = false;

                this.sidebarColClass = function() {
                    var cls = [];
                    angular.forEach(this.sizes, function(v, k) {
                        cls.push('col-' + k + '-' + v);
                    });
                    return cls.join(' ');
                };

                this.toggleSidebar = function() {
                    var ele = angular.element('#' + this.selector);
                    this.isShowSidebar = ele.is('.hidden-xs');
                    ele.toggleClass('hidden-xs');
                };
            },
            controllerAs: 'ctrl',
            templateUrl: '/templates/room/sidebar.html',
            link: function(scope, elem, attrs, ctrl) {
                ctrl.selector = scope.chatPrefix;
                ctrl.sizes = scope.chatSidebar;
            }
        };
    };

    directives.chatMessageImage = function() {
        return {
            restrict: 'A',
            link: function(scope, elem, attrs, ctrl) {
                elem.on('load', function() {
                    elem.unwrap();
                }).on('error', function() {
                    var a = angular.element('<a>').attr({
                        target: '_blank',
                        href: elem.attr('src')
                    }).text(elem.attr('src'));
                    elem.unwrap().replaceWith(a);
                }).wrap(angular.element('<div>').addClass('chat-img-loading'));
            }
        };
    };

    return chatapp;
}(window.chatapp = window.chatapp || {})));
