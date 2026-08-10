using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ResumableFileTransfer.Common
{
    public static class EnumExtension
    {
        /// <summary>
        /// Gets the description by enum.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        public static string GetDescription(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            if (fieldInfo == null)
            {
                return string.Empty;
            }

            var customAttrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            if (customAttrs.Length == 0)
                return fieldInfo.Name;

            return customAttrs[0].Description ?? fieldInfo.Name;
        }
    }
}
