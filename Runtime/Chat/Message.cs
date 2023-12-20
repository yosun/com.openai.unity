// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace OpenAI.Chat
{
    [Preserve]
    [Serializable]
    public sealed class Message
    {
        [Preserve]
        public Message() { }

        [Preserve]
        internal Message(Delta other) => CopyFrom(other);

        [Preserve]
        [Obsolete("Use new constructor args")]
        public Message(Role role, string content, string name, Function function)
            : this(role, content, name)
        {
            Name = name;
            Function = function;
        }

        /// <summary>
        /// Creates a new message to insert into a chat conversation.
        /// </summary>
        /// <param name="role">
        /// The <see cref="OpenAI.Role"/> of the author of this message.
        /// </param>
        /// <param name="content">
        /// The contents of the message.
        /// </param>
        /// <param name="name"></param>
        [Preserve]
        public Message(Role role, IEnumerable<Content> content, string name = null)
        {
            Role = role;
            Content = content?.ToList();
            Name = name;
        }

        /// <summary>
        /// Creates a new message to insert into a chat conversation.
        /// </summary>
        /// <param name="role">
        /// The <see cref="OpenAI.Role"/> of the author of this message.
        /// </param>
        /// <param name="content">
        /// The contents of the message.
        /// </param>
        /// <param name="name"></param>
        [Preserve]
        public Message(Role role, string content, string name = null)
        {
            Role = role;
            Content = content;
            Name = name;
        }

        /// <inheritdoc />
        [Preserve]
        public Message(Tool tool, string content)
            : this(Role.Tool, content, tool.Function.Name)
        {
            ToolCallId = tool.Id;
        }

        /// <summary>
        /// Creates a new message to insert into a chat conversation.
        /// </summary>
        /// <param name="tool">Tool used for message.</param>
        /// <param name="content">Tool function response.</param>
        [Preserve]
        public Message(Tool tool, IEnumerable<Content> content)
            : this(Role.Tool, content, tool.Function.Name)
        {
            ToolCallId = tool.Id;
        }

        [SerializeField]
        private string name;

        /// <summary>
        /// Optional, The name of the author of this message.<br/>
        /// May contain a-z, A-Z, 0-9, and underscores, with a maximum length of 64 characters.
        /// </summary>
        [Preserve]
        [JsonProperty("name")]
        public string Name
        {
            get => name;
            private set => name = value;
        }

        [Preserve]
        [SerializeField]
        private Role role;

        /// <summary>
        /// The <see cref="OpenAI.Role"/> of the author of this message.
        /// </summary>
        [Preserve]
        [JsonProperty("role")]
        public Role Role
        {
            get => role;
            private set => role = value;
        }

        [SerializeField]
        [TextArea(1, 30)]
        private string content;

        private object contentList;

        /// <summary>
        /// The contents of the message.
        /// </summary>
        [Preserve]
        [JsonProperty("content", DefaultValueHandling = DefaultValueHandling.Populate, NullValueHandling = NullValueHandling.Include, Required = Required.AllowNull)]
        public object Content
        {
            get => contentList ?? content;
            private set
            {
                if (value is string s)
                {
                    content = s;
                }
                else
                {
                    contentList = value;
                }
            }
        }

        private List<Tool> toolCalls;

        /// <summary>
        /// The tool calls generated by the model, such as function calls.
        /// </summary>
        [Preserve]
        [JsonProperty("tool_calls")]
        public IReadOnlyList<Tool> ToolCalls
        {
            get => toolCalls;
            private set => toolCalls = value.ToList();
        }

        [Preserve]
        [JsonProperty("tool_call_id")]
        public string ToolCallId { get; private set; }

        /// <summary>
        /// The function that should be called, as generated by the model.
        /// </summary>
        [Preserve]
        [Obsolete("Replaced by ToolCalls")]
        [JsonProperty("function_call")]
        public Function Function { get; private set; }

        [Preserve]
        public override string ToString() => Content?.ToString() ?? string.Empty;

        [Preserve]
        public static implicit operator string(Message message) => message?.ToString();

        [Preserve]
        internal void CopyFrom(Delta other)
        {
            if (Role == 0 &&
                other?.Role > 0)
            {
                Role = other.Role;
            }

            if (other?.Content != null)
            {
                content += other.Content;
            }

            if (!string.IsNullOrWhiteSpace(other?.Name))
            {
                Name = other.Name;
            }

            if (other is { ToolCalls: not null })
            {
                toolCalls ??= new List<Tool>();

                foreach (var otherToolCall in other.ToolCalls)
                {
                    if (otherToolCall == null) { continue; }

                    if (otherToolCall.Index.HasValue)
                    {
                        if (otherToolCall.Index + 1 > toolCalls.Count)
                        {
                            toolCalls.Insert(otherToolCall.Index.Value, new Tool(otherToolCall));
                        }

                        toolCalls[otherToolCall.Index.Value].CopyFrom(otherToolCall);
                    }
                    else
                    {
                        toolCalls.Add(new Tool(otherToolCall));
                    }
                }
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if (other?.Function != null)
            {
                if (Function == null)
                {
                    Function = new Function(other.Function);
                }
                else
                {
                    Function.CopyFrom(other.Function);
                }
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
