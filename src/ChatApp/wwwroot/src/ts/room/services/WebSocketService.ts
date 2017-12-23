import { IWindowService, } from "angular";

enum WSMessageTypes {
    CREATE_MESSAGE = 10,
    MODIFY_MESSAGE = 11,
    DELETE_MESSAGE = 12,
    CREATE_ROOM = 20,
    MODIFY_ROOM = 22,
    DELETE_ROOM = 23,
    JOIN_ROOM = 24,
    DEFECT_ROOM = 25,
    MODIFY_ROOM_AVATAR = 26,
    CREATE_MEMBER = 30,
    DELETE_MEMBER = 33
}

interface WsHandler {
    (WSMessageTypes): void;
}

class WebSocketService {
    private readonly location: Location;

    public readonly handlers: Array<Array<WsHandler>>;
    public socket: WebSocket;

    constructor(
        private WebSocketFactory: (url: string) => WebSocket,
        window: IWindowService
    ) {
        this.handlers = [];
        this.location = window.location;
    }

    isEnable(): boolean {
        return typeof this.WebSocketFactory === 'function';
    }

    isClosed(): boolean {
        return this.socket.readyState === this.socket.CLOSED;
    }

    isOpen(): boolean {
        if (!this.socket) return false;
        return this.socket.readyState === this.socket.OPEN;
    }

    public connect(): WebSocketService {
        if (!this.isEnable() || this.isOpen())
            return this;

        const socket = this.createWebSocket();

        socket.onopen = (e) => console.log(`socket open.\n${e}`);
        socket.onclose = (e) => console.log(`socket close.\n${e}`);
        socket.onerror = (e) => console.log(`socket error.\n${e}`);
        socket.onmessage = (e) => {
            var rawMessage = e.data;
            var objMessage = JSON.parse(rawMessage);
            this.handleMessage(objMessage.messageType as WSMessageTypes, objMessage);
        };

        this.socket = socket;
        return this;
    }

    on(type: WSMessageTypes, handler: WsHandler): WebSocketService {
        const handlers = this.handlers;
        handlers[type] = handlers[type] || [];
        if (handlers[type].indexOf(handler) === -1) {
            handlers[type].push(handler);
        }
        return this;
    }

    private createWebSocket(): WebSocket {
        const protocol = this.location.protocol.replace("http", "ws");
        const hostname = this.location.hostname;
        const port = this.location.port ? ":" + this.location.port : "";
        const pathname = this.location.pathname.replace(/\/([^\/]+)\/?$/, "/ws/rooms/connect");
        const url = `${protocol}//${hostname}${port}${pathname}`;
        return this.WebSocketFactory(url);
    }

    private handleMessage(type: WSMessageTypes, message: object) {
        const handles = this.handlers[type];
        if (handles) {
            handles.forEach((handle) => {
                handle(message);
            });
        }
    }
}

export {
    WSMessageTypes,
    WebSocketService
}