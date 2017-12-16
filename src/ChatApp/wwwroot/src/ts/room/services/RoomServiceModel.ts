import { RoomModel, RoomMessageModel, RoomMemberModel } from "./Models";
import { RoomHttpService } from "./RoomHttpService";
import { extend } from "angular";

class RoomServiceModel extends RoomModel {
    public message: RoomMessageModel;
    public unReadMessageCount: number = 0;
    
    protected nowfetchNewMessage?: boolean = undefined;
    protected hasOldMessages?: boolean = undefined;
    protected messageMap: Map<number, RoomMessageModel> = new Map<number, RoomMessageModel>();
    protected membersMap: Map<string, RoomMemberModel> = new Map<string, RoomMemberModel>();

    constructor(room: RoomModel, protected httpService: RoomHttpService) {
        super(room);
        this.message = new RoomMessageModel();
    }

    getRoomContents() {
        return this.httpService.all(this.getMembers(), this.fetchNewMessages());
    }

    fetchNewMessages() {
        if (this.nowfetchNewMessage)
            return this.httpService.reject;
        this.nowfetchNewMessage = true;
        const messageId = this.messages.length ? this.messages[this.messages.length - 1].id : null;
        return this.httpService.getNewMessages(this.id, messageId)
            .then((messages) => this.addNewMessages(messages))
            .finally(() => { this.nowfetchNewMessage = false });
    }

    fetchOldMessages() {
        const messageId = this.messages.length ? this.messages[0].id : null;
        if (!this.hasOldMessages || messageId == null)
            return this.httpService.reject;
        this.hasOldMessages = false;
        return this.httpService.getOldMessages(this.id, messageId)
            .then((messages) => {
                if (messages && messages.length) {
                    this.hasOldMessages = true;
                    this.addOldMessages(messages);
                }
            });
    }

    getMembers() {
        return this.httpService.getMembers(this.id)
            .then((members) => this.setMembers(members));
    }

    postMessage() {
        const message = this.message;
        message.isPosting = true;
        return this.httpService.postMessage(this.id, message)
            .then((message) => { this.message = {}; })
            .catch(() => { message.isPosting = false; });
    }

    addMessage(message: RoomMessageModel, push: boolean) {
        if (!this.messageMap.has(message.id)) {
            this.messageMap.set(message.id, message);
            push ?
                this.messages.push(message) :
                this.messages.unshift(message);
        }

        if (this.hasOldMessages === undefined) {
            this.hasOldMessages = this.messages.length > 0;
        }

        return this;
    }

    addMessages(messages: Array<RoomMessageModel>, push: boolean) {
        if (messages) {
            messages.forEach((message) => this.addMessage(message, push));
        }
        return this;
    }

    addOldMessages(messages: Array<RoomMessageModel>) {
        return this.addMessages(messages, false);
    }

    addNewMessages(messages: Array<RoomMessageModel>) {
        return this.addMessages(messages, true);
    }

    setMembers(members: Array<RoomMemberModel>) {
        this.membersMap = new Map<string, RoomMemberModel>();
        this.members = [];
        if (members) {
            members.forEach((member) => this.addMember(member));
        }
        return this;
    }

    addMember(member: RoomMemberModel) {
        if (!member || this.membersMap.has(member.id))
            return this;
        this.members.push(member);
        this.membersMap.set(member.id, member);
        return this;
    }

    removeMember(member: RoomMemberModel) {
        if (!member) return this;
        const members = this.members;
        for (let i = 0, l = members.length; i < l; i++) {
            const m = members[i];
            if (m.id == member.id) {
                this.membersMap.delete(m.id);
                members.splice(i, 1);
                break;
            }
        }
        return this;
    }
}

export {
    RoomServiceModel
}