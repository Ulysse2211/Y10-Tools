using System;

namespace Y10_Tools
{
    public static class EventManager
    {
        public static event Action ShowOverlayRequested;
        public static event Action HideOverlayRequested;
        public static event Action DeviceUpdatedRequested;

        public static void TriggerShowOverlay()
        {
            ShowOverlayRequested?.Invoke();
        }

        public static void DeviceUpdated()
        {
            DeviceUpdatedRequested?.Invoke();
        }

        public static void TriggerHideOverlay()
        {
            HideOverlayRequested?.Invoke();
        }
    }
}
