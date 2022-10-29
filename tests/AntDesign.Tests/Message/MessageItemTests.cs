// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace AntDesign.Tests.Avatar
{
    public class MessageItemTests : AntDesignTestBase
    {
        [Theory]
        [InlineData(MessageType.Info, "ant-message-info", "info-circle")]
        [InlineData(MessageType.Warning, "ant-message-warning", "exclamation-circle")]
        [InlineData(MessageType.Error, "ant-message-error", "close-circle")]
        [InlineData(MessageType.Success, "ant-message-success", "check-circle")]
        [InlineData(MessageType.Loading, "ant-message-loading", "loading")]
        public void ItShouldRenderProperlyForType(MessageType type, string expectedClass, string expectedIcon)
        {
            var config = new MessageConfig
            {
                Type = type,
                Content = "Test"
            };

            var systemUnderTest = RenderComponent<MessageItem>(parameters => parameters.Add(x => x.Config, config));

            systemUnderTest.MarkupMatches(@$"<div class=""ant-message-notice ant-move-up-enter ant-move-up-enter-active ant-move-up"">
                <div class=""ant-message-notice-content"">
                    <div class=""ant-message-custom-content {expectedClass}"">
                        <span role=""img"" class=""anticon anticon-{expectedIcon}"" id:ignore>
                            <svg diff:ignoreAttributes diff:ignoreChildren></svg>
                        </span>
                        <span>Test</span>
                    </div>
                </div>
            </div>");
        }

        [Fact]
        public void ItShouldRenderContentRenderFragment()
        {
            RenderFragment fragment = (builder) =>
            {
                builder.OpenElement(0, "span");
                builder.AddContent(1, "Testing Fragment");
                builder.CloseElement();
            };

            var config = new MessageConfig
            {
                Type = MessageType.Success,
                Content = fragment
            };

            var systemUnderTest = RenderComponent<MessageItem>(parameters => parameters.Add(x => x.Config, config));

            systemUnderTest.Find(".ant-message-custom-content").MarkupMatches(@"<div class:ignore>
                <span diff:ignoreAttributes diff:ignoreChildren></span>
                <span>
                    <span>Testing Fragment</span>
                </span>
            </div>");
        }
    }
}
