import { RoomModel, RoomMemberModel } from "./Models";
import { RoomServiceModel } from "./RoomServiceModel";
import { HttpServiceBase, IRoomPromise } from "./HttpServiceBase";
import { IPromise } from "angular";

class RoomAdminService extends HttpServiceBase {
    constructor($http, $q) {
        super($http, $q);
        this.baseUrl = "../api/rooms/admin/";
    }

    searchAddMembers(room: RoomServiceModel, search: string): IPromise<RoomMemberModel[]> {
        return this.httpGet(`${room.id}/members/search/${search}`);
    }

    addMember(room: RoomServiceModel, member: RoomMemberModel) {
        return this.httpPost(`${room.id}/members/add`, member)
            .then((member) => room.addMember(member));
    }

    removeMember(room: RoomServiceModel, member: RoomMemberModel) {
        return (!member || member.isAdmin)
            ? this.reject
            : this.httpPost(`${room.id}/members/remove`, member)
                .then((member) => room.removeMember(member));
    }

    editRoom(room: RoomModel, editroom: RoomModel) {
        return this.httpPost(`${room.id}/rooms/edit`, editroom);
    }

    removeRoom(room: RoomModel): IRoomPromise<RoomModel> {
        return this.httpPost(`${room.id}/rooms/remove`);
    }

    uploadImage(room: RoomModel, blob: Blob) {
        const formData = new FormData();
        formData.append('ImageFile', new File([blob], 'RoomAvatar.png', { type: blob.type }));
        return this.httpPost(`${room.id}/avatars/upload`, formData, {
            headers: { 'Content-Type': undefined },
            transformRequest: function(id) { return id; }
        });
    }
}

export {
    RoomAdminService
}
