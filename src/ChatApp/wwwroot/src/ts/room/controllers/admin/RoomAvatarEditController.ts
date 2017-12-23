import { RoomAdminService } from "../../services/RoomAdminService";
import { IModalServiceInstance } from "angular-ui-bootstrap";
import { RoomServiceModel } from "../../services/RoomServiceModel";

interface CliperCompornant {
    cliper: JQueryImageCliper,
    range: { min:number, val:number, max:number }
}

export class RoomAvatarEditController {
    public c: CliperCompornant;

    constructor(
        private adminService: RoomAdminService,
        private $uibModalInstance: IModalServiceInstance,
        private room: RoomServiceModel
    ) {}

    fileSelect() {
        this.c.cliper.openFileDialog()
            .then((files) => this.c.cliper.loadFile(files[0]));
    }

    changeRange() {
        const info = this.c.cliper.imageInfo();
        this.c.cliper.zoom(this.c.range.val - info.width);
    }

    close() {
        this.$uibModalInstance.dismiss();
    }

    disableUpload() {
        return !this.c.cliper.getSrc();
    }

    upload() {
        if (this.disableUpload())
            return;

        this.c.cliper.toBlob().then((blob) => {
            return this.adminService.uploadImage(this.room, blob)
                .then((avatarId) => this.room.avatarId = avatarId);
        }).fail(function () {
            console.log(arguments);
        }).always(() => this.close());
    }

    static get id(): string {
        return "RoomAvatarEditController";
    }

    static get injector(): Array<any> {
        return [
            'RoomAdminService',
            '$uibModalInstance',
            'room',
            RoomAvatarEditController
        ];
    }
}