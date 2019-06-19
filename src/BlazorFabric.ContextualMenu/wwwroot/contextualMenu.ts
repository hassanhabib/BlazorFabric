﻿//declare interface Window { debounce(func: Function, wait: number, immediate: boolean): Function }


namespace BlazorFabricContextualMenu {

    interface DotNetReferenceType {

        invokeMethod<T>(methodIdentifier: string, ...args: any[]): T;
        invokeMethodAsync<T>(methodIdentifier: string, ...args: any[]): Promise<T>;
    }

    interface IRectangle {
        left: number;
        top: number;
        width: number;
        height: number;
        right?: number;
        bottom?: number;
    }

    export function registerHandlers(targetElement: HTMLElement, contextualMenuItem: DotNetReferenceType): number[] {
        var window = targetElement.ownerDocument.defaultView;
              
        var mouseEnterId = Handler.addListener(targetElement, "mouseenter", (ev: Event) => { ev.preventDefault(); contextualMenuItem.invokeMethodAsync("MouseEnterHandler"); }, false);
        var mouseLeaveId = Handler.addListener(targetElement, "mouseleave", (ev: Event) => { ev.preventDefault();  contextualMenuItem.invokeMethodAsync("MouseLeaveHandler"); }, false);
        return [mouseEnterId, mouseLeaveId];
    }

    export function unregisterHandlers(ids: number[]): void {
       
        for (let id of ids) {
            Handler.removeListener(id);
        }
    }

    interface EventParams {
        element: HTMLElement | Window;
        event: string;
        handler: (ev: Event) => void;
        capture: boolean;
    }
    interface Map<T> {
        [K: number]: T;
    }

    class Handler {

        static i: number = 1;
        static listeners: Map<EventParams> = {};

        static addListener(element: HTMLElement | Window, event: string, handler: (ev: Event) => void, capture: boolean): number {
            element.addEventListener(event, handler, capture);
            this.listeners[this.i] = { capture: capture, event: event, handler: handler, element: element };
            return this.i++;
        }
        static removeListener(id: number): void {
            if (id in this.listeners) {
                var h = this.listeners[id];
                h.element.removeEventListener(h.event, h.handler, h.capture);
                delete this.listeners[id];
            }
        }
    }

 
}




window['BlazorFabricContextualMenu'] = BlazorFabricContextualMenu || {};
