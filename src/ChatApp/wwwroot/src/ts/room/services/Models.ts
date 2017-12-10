class RoomModel {
    id?: string;
    messages?: Array<RoomMessageModel>;
    members?: Array<RoomMemberModel>;

    constructor(room?: RoomModel) {
        angular.extend(this, room);
        this.messages = this.messages || [];
        this.members = this.members || [];
    }
}

class RoomMessageModel {
    id?: number;
    message?: string;

    isPosting?: boolean;
}

class RoomMemberModel {
    id?: string;
    isAdmin?: boolean;
}

export {
    RoomModel,
    RoomMessageModel,
    RoomMemberModel
}