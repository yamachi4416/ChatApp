import { RemoveRoomController } from "./controllers/admin/RemoveRoomController"
import { RoomMemberAddController } from "./controllers/admin/RoomMemberAddController"
import { RoomMemberRemoveController } from "./controllers/admin/RoomMemberRemoveController"
import { RoomEditController } from "./controllers/admin/RoomEditController"
import { RoomAvatarEditController } from "./controllers/admin/RoomAvatarEditController"
import { RoomDetailController } from "./controllers/RoomDetailController"

angular.module('ChatApp')
    .controller(RemoveRoomController.id, RemoveRoomController.injector)
    .controller(RoomMemberAddController.id, RoomMemberAddController.injector)
    .controller(RoomMemberRemoveController.id, RoomMemberRemoveController.injector)
    .controller(RoomEditController.id, RoomEditController.injector)
    .controller(RoomAvatarEditController.id, RoomAvatarEditController.injector)
    .controller(RoomDetailController.id, RoomDetailController.injector)
    .controller('RoomController', ['RoomContext', '$timeout', '$location', '$rootScope', '$uibModal', '$window', '$document',
        function (RoomContext, $timeout, $location, $rootScope, $uibModal, $window, $document) {
            var c = RoomContext;
            var ws = c.ws;

            ws.on(ws.types.CREATE_MESSAGE, function (res) {
                var room = c.getRoom(res.roomId);
                if (!room) return;

                if (c.room === room) {
                    if ($document[0].hidden) {
                        $timeout(function () {
                            room.unReadMessageCount += 1;
                        });
                    }
                    fetchNewMessages(room, true);
                } else {
                    $timeout(function () {
                        room.unReadMessageCount += 1;
                    });
                }
            }).on(ws.types.CREATE_MEMBER, function (res) {
                var room = c.getRoom(res.roomId);
                if (!room) return;

                $timeout(function () {
                    if (c.room === room) {
                        room.addMember(res.message);
                    } else {

                    }
                });
            }).on(ws.types.DELETE_MEMBER, function (res) {
                var room = c.getRoom(res.roomId);
                if (!room) return;

                $timeout(function () {
                    if (c.room === room) {
                        room.removeMember(res.message);
                    } else {

                    }
                });
            }).on(ws.types.DEFECT_ROOM, function (res) {
                $timeout(function () {
                    c.removeRoom(res.roomId);
                });
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
                });
            }).on(ws.types.MODIFY_ROOM_AVATAR, function (res) {
                var room = c.getRoom(res.roomId);
                if (!room) return;
                $timeout(function () {
                    room.avatarId = res.message.avatarId;
                });
            }.bind(this));

            angular.element($window).on('focus', function () {
                ws.connect();
                if (c.room) {
                    c.room.unReadMessageCount = 0;
                    c.room.fetchNewMessages();
                }
            });

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

                room.unReadMessageCount = 0;
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

            function openModalUi(options, resolve?, reject?) {
                var modalInstance = $uibModal.open(
                    angular.extend({
                    }, options));

                modalInstance.result.then(
                    resolve || angular.noop,
                    reject = reject || angular.noop
                );

                return modalInstance;
            }

            function openAdminModalUi(...options) {
                if (!c.room.isAdmin) return;

                return openModalUi.apply(null, arguments);
            }

            this.OpenAddMember = function () {
                openAdminModalUi({
                    templateUrl: '/modal/member/add.tmpl.html',
                    controller: RoomMemberAddController.id,
                    controllerAs: 'ctrl',
                    resolve: { room: c.room }
                });
            };

            this.OpenRemoveMember = function () {
                openAdminModalUi({
                    templateUrl: '/modal/member/remove.tmpl.html',
                    controller: RoomMemberRemoveController.id,
                    controllerAs: 'ctrl',
                    resolve: { room: c.room }
                });
            };

            this.OpenCreateRoom = function () {
                openModalUi({
                    templateUrl: '/modal/room/create.tmpl.html',
                    controller: 'RoomCreateController',
                    controllerAs: 'ctrl'
                }, function (room) {
                    c.addRoom(room);
                    this.SelectRoom(room);
                }.bind(this));
            };

            this.OpenEditRoom = function (room) {
                openAdminModalUi({
                    templateUrl: '/modal/room/edit.tmpl.html',
                    controller: RoomEditController.id,
                    controllerAs: 'ctrl',
                    resolve: { room: room }
                });
            };

            this.OpenDetailRoom = function (room) {
                openModalUi({
                    templateUrl: '/modal/room/detail.tmpl.html',
                    controller: RoomDetailController.id,
                    controllerAs: 'ctrl',
                    resolve: { room: room }
                });
            };

            this.OpenRemoveRoom = function (room) {
                openAdminModalUi({
                    size: 'sm',
                    templateUrl: '/modal/room/delete-confirm.tmpl.html',
                    controller: RemoveRoomController.id,
                    controllerAs: 'ctrl',
                    resolve: { room: room }
                });
            };

            this.OpenRoomImageEditDialog = function (room) {
                openAdminModalUi({
                    size: 'sm',
                    templateUrl: '/modal/room/edit-image.tmpl.html',
                    controller: RoomAvatarEditController.id,
                    controllerAs: 'ctrl',
                    resolve: { room: room }
                });
            };

            this.InitRooms();
        }])
    .controller('RoomCreateController', ['RoomHttpService', '$uibModalInstance',
        function (service, $uibModalInstance) {
            this.room = {};

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
        }]);
