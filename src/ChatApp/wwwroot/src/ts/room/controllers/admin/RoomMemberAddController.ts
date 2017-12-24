import { RoomAdminService } from "../../services/RoomAdminService";
import { RoomServiceModel } from "../../services/RoomServiceModel";
import { IModalServiceInstance } from "angular-ui-bootstrap"

export class RoomMemberAddController {
    public readonly icon = "plus";
    public members = [];
    public search = "";

    constructor(
        private adminService: RoomAdminService,
        private $uibModalInstance: IModalServiceInstance,
        private room: RoomServiceModel
    ) {}

    close() {
        return this.$uibModalInstance.dismiss();
    }

    searchAddMembers() {
        return this.adminService.searchAddMembers(this.room, this.search)
            .then((members) => {
                this.members = members;
            });
    }

    doMember(idx: number) {
        const member = this.members[idx];
        this.members.splice(idx, 1);
        return this.adminService.addMember(this.room, member);
    }

    static get id(): string {
        return "RoomMemberAddController";
    }

    static get injector(): Array<any> {
        return [
            'RoomAdminService',
            '$uibModalInstance',
            'room',
            RoomMemberAddController
        ];
    }
}
