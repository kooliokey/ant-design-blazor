// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AntDesign.Tests.Avatar
{
    public class MessageServiceMessageTests : AntDesignTestBase
    {
        public MessageServiceMessageTests()
        {
            ComponentFactories.AddStub<MessageItem>(parameters => $"<span>Stub: {parameters.Get(x => x.Config).Type} - {parameters.Get(x => x.Config).Content}</span>");
        }

        [Fact]
        public void ItShouldRenderMessage()
        {
            var systemUnderTest = new MessageService();
            Services.AddScoped(_ => systemUnderTest);

            var rendered = Render((builder) =>
            {
                builder.OpenComponent<Message>(0);
                builder.CloseComponent();
            });

            rendered.InvokeAsync(() =>
            {
                systemUnderTest.Success("Testing 1, 2, 3");
            });

            rendered.MarkupMatches(@"<div>
                <div class=""ant-message"" style=""top: 24px;"" >
                    <span>
                        <span>Stub: Success - System.String: Testing 1, 2, 3</span>
                    </span>
                </div>
            </div>");
        }

        [Fact]
        public void ItShouldRenderMultipleMessagesAtOnce()
        {
            var systemUnderTest = new MessageService();
            Services.AddScoped(_ => systemUnderTest);

            var rendered = Render((builder) =>
            {
                builder.OpenComponent<Message>(0);
                builder.CloseComponent();
            });

            rendered.InvokeAsync(() =>
            {
                systemUnderTest.Success("Success...");
                systemUnderTest.Error("Error...");
                systemUnderTest.Warn("Warn...");
                systemUnderTest.Warning("Warning...");
                systemUnderTest.Info("Info...");
                systemUnderTest.Loading("Loading...");
            });

            rendered.MarkupMatches(@"<div>
                <div class=""ant-message"" style=""top: 24px;"" >
                    <span>
                        <span>Stub: Success - System.String: Success...</span>
                        <span>Stub: Error - System.String: Error...</span>
                        <span>Stub: Warning - System.String: Warn...</span>
                        <span>Stub: Warning - System.String: Warning...</span>
                        <span>Stub: Info - System.String: Info...</span>
                        <span>Stub: Loading - System.String: Loading...</span>
                    </span>
                </div>
            </div>");
        }

        [Fact]
        public async Task ItShouldHideOldMessagesWhenAddingMoreThanMaxCount()
        {
            var systemUnderTest = new MessageService();
            Services.AddScoped(_ => systemUnderTest);

            var rendered = Render((builder) =>
            {
                builder.OpenComponent<Message>(0);
                builder.CloseComponent();
            });

            await rendered.InvokeAsync(() => systemUnderTest.Config(new MessageGlobalConfig
            {
                MaxCount = 4
            }));

            await rendered.InvokeAsync(() =>
            {
                systemUnderTest.Success("1");
                systemUnderTest.Success("2");
                systemUnderTest.Success("3");
                systemUnderTest.Success("4");
                systemUnderTest.Success("5");
                systemUnderTest.Success("6");
                systemUnderTest.Success("7");
                systemUnderTest.Success("8");
            });

            rendered.MarkupMatches(@"<div>
                <div class=""ant-message"" style=""top: 24px;"">
                    <span>
                        <span>Stub: Success - System.String: 5</span>
                        <span>Stub: Success - System.String: 6</span>
                        <span>Stub: Success - System.String: 7</span>
                        <span>Stub: Success - System.String: 8</span>
                    </span>
                </div>
            </div>");
        }

        [Fact]
        public async Task ItShouldUpdateExistingMessageConfigOnAddAgain()
        {
            var systemUnderTest = new MessageService();
            Services.AddScoped(_ => systemUnderTest);

            var rendered = Render((builder) =>
            {
                builder.OpenComponent<Message>(0);
                builder.CloseComponent();
            });

            var key = "Test123";

            await rendered.InvokeAsync(() =>
            {
                systemUnderTest.Open(new MessageConfig
                {
                    Key = key,
                    Type = MessageType.Loading,
                    Content = "Testing updating existing"
                });

                systemUnderTest.Open(new MessageConfig
                {
                    Key = key,
                    Type = MessageType.Success,
                    Content = "New content"
                });
            });

            rendered.MarkupMatches(@"<div>
                <div class=""ant-message"" style=""top: 24px;"">
                    <span>
                        <span>Stub: Success - System.String: New content</span>
                    </span>
                </div>
            </div>");
        }

        [Fact]
        public async Task ItShouldRemoveMesageHalfSecondAfterDuration()
        {
            var systemUnderTest = new MessageService();
            Services.AddScoped(_ => systemUnderTest);

            var rendered = Render((builder) =>
            {
                builder.OpenComponent<Message>(0);
                builder.CloseComponent();
            });

            await rendered.InvokeAsync(() => systemUnderTest.Config(new MessageGlobalConfig
            {
                Duration = 1
            }));

            await rendered.InvokeAsync(() =>
            {
                systemUnderTest.Success("1");
            });

            await Task.Delay(TimeSpan.FromSeconds(1.6));

            rendered.MarkupMatches(string.Empty);
        }

        [Fact]
        public async Task ItShouldNotAutoDismissMessageWithZeroDuration()
        {
            var systemUnderTest = new MessageService();
            Services.AddScoped(_ => systemUnderTest);

            var rendered = Render((builder) =>
            {
                builder.OpenComponent<Message>(0);
                builder.CloseComponent();
            });

            await rendered.InvokeAsync(() =>
            {
                systemUnderTest.Success("1", 0);
            });

            await Task.Delay(TimeSpan.FromSeconds(1));

            rendered.MarkupMatches(@"<div>
                <div class=""ant-message"" style=""top: 24px;"">
                    <span>
                        <span>Stub: Success - System.String: 1</span>
                    </span>
                </div>
            </div>");
        }

        [Fact]
        public async Task ItShouldDismissMessageWithZeroDurationHalfSecondAfterTriggered()
        {
            var systemUnderTest = new MessageService();
            Services.AddScoped(_ => systemUnderTest);

            var rendered = Render((builder) =>
            {
                builder.OpenComponent<Message>(0);
                builder.CloseComponent();
            });

            Task message = Task.CompletedTask;
            await rendered.InvokeAsync(() =>
            {
                message = systemUnderTest.Success("1", 0);
            });

            await rendered.InvokeAsync(() =>
            {
                message.Start();
            });

            await Task.Delay(501);

            rendered.MarkupMatches(string.Empty);
        }

        [Fact]
        public async Task ItShouldDismissAllMessagesWhenMessageServiceDestroyCalled()
        {
            var systemUnderTest = new MessageService();
            Services.AddScoped(_ => systemUnderTest);

            var rendered = Render((builder) =>
            {
                builder.OpenComponent<Message>(0);
                builder.CloseComponent();
            });

            await rendered.InvokeAsync(() =>
            {
                systemUnderTest.Success("1", 0);
                systemUnderTest.Success("1", 0);
            });

            await rendered.InvokeAsync(() =>
            {
                systemUnderTest.Destroy();
            });

            await Task.Delay(501);

            rendered.MarkupMatches(string.Empty);
        }
    }
}
