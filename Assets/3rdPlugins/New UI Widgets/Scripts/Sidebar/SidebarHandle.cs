namespace UIWidgets
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// Sidebar handle.
    /// </summary>
    public class SidebarHandle : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler,IPointerUpHandler
    {
        /// <summary>
        /// BeginDrag event.
        /// </summary>
        public PointerUnityEvent BeginDragEvent = new PointerUnityEvent();

        /// <summary>
        /// EndDrag event.
        /// </summary>
        public PointerUnityEvent EndDragEvent = new PointerUnityEvent();

        /// <summary>
        /// Drag event.
        /// </summary>
        public PointerUnityEvent DragEvent = new PointerUnityEvent();

        public PointerUnityEvent PointerDownEvent = new PointerUnityEvent();
		public PointerUnityEvent PointerUpEvent = new PointerUnityEvent();

        /// <summary>
        /// Called by a BaseInputModule before a drag is started.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            BeginDragEvent.Invoke(eventData);
        }

        /// <summary>
        /// Called by a BaseInputModule when a drag is ended.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public void OnEndDrag(PointerEventData eventData)
        {
            EndDragEvent.Invoke(eventData);
        }

        /// <summary>
        /// When draging is occuring this will be called every time the cursor is moved.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public void OnDrag(PointerEventData eventData)
        {
            DragEvent.Invoke(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDownEvent.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
			PointerUpEvent.Invoke(eventData);
        }
    }
}