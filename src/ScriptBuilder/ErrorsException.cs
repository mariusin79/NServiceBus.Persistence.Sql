﻿using System;

namespace NServiceBus.Persistence.Sql
{
    public class ErrorsException : Exception
    {
        public string FileName { get; set; }
        public ErrorsException(string message) : base(message)
        {
        }
    }
}