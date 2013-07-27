using System;

namespace flash.events
{
    public class EventDispatcher { 
        
        public Action<Event> deactivateActions;
        public Action<Event> activateActions;
        public Action<Event> addedToStageActions;
        public Action<Event> enterFrameActions;

        public void addEventListener(string type, Action<Event>  action) {
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

        public Action<MouseEvent> mouseDownActions;
        public Action<MouseEvent> mouseMoveActions;
        public Action<MouseEvent> mouseUpActions;

        public void addEventListener(string type, Action<MouseEvent>  action) {
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

        public Action<KeyboardEvent> keyDownActions;
        public Action<KeyboardEvent> keyUpActions;

        public void addEventListener(string type, Action<KeyboardEvent>  action) {
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

        public void removeEventListener(string type, Action<Event>  action) {
            switch(type) {
                case Event.ADDED_TO_STAGE:
                    addedToStageActions -= action;
                    break;
                default:
                    throw new NotImplementedException();

            }
        }

        public void removeEventListener(string type, Action<MouseEvent>  action) {
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

        public void removeEventListener(string type, Action<KeyboardEvent>  action) {
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
