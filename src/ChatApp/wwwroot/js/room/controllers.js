angular.module('ChatApp')
    .controller('RoomController', ['RoomContext', '$location', '$rootScope', '$uibModal',
        function (RoomContext, $location, $rootScope, $uibModal) {
            var c = RoomContext;

            this.InitRooms = function () {
                return c.fetchJoinRooms()
                    .then(function (rooms) {
                        this.SelectRoom(c.getRoomOrFirst($location.hash()));
                        $rootScope.$broadcast('chatRoomMessageReady');
                    }.bind(this));
            };

            this.SelectRoom = function (room) {
                if (room == null || c.room == room) return;
                return $location.hash(room.id);
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

            this.OpenAddMember = function() {
                if (!c.room.isAdmin) return;
                
                var modalInstance = $uibModal.open({
                    templateUrl: '/templates/room/modal-members-add.html',
                    controller: 'RoomMemberController',
                    controllerAs: 'ctrl'
                });

                modalInstance.result.then(function() {
                    console.log(arguments);
                }, function() {
                    console.log(arguments);
                });
            };

            this.InitRooms();
        }])
    .controller('RoomMemberController', ['RoomContext', 'RoomAdminService', '$uibModalInstance',
        function(RoomContext, service, $uibModalInstance) {
            var c = RoomContext;

            this.icon = 'plus';
            this.members = [];
            this.search = '';

            this.close = function() {
                $uibModalInstance.dismiss();
            };

            this.searchAddMembers = function() {
                var room = c.room;
                var search = this.search;

                if (room == null) return;
                if (!search || search.length < 2) return;
                
                return service.searchAddMembers(room, search)
                    .then(function(members) {
                        this.members = members;
                    }.bind(this));
            };

            this.doMember = function(idx) {
                var room = c.room;
                var members = this.members;
                var member = members[idx];
                members.splice(idx, 1);
                return service.addMember(room, member);
            };

        }]);