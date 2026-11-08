using System;
using System.Collections.Generic;

namespace AXS.Core.Utilities
{
    /// <summary>
    /// 型安全なインプロセスPub/Subイベントバス。プロジェクト内の疎結合通信を実現します。
    /// </summary>
    public static class EventBus<TEvent>
    {
        private static readonly List<Action<TEvent>> Subscribers = new List<Action<TEvent>>();

        public static void Subscribe(Action<TEvent> handler)
        {
            if (handler != null && !Subscribers.Contains(handler))
            {
                Subscribers.Add(handler);
            }
        }

        public static void Unsubscribe(Action<TEvent> handler)
        {
            if (handler != null)
            {
                Subscribers.Remove(handler);
            }
        }

        public static void Publish(TEvent eventData)
        {
            for (int i = Subscribers.Count - 1; i >= 0; i--)
            {
                try
                {
                    Subscribers[i]?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
        }

        public static void Clear()
        {
            Subscribers.Clear();
        }
    }
}
