(function angular_services(chatapp) {
    angular.module('ChatApp')
        .value('RoomClass', chatapp.objects.Room)
        .service('RoomService', ['$http', '$q', chatapp.services.RoomService])
        .service('RoomAdminService', ['$http', '$q', chatapp.services.RoomAdminService])
        .service('RoomContext', ['$rootScope', 'RoomService', 'RoomClass',
            function ($scope, RoomService, RoomClass) {
                return $scope.c = new chatapp.services.RoomContext(RoomService, RoomClass);
            }]);
}
(function chatapp_services(chatapp) {
    var services = chatapp.services = {};

    function httpServiceExtend(baseUrl, klass, prototype) {
        var $http, $q;

        function Klass(http, q) {
            $http = http;
            $q = q;
            klass && klass.apply(this, arguments);
        }

        angular.extend(Klass.prototype, {
            _defer: function() {
                return $q.defer();
            },
            _promise: function() {
                return this._defer().promise;
            },
            _reject: function() {
                var d = this._defer();
                d.reject();
                return d.promise.then(angular.noop, angular.noop);
            },
            _all: function() {
                return $q.all(arguments);
            },
            _httpGet: function(path, config) {
                var config = angular.extend({}, config);
                return $http.get(baseUrl + path, config)
                    .then(function (res) {
                        return res.data;
                    });
            },
            _httpPost: function(path, data, config) {
                var config = angular.extend({}, config);
                return $http.post(baseUrl + path, data, config)
                    .then(function(res) {
                        return res.data;
                    });
            }
        }, prototype);

        return Klass;
    }

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
                if (!this.room) return this._service._reject();
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
            removeRoom: function(roomid) {
                var r = this.getRoom(roomid);
                if (r) {
                    var rooms = this.rooms;
                    delete this.roomIdMap[roomid];
                    rooms.splice(rooms.indexOf(r), 1);
                    angular.forEach(r, function(v, k) {
                        delete r[k];
                    });
                }
                
                return this;
            },
            getRoom: function(roomid) {
                return this.roomIdMap[roomid];
            },
            getRoomOrFirst: function (roomid) {
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

    services.RoomAdminService = (function() {
        return httpServiceExtend('/api/rooms/admin/', function() {

        }, {
            _request: function() {
                var args = [].slice.call(arguments),
                    method = args.shift(),
                    room = args.shift();
                
                if (!room || !room.isAdmin)
                    return this._reject();
                
                args.unshift(room.id + '/' + args.shift());
                return this['_http' + method].apply(this, args);
            },
            searchAddMembers: function(room, search) {
                return this._request('Get', room, 'members/search/' + search);
            },
            addMember: function(room, member) {
                return this._request('Post', room, 'members/add', member)
                    .then(room.addMember.bind(room));
            },
            removeMember: function(room, member) {
                return (!member || member.isAdmin)
                    ? this._reject()
                    : this._request('Post', room, 'members/remove', member)
                        .then(room.removeMember.bind(room));
            },
            editRoom: function(room, editroom) {
                return this._request('Post', room, 'rooms/edit', editroom);
            },
            removeRoom: function(room) {
                return this._request('Post', room, 'rooms/remove');
            }
        })
    }());

    services.RoomService = (function() {
        return httpServiceExtend('/api/rooms/', function() {
        }, {
            getJoinRooms: function () {
                return this._httpGet('joins');
            },
            getMembers: function (room) {
                return this._httpGet('members/' + room.id);
            },
            getNewMessages: function (room) {
                var url = 'messages/' + room.id + '/new';
                if (room.messages && room.messages.length) {
                    url += '/' + room.messages[room.messages.length - 1].id;
                }
                return this._httpGet(url);
            },
            getRoomContents: function (room) {
                return this._all(this.getMembers(room), this.getNewMessages(room))
                    .then(function (d) {
                        return room.setMembers(d[0]).addNewMessages(d[1]);
                    });
            },
            getOldMessages: function (room) {
                if (room.messages && room.messages[0]) {
                    var url = 'messages/' + room.id + '/old/';
                    url += room.messages[0].id;
                    return this._httpGet(url)
                        .then(function (messages) {
                            if (!messages || !messages.length) {
                                return this._reject();
                            }
                            return messages;
                        }.bind(this));
                }
                return this._reject();
            },
            createRoom: function(room) {
                return this._httpPost('rooms/create', room);
            },
            postMessage: function(room, message) {
                return this._httpPost('messages/' + room.id + '/create', message);
            }
        });
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
                    return this._service._reject();
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
                    return this._service._reject();
                this.nowfetchNewMessage = true;
                return this._service.getNewMessages(this)
                    .then(function(messages) {
                        this.addNewMessages(messages);
                    }.bind(this)).finally(function() {
                        this.nowfetchNewMessage = false;
                    }.bind(this));
            },
            addMessage: function(message, push) {
                if (!message) return this;

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
                this._membersMap = {};
                this.members = [];
                angular.forEach(members, function(member) {
                    this.addMember(member);
                }.bind(this));
                return this;
            },
            addMember: function(member) {
                if (!member) return this;
                if (this._membersMap[member.id]) return;
                this.members.push(member);
                this._membersMap[member.id] = member;
                return this;
            },
            removeMember: function(member) {
                if (!member) return this;
                var members = this.members;
                for (var i = 0, l = members.length; i < l; i++) {
                    var m = members[i];
                    if (m.id == member.id) {
                        delete this._membersMap[m.id];
                        members.splice(i, 1);
                        break;
                    }
                }
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