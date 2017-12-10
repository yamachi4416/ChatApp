import { RoomAdminService } from "../../services/RoomAdminService";
import { RoomContext } from "../../services/RoomContext";
import { RoomServiceModel } from "../../services/RoomServiceModel";
import { ILocationService } from "angular";
import { IModalServiceInstance } from "angular-ui-bootstrap"

export class RemoveRoomController {
    constructor(
        private adminService: RoomAdminService,
        private $uibModalInstance: IModalServiceInstance,
        private $location: ILocationService,
        private roomContext: RoomContext,
        private room: RoomServiceModel
    ) {}

    ok() {
        this.adminService.removeRoom(this.room)
            .then((room) => {
                this.roomContext.removeRoom(room.id);
                this.$uibModalInstance.close();
                this.$location.hash("");
            });
    }

    close() {
        this.$uibModalInstance.dismiss();
    }

    static get id(): string {
        return "RemoveRoomController";
    }

    static get injector(): Array<any> {
        return [
            'RoomAdminService',
            '$uibModalInstance',
            '$location',
            'RoomContext',
            'room',
            RemoveRoomController
        ];
    }
}