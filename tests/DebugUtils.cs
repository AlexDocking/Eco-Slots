﻿using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Parts.Tests
{
    public class DebugUtils
    {
        public static void AssertEquals<T>(T expected, T actual, string message = null) => Assert(object.Equals(expected, actual), message + $":'{actual}' is not equal to '{expected}'");
        public static void Assert(bool val, string message) { if (!val) Fail(message); }
        public static void Fail(string message)
        {
            if (string.IsNullOrEmpty(message)) message = "<no message>";
            message += "\n" + System.Environment.StackTrace;
            Log.WriteErrorLine(Localizer.Do($"Assertion failed: {message}"));
        }
    }
}