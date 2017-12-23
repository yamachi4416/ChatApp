import { IModalServiceInstance } from "angular-ui-bootstrap";
import { RoomServiceModel } from "../services/RoomServiceModel";

export class RoomDetailController {
    constructor(
        private $uibModalInstance: IModalServiceInstance,
        private room: RoomServiceModel
    ) {}

    close() {
        this.$uibModalInstance.dismiss();
    }

    static get id(): string {
        return "RoomDetailController";
    }

    static get injector(): Array<any> {
        return [
            "$uibModalInstance",
            "room",
            RoomDetailController
        ];
    }
}
