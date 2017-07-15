angular.module('ChatApp')
    .controller('RoomController', ['RoomContext', '$timeout', '$location', '$rootScope', '$uibModal', '$window',
        function (RoomContext, $timeout, $location, $rootScope, $uibModal, $window) {
            var c = RoomContext;
            var ws = c.ws;

            ws.on(ws.types.CREATE_MESSAGE, function (res) {
                var room = c.getRoom(res.roomId);
                if (!room) return;

                if (c.room === room) {
                    fetchNewMessages(room, true);
                } else {

                }
            }.bind(this)).on(ws.types.CREATE_MEMBER, function (res) {
                var room = c.getRoom(res.roomId);
                if (!room) return;

                $timeout(function () {
                    if (c.room === room) {
                        room.addMember(res.message);
                    } else {

                    }
                }.bind(this));
            }.bind(this)).on(ws.types.DELETE_MEMBER, function (res) {
                var room = c.getRoom(res.roomId);
                if (!room) return;

                $timeout(function () {
                    if (c.room === room) {
                        room.removeMember(res.message);
                    } else {

                    }
                }.bind(this));
            }.bind(this)).on(ws.types.DEFECT_ROOM, function (res) {
                $timeout(function () {
                    c.removeRoom(res.roomId);
                }.bind(this));
            }).on(ws.types.JOIN_ROOM, function (res) {
                var len = (c.rooms || []).length;
                $timeout(function () {
                    c.addRoom(res.message);
                    if (len == 0) {
                        this.SelectRoom(c.getRoomOrFirst(res.roomId));
                    }
                }.bind(this));
            }.bind(this)).on(ws.types.DELETE_ROOM, function (res) {
                $timeout(function () {
                    c.removeRoom(res.roomId);
                }.bind(this));
            });

            $($window).on('focus', function() {
                ws.connect();
                if (c.room) {
                    fetchNewMessages(c.room, false);
                }
            }.bind(this));

            this.InitRooms = function () {
                return c.fetchJoinRooms()
                    .then(function (rooms) {
                        this.SelectRoom(c.getRoomOrFirst($location.hash()));
                        $rootScope.$broadcast('chatRoomMessageReady');
                        ws.connect();
                    }.bind(this));
            };

            this.SelectRoom = function (room) {
                if (room == null) {
                    return $location.hash(null);
                }
                if (c.room == room) return;
                return $location.hash(room.id);
            };

            this.ChangeRoom = function () {
                var roomid = $location.hash();
                if (roomid) {
                    return c.selectRoom(roomid).fetchRoomContents();
                }
                var room = c.getRoomOrFirst(roomid);
                if (room) {
                    this.SelectRoom(room);
                }
            };

            this.GetOldmessages = function () {
                return c.room && c.room.fetchOldMessages();
            };

            this.PostMessage = function () {
                if (c.room) {
                    var room = c.room;
                    var promise = room.postMessage();
                    return ws.isEnable() ?
                        promise :
                        promise.then(function () {
                            return fetchNewMessages(room, true);
                        });
                }
            };

            function fetchNewMessages(room, ifShowBottom) {
                return room
                    .fetchNewMessages()
                    .then(function () {
                        if (c.room && c.room.id === room.id) {
                            $rootScope.$broadcast('chatRoomMessageScrollBottom', {
                                ifShowBottom: !!ifShowBottom
                            });
                        }
                    });
            }

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

            this.OpenAddMember = function () {
                openAdminModalUi({
                    templateUrl: '/templates/room/modal-members-add.html',
                    controller: 'RoomMemberAddController',
                    controllerAs: 'ctrl',
                    resolve: {
                        room: c.room
                    }
                });
            };

            this.OpenRemoveMember = function () {
                openAdminModalUi({
                    templateUrl: '/templates/room/modal-members-remove.html',
                    controller: 'RoomMemberRemoveController',
                    controllerAs: 'ctrl',
                    resolve: {
                        room: c.room
                    }
                });
            };

            this.OpenCreateRoom = function () {
                openModalUi({
                    templateUrl: '/templates/room/modal-room-create.html',
                    controller: 'RoomCreateController',
                    controllerAs: 'ctrl'
                }, function (room) {
                    c.addRoom(room);
                    this.SelectRoom(room);
                }.bind(this));
            };

            this.OpenEditRoom = function (room) {
                openAdminModalUi({
                    templateUrl: '/templates/room/modal-room-edit.html',
                    controller: 'RoomEditController',
                    controllerAs: 'ctrl',
                    resolve: {
                        room: room
                    }
                }, function (room) {
                    if (room && room.id) {
                        angular.extend(c.getRoom(room.id), {
                            name: room.name,
                            description: room.description,
                            updatedDate: room.updatedDate
                        });
                    }
                }.bind(this));
            };

            this.OpenDetailRoom = function (room) {
                openModalUi({
                    templateUrl: '/templates/room/modal-room-detail.html',
                    controller: 'RoomDetailController',
                    controllerAs: 'ctrl',
                    resolve: {
                        room: room
                    }
                });
            };

            this.OpenRemoveRoom = function (room) {
                openAdminModalUi({
                    templateUrl: '/templates/room/modal-confirm.html',
                    controller: ['RoomAdminService', '$uibModalInstance',
                        function (service, $uibModalInstance) {
                            this.title = '本当に"' + room.name + '"を削除しますか？';
                            this.close = function () {
                                $uibModalInstance.dismiss();
                            };
                            this.ok = function () {
                                service.removeRoom(room)
                                    .then(function (room) {
                                        $uibModalInstance.close(room && room.id);
                                    });
                            };
                        }],
                    size: 'sm',
                    controllerAs: 'ctrl'
                }, function (roomid) {
                    c.removeRoom(roomid);
                    $location.hash('');
                }.bind(this));
            };

            this.InitRooms();
        }])
    .controller('RoomDetailController', ['$uibModalInstance', 'room',
        function ($uibModalInstance, room) {
            this.room = room;

            this.close = function () {
                $uibModalInstance.dismiss();
            };
        }])
    .controller('RoomEditController', ['RoomAdminService', '$uibModalInstance', 'room',
        function (service, $uibModalInstance, room) {
            var _room = room;
            this.room = {
                id: _room.id,
                name: _room.name,
                description: _room.description
            };

            this.doRoomLabel = '変更';

            this.close = function () {
                $uibModalInstance.dismiss();
            };

            this.doRoom = function (room) {
                return service.editRoom(_room, this.room)
                    .then(function (room) {
                        $uibModalInstance.close(room);
                    }, function (res) {
                        console.log(res.data);
                    });
            };
        }
    ])
    .controller('RoomCreateController', ['RoomContext', 'RoomService', '$uibModalInstance',
        function (RoomContext, service, $uibModalInstance) {
            var c = RoomContext;

            this.room = {};
            this.doRoomLabel = '作成';

            this.close = function () {
                $uibModalInstance.dismiss();
            };

            this.doRoom = function () {
                return service.createRoom(this.room)
                    .then(function (room) {
                        $uibModalInstance.close(room);
                    }, function (res) {
                        console.log(res.data);
                    });
            };
        }])
    .controller('RoomMemberRemoveController', ['RoomAdminService', '$uibModalInstance', 'room',
        function (service, $uibModalInstance, room) {
            this.icon = 'trash';
            this.members = (room.members || [])
                .filter(function (member) {
                    return !member.isAdmin;
                });

            this.close = function () {
                $uibModalInstance.dismiss();
            };

            this.doMember = function (idx) {
                var member = this.members[idx];
                this.members.splice(idx, 1);
                return service.removeMember(room, member);
            };
        }])
    .controller('RoomMemberAddController', ['RoomAdminService', '$uibModalInstance', 'room',
        function (service, $uibModalInstance, room) {
            this.icon = 'plus';
            this.members = [];
            this.search = '';

            this.close = function () {
                $uibModalInstance.dismiss();
            };

            this.searchAddMembers = function () {
                return service.searchAddMembers(room, this.search)
                    .then(function (members) {
                        this.members = members;
                    }.bind(this));
            };

            this.doMember = function (idx) {
                var member = this.members[idx];
                this.members.splice(idx, 1);
                return service.addMember(room, member);
            };

        }]);
