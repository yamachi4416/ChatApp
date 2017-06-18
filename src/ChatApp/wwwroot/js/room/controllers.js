angular.module('ChatApp')
    .controller('RoomController', ['RoomContext', '$location', '$rootScope',
        function (RoomContext, $location, $rootScope) {
            var c = RoomContext;

            this.InitRooms = function () {
                return c.fetchJoinRooms()
                    .then(function (rooms) {
                        this.SelectRoom(c.getRoomOrFirst($location.hash()));
                        $rootScope.$broadcast('$locationChangeSuccess');
                    }.bind(this));
            };

            this.SelectRoom = function (room) {
                if (room == null || c.room == room) return;
                $location.hash(room.id);
            };

            this.ChangeRoom = function() {
                return c.selectRoom($location.hash()).fetchRoomContents();
            };

            this.GetOldmessages = function () {
                return c.room && c.room.fetchOldMessages();
            };

            this.PostMessage = function() {
                if (c.room) {
                    return c.room.postMessage().then(function() {
                        return c.room.fetchNewMessages().then(function() {
                            $rootScope.$broadcast('chatRoomMessageScrollBottom', {
                                ifShowBottom: true
                            });
                        });
                    });
                } 
            };

            this.InitRooms();
        }]);