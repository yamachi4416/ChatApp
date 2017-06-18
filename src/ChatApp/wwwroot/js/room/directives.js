var ChatApp = ChatApp || {};
ChatApp.Directives = ChatApp.Directives || {};

ChatApp.Directives.ChatRoom = function ($timeout, $location, $rootScope) {
    var containar, 
        content = function() { return containar.find('.media-list'); },
        postform,
        _saveScrollMap = {},
        _getOldMessages,
        _chatRoomChange;

    function restoreScroll(roomid, defered) {
        if (!defered) return;
        containar.css({ 'opacity': '0' });
        return defered.finally(function() {
            $timeout(function () {
                containar.scrollTop(_saveScrollMap[roomid] || content().height());
                containar.animate({ 'opacity': '1' });
            }, 200);
        });
    }

    function keepScrollTop(defered) {
        return defered && defered.then(function() {
            var oldHeight = content().height();
            return $timeout(function () {
                containar.scrollTop(content().height() - oldHeight);
            });
        });
    }

    function getOldMessages(e) {
        if (!getOldMessages.enable) return;
        if (containar.scrollTop() === 0) {
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
            var cm = containar;
            var cf = postform;
            cm.css({ 'overflow-y': 'scroll' });
            cm.height(angular.element(window).height() - cf.height() - cm.offset().top - 30);
        });
    }

    function registerHandles(scope) {
        containar.bind('scroll', function(e) {
            if (scope.chatRoom == null) return;
            _saveScrollMap[scope.chatRoom.id] = containar.scrollTop();
            return keepScrollTop(getOldMessages(e));
        });

        $rootScope.$on('$locationChangeSuccess', function(e) {
            return restoreScroll($location.hash(), _chatRoomChange());
        });

        $rootScope.$on('chatRoomMessageReady', function() {
            angular.element(window).bind('resize', justMessageSize);
            justMessageSize();
            $timeout(function() {
                postform.show();
            });
        });

        $rootScope.$on('chatRoomMessageScrollBottom', function(e, opts) {
            var opt = angular.extend({}, opts);
            var oldTop = containar.scrollTop();
            var oldHeight = content().height();
            if (opt.ifShowBottom) {
                if (oldHeight - oldTop > containar.height()) return;
            }
            $timeout(function() {
                var height = content().height();
                containar.scrollTop(height - containar.height());
            });
        });
    }

    function link(scope, elem, attrs, ctrl) {
        containar = angular.element('#' + scope.chatPrefix);
        postform = angular.element('#' + scope.chatPrefix + '-form');

        _getOldMessages = scope.oldMessages;
        _chatRoomChange = scope.chatRoomChange;

        registerHandles(scope);
    }

    return {
        scope: {
            chatPrefix: '@',
            chatRoom: '=',
            chatRoomChange: '&',
            oldMessages: '&'
        },
        restrict: 'A',
        link: link
    };
};

ChatApp.Directives.ChatSidebar = function() {
    return {
        scope: {
            chatPrefix: '@'
        },
        restrict: 'A',
        transclude: true,
        replace: true,
        controller: function() {
            this.selector = '';
            this.isShowSidebar = false;

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
        }
    };
};

angular.module('ChatApp')
    .directive('chatRoom', ['$timeout', '$location', '$rootScope', ChatApp.Directives.ChatRoom])
    .directive('chatSidebar', [ChatApp.Directives.ChatSidebar]);