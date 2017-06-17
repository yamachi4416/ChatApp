angular.module('ChatApp')
    .controller('RoomController', ['RoomService', 'RoomContainer', '$location', '$rootScope',
        function (RoomService, RoomContainer, $location, $rootScope) {
            var c = RoomContainer;

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
                c.selectRoom($location.hash());
                return c.room && RoomService.getRoomContents(c.room);
            };

            this.GetOldmessages = function () {
                return c.room && c.room.fetchOldMessages();
            };

            this.InitRooms();
        }]);