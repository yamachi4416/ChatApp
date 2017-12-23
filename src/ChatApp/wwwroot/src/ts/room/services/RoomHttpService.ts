import { IRoomPromise, HttpServiceBase } from "./HttpServiceBase";
import { IHttpService, IQService } from "angular";
import { RoomModel, RoomMessageModel } from "./Models";

class RoomHttpService extends HttpServiceBase {
    constructor($http: IHttpService, $q: IQService) {
        super($http, $q);
        this.baseUrl = "../api/rooms/";
    }

    getJoinRooms() {
        return this.httpGet("joins");
    }

    getMembers(roomId: string) {
        return this.httpGet(`members/${roomId}`);
    }

    getNewMessages(roomId: string, messageId: number) {
        let url = `messages/${roomId}/new`;
        if (messageId !== null) {
            url = `${url}/${messageId}`;
        }
        return this.httpGet(url);
    }

    getOldMessages(roomId: string, messageId?: number): IRoomPromise<Array<RoomMessageModel>> {
        if (messageId) {
            return this.httpGet(`messages/${roomId}/old/${messageId}`)
                .then((message) => {
                    return message ? message : this.reject;
                });
        } else {
            return this.reject;
        }
    }

    createRoom(room: RoomModel) {
        return this.httpPost('rooms/create', room);
    }

    postMessage(roomId: string, message : RoomMessageModel) {
        return this.httpPost(`messages/${roomId}/create`, {
            message: message.message
        });
    }
}

export {
    RoomHttpService
}
