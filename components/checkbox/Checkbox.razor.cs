﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace AntDesign
{
    /**
    <summary>
    <para>Checkbox component.</para>

    <h2>When To Use</h2>

    <list type="bullet">
        <item>Used for selecting multiple values from several options.</item>
        <item>If you use only one checkbox, it is the same as using Switch to toggle between two states. </item>
    </list>

    <para>The difference is that Switch will trigger the state change directly, but Checkbox just marks the state as changed and this needs to be submitted.</para>
    </summary>
    <seealso cref="CheckboxGroup"/>
    */
    [Documentation(DocumentationCategory.Components, DocumentationType.DataEntry, "https://gw.alipayobjects.com/zos/alicdn/8nbVbHEm_/CheckBox.svg")]
    public partial class Checkbox : AntInputBoolComponentBase
    {
        /// <summary>
        /// Content to display next to checkbox
        /// </summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Callback executed when checked state changes
        /// </summary>
        //[Obsolete] attribute does not work with [Parameter] for now. Tracking issue: https://github.com/dotnet/razor/issues/7657
        [Obsolete("Instead use @bind-Checked or EventCallback<bool> CheckedChanged .")]
        [Parameter]
        public EventCallback<bool> CheckedChange { get; set; }

        [Obsolete("Currently not implemented")]
        [Parameter]
        public Expression<Func<bool>> CheckedExpression { get; set; }

        /// <summary>
        /// Indeterminate checked state of checkbox
        /// </summary>
        [Parameter]
        public bool Indeterminate { get; set; }

        /// <summary>
        /// Label for checkbox
        /// </summary>
        [Parameter]
        public string Label { get; set; }

        [CascadingParameter]
        public CheckboxGroup CheckboxGroup { get; set; }

        internal bool IsFromOptions { get; set; }
        protected override void OnInitialized()
        {
            base.OnInitialized();
            SetClass();
            CheckboxGroup?.AddItem(this);
        }

        protected override void Dispose(bool disposing)
        {
            CheckboxGroup?.RemoveItem(this);
            base.Dispose(disposing);
        }

        protected ClassMapper ClassMapperLabel { get; } = new ClassMapper();

        private string _prefixCls = "ant-checkbox";

        protected void SetClass()
        {
            ClassMapperLabel.Clear()
                .Add($"{_prefixCls}-wrapper")
                .If($"{_prefixCls}-wrapper-checked", () => Checked);

            ClassMapper.Clear()
                .Add(_prefixCls)
                .If($"{_prefixCls}-checked", () => Checked && !Indeterminate)
                .If($"{_prefixCls}-disabled", () => Disabled)
                .If($"{_prefixCls}-indeterminate", () => Indeterminate)
                .If($"{_prefixCls}-rtl", () => RTL);
        }

        protected async Task InputCheckedChange(ChangeEventArgs args)
        {
            if (args != null && args.Value is bool value)
            {
                await base.ChangeValue(value);

                if (CheckedChange.HasDelegate) //kept for compatibility reasons with previous versions
                    await CheckedChange.InvokeAsync(value);
                CheckboxGroup?.OnCheckboxChange(this);
            }
        }

        internal void SetValue(bool value) => Checked = value;
    }
}
