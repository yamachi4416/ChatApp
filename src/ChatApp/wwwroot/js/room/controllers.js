angular.module('ChatApp')
    .controller('RoomController', ['RoomService', 'RoomContainer', '$location',
        function (RoomService, RoomContainer, $location) {
            var c = this.c = RoomContainer;

            this.InitRooms = function () {
                RoomService.getJoinRooms()
                    .then(function (rooms) {
                        c.rooms = rooms;
                        this.SelectRoom(rooms[0]);
                    }.bind(this));
            };

            this.SelectRoom = function (room) {
                if (c.room === room) return;
                c.room = room;
                RoomService.getMembers(room)
                    .then(function (members) {
                        room.members = members;
                    });

                RoomService.getNewMessages(room)
                    .then(function (messages) {
                        room.messages = messages;
                        this.readyLoadOldMessages = true;
                    }.bind(this));
            };

            this.GetOldmessage = function () {
                this.readyLoadOldMessages = false;
                var room = c.room;
                RoomService.getOldMessages(room)
                    .then(function (messages) {
                        if (!messages || !messages.length) return;
                        angular.forEach(messages, function (message) {
                            room.messages.push(message);
                        });
                        this.readyLoadOldMessages = true;
                    }.bind(this));
            };

            this.InitRooms();
        }]);