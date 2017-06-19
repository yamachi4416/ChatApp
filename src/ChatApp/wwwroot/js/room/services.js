(function angular_services(chatapp) {
    angular.module('ChatApp')
        .value('RoomClass', chatapp.objects.Room)
        .service('RoomService', ['$http', '$q', chatapp.services.RoomService])
        .service('RoomContext', ['$rootScope', 'RoomService', 'RoomClass',
            function ($scope, RoomService, RoomClass) {
                return $scope.c = new chatapp.services.RoomContext(RoomService, RoomClass);
            }]);
}
(function chatapp_services(chatapp) {
    var services = chatapp.services = {};

    services.RoomContext = (function() {

        function RoomContext(service, roomClass) {
            this.roomIdMap = {};
            this.rooms = [];
            this.room = null;
            this._service = service;
            this._roomClass = roomClass;
        }

        angular.extend(RoomContext.prototype, {
            fetchJoinRooms: function() {
                return this._service.getJoinRooms()
                    .then(function(rooms) {
                        this.addRooms(rooms);
                        return this.rooms;
                    }.bind(this));    
            },
            fetchRoomContents: function() {
                if (!this.room) return this._service.reject();
                return this._service.getRoomContents(this.room);
            },
            addRoom: function(room) {
                if (room && room.id && !this.roomIdMap[room.id]) {
                    room = new this._roomClass(room, this._service);
                    this.roomIdMap[room.id] = room;
                    this.rooms.push(room);
                }
                return this;
            },
            addRooms: function(rooms) {
                angular.forEach(rooms, function(room) {
                    this.addRoom(room)
                }, this);

                return this;
            },
            getRoom: function(roomid) {
                return this.roomIdMap[roomid];
            },
            getRoomOrFirst(roomid) {
                return this.getRoom(roomid) || this.rooms[0];
            },
            selectRoom: function(roomid) {
                var room = this.getRoom(roomid);
                if (room) {
                    this.room = room;
                }
                return this;
            }
        });

        return RoomContext;
    })();

    services.RoomService = (function() {
        var baseUrl = '/api/rooms/';
        var $http, $q;

        function RoomService(http, q) {
            $http = $http || http;
            $q = $q || q;
        }

        angular.extend(RoomService.prototype, {
            getJoinRooms: function () {
                return httpGet('joins');
            },
            getMembers: function (room) {
                return httpGet('members/' + room.id);
            },
            getNewMessages: function (room) {
                var url = 'messages/' + room.id + '/new';
                if (room.messages && room.messages.length) {
                    url += '/' + room.messages[room.messages.length - 1].id;
                }
                return httpGet(url);
            },
            getRoomContents: function (room) {
                return $q.all([this.getMembers(room), this.getNewMessages(room)])
                    .then(function (d) {
                        return room.setMembers(d[0]).addNewMessages(d[1]);
                    });
            },
            getOldMessages: function (room) {
                if (room.messages && room.messages[0]) {
                    var url = 'messages/' + room.id + '/old/';
                    url += room.messages[0].id;
                    return httpGet(url)
                        .then(function (messages) {
                            if (!messages || !messages.length) {
                                return this.reject();
                            }
                            return messages;
                        }.bind(this));
                }
                return this.reject();
            },
            postMessage: function(room, message) {
                return httpPost('messages/' + room.id + '/create', message);
            },
            reject: function() {
                var d = $q.defer();
                d.reject();
                return d.promise.then(angular.noop, angular.noop);
            }
        });

        return RoomService;

        function httpGet(path, config) {
            var config = angular.extend({}, config);
            return $http.get(baseUrl + path, config)
                .then(function (res) {
                    return res.data;
                });
        };

        function httpPost(path, data, config) {
            var config = angular.extend({}, config);
            console.log(data);
            return $http.post(baseUrl + path, data, config)
                .then(function(res) {
                    return res.data;
                });
        }
    })();

    return chatapp;
}
(function chatapp_objects(chatapp) {
    var objects = chatapp.objects = {};

    objects.Room = (function() {
        function Room(room, service) {
            angular.extend(this, room);
            this.messages = this.messages || [];
            this.members = this.members || [];
            this._messageMap = {};
            this._membersMap = {};
            this._service = service;
            this.message = {};
        }

        angular.forEach({
            fetchOldMessages: function() {
                if (!this.hasOldMessages)
                    return this._service.reject();
                this.hasOldMessages = false;
                return this._service.getOldMessages(this)
                        .then(function (messages) {
                            if (messages && messages.length) {
                                this.hasOldMessages = true;
                                this.addOldMessages(messages);
                            }
                        }.bind(this));
            },
            fetchNewMessages: function() {
                if (this.nowfetchNewMessage)
                    return this._service.reject();
                this.nowfetchNewMessage = true;
                return this._service.getNewMessages(this)
                    .then(function(messages) {
                        this.addNewMessages(messages);
                    }.bind(this)).finally(function() {
                        this.nowfetchNewMessage = false;
                    }.bind(this));
            },
            addMessage: function(message, push) {
                if (!this._messageMap[message.id]) {
                    this._messageMap[message.id] = message;
                    push ?
                        this.messages.push(message) :
                        this.messages.unshift(message);
                }

                if (angular.isUndefined(this.hasOldMessages)) {
                    this.hasOldMessages = this.messages.length > 0;
                }

                return this;
            },
            addMessages: function(messages, push) {
                angular.forEach(messages, function(message) {
                    this.addMessage(message, push);
                }, this);

                return this;
            },
            addOldMessages: function(messages) {
                return this.addMessages(messages, false);
            },
            addNewMessages: function(messages) {
                return this.addMessages(messages, true);
            },
            setMembers: function(members) {
                this.members = members;
                return this;
            },
            postMessage: function() {
                var message = this.message;
                message.isPosting = true;
                return this._service.postMessage(this, {
                    message: message.message
                }).then(function(message) {
                    console.log(arguments);
                    this.message = {};
                    //this.addMessage(message, true);
                }.bind(this), function() {
                    console.log('E');
                    console.log(arguments);
                }.bind(this)).finally(function() {
                    message.isPosting = false;
                });
            }
        }, function(p, k) {
            Room.prototype[k] = p;
        });

        return Room;
    })();

    return chatapp;
}(window.chatapp = window.chatapp || {}))));