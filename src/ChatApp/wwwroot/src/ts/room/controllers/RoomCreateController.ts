import { RoomHttpService } from "../services/RoomHttpService";
import { IModalServiceInstance } from "angular-ui-bootstrap";
import { RoomModel } from "../services/Models";

export class RoomCreateController {
    public room: RoomModel = new RoomModel();

    constructor(
        private service: RoomHttpService,
        private $uibModalInstance: IModalServiceInstance
    ) { }

    close() {
        this.$uibModalInstance.dismiss();
    }

    doRoom() {
        return this.service.createRoom(this.room)
            .then(room => this.$uibModalInstance.close(room), res => console.log(res.data));
    }

    static get id(): string {
        return "RoomCreateController";
    }

    static get injector(): Array<any> {
        return [
            "RoomHttpService",
            "$uibModalInstance",
            RoomCreateController
        ];
    }
}