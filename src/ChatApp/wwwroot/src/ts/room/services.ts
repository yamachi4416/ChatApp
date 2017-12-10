import { WSMessageTypes, WebSocketService } from "./services/WebSocketService"
import { RoomHttpService } from "./services/RoomHttpService";
import { RoomServiceModel } from "./services/RoomServiceModel";
import { RoomModel } from "./services/Models";
import { RoomContext } from "./services/RoomContext";
import { RoomAdminService } from "./services/RoomAdminService";

angular.module('ChatApp')
    .value('WSMessageTypes', WSMessageTypes)
    .factory('WebSocket', [function () {
        return function(url: string) {
            return new WebSocket(url)
        }
    }])
    .service('RoomHttpService', ['$http', '$q', RoomHttpService])
    .service('RoomWSService', ['WebSocket', '$window', WebSocketService])
    .factory('RoomModelFactory', ['RoomHttpService', function (service: RoomHttpService) {
        return function (room: RoomModel) {
            return new RoomServiceModel(room, service)
        };
    }])
    .service('RoomAdminService', ['$http', '$q', RoomAdminService])
    .service('RoomContext', ['$rootScope', 'RoomWSService', 'RoomHttpService', 'RoomModelFactory',
        function ($scope, RoomWSService, RoomHttpService, RoomModelFactory) {
            const context = new RoomContext(RoomWSService, RoomHttpService, RoomModelFactory);
            return $scope.c = context;
        }
    ]);