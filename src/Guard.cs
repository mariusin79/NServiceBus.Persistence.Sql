﻿
using System;
using System.Collections;

static class Guard
{
    public static void AgainstNull(string argumentName, object value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstNullAndEmpty(string argumentName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstSqlDelimiters(string argumentName, string value)
    {
        if (value.Contains("]") || value.Contains("[") || value.Contains("`"))
        {
            throw new ArgumentException($"The argument '{value}' contains a ']', '[' or '`'. Names and schemas automatically quoted.");
        }
    }

    public static void AgainstEmpty(string argumentName, string value)
    {
        if (value !=null && string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstNullAndEmpty(string argumentName, ICollection value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(argumentName);
        }
        if (value.Count == 0)
        {
            throw new ArgumentOutOfRangeException(argumentName);
        }
    }

    public static void AgainstNegativeAndZero(string argumentName, int value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(argumentName);
        }
    }

    public static void AgainstNegative(string argumentName, int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(argumentName);
        }
    }

    public static void AgainstNegativeAndZero(string argumentName, TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(argumentName);
        }
    }

    public static void AgainstNegative(string argumentName, TimeSpan value)
    {
        if (value < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(argumentName);
        }
    }
}
