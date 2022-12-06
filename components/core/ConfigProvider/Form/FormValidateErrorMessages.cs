﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace AntDesign
{
    public class FormValidateErrorMessages
    {
        private static readonly string _typeTemplate = "'{0}' is not a valid {1}";

        /// <summary>
        /// Default generic validation error message
        /// </summary>
        public string Default { get; set; } = "Validation error on field '{0}'";

        /// <summary>
        /// Default validation message for the Required rule
        /// </summary>
        public string Required { get; set; } = "'{0}' is required";

        /// <summary>
        /// Default validation message for the OneOf rule
        /// </summary>
        public string OneOf { get; set; } = "'{0}' must be one of [{1}]";

        [Obsolete("Currently unused")]
        public string Whitespace { get; set; } = "'{0}' cannot be empty";

        /// <summary>
        /// Messages for when a field's type does not match the expected type
        /// </summary>
        public TypesMessage Types { get; set; } = new()
        {
            String = _typeTemplate,
            Array = _typeTemplate,
            Object = _typeTemplate,
            Enum = _typeTemplate,
            Number = _typeTemplate,
            Date = _typeTemplate,
            Boolean = _typeTemplate,
            Integer = _typeTemplate,
            Float = _typeTemplate,
            Regexp = _typeTemplate,
            Email = _typeTemplate,
            Url = _typeTemplate,
        };

        /// <summary>
        /// Messages for string type
        /// </summary>
        public StringMessage String { get; set; } = new()
        {
            Len = "'{0}' must be exactly {1} characters",
            Min = "'{0}' must be at least {1} characters",
            Max = "'{0}' cannot be longer than {1} characters",
            Range = "'{0}' must be between {1} and {2} characters",
        };

        /// <summary>
        /// Messages for number type
        /// </summary>
        public NumberMessage Number { get; set; } = new()
        {
            Len = "'{0}' must equal {1}",
            Min = "'{0}' cannot be less than {1}",
            Max = "'{0}' cannot be greater than {1}",
            Range = "'{0}' must be between {1} and {2}",
        };

        /// <summary>
        /// Messages for array type
        /// </summary>
        public ArrayMessage Array { get; set; } = new()
        {
            Len = "'{0}' must be exactly {1} in length",
            Min = "'{0}' cannot be less than {1} in length",
            Max = "'{0}' cannot be greater than {1} in length",
            Range = "'{0}' must be between {1} and {2} in length",
        };

        /// <summary>
        /// Messages for the Pattern validation rule
        /// </summary>
        public PatternMessage Pattern { get; set; } = new()
        {
            Mismatch = "'{0}' does not match pattern {1}",
        };

        internal string GetTypeMessage(FormFieldType type)
        {
            return type switch
            {
                FormFieldType.String => Types.String,
                FormFieldType.Number => Types.Number,
                FormFieldType.Boolean => Types.Boolean,
                FormFieldType.Regexp => Types.Regexp,
                FormFieldType.Integer => Types.Integer,
                FormFieldType.Float => Types.Float,
                FormFieldType.Array => Types.Array,
                FormFieldType.Object => Types.Object,
                FormFieldType.Enum => Types.Enum,
                FormFieldType.Date => Types.Date,
                FormFieldType.Url => Types.Url,
                FormFieldType.Email => Types.Email,
                _ => "",
            };
        }

        #region classes
        public class TypesMessage
        {
            public string String { get; set; }
            public string Array { get; set; }
            public string Object { get; set; }
            public string Enum { get; set; }
            public string Number { get; set; }
            public string Date { get; set; }
            public string Boolean { get; set; }
            public string Integer { get; set; }
            public string Float { get; set; }
            public string Regexp { get; set; }
            public string Email { get; set; }
            public string Url { get; set; }
        }

        public class StringMessage
        {
            public string Len { get; set; }
            public string Min { get; set; }
            public string Max { get; set; }
            public string Range { get; set; }
        }

        public class NumberMessage
        {
            public string Len { get; set; }
            public string Min { get; set; }
            public string Max { get; set; }
            public string Range { get; set; }

        }

        public class ArrayMessage
        {
            public string Len { get; set; }
            public string Min { get; set; }
            public string Max { get; set; }
            public string Range { get; set; }

        }

        public class PatternMessage
        {
            public string Mismatch { get; set; }
        }

        public class DateMessage
        {
            public string Format { get; set; }
            public string Parse { get; set; }
            public string Invalid { get; set; }
        };

        #endregion classes
    }
}
