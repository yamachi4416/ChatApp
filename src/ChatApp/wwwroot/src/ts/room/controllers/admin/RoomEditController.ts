import { RoomAdminService } from "../../services/RoomAdminService";
import { RoomServiceModel } from "../../services/RoomServiceModel";
import { IModalServiceInstance } from "angular-ui-bootstrap"
import { RoomModel } from "../../services/Models";

export class RoomEditController {
    public readonly room: RoomModel = new RoomModel();
    private readonly roomServiceModel: RoomServiceModel;

    constructor(
        private adminService: RoomAdminService,
        private $uibModalInstance: IModalServiceInstance,
        room: RoomServiceModel
    ) {
        this.roomServiceModel = room;
        this.room.id = this.roomServiceModel.id;
        this.room.name = this.roomServiceModel.name;
        this.room.description = this.roomServiceModel.description;
    }

    close() {
        return this.$uibModalInstance.dismiss();
    }

    doRoom(room) {
        return this.adminService.editRoom(this.roomServiceModel, this.room)
            .then((room) => {
                if (room && room.id) {
                    this.roomServiceModel.name = room.name;
                    this.roomServiceModel.description = room.description;
                    this.roomServiceModel.updatedDate = room.updatedDate;
                }
                this.$uibModalInstance.close();
            }, (res) => {
                console.log(res.data);
            });
    }

    static get id(): string {
        return "RoomEditController";
    }

    static get injector(): Array<any> {
        return [
            'RoomAdminService',
            '$uibModalInstance',
            'room',
            RoomEditController
        ];
    }
}
