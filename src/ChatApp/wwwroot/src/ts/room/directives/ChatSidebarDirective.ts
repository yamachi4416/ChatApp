import { IDirective, IDirectiveFactory } from "angular";

export interface ChatSidebarScope {

}

export class ChatSidebarController implements ChatSidebarScope {
    public isShowSidebar = false;

    toggleSidebar() {
        const ele = angular.element('#chat-sidebar');
        this.isShowSidebar = ele.is('.hidden-xs');
        ele.toggleClass('hidden-xs');
    }
}

export class ChatSidebarDirective implements IDirective {
    scope: ChatSidebarScope = {};
    restrict = 'A';
    transclude = true;
    replace = true;
    controller = ChatSidebarController;
    controllerAs = 'ctrl';
    bindToController = true;
    templateUrl = '/sidebar/main.tmpl.html';

    static get Factory(): IDirectiveFactory {
        const factory: IDirectiveFactory = () => {
            return new ChatSidebarDirective();
        };
        return factory;
    }
}