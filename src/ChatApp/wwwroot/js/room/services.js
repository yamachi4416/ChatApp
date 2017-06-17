var ChatApp = ChatApp || {};

ChatApp.Objects = ChatApp.Objects || {};

ChatApp.Objects.Room = (function() {
    function Room(room, service) {
        angular.extend(this, room);
        this.messages = this.messages || [];
        this.members = this.members || [];
        this._messageMap = {};
        this._membersMap = {};
        this._service = service;
    }

    angular.forEach({
        fetchOldMessages: function() {
            if (!this.hasOldMessages) return;
            this.hasOldMessages = false;
            return this._service.getOldMessages(this)
                    .then(function (messages) {
                        this.hasOldMessages = true;
                        this.addOldMessages(messages);
                    }.bind(this));
        },
        addMessage: function(message, push) {
            if (!this._messageMap[message.id]) {
                this._membersMap[message.id] = message;
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
        }
    }, function(p, k) {
        Room.prototype[k] = p;
    });

    return Room;
})();

ChatApp.Services = ChatApp.Services || {};

ChatApp.Services.RoomContainer = (function() {

    function RoomContainer(service) {
        this.roomIdMap = {};
        this.rooms = [];
        this.room = null;
        this._service = service;
    }

    angular.forEach({
        fetchJoinRooms: function() {
            return this._service.getJoinRooms()
                .then(function(rooms) {
                    this.addRooms(rooms);
                    return this.rooms;
                }.bind(this));    
        },
        addRoom: function(room) {
            if (room && room.id && !this.roomIdMap[room.id]) {
                room = new ChatApp.Objects.Room(room, this._service);
                this.roomIdMap[room.id] = room;
                this.rooms.push(room);
            }
        },
        addRooms: function(rooms) {
            angular.forEach(rooms, function(room) {
                this.addRoom(room)
            }, this);
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
                return this.room = room;
            }
        }
    }, function(p, k) {
        RoomContainer.prototype[k] = p;
    });

    return RoomContainer;
})();

ChatApp.Services.RoomService = (function() {
    var baseUrl = '/api/rooms';
    var $http, $q;

    function RoomService(http, q) {
        $http = $http || http;
        $q = $q || q;
    }

    angular.forEach({
        getJoinRooms: function () {
            return httpGet('/');
        },
        getMembers: function (room) {
            return httpGet('/members/' + room.id);
        },
        getNewMessages: function (room) {
            var url = '/messages/' + room.id + '/new';
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
                var url = '/messages/' + room.id + '/old/';
                url += room.messages[0].id;
                return httpGet(url)
                    .then(function (messages) {
                        if (!messages || !messages.length) {
                            return reject();
                        }
                        return messages;
                    });
            }
            return reject();
        }
    }, function(p, k) {
        RoomService.prototype[k] = p;
    });

    return RoomService;

    function reject() {
        var d = $q.defer();
        d.reject();
        return d.promise.then(angular.noop, angular.noop);
    };

    function httpGet(path, config) {
        var d = $q.defer();
        var config = angular.extend({
        }, config || {});
        $http.get(baseUrl + path, config)
            .then(function (res) {
                d.resolve(res.data);
            }, function (res) {
                window.open().document.write(res.data);
                d.reject();
            });
        return d.promise;
    };
})();

angular.module('ChatApp')
    .service('RoomService', ['$http', '$q', ChatApp.Services.RoomService])
    .service('RoomContainer', ['$rootScope', 'RoomService', function ($scope, RoomService) {
        return $scope.c = new ChatApp.Services.RoomContainer(RoomService);
    }]);