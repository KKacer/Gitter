﻿using Blazor.Gitter.Library;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor.Gitter.Core.Components.Shared
{
    public class MessageModel : ComponentBase
    {
        [Inject] IChatApi GitterApi { get; set; }
        [Inject] IAppState State { get; set; }
        [Inject] ILocalisationHelper Localisation { get; set; }
        [Parameter] internal IChatMessage MessageData { get; set; }
        [Parameter] protected string RoomId { get; set; }
        [Parameter] protected string UserId { get; set; }

        SemaphoreSlim slim = new SemaphoreSlim(1, 1);

        internal string MessageClassList(IChatMessage message) =>
            new BlazorComponentUtilities.CssBuilder()
                .AddClass("list-group-item")
                .AddClass("flex-column")
                .AddClass("align-items-start")
                .AddClass("bg-inherit")
                .AddClass("text-inherit")
                .AddClass("list-group-item-success", message.Unread)
                .AddClass("list-group-item-warning", message.Mentions.Any(m => m.UserId == UserId))
                .Build();
        internal string LocalTime(DateTime dateTime) =>
        TimeZoneInfo
            .ConvertTime(
                dateTime,
                Localisation.LocalTimeZoneInfo
            )
            .ToString(
                "G",
                Localisation.LocalCultureInfo
            );

        internal async Task MarkRead()
        {
            try
            {
                if (await slim.WaitAsync(1))
                {
                    if (MessageData.Unread)
                    {
                        await Task.Delay(1000);
                        State.RecordActivity();
                        if (await GitterApi.MarkChatMessageAsRead(UserId, RoomId, MessageData.Id))
                        {
                            MessageData.Unread = false;
                        }
                    }
                }
            }
            catch {}
            finally
            {
                slim.Release();
            }
        }
    }
}
