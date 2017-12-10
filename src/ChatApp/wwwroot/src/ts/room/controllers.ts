import { RemoveRoomController } from "./controllers/admin/RemoveRoomController"

angular.module('ChatApp')
    .controller(RemoveRoomController.id, RemoveRoomController.injector)
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
                    controller: 'RoomMemberAddController',
                    controllerAs: 'ctrl',
                    resolve: {
                        room: c.room
                    }
                });
            };

            this.OpenRemoveMember = function () {
                openAdminModalUi({
                    templateUrl: '/modal/member/remove.tmpl.html',
                    controller: 'RoomMemberRemoveController',
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
                    templateUrl: '/modal/room/detail.tmpl.html',
                    controller: 'RoomDetailController',
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
                    templateUrl: '/modal/room/edit-image.tmpl.html',
                    controller: ['RoomAdminService', '$uibModalInstance',
                        function (service, $uibModalInstance) {

                            function updateRange(range, info) {
                                $timeout(function () {
                                    range.max = info.maxWidth;
                                    range.min = info.minWidth;
                                    $timeout(function () { range.val = info.width; });
                                });
                            }

                            this.fileSelect = function () {
                                var cliper = this.c.cliper;
                                cliper.openFileDialog()
                                    .then(function (files) {
                                        cliper.loadFile(files[0]).then(function (info) {
                                            updateRange(this.c.range, info);
                                        }.bind(this));
                                    }.bind(this));
                            };

                            this.changeRange = function () {
                                var info = this.c.cliper.imageInfo();
                                var range = this.c.range;
                                this.c.cliper.zoom(range.val - info.width);
                            };

                            this.close = function () {
                                $uibModalInstance.dismiss();
                            };

                            this.disableUpload = function () {
                                return !this.c.cliper.getSrc();
                            };

                            this.upload = function () {
                                if (this.disableUpload())
                                    return;

                                var cliper = this.c.cliper;
                                cliper.toBlob().then(function (blob) {
                                    return service.uploadImage(room, blob)
                                        .then(function (avatarId) {
                                            room.avatarId = avatarId;
                                        });
                                }).fail(function () {
                                    console.log(arguments);
                                }).always(function () {
                                    $uibModalInstance.dismiss();
                                });
                            };
                        }],
                    size: 'sm',
                    controllerAs: 'ctrl'
                });
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
