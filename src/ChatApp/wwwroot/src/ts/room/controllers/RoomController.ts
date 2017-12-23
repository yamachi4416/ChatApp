import { RoomContext } from "../services/RoomContext";
import { ITimeoutService, IWindowService, IDocumentService, ILocationService, IPromise } from "angular";
import { IRootScopeService } from "angular";
import { IModalService, IModalSettings } from "angular-ui-bootstrap";
import { RoomModel } from "../services/Models";
import { RoomServiceModel } from "../services/RoomServiceModel";
import { RoomMemberAddController } from "./admin/RoomMemberAddController";
import { RoomMemberRemoveController } from "./admin/RoomMemberRemoveController";
import { RoomCreateController } from "./RoomCreateController";
import { RoomEditController } from "./admin/RoomEditController";
import { RoomDetailController } from "./RoomDetailController";
import { RemoveRoomController } from "./admin/RemoveRoomController";
import { RoomAvatarEditController } from "./admin/RoomAvatarEditController";
import { WSMessageTypes } from "../services/WebSocketService";

export class RoomController {
    private get ws() { return this.context.ws; }

    constructor(
        private context: RoomContext,
        private $timeout: ITimeoutService,
        private $location: ILocationService,
        private $rootScope: IRootScopeService,
        private $uibModal: IModalService,
        private $window: IWindowService,
        private $document: IDocumentService
    ) {
        this.registerWsEvents();
        this.registerWindowEvents();
        this.InitRooms();
    }

    InitRooms() {
        return this.context.fetchJoinRooms()
            .then((rooms) => {
                this.SelectRoom(this.context.getRoomOrFirst(this.$location.hash()));
                this.$rootScope.$broadcast('chatRoomMessageReady');
                this.ws.connect();
            });
    }

    SelectRoom(room: RoomServiceModel) {
        if (room == null) {
            return this.$location.hash(null);
        }
        if (this.context.room == room) return;

        room.unReadMessageCount = 0;
        return this.$location.hash(room.id);
    }

    ChangeRoom() {
        const roomid = this.$location.hash();
        if (roomid) {
            return this.context.selectRoom(roomid).fetchRoomContents();
        }
        const room = this.context.getRoomOrFirst(roomid);
        if (room) {
            this.SelectRoom(room);
        }
    }

    GetOldmessages() {
        return this.context.room
            && this.context.room.fetchOldMessages();
    }

    PostMessage() {
        if (this.context.room) {
            const room = this.context.room;
            const promise = room.postMessage();
            return this.ws.isEnable() ?
                promise : promise.then(() => this.fetchNewMessages(room, true));
        }
    }

    OpenAddMember() {
        this.openAdminModalUi({
            templateUrl: '/modal/member/add.tmpl.html',
            controller: RoomMemberAddController.id,
            controllerAs: 'ctrl',
            resolve: { room: this.context.room }
        });
    }

    OpenRemoveMember() {
        this.openAdminModalUi({
            templateUrl: '/modal/member/remove.tmpl.html',
            controller: RoomMemberRemoveController.id,
            controllerAs: 'ctrl',
            resolve: { room: this.context.room }
        });
    }

    OpenCreateRoom() {
        this.openModalUi({
            templateUrl: '/modal/room/create.tmpl.html',
            controller: RoomCreateController.id,
            controllerAs: 'ctrl'
        }, (room: RoomServiceModel) => {
            this.context.addRoom(room);
            this.SelectRoom(room);
        });
    }

    OpenEditRoom(room: RoomServiceModel) {
        this.openAdminModalUi({
            templateUrl: '/modal/room/edit.tmpl.html',
            controller: RoomEditController.id,
            controllerAs: 'ctrl',
            resolve: { room: room }
        });
    }

    OpenDetailRoom(room: RoomServiceModel) {
        this.openModalUi({
            templateUrl: '/modal/room/detail.tmpl.html',
            controller: RoomDetailController.id,
            controllerAs: 'ctrl',
            resolve: { room: room }
        });
    }

    OpenRemoveRoom(room: RoomServiceModel) {
        this.openAdminModalUi({
            size: 'sm',
            templateUrl: '/modal/room/delete-confirm.tmpl.html',
            controller: RemoveRoomController.id,
            controllerAs: 'ctrl',
            resolve: { room: room }
        });
    }

    OpenRoomImageEditDialog(room: RoomServiceModel) {
        this.openAdminModalUi({
            size: 'sm',
            templateUrl: '/modal/room/edit-image.tmpl.html',
            controller: RoomAvatarEditController.id,
            controllerAs: 'ctrl',
            resolve: { room: room }
        });
    }

    private openModalUi(
        options: IModalSettings,
        ...procs: ((any) => void | IPromise<any>)[]
    ) {
        const modalInstance = this.$uibModal.open(angular.extend({}, options));
        const result = modalInstance.result
        result.then.apply(result, procs);
        return modalInstance;
    }

    private openAdminModalUi(
        options: IModalSettings,
        ...procs: ((any) => void | IPromise<any>)[]
    ) {
        if (!this.context.room.isAdmin) return;
        const args = (<any[]>[options]).concat(procs);
        return this.openModalUi.apply(this, args);
    }

    private fetchNewMessages(room: RoomServiceModel, ifShowBottom: boolean) {
        return room.fetchNewMessages()
            .then(() => {
                if (this.context.room && this.context.room.id === room.id) {
                    this.$rootScope.$broadcast('chatRoomMessageScrollBottom', {
                        ifShowBottom: !!ifShowBottom
                    });
                }
            });
    }

    private registerWindowEvents() {
        angular.element(this.$window).on('focus', () => {
            this.ws.connect();
            if (this.context.room) {
                this.context.room.unReadMessageCount = 0;
                this.context.room.fetchNewMessages();
            }
        });
    }

    private registerWsEvents() {
        this.ws.on(WSMessageTypes.CREATE_MESSAGE, (res) => {
            const room = this.context.getRoom(res.roomId);
            if (!room) return;
            if (this.context.room === room) {
                if (this.$document[0].hidden) {
                    this.$timeout(() => room.unReadMessageCount += 1);
                }
                this.fetchNewMessages(room, true);
            } else {
                this.$timeout(() => room.unReadMessageCount += 1);
            }
        }).on(WSMessageTypes.CREATE_MEMBER, (res) => {
            const room = this.context.getRoom(res.roomId);
            if (room && this.context.room === room) {
                this.$timeout(() => room.addMember(res.message));
            }
        }).on(WSMessageTypes.DELETE_MEMBER, (res) => {
            const room = this.context.getRoom(res.roomId);
            if (room && this.context.room === room) {
                this.$timeout(() => room.removeMember(res.message));
            }
        }).on(WSMessageTypes.DEFECT_ROOM, (res) => {
            this.$timeout(() => this.context.removeRoom(res.roomId));
        }).on(WSMessageTypes.JOIN_ROOM, (res) => {
            const len = (this.context.rooms || []).length;
            this.$timeout(() => {
                this.context.addRoom(res.message);
                if (len == 0) {
                    this.SelectRoom(this.context.getRoomOrFirst(res.roomId));
                }
            });
        }).on(WSMessageTypes.DELETE_ROOM, (res) => {
            this.$timeout(() => this.context.removeRoom(res.roomId));
        }).on(WSMessageTypes.MODIFY_ROOM_AVATAR, (res) => {
            const room = this.context.getRoom(res.roomId);
            if (room) {
                this.$timeout(() => room.avatarId = res.message.avatarId);
            }
        });
    }

    static get id(): string {
        return "RoomController";
    }

    static get injector(): Array<any> {
        return [
            "RoomContext",
            "$timeout",
            "$location",
            "$rootScope",
            "$uibModal",
            "$window",
            "$document",
            RoomController
        ];
    }
}
