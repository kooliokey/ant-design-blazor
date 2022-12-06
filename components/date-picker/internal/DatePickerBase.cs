﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AntDesign.core.Extensions;
using AntDesign.Datepicker.Locale;
using AntDesign.Internal;
using AntDesign.JsInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OneOf;

namespace AntDesign
{
    public abstract class DatePickerBase<TValue> : AntInputComponentBase<TValue>, IDatePicker
    {
        DateTime? IDatePicker.HoverDateTime { get; set; }
        private TValue _swpValue;

        public const int START_PICKER_INDEX = 0;
        public const int END_PICKER_INDEX = 0;

        [Parameter]
        public string PrefixCls { get; set; } = "ant-picker";

        protected string _picker;
        protected bool _isSetPicker = false;
        private bool _isNullableEvaluated;
        private bool _isNullable;

        /// <summary>
        /// Stores information if TValue is a nullable type.
        /// </summary>
        protected bool IsNullable
        {
            get
            {
                if (!_isNullableEvaluated)
                {
                    Type type = typeof(TValue);
                    _isNullable = type.IsAssignableFrom(typeof(DateTime?)) || type.IsAssignableFrom(typeof(DateTime?[]));
                    _isNullableEvaluated = true;
                }
                return _isNullable;
            }
        }

        /// <summary>
        /// Set picker type
        /// </summary>
        [Parameter]
        public string Picker
        {
            get => _picker;
            set
            {
                _isSetPicker = true;
                _picker = value;
                InitPicker(value);
            }
        }

        /// <summary>
        /// Selector for placing the container of the popup in
        /// </summary>
        [Parameter]
        public string PopupContainerSelector { get; set; }

        /// <summary>
        /// Disable the date picker. 
        /// When given a single boolean, it will disable all of it. 
        /// When given an array of booleans, it represents disabling the start/end of a range: [start, end]
        /// </summary>
        [Parameter]
        public OneOf<bool, bool[]> Disabled { get; set; } = new bool[] { false, false };

        /// <summary>
        /// Overlay adjustment strategy (when for example browser resize is happening)
        /// </summary>
        [Parameter]
        public TriggerBoundaryAdjustMode BoundaryAdjustMode { get; set; } = TriggerBoundaryAdjustMode.InView;

        /// <summary>
        /// Show a border or not
        /// </summary>
        /// <default value="true"/>
        [Parameter]
        public bool Bordered { get; set; } = true;

        /// <summary>
        /// Autofocus on the input or not
        /// </summary>
        /// <default value="false"/>
        [Parameter]
        public bool AutoFocus { get; set; } = false;

        /// <summary>
        /// If the picker is open or not
        /// </summary>
        /// <default value="false" />
        [Parameter]
        public bool Open { get; set; }

        /// <summary>
        /// If the picker is read-only or not
        /// </summary>
        /// <default value="false"/>
        [Parameter]
        public bool InputReadOnly { get; set; } = false;

        /// <summary>
        /// Whether to show the Today button which selects Today from any date
        /// </summary>
        /// <default value="true"/>
        [Parameter]
        public bool ShowToday { get; set; } = true;

        /// <summary>
        /// Locale for localizing UI strings
        /// </summary>
        /// <default value="LocaleProvider.CurrentLocale.DatePicker" />
        [Parameter]
        public DatePickerLocale Locale
        {
            get { return _locale; }
            set
            {
                _locale = value;
                _isLocaleSetOutside = true;
            }
        }

        /// <summary>
        /// CultureInfo to use for localization
        /// </summary>
        /// <default value="CultureInfo for Locale"/>
        [Parameter]
        public override CultureInfo CultureInfo
        {
            get
            {
                return _isCultureInfoOutside ? base.CultureInfo : _locale.GetCultureInfo();
            }
            set
            {
                _isCultureInfoOutside = true;
                if (!_isLocaleSetOutside &&
                    (
                    (base.CultureInfo != value && base.CultureInfo.Name != value.Name)
                    ||
                    LocaleProvider.CurrentLocale.LocaleName != value.Name
                    ))
                {
                    _locale = LocaleProvider.GetLocale(value).DatePicker;
                }
                base.CultureInfo = value;
            }
        }

        /// <summary>
        /// If time should be shown or not. Contains the boolean decision made from setting <see cref="ShowTime" />
        /// </summary>
        public bool IsShowTime { get; protected set; }

        /// <summary>
        /// Time format when showing time. Contains the string format from setting <see cref="ShowTime" /> with a string
        /// </summary>
        public string ShowTimeFormat { get; protected set; }

        protected OneOf<bool, string> _showTime = null;

        private bool _timeFormatProvided;

        /// <summary>
        /// Show time or not. 
        /// <para>When boolean, it sets ShowTime to the boolean.</para>
        /// <para>When string, it sets ShowTime to true and uses the string value as the time format.</para>
        /// </summary>
        [Parameter]
        public OneOf<bool, string> ShowTime
        {
            get => _showTime;
            set
            {
                _showTime = value;

                value.Switch(booleanValue =>
                {
                    IsShowTime = booleanValue;
                }, strValue =>
                {
                    IsShowTime = true;
                    _timeFormatProvided = true;
                    ShowTimeFormat = strValue;
                });
            }
        }

        /// <summary>
        /// Allow clearing the selected value or not
        /// </summary>
        /// <default value="true"/>
        [Parameter]
        public bool AllowClear { get; set; } = true;

        protected string[] _placeholders = new string[] { "", "" };
        protected OneOf<string, string[]> _placeholder;

        /// <summary>
        /// Placeholder for input
        /// </summary>
        [Parameter]
        public OneOf<string, string[]> Placeholder
        {
            get => _placeholder;
            set
            {
                _placeholder = value;
                value.Switch(single =>
                {
                    _placeholders[0] = single;
                }, arr =>
                {
                    _placeholders[0] = arr.Length > 0 ? arr[0] : _placeholders[0];
                    _placeholders[1] = arr.Length > 1 ? arr[1] : _placeholders[1];
                });
            }
        }

        /// <summary>
        /// Style applied to popup
        /// </summary>
        [Parameter]
        public string PopupStyle { get; set; }

        /// <summary>
        /// Picker class name
        /// </summary>
        [Parameter]
        public string ClassName { get; set; }

        /// <summary>
        /// Class name for popover dropdown
        /// </summary>
        [Parameter]
        public string DropdownClassName { get; set; }

        /// <summary>
        /// Format for the DateTime display
        /// </summary>
        [Parameter]
        public string Format { get; set; }

        private TValue _defaultValue;

        /// <summary>
        /// Default value
        /// </summary>
        [Parameter]
        public TValue DefaultValue
        {
            get => _defaultValue;
            set => _defaultValue = SortValue(value);
        }

        protected bool[] UseDefaultPickerValue { get; } = new bool[2];
        private TValue _defaultPickerValue;

        /// <summary>
        /// Default value of the picker
        /// </summary>
        [Parameter]
        public TValue DefaultPickerValue
        {
            get => _defaultPickerValue;
            set => _defaultPickerValue = SortValue(value);
        }

        /// <summary>
        /// Custom suffix icon
        /// </summary>
        [Parameter]
        public RenderFragment SuffixIcon { get; set; }

        /// <summary>
        /// Range selection presets to allow the user to select a range with one button click
        /// </summary>
        [Parameter]
        public Dictionary<string, DateTime?[]> Ranges { get; set; } = new Dictionary<string, DateTime?[]>();

        /// <summary>
        /// Extra content to display in picker footer
        /// </summary>
        [Parameter]
        public RenderFragment RenderExtraFooter { get; set; }

        /// <summary>
        /// Callback executed when clear is clicked
        /// </summary>
        [Obsolete("Use OnClear instead")]
        [Parameter]
        public EventCallback OnClearClick { get; set; }

        /// <summary>
        /// Called when clear button clicked.
        /// </summary>
        [Parameter]
        public EventCallback OnClear { get; set; }

        /// <summary>
        /// Callback executed when ok is clicked
        /// </summary>
        [Parameter]
        public EventCallback OnOk { get; set; }

        /// <summary>
        /// Callback executed when popover calendar is opened or closed
        /// </summary>
        [Parameter]
        public EventCallback<bool> OnOpenChange { get; set; }

        /// <summary>
        /// Callback executed when the type of panel displayed changes
        /// </summary>
        [Parameter]
        public EventCallback<DateTimeChangedEventArgs> OnPanelChange { get; set; }

        /// <summary>
        /// Function to determine if a provided date should be disabled
        /// </summary>
        [Parameter]
        public Func<DateTime, bool> DisabledDate { get; set; } = null;

        /// <summary>
        /// Function to determine if a hours in a date should be disabled
        /// </summary>
        [Parameter]
        public Func<DateTime, int[]> DisabledHours { get; set; } = null;

        /// <summary>
        /// Function to determine if a minutes in a date should be disabled
        /// </summary>
        [Parameter]
        public Func<DateTime, int[]> DisabledMinutes { get; set; } = null;

        /// <summary>
        /// Function to determine if a seconds in a date should be disabled
        /// </summary>
        [Parameter]
        public Func<DateTime, int[]> DisabledSeconds { get; set; } = null;

        /// <summary>
        /// Function to determine what pieces of time should be disabled in a date
        /// </summary>
        [Parameter]
        public Func<DateTime, DatePickerDisabledTime> DisabledTime { get; set; } = null;

        /// <summary>
        /// Custom rendering for date cells
        /// </summary>
        [Parameter]
        public Func<DateTime, DateTime, RenderFragment> DateRender { get; set; }

        /// <summary>
        /// Custom rendering for month cells
        /// </summary>
        // TODO: need locale
        [Parameter]
        public Func<DateTime, RenderFragment> MonthCellRender { get; set; }

        /// <summary>
        /// When true, will use 12 hour time. When false will use 24 hour time
        /// </summary>
        /// <default value="false" />
        [Parameter]
        public bool Use12Hours { get; set; }

        /// <summary>
        /// Date used for "Today"
        /// </summary>
        public DateTime CurrentDate { get; set; } = DateTime.Today;

        protected DateTime[] PickerValues { get; } = new DateTime[] { DateTime.Today, DateTime.Today };

        /// <summary>
        /// If picker is a range picker or not
        /// </summary>
        public bool IsRange { get; protected set; }

        protected DatePickerInput _inputStart;
        protected DatePickerInput _inputEnd;
        protected OverlayTrigger _dropDown;
        protected bool _duringFocus;

        protected string _activeBarStyle = "";
        protected string _rangeArrowStyle = "";

        internal DatePickerStatus[] _pickerStatus = new DatePickerStatus[] { new DatePickerStatus(), new DatePickerStatus() };

        protected Stack<string> _prePickerStack = new Stack<string>();
        protected bool _isClose = true;
        protected bool _needRefresh;
        protected bool _duringManualInput;
        private bool _isLocaleSetOutside;
        private bool _isCultureInfoOutside;
        private DatePickerLocale _locale = LocaleProvider.CurrentLocale.DatePicker;
        protected bool _openingOverlay;

        protected ClassMapper _panelClassMapper = new ClassMapper();

        private static readonly int[] _hours = Enumerable.Range(0, 24).ToArray();
        private static readonly int[] _minutesSeconds = Enumerable.Range(0, 60).ToArray();

        internal event EventHandler<bool> OverlayVisibleChanged;

        private readonly object _eventLock = new();

        protected bool HasTimeInput => IsShowTime || Picker == DatePickerType.Time;

        event EventHandler<bool> IDatePicker.OverlayVisibleChanged
        {
            add
            {
                lock (_eventLock)
                {
                    OverlayVisibleChanged += value;
                }
            }

            remove
            {
                lock (_eventLock)
                {
                    OverlayVisibleChanged -= value;
                }
            }
        }

        protected override void OnInitialized()
        {
            // set default picker type
            if (_isSetPicker == false)
            {
                Picker = DatePickerType.Date;
            }

            if (_placeholder.Value is null)
            {
                _placeholder = IsRange ? new string[] { } : string.Empty;
            }

            this.SetClass();

            base.OnInitialized();
        }

        protected bool _shouldRender = true;

        protected override bool ShouldRender()
        {
            if (!_shouldRender)
            {
                _shouldRender = true;
                return false;
            }
            return base.ShouldRender();
        }

        public override Task SetParametersAsync(ParameterView parameters)
        {
            _needRefresh = true;

            if (!_timeFormatProvided || string.IsNullOrEmpty(ShowTimeFormat))
            {
                ShowTimeFormat = Use12Hours ? Locale.Lang.TimeFormat12Hour : Locale.Lang.TimeFormat;
            }

            return base.SetParametersAsync(parameters);
        }

        protected void SetClass()
        {
            this.ClassMapper.Clear()
                .Add(PrefixCls)
                .Get(() => $"{PrefixCls}-{Size}")
                .If($"{PrefixCls}-rtl", () => RTL)
                .If($"{PrefixCls}-borderless", () => Bordered == false)
                .If($"{PrefixCls}-disabled", () => IsDisabled() == true)
                .If($"{ClassName}", () => !string.IsNullOrEmpty(ClassName))
                .If($"{PrefixCls}-range", () => IsRange == true)
                .If($"{PrefixCls}-focused", () => AutoFocus == true)
                .If($"{PrefixCls}-status-error", () => ValidationMessages.Length > 0)
               //.If($"{PrefixCls}-normal", () => Image.IsT1 && Image.AsT1 == Empty.PRESENTED_IMAGE_SIMPLE)
               //.If($"{PrefixCls}-{Direction}", () => Direction.IsIn("ltr", "rlt"))
               ;

            _panelClassMapper
                .Add($"{PrefixCls}-panel")
                .If($"{PrefixCls}-panel-rtl", () => RTL);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await Js.InvokeVoidAsync(JSInteropConstants.AddPreventKeys, _inputStart.Ref, new[] { "ArrowUp", "ArrowDown" });
            }

            if (_needRefresh && IsRange)
            {
                if (_inputStart.IsOnFocused)
                {
                    HtmlElement element = await JsInvokeAsync<HtmlElement>(JSInteropConstants.GetDomInfo, _inputStart.Ref);
                    _activeBarStyle = $"width: {element.ClientWidth - 10}px; position: absolute; transform: translate3d(0px, 0px, 0px);";
                    _rangeArrowStyle = $"left: 12px";
                }
                else if (_inputEnd.IsOnFocused)
                {
                    HtmlElement element = await JsInvokeAsync<HtmlElement>(JSInteropConstants.GetDomInfo, _inputEnd.Ref);
                    int translateDistance = element.ClientWidth + 16;

                    if (RTL)
                    {
                        translateDistance = -translateDistance;
                    }

                    _activeBarStyle = $"width: {element.ClientWidth - 10}px; position: absolute; transform: translate3d({translateDistance}px, 0px, 0px);";
                    _rangeArrowStyle = $"left: {element.ClientWidth + 30}px";
                }
                else
                {
                    _activeBarStyle = "display: none";
                }

                StateHasChanged();
            }

            _needRefresh = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _ = InvokeAsync(async () =>
            {
                await Js.InvokeVoidAsync(JSInteropConstants.RemovePreventKeys, _inputStart.Ref);
            });
        }

        protected string GetInputValue(int index = 0)
        {
            DateTime? tryGetValue = GetIndexValue(index);

            if (tryGetValue == null)
            {
                return "";
            }

            DateTime value = (DateTime)tryGetValue;

            return GetFormatValue(value, index);
        }

        protected void ChangeFocusTarget(bool inputStartFocus, bool inputEndFocus)
        {
            if (!IsRange)
            {
                return;
            }
            _duringManualInput = false;
            _needRefresh = true;
            _inputStart.IsOnFocused = inputStartFocus;
            _inputEnd.IsOnFocused = inputEndFocus;

            SetDisabledTime();
        }

        protected virtual Task PickerClicked()
        {
            AutoFocus = true;
            return Task.CompletedTask;
        }

        protected virtual async Task OnSelect(DateTime date, int index, bool switchFocus = true, bool closeDropdown = true)
        {
            _duringManualInput = false;

            var isInitialPickerType = _picker == _pickerStatus[index].InitPicker;

            if (isInitialPickerType)
            {
                _pickerStatus[index].SelectedValue = date;

                if (IsRange)
                {
                    if (!HasTimeInput)
                    {
                        if (IsValidRange(date, index))
                        {
                            var otherIndex = Math.Abs(index - 1);

                            if (_pickerStatus[otherIndex].SelectedValue is not null)
                            {
                                ChangeValue(_pickerStatus[otherIndex].SelectedValue.Value, otherIndex, closeDropdown);
                            }

                            ChangeValue(date, index, closeDropdown);
                        }
                    }
                }
                else if (!HasTimeInput)
                {
                    ChangeValue(date, index, closeDropdown);
                }

                // auto focus the other input
                if (switchFocus)
                {
                    if (IsRange && !HasTimeInput)
                    {
                        await SwitchFocus(index);
                    }
                    else
                    {
                        await Focus(index);
                    }
                }
            }
            else
            {
                _picker = _prePickerStack.Pop();
            }

            if (!isInitialPickerType || !IsRange || IsShowTime)
            {
                ChangePickerValue(date, index);
            }

            SetDisabledTime();
        }

        internal async Task OnOkClick()
        {
            var index = GetOnFocusPickerIndex();

            if (IsRange)
            {
                var otherIndex = Math.Abs(index - 1);
                var otherValue = GetIndexValue(otherIndex);

                if (_pickerStatus[index].SelectedValue is not null && otherValue is not null
                    && IsValidRange(_pickerStatus[index].SelectedValue.Value, index))
                {
                    if (_pickerStatus[otherIndex].SelectedValue is not null)
                    {
                        ChangeValue(_pickerStatus[otherIndex].SelectedValue.Value, otherIndex);
                    }

                    ChangeValue(_pickerStatus[index].SelectedValue.Value, index);
                }

                if (!(await SwitchFocus(index)))
                {
                    Close();
                }
            }
            else
            {
                if (HasTimeInput && _pickerStatus[index].SelectedValue is not null)
                {
                    ChangeValue(_pickerStatus[index].SelectedValue.Value, index);
                }

                Close();
            }

            await OnOk.InvokeAsync(null);
        }

        internal void OnRangeItemOver(DateTime?[] range)
        {
            _swpValue = Value;
            Value = DataConvertionExtensions.Convert<DateTime?[], TValue>(new DateTime?[] { range[0], range[1] });
        }

        internal void OnRangeItemOut(DateTime?[] range) => Value = _swpValue;

        internal void OnRangeItemClicked(DateTime?[] range)
        {
            _swpValue = DataConvertionExtensions.Convert<DateTime?[], TValue>(new DateTime?[] { range[0], range[1] });
            ChangeValue((DateTime)range[0], 0);
            ChangeValue((DateTime)range[1], 1);
            Close();
        }

        protected async Task<bool> SwitchFocus(int index)
        {
            if (index == 0 && (_pickerStatus[1].SelectedValue is null || Open) && !_inputEnd.IsOnFocused && !IsDisabled(1))
            {
                await Blur(0);
                await Focus(1);
            }
            else if (index == 1 && (_pickerStatus[0].SelectedValue is null || Open) && !_inputStart.IsOnFocused && !IsDisabled(0))
            {
                await Blur(1);
                await Focus(0);
            }
            else
            {
                return false;
            }

            SetDisabledTime();

            return true;
        }

        protected abstract Task OnBlur(int index);

        protected void InitPicker(string picker)
        {
            if (string.IsNullOrEmpty(_pickerStatus[0].InitPicker))
            {
                _pickerStatus[0].InitPicker = picker;
            }
            if (string.IsNullOrEmpty(_pickerStatus[1].InitPicker))
            {
                _pickerStatus[1].InitPicker = picker;
            }
            ResetPlaceholder();
        }

        protected bool IsDisabled(int? index = null)
        {
            bool disabled = false;

            Disabled.Switch(single =>
            {
                disabled = single;
            }, arr =>
            {
                if (index == null || index > 1 || index < 0)
                {
                    disabled = arr[0] && arr[1];
                }
                else
                {
                    disabled = arr[(int)index];
                }
            });

            return disabled;
        }

        /// <summary>
        /// Close the popover
        /// </summary>
        public void Close()
        {
            _duringManualInput = false;
            _dropDown?.Hide();
        }

        /// <summary>
        /// Add focus to picker
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task Focus(int index = 0)
        {
            DatePickerInput input = null;

            if (index == 0)
            {
                input = _inputStart;
            }
            else if (index == 1 && IsRange)
            {
                input = _inputEnd;
            }

            if (input != null)
            {
                input.IsOnFocused = true;
                await FocusAsync(input.Ref);
                _needRefresh = true;
            }
        }

        /// <summary>
        /// Remove focus from picker
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task Blur(int index = 0)
        {
            DatePickerInput input = null;
            _duringManualInput = false;
            if (index == 0)
            {
                input = _inputStart;
            }
            else if (index == 1 && IsRange)
            {
                input = _inputEnd;
            }

            if (input != null)
            {
                input.IsOnFocused = false;
                await JsInvokeAsync(JSInteropConstants.Blur, input.Ref);
                _needRefresh = true;
            }
        }

        /// <summary>
        /// Get index of picker which is currently focused
        /// </summary>
        /// <returns>Index of picker. 0 can also mean neither is focused.</returns>
        public int GetOnFocusPickerIndex()
        {
            if (_inputStart != null && _inputStart.IsOnFocused)
            {
                return 0;
            }

            if (_inputEnd != null && _inputEnd.IsOnFocused)
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Get pickerValue by picker index. Note that index refers to a picker panel
        /// and not to input text. For RangePicker 2 inputs generate 2 panels.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DateTime GetIndexPickerValue(int index)
        {
            int tempIndex = GetOnFocusPickerIndex();

            var pickerValue = PickerValues[tempIndex];

            if (index == 0 || HasTimeInput)
            {
                return pickerValue;
            }
            else
            {
                //First picker panel will show the value, second panel shows next
                //expected value that depends on Picker type
                return GetClosingDate(pickerValue);
            }
        }

        internal DateTime GetClosingDate(DateTime pickerValue, int offset = 1)
        {
            return Picker switch
            {
                DatePickerType.Year => pickerValue.AddYears(offset * 10),
                DatePickerType.Quarter or DatePickerType.Decade or DatePickerType.Month => pickerValue.AddYears(offset),
                _ => pickerValue.AddMonths(offset)
            };
        }

        public void ChangePlaceholder(string placeholder, int index = 0)
        {
            _placeholders[index] = placeholder;

            StateHasChanged();
        }

        public void ResetPlaceholder(int rangePickerIndex = -1)
        {
            _placeholder.Switch(single =>
            {
                var placeholder = string.IsNullOrEmpty(single) ? DatePickerPlaceholder.GetPlaceholderByType(Picker, Locale) : single;
                _placeholders[0] = placeholder;
                _placeholders[1] = placeholder;
            }, arr =>
            {
                var (startPlaceholder, endPlaceholder) = DatePickerPlaceholder.GetRangePlaceHolderByType(Picker, Locale);

                if (rangePickerIndex >= 0)
                {
                    var placeholder = arr.Length > rangePickerIndex ? arr[rangePickerIndex] : null;
                    placeholder ??= rangePickerIndex == 0 ? startPlaceholder : endPlaceholder;
                    _placeholders[rangePickerIndex] = placeholder;
                }
                else
                {
                    _placeholders[0] = arr.Length > 0 ? arr[0] : startPlaceholder;
                    _placeholders[1] = arr.Length > 1 ? arr[1] : endPlaceholder;
                }
            });
        }

        private int _htmlInputSize;

        protected int HtmlInputSize
        {
            get
            {
                if (_htmlInputSize == 0)
                {
                    _htmlInputSize = InternalFormat.Length + (int)(InternalFormat.Count(ch => ch > 127) * 1.34) + 2;
                    if (_htmlInputSize < 12)
                    {
                        _htmlInputSize = 12;
                    }
                }
                return _htmlInputSize;
            }
        }

        private string _internalFormat;

        private string InternalFormat
        {
            get
            {
                if (string.IsNullOrEmpty(_internalFormat))
                {
                    if (!string.IsNullOrEmpty(Format))
                        _internalFormat = Format;
                    else
                        _internalFormat = _pickerStatus[0].InitPicker switch
                        {
                            DatePickerType.Date => GetTimeFormat(),
                            DatePickerType.Month => Locale.Lang.YearMonthFormat,
                            DatePickerType.Year => Locale.Lang.YearFormat,
                            DatePickerType.Time when Use12Hours => Locale.Lang.TimeFormat12Hour,
                            DatePickerType.Time => Locale.Lang.TimeFormat,
                            DatePickerType.Week => $"{Locale.Lang.YearFormat}-0{Locale.Lang.Week}",
                            DatePickerType.Quarter => $"{Locale.Lang.YearFormat}-Q0",
                            _ => Locale.Lang.DateFormat,
                        };
                }
                return _internalFormat;
            }
        }

        private string GetTimeFormat()
        {
            if (IsShowTime)
            {
                if (_timeFormatProvided)
                {
                    return $"{Locale.Lang.DateFormat} {ShowTimeFormat}";
                }
                return Use12Hours ? Locale.Lang.DateTimeFormat12Hour : Locale.Lang.DateTimeFormat;
            }
            return Locale.Lang.DateFormat;
        }

        private FormatAnalyzer _formatAnalyzer;
        protected FormatAnalyzer FormatAnalyzer => _formatAnalyzer ??= new(InternalFormat, Picker, Locale, CultureInfo);

        public string GetFormatValue(DateTime value, int index)
        {
            string format;
            if (string.IsNullOrEmpty(Format))
                format = _pickerStatus[index].InitPicker switch
                {
                    DatePickerType.Week => $"{Locale.Lang.YearFormat}-{DateHelper.GetWeekOfYear(value, Locale.FirstDayOfWeek)}{Locale.Lang.Week}",
                    DatePickerType.Quarter => $"{Locale.Lang.YearFormat}-{DateHelper.GetDayOfQuarter(value)}",
                    _ => InternalFormat,
                };
            else
                format = InternalFormat;
            return value.ToString(format, CultureInfo);
        }

        /// <summary>
        /// Changes what date(s) will be visible on the picker.
        /// </summary>
        /// <param name="date">New date to be saved.</param>
        /// <param name="index">Index of the input box, where 0 = inputStart and 1 = inputEnd (only RangePicker)</param>
        internal void ChangePickerValue(DateTime date, int? index = null)
        {
            if (index == null)
                index = GetOnFocusPickerIndex();

            PickerValues[index.Value] = date;
            if (IsRange)
            {
                if (!UseDefaultPickerValue[1] && !_pickerStatus[1].IsValueSelected && index == 0)
                {
                    PickerValues[1] = date;
                }
                else if (!UseDefaultPickerValue[0] && !_pickerStatus[0].IsValueSelected && index == 1)
                {
                    PickerValues[0] = date;
                }
            }

            if (OnPanelChange.HasDelegate)
            {
                OnPanelChange.InvokeAsync(new DateTimeChangedEventArgs
                {
                    Date = PickerValues[index.Value],
                    DateString = _picker
                });
            }

            StateHasChanged();
        }

        public void ChangePickerType(string type)
        {
            ChangePickerType(type, 0);
        }

        public virtual void ChangePickerType(string type, int index)
        {
            _prePickerStack.Push(_picker);
            _picker = type;

            if (OnPanelChange.HasDelegate)
            {
                OnPanelChange.InvokeAsync(new DateTimeChangedEventArgs
                {
                    Date = PickerValues[index],
                    DateString = _picker
                });
            }

            StateHasChanged();
        }

        /// <summary>
        /// Change the value of the given picker panel
        /// </summary>
        /// <param name="value">Value to change to</param>
        /// <param name="index">Index of the picker panel to change</param>
        /// <param name="closeDropdown">Close the panel when set or not</param>
        public abstract void ChangeValue(DateTime value, int index = 0, bool closeDropdown = true);

        /// <summary>
        /// Clear the value for the given picker panel index
        /// </summary>
        /// <param name="index">Index of the picker panel to clear</param>
        /// <param name="closeDropdown">Close the panel when cleared or not</param>
        public abstract void ClearValue(int index = 0, bool closeDropdown = true);

        /// <summary>
        /// Get value of picker panel at index
        /// </summary>
        /// <param name="index">Index of picker panel to get value of</param>
        /// <returns>DateTime value of panel</returns>
        public abstract DateTime? GetIndexValue(int index);

        protected TValue SortValue(TValue value)
        {
            if (value == null)
            {
                return value;
            }
            TValue orderedValue = value;
            if (IsRange)
            {
                if (IsNullable)
                {
                    var tempValue = value as DateTime?[];
                    if (tempValue[0] == null || tempValue[1] == null)
                        return orderedValue;

                    if ((tempValue[0] ?? DateTime.Now).CompareTo((tempValue[1] ?? DateTime.Now)) > 0)
                        orderedValue = DataConvertionExtensions.Convert<DateTime?[], TValue>(new DateTime?[] { tempValue[1], tempValue[0] });
                }
                else
                {
                    var tempValue = value as DateTime[];
                    if (tempValue[0].CompareTo(tempValue[1]) > 0)
                        orderedValue = DataConvertionExtensions.Convert<DateTime[], TValue>(new DateTime[] { tempValue[1], tempValue[0] });
                }
            }
            return orderedValue;
        }

        protected void InvokeInternalOverlayVisibleChanged(bool visible)
        {
            var index = GetOnFocusPickerIndex();

            _pickerStatus[index].SelectedValue = null;

            if (IsRange)
            {
                _pickerStatus[Math.Abs(index - 1)].SelectedValue = null;

                if (!visible)
                {
                    ResetPlaceholder();
                }
                else
                {
                    _pickerStatus[index].SelectedValue = GetIndexValue(index);
                }
            }

            OverlayVisibleChanged?.Invoke(this, visible);
        }

        protected void SetDisabledTime()
        {
            if (!IsRange || !IsShowTime)
            {
                return;
            }

            var endValue = GetIndexValue(1);
            var startValue = GetIndexValue(0);
            var isSameDate = startValue?.Date == endValue?.Date;

            if (_inputStart.IsOnFocused)
            {
                DisabledHours = dateTime => isSameDate ?
                   _hours.Where(h => h > endValue?.Hour).ToArray() : Array.Empty<int>();
                DisabledMinutes = dateTime => isSameDate && startValue?.Hour == endValue?.Hour ?
                   _minutesSeconds.Where(m => m > endValue?.Minute).ToArray() : Array.Empty<int>();
                DisabledSeconds = dateTime => isSameDate && startValue?.Hour == endValue?.Hour && startValue?.Minute == endValue?.Minute ?
                   _minutesSeconds.Where(s => s > endValue?.Second).ToArray() : Array.Empty<int>();
            }
            else if (_inputEnd.IsOnFocused)
            {
                DisabledHours = dateTime => isSameDate ?
                   _hours.Where(h => h < startValue?.Hour).ToArray() : Array.Empty<int>();
                DisabledMinutes = dateTime => isSameDate && startValue?.Hour == endValue?.Hour ?
                   _minutesSeconds.Where(m => m < startValue?.Minute).ToArray() : Array.Empty<int>();
                DisabledSeconds = dateTime => isSameDate && startValue?.Hour == endValue?.Hour && startValue?.Minute == endValue?.Minute ?
                   _minutesSeconds.Where(s => s < startValue?.Second).ToArray() : Array.Empty<int>();
            }
        }

        protected bool IsValidRange(DateTime newValue, int newValueIndex)
        {
            var otherValue = GetIndexValue(Math.Abs(newValueIndex - 1));

            if (otherValue is null)
            {
                return false;
            }

            return newValueIndex switch
            {
                0 when newValue > otherValue => false,
                1 when newValue < otherValue => false,
                _ => true
            };
        }

        internal void OnNowClick()
        {
            ChangeValue(DateTime.Now, GetOnFocusPickerIndex());
            Close();
        }
    }
}
