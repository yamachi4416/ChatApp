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

            function openModalUi(options, resolve, reject) {
                var modalInstance = $uibModal.open(
                    angular.extend({
                    }, options));

                modalInstance.result.then(
                    resolve || angular.noop,
                    reject = reject || angular.noop
                );

                return modalInstance;
            }

            function openAdminModalUi() {
                if (!c.room.isAdmin) return;

                return openModalUi.apply(null, arguments);
            }

            this.OpenAddMember = function() {
                openAdminModalUi({
                    templateUrl: '/templates/room/modal-members-add.html',
                    controller: 'RoomMemberAddController',
                    controllerAs: 'ctrl'
                });
            };

            this.OpenRemoveMember = function() {
                openAdminModalUi({
                    templateUrl: '/templates/room/modal-members-remove.html',
                    controller: 'RoomMemberRemoveController',
                    controllerAs: 'ctrl'
                });
            };

            this.OpenCreateRoom = function() {
                openModalUi({
                    templateUrl: '/templates/room/modal-room-create.html',
                    controller: 'RoomCreateController',
                    controllerAs: 'ctrl'
                }, function(room) {
                    c.addRoom(room);
                    this.SelectRoom(room);
                }.bind(this));
            };

            this.OpenEditRoom = function(room) {
                openAdminModalUi({
                    templateUrl: '/templates/room/modal-room-edit.html',
                    controller: 'RoomEditController',
                    controllerAs: 'ctrl',
                    resolve: {
                        room: room
                    }
                }, function(room) {
                    console.log(room);
                    if (room && room.id) {
                        angular.extend(c.getRoom(room.id), {
                            name: room.name,
                            description: room.description,
                            updatedDate: room.updatedDate
                        });
                    }
                }.bind(this));
            };

            this.OpenDetailRoom = function(room) {
                openModalUi({
                    templateUrl: '/templates/room/modal-room-detail.html',
                    controller: 'RoomDetailController',
                    controllerAs: 'ctrl',
                    resolve: {
                        room: room
                    }
                });
            };

            this.InitRooms();
        }])
    .controller('RoomDetailController', ['$uibModalInstance', 'room',
        function($uibModalInstance, room) {
            this.room = room;

            this.close = function() {
                $uibModalInstance.dismiss();
            };
        }])
    .controller('RoomEditController', ['RoomAdminService', '$uibModalInstance', 'room',
        function(service, $uibModalInstance, room) {
            var _room = room;
            this.room = {
                id: _room.id,
                name: _room.name,
                description: _room.description
            };

            this.doRoomLabel = '変更';

            this.close = function() {
                $uibModalInstance.dismiss();
            };

            this.doRoom = function(room) {
                return service.editRoom(_room, this.room)
                    .then(function(room) {
                        $uibModalInstance.close(room);
                    }, function(res) {
                        console.log(res.data);
                    });
            };
        }
    ])
    .controller('RoomCreateController', ['RoomContext', 'RoomService', '$uibModalInstance',
        function(RoomContext, service, $uibModalInstance) {
            var c = RoomContext;

            this.room = {};
            this.doRoomLabel = '作成';

            this.close = function() {
                $uibModalInstance.dismiss();
            };

            this.doRoom = function() {
                return service.createRoom(this.room)
                    .then(function(room) {
                        $uibModalInstance.close(room);
                    }, function(res) {
                        console.log(res.data);
                    });
            };
        }])
    .controller('RoomMemberRemoveController', ['RoomContext', 'RoomAdminService', '$uibModalInstance',
        function(RoomContext, service, $uibModalInstance) {
            var c = RoomContext;

            this.icon = 'trash';
            this.members = (c.room.members || [])
                .filter(function(member) {
                    return !member.isAdmin;
                });

            this.close = function() {
                $uibModalInstance.dismiss();
            };

            this.doMember = function(idx) {
                var room = c.room;
                var member = this.members[idx];
                this.members.splice(idx, 1);
                return service.removeMember(room, member);
            };
        }])
    .controller('RoomMemberAddController', ['RoomContext', 'RoomAdminService', '$uibModalInstance',
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
                var member = this.members[idx];
                this.members.splice(idx, 1);
                return service.addMember(room, member);
            };

        }]);