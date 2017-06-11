angular.module('ChatApp', ['infinite-scroll']);

angular.module('ChatApp')
    .service('RoomContainer', [function () {
        var container = {};
        return container;
    }])
    .service('RoomService', ['$http', '$q', function ($http, $q) {
        var service = {
            getJoinRooms: function () {
                return httpGet('/');
            },
            getMembers: function (room) {
                return httpGet('/members/' + room.id);
            },
            getNewMessages: function (room) {
                var url = '/messages/' + room.id + '/new';
                if (room.messges && room.messages[0]) {
                    url += '/' + room.messages[0].id;
                }
                return httpGet(url);
            },
            getOldMessages: function (room) {
                if (room.messages && room.messages.length) {
                    var url = '/messages/' + room.id + '/old/';
                    url += room.messages[room.messages.length - 1].id;
                    return httpGet(url);
                }

                return reject();
            }
        };

        var reject = function () {
            var d = $q.defer();
            d.reject();
            return d.promise.then(function () { }, function () { });
        };

        var httpGet = function (path, config) {
            var d = $q.defer();
            var config = angular.extend({
            }, config || {});
            $http.get('/api/rooms' + path, config)
                .then(function (res) {
                    d.resolve(res.data);
                }, function (res) {
                    console.log(res);
                    window.open().document.write(res.data);
                });
            return d.promise;
        };

        return service;
    }]);