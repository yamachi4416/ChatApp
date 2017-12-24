import { ChatSidebarDirective } from "./directives/ChatSidebarDirective";
import { ChatMessageImageDirective } from "./directives/ChatMessageImageDirective";
import { ChatAutoresizeDirective } from "./directives/ChatAutoresizeDirective";
import { ChatImageCliperDirective } from "./directives/ChatImageCliperDirective";
import { ChatMediaClassDirective } from "./directives/ChatMediaClassDirective";
import { ChatRoomContainerDirective } from "./directives/ChatRoomContainerDirective";

angular.module('ChatApp')
    .value('chatMessageNoImage', '../images/noimage.jpg')
    .directive('chatRoom', ChatRoomContainerDirective.Factory)
    .directive('chatSidebar', ChatSidebarDirective.Factory)
    .directive('chatMessageImage', ChatMessageImageDirective.Factory)
    .directive('chatImageCliper', ChatImageCliperDirective.Factory)
    .directive('chatAutoresize', ChatAutoresizeDirective.Factory)
    .directive('mediaClass', ChatMediaClassDirective.Factory);