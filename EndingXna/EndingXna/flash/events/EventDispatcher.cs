using System;

namespace flash.events
{
    public class EventDispatcher { 
        
        public Action1<Event> deactivateActions;
        public Action1<Event> activateActions;
        public Action1<Event> addedToStageActions;
        public Action1<Event> enterFrameActions;

        public void addEventListener(string type, Action1<Event>  action) {
            switch(type) {
                case Event.DEACTIVATE:
                    deactivateActions += action;
                    break;
                case Event.ACTIVATE:
                    activateActions += action;
                    break;
                case Event.ADDED_TO_STAGE:
                    addedToStageActions += action;
                    break;
                case Event.ENTER_FRAME:
                    enterFrameActions += action;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public Action1<MouseEvent> mouseDownActions;
        public Action1<MouseEvent> mouseMoveActions;
        public Action1<MouseEvent> mouseUpActions;

        public void addEventListener(string type, Action1<MouseEvent>  action) {
            switch(type) {
                case MouseEvent.MOUSE_DOWN:
                    mouseDownActions += action;
                    break;
                case MouseEvent.MOUSE_MOVE:
                    mouseMoveActions += action;
                    break;
                case MouseEvent.MOUSE_UP:
                    mouseUpActions += action;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public Action1<KeyboardEvent> keyDownActions;
        public Action1<KeyboardEvent> keyUpActions;

        public void addEventListener(string type, Action1<KeyboardEvent>  action) {
            switch(type) {
                case KeyboardEvent.KEY_DOWN:
                    keyDownActions += action;
                    break;
                case KeyboardEvent.KEY_UP:
                    keyUpActions += action;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void removeEventListener(string type, Action1<Event>  action) {
            switch(type) {
                case Event.ADDED_TO_STAGE:
                    addedToStageActions -= action;
                    break;
                default:
                    throw new NotImplementedException();

            }
        }

        public void removeEventListener(string type, Action1<MouseEvent>  action) {
            switch(type) {
                case MouseEvent.MOUSE_DOWN:
                    mouseDownActions -= action;
                    break;
                case MouseEvent.MOUSE_MOVE:
                    mouseMoveActions -= action;
                    break;
                case MouseEvent.MOUSE_UP:
                    mouseUpActions -= action;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void removeEventListener(string type, Action1<KeyboardEvent>  action) {
            switch(type) {
                case KeyboardEvent.KEY_DOWN:
                    keyDownActions -= action;
                    break;
                case KeyboardEvent.KEY_UP:
                    keyUpActions -= action;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }   
    }
}
