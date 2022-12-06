﻿using System;
using System.Threading;
using Microsoft.AspNetCore.Components;
using OneOf;

namespace AntDesign
{
    public class MessageConfig
    {
        internal string AnimationClass { get; set; } = MessageAnimationType.Enter;

        internal CancellationTokenSource Cts { get; set; }

        /// <summary>
        /// Content for message
        /// </summary>
        public OneOf<string, RenderFragment> Content { get; set; }

        /// <summary>
        /// Time before auto-dismiss, in seconds
        /// </summary>
        public double? Duration { get; set; } = null;

        /// <summary>
        /// Icon for message
        /// </summary>
        public RenderFragment Icon { get; set; } = null;

        /// <summary>
        /// Callback executed on close of message
        /// </summary>
        public event Action OnClose;

        internal void InvokeOnClose()
        {
            OnClose?.Invoke();
        }

        /// <summary>
        /// Unique identifier for component
        /// </summary>
        public string Key { get; set; } = null;

        /// <summary>
        /// Style of message
        /// </summary>
        public MessageType Type { get; set; }
    }
}
