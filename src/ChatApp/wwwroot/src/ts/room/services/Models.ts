class RoomModel {
    id?: string;
    name?: string;
    description?: string;
    messages?: Array<RoomMessageModel>;
    members?: Array<RoomMemberModel>;
    avatarId?: string;

    constructor(room?: RoomModel) {
        angular.extend(this, room);
        this.messages = this.messages || [];
        this.members = this.members || [];
    }

    avatarUrl() {
        return `../ChatRoomAvatar/Get/${this.avatarId}`;
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