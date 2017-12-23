import { RemoveRoomController } from "./controllers/admin/RemoveRoomController"
import { RoomMemberAddController } from "./controllers/admin/RoomMemberAddController"
import { RoomMemberRemoveController } from "./controllers/admin/RoomMemberRemoveController"
import { RoomEditController } from "./controllers/admin/RoomEditController"
import { RoomAvatarEditController } from "./controllers/admin/RoomAvatarEditController"
import { RoomDetailController } from "./controllers/RoomDetailController"
import { RoomCreateController } from "./controllers/RoomCreateController"
import { RoomController } from "./controllers/RoomController";

angular.module('ChatApp')
    .controller(RemoveRoomController.id, RemoveRoomController.injector)
    .controller(RoomMemberAddController.id, RoomMemberAddController.injector)
    .controller(RoomMemberRemoveController.id, RoomMemberRemoveController.injector)
    .controller(RoomEditController.id, RoomEditController.injector)
    .controller(RoomAvatarEditController.id, RoomAvatarEditController.injector)
    .controller(RoomDetailController.id, RoomDetailController.injector)
    .controller(RoomCreateController.id, RoomCreateController.injector)
    .controller(RoomController.id, RoomController.injector);