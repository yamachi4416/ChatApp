import { RoomAdminService } from "../../services/RoomAdminService";
import { RoomServiceModel } from "../../services/RoomServiceModel";
import { IModalServiceInstance } from "angular-ui-bootstrap"
import { RoomMemberModel } from "../../services/Models";

export class RoomMemberRemoveController {
    public readonly icon = "trash";
    public readonly members: RoomMemberModel[];

    constructor(
        private adminService: RoomAdminService,
        private $uibModalInstance: IModalServiceInstance,
        private room: RoomServiceModel
    ) {
        this.members = (room.members || [])
            .filter((member) => !member.isAdmin);
    }

    close() {
        return this.$uibModalInstance.dismiss();
    }

    doMember(idx: number) {
        const member = this.members[idx];
        this.members.splice(idx, 1);
        return this.adminService.removeMember(this.room, member);
    }

    static get id(): string {
        return "RoomMemberRemoveController";
    }

    static get injector(): Array<any> {
        return [
            'RoomAdminService',
            '$uibModalInstance',
            'room',
            RoomMemberRemoveController
        ];
    }
}
