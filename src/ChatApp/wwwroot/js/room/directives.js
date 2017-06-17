angular.module('ChatApp')
    .directive('chatRoom', ['$timeout', '$location', '$rootScope',
        function ($timeout, $location, $rootScope) {

            var _containar, 
                _content,
                _saveScrollMap = {},
                _getOldMessages,
                _chatRoomChange;

            function restoreScroll(roomid, defered) {
                if (!defered) return;
                _containar.css({ 'opacity': '0' });
                return defered.then(function() {
                }).finally(function() {
                    $timeout(function () {
                        _containar.scrollTop(_saveScrollMap[roomid] || _content.height());
                        _containar.animate({ 'opacity': '1' });
                    });
                });
            }

            function keepScrollTop(defered) {
                return defered && defered.then(function() {
                    var oldHeight = _content.height();
                    return $timeout(function () {
                        _containar.scrollTop(_content.height() - oldHeight);
                    });
                });
            }

            function getOldMessages(e) {
                if (!getOldMessages.enable) return;
                if (_containar.scrollTop() === 0) {
                    var def = _getOldMessages();
                    if (!def) return;
                    getOldMessages.enable = false;
                    return def.finally(function() {
                        getOldMessages.enable = true;
                    });
                }
            }

            getOldMessages.enable = true;

            return {
                scope: {
                    chatRoom: '=',
                    chatRoomChange: '&',
                    oldMessages: '&'
                },
                restrict: 'A',
                link: function (scope, elem, attrs, ctrl) {
                    _containar = angular.element(elem);
                    _content = angular.element(_containar[0].firstElementChild);

                    _getOldMessages = scope.oldMessages;
                    _chatRoomChange = scope.chatRoomChange;

                    _containar.bind('scroll', function(e) {
                        if (scope.chatRoom == null) return;
                        _saveScrollMap[scope.chatRoom.id] = _containar.scrollTop();
                        return keepScrollTop(getOldMessages(e));
                    });

                    $rootScope.$on('$locationChangeSuccess', function(e) {
                        return restoreScroll($location.hash(), _chatRoomChange());
                    });
                }
            };
        }]);