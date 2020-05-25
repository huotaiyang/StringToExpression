using System;
using System.Collections.Generic;

namespace StringToExpression
{
    /// <summary>
    /// 类型隐式转换
    /// </summary>
    internal static class TypeImplicitConvert
    {
        /// <summary>
        /// 可以进行隐式类型转换的类型字典。
        /// </summary>
        private static readonly Dictionary<Type, HashSet<TypeCode>> ImplicitConversions = new Dictionary<Type, HashSet<TypeCode>>()
        {
            {typeof(short), new HashSet<TypeCode>(){ TypeCode.SByte, TypeCode.Byte } },
            { typeof(ushort), new HashSet<TypeCode>(){ TypeCode.Char, TypeCode.Byte } },
            { typeof(int), new HashSet<TypeCode>(){ TypeCode.Char, TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16 } },
            { typeof(uint), new HashSet<TypeCode>(){ TypeCode.Char, TypeCode.Byte, TypeCode.UInt16 } },
            { typeof(long), new HashSet<TypeCode>(){ TypeCode.Char, TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32 } },
            { typeof(ulong), new HashSet<TypeCode>(){ TypeCode.Char, TypeCode.Byte, TypeCode.UInt16, TypeCode.UInt32 } },
            { typeof(float), new HashSet<TypeCode>(){ TypeCode.Char, TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64 } },
            { typeof(double), new HashSet<TypeCode>(){ TypeCode.Char, TypeCode.SByte, TypeCode.Byte, TypeCode.Int16,TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single } },
            { typeof(decimal), new HashSet<TypeCode>(){ TypeCode.Char, TypeCode.SByte, TypeCode.Byte,TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64 } },
        };

        /// <summary>
        /// 满足ImplicitConversions，但根据VS编译规则，不能隐式转换的，需要过滤
        /// </summary>
        private static readonly List<Type> FilterImplicitConversions = new List<Type> { typeof(long), typeof(ulong) };

        /// <summary>
        /// 标准隐式转换
        /// </summary>
        /// <param name="type1">类型1</param>
        /// <param name="type2">类型2</param>
        /// <returns>标准隐式转换后的类型，不能转换返回null</returns>
        internal static Type StandardImplicit(Type type1, Type type2)
        {
            Type result = null;
            bool hasNullableType = type1.IsNullable() || type2.IsNullable();
            type1 = type1.GetNoneNullableType();
            type2 = type2.GetNoneNullableType();
            if (type1 == type2 || type1.IsStandardImplicitFrom(type2))
            {
                result = type1;
            }
            else if (type2.IsStandardImplicitFrom(type1))
            {
                result = type2;
            }
            else
            {
                // 排除掉不支持转换的，然后判断是否能同时隐式转换成同一类型
                if (!(FilterImplicitConversions.Contains(type1) && FilterImplicitConversions.Contains(type2)))
                {
                    foreach (var item in ImplicitConversions)
                    {
                        if (item.Value.Contains(Type.GetTypeCode(type1)) && item.Value.Contains(Type.GetTypeCode(type2)))
                        {
                            result = item.Key;
                        }
                    }
                }
            }

            if (result != null && hasNullableType)
            {
                result = result.GetNullableType();
            }

            return result;
        }

        /// <summary>
        /// 是否可以隐式转换
        /// </summary>
        /// <param name="targetType">目标类型</param>
        /// <param name="fromType">源类型</param>
        /// <returns>是否可以隐式转换</returns>
        internal static bool IsStandardImplicitFrom(this Type targetType, Type fromType)
        {
            // 判断隐式数值转换。
            HashSet<TypeCode> typeSet;
            // 加入 IsEnum 的判断，是因为枚举的 TypeCode 是其基类型的 TypeCode，会导致判断失误。
            if (!targetType.IsEnum && ImplicitConversions.TryGetValue(targetType, out typeSet))
            {
                if (!fromType.IsEnum && typeSet.Contains(Type.GetTypeCode(fromType)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
