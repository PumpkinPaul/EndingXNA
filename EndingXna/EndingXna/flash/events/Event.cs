using System;

namespace flash.events
{
    public class Event : EventArgs { 
    
        public const string ACTIVATE = "activate";
        public const string DEACTIVATE = "deactivate";
        public const string ENTER_FRAME = "enterFrame";
        public const string ADDED_TO_STAGE = "addedToStage";
    }
}
