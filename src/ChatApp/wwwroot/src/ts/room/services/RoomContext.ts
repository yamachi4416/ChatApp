import { RoomServiceModel } from "./RoomServiceModel";
import { RoomHttpService } from "./RoomHttpService";
import { RoomModel } from "./Models";
import { WebSocketService } from "./WebSocketService";

interface RoomServiceModelFactory {
    (RoomModel) : RoomServiceModel;
}

class RoomContext {

    private roomIdMap: Map<string, RoomServiceModel> = new Map<string, RoomServiceModel>();

    public rooms: Array<RoomServiceModel> = [];
    public room: RoomServiceModel = null;

    constructor(
        public ws: WebSocketService,
        protected httpService: RoomHttpService,
        protected roomFactory: RoomServiceModelFactory
    ) {

    }

    fetchJoinRooms() {
        return this.httpService.getJoinRooms()
            .then((rooms) => this.addRooms(rooms).rooms);
    }

    fetchRoomContents() {
        if (!this.room) return this.httpService.reject;
        return this.room.getRoomContents();
    }

    addRoom(room: RoomModel) {
        if (room && room.id && !this.roomIdMap[room.id]) {
            const roomService = this.roomFactory(room);
            this.roomIdMap.set(roomService.id, roomService);
            this.rooms.push(roomService);
        }
        return this;
    }

    addRooms(rooms: Array<RoomModel>) {
        if (rooms) {
            rooms.forEach((room) => this.addRoom(room));
        }
        return this;
    }

    removeRoom(roomid: string) {
        const room = this.getRoom(roomid);
        if (room) {
            console.log(room);
            const rooms = this.rooms;
            this.roomIdMap.delete(roomid);
            rooms.splice(rooms.indexOf(room), 1);
        }
        return this;
    }

    getRoom(roomid: string) {
        return this.roomIdMap.get(roomid);
    }

    getRoomOrFirst(roomid: string) {
        return this.getRoom(roomid) || this.rooms[0];
    }

    selectRoom(roomid: string) {
        const room = this.getRoom(roomid);
        if (room) {
            this.room = room;
        }
        return this;
    }

    getUnreadMessageInfo() {
        var fg = this.room && this.room.unReadMessageCount || 0;
        var bg = this.rooms.reduce(function(a, b) { return a + b.unReadMessageCount; }, 0);
        if (fg + bg > 0) {
            return "[" + fg + "/" + bg + "] ";
        } else {
            return "";
        }
    }
}

export {
    RoomContext,
    RoomServiceModelFactory
}